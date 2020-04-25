﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TSSArt.StateMachine
{
	using DefaultHistoryContent = Dictionary<IIdentifier, ImmutableArray<IExecEvaluator>>;

	public sealed class StateMachineInterpreter
	{
		private const    string                                   StateStorageKey                  = "state";
		private const    string                                   StateMachineDefinitionStorageKey = "smd";
		private readonly DataModelValue                           _arguments;
		private readonly ImmutableDictionary<string, string>      _configuration;
		private readonly ImmutableArray<ICustomActionFactory>     _customActionProviders;
		private readonly ImmutableArray<IDataModelHandlerFactory> _dataModelHandlerFactories;
		private readonly CancellationToken                        _destroyToken;
		private readonly IErrorProcessor                          _errorProcessor;
		private readonly ChannelReader<IEvent>                    _eventChannel;
		private readonly ExternalCommunicationWrapper             _externalCommunication;
		private readonly LoggerWrapper                            _logger;
		private readonly INotifyStateChanged?                     _notifyStateChanged;
		private readonly PersistenceLevel                         _persistenceLevel;
		private readonly ImmutableArray<IResourceLoader>          _resourceLoaders;
		private readonly string                                   _sessionId;

		private readonly IStateMachineValidator  _stateMachineValidator = StateMachineValidator.Instance;
		private readonly CancellationToken       _stopToken;
		private readonly IStorageProvider        _storageProvider;
		private readonly CancellationToken       _suspendToken;
		private          CancellationTokenSource _anyTokenSource;
		private          IStateMachineContext    _context;
		private          IDataModelHandler       _dataModelHandler;
		private          DataModelValue          _doneData;
		private          InterpreterModel        _model;
		private          bool                    _stop;

		private StateMachineInterpreter(string sessionId, ChannelReader<IEvent> eventChannel, in InterpreterOptions options)
		{
			_sessionId = sessionId;
			_eventChannel = eventChannel;
			_suspendToken = options.SuspendToken;
			_stopToken = options.StopToken;
			_destroyToken = options.DestroyToken;
			_resourceLoaders = options.ResourceLoaders;
			_customActionProviders = options.CustomActionProviders;
			_dataModelHandlerFactories = options.DataModelHandlerFactories;
			_logger = new LoggerWrapper(options.Logger ?? DefaultLogger.Instance, sessionId);
			_externalCommunication = new ExternalCommunicationWrapper(options.ExternalCommunication, sessionId);
			_storageProvider = options.StorageProvider ?? NullStorageProvider.Instance;
			_configuration = options.Configuration ?? ImmutableDictionary<string, string>.Empty;
			_errorProcessor = options.ErrorProcessor ?? DefaultErrorProcessor.Instance;
			_persistenceLevel = options.PersistenceLevel;
			_notifyStateChanged = options.NotifyStateChanged;
			_arguments = options.Arguments.AsConstant();

			_anyTokenSource = null!;
			_dataModelHandler = null!;
			_context = null!;
			_model = null!;
		}

		private bool IsPersistingEnabled => _persistenceLevel != PersistenceLevel.None;

		private bool Running
		{
			get => !_stop && (!IsPersistingEnabled || _context.PersistenceContext.GetState((int) StateBagKey.Stop) == 0);
			set
			{
				_stop = !value;

				if (IsPersistingEnabled)
				{
					_context.PersistenceContext.SetState((int) StateBagKey.Stop, value ? 0 : 1);
				}
			}
		}

		public static ValueTask<DataModelValue> RunAsync(string sessionId, IStateMachine? stateMachine, ChannelReader<IEvent> eventChannel, in InterpreterOptions options = default)
		{
			if (sessionId == null) throw new ArgumentNullException(nameof(sessionId));
			if (eventChannel == null) throw new ArgumentNullException(nameof(eventChannel));

			return new StateMachineInterpreter(sessionId, eventChannel, options).Run(stateMachine);
		}

		private async ValueTask<InterpreterModel> BuildInterpreterModel(IStateMachine? stateMachine)
		{
			var wrapperErrorProcessor = new WrapperErrorProcessor(_errorProcessor);

			var interpreterModel = IsPersistingEnabled ? await TryRestoreInterpreterModel(stateMachine, wrapperErrorProcessor).ConfigureAwait(false) : null;

			if (interpreterModel != null)
			{
				return interpreterModel;
			}

			Infrastructure.Assert(stateMachine != null);

			var dataModelHandlerFactory = GetDataModelHandlerFactory(stateMachine.DataModelType, _dataModelHandlerFactories, wrapperErrorProcessor);
			_dataModelHandler = dataModelHandlerFactory.CreateHandler(wrapperErrorProcessor);

			_stateMachineValidator.Validate(stateMachine, wrapperErrorProcessor);

			var interpreterModelBuilder = new InterpreterModelBuilder(stateMachine, _dataModelHandler, _customActionProviders, wrapperErrorProcessor);
			interpreterModel = await interpreterModelBuilder.Build(_resourceLoaders, _stopToken).ConfigureAwait(false);

			_errorProcessor?.ThrowIfErrors();
			wrapperErrorProcessor.ThrowIfErrors();

			if (IsPersistingEnabled)
			{
				await SaveInterpreterModel(interpreterModel).ConfigureAwait(false);
			}

			return interpreterModel;
		}

		private async ValueTask<InterpreterModel?> TryRestoreInterpreterModel(IStateMachine? stateMachine, IErrorProcessor errorProcessor)
		{
			var storage = await _storageProvider.GetTransactionalStorage(partition: default, StateMachineDefinitionStorageKey, _stopToken).ConfigureAwait(false);
			await using (storage.ConfigureAwait(false))
			{
				var bucket = new Bucket(storage);

				if (bucket.TryGet(Key.Version, out int version) && version != 1)
				{
					throw new StateMachinePersistenceException(Resources.Exception_Persisted_state_can_t_be_read__Unsupported_version_);
				}

				if (bucket.TryGet(Key.SessionId, out string? sessionId) && sessionId != _sessionId)
				{
					throw new StateMachinePersistenceException(Resources.Exception_Persisted_state_can_t_be_read__Stored_and_provided_SessionIds_does_not_match);
				}

				if (!bucket.TryGet(Key.StateMachineDefinition, out var memory))
				{
					return null;
				}

				var smdBucket = new Bucket(new InMemoryStorage(memory.Span));


				var dataModelHandlerFactory = GetDataModelHandlerFactory(smdBucket.GetString(Key.DataModelType), _dataModelHandlerFactories, errorProcessor);
				_dataModelHandler = dataModelHandlerFactory.CreateHandler(DefaultErrorProcessor.Instance);

				ImmutableDictionary<int, IEntity>? entityMap = null;

				if (stateMachine != null)
				{
					entityMap = new InterpreterModelBuilder(stateMachine, _dataModelHandler, _customActionProviders, DefaultErrorProcessor.Instance).Build().EntityMap;
				}

				var restoredStateMachine = new StateMachineReader().Build(smdBucket, entityMap);

				if (stateMachine != null)
				{
					//TODO: Validate stateMachine vs restoredStateMachine (number of elements should be the same and documentId should point to the same entity type)
				}

				var wrapperErrorProcessor = new WrapperErrorProcessor(_errorProcessor);

				var interpreterModelBuilder = new InterpreterModelBuilder(restoredStateMachine, _dataModelHandler, _customActionProviders, wrapperErrorProcessor);

				_errorProcessor?.ThrowIfErrors();
				wrapperErrorProcessor.ThrowIfErrors();

				return interpreterModelBuilder.Build();
			}
		}

		private async ValueTask SaveInterpreterModel(InterpreterModel interpreterModel)
		{
			var storage = await _storageProvider.GetTransactionalStorage(partition: default, StateMachineDefinitionStorageKey, _stopToken).ConfigureAwait(false);
			await using (storage.ConfigureAwait(false))
			{
				SaveToStorage(interpreterModel.Root.As<IStoreSupport>(), new Bucket(storage));

				void SaveToStorage(IStoreSupport root, Bucket bucket)
				{
					var memoryStorage = new InMemoryStorage();
					root.Store(new Bucket(memoryStorage));

					Span<byte> span = stackalloc byte[memoryStorage.GetTransactionLogSize()];
					memoryStorage.WriteTransactionLogToSpan(span);

					bucket.Add(Key.Version, value: 1);
					bucket.Add(Key.SessionId, _sessionId);
					bucket.Add(Key.StateMachineDefinition, span);
				}

				await storage.CheckPoint(level: 0, _stopToken).ConfigureAwait(false);
			}
		}

		private static IDataModelHandlerFactory GetDataModelHandlerFactory(string? dataModelType, ImmutableArray<IDataModelHandlerFactory> factories, IErrorProcessor errorProcessor)
		{
			if (!factories.IsDefaultOrEmpty)
			{
				foreach (var factory in factories)
				{
					if (factory.CanHandle(dataModelType ?? NoneDataModelHandler.DataModelType))
					{
						return factory;
					}
				}
			}

			switch (dataModelType)
			{
				case null:
				case NoneDataModelHandler.DataModelType:
					return NoneDataModelHandler.Factory;

				case RuntimeDataModelHandler.DataModelType:
					return RuntimeDataModelHandler.Factory;

				default:
					errorProcessor.AddError<StateMachineInterpreter>(entity: null, Res.Format(Resources.Exception_Cant_find_DataModelHandlerFactory_for_DataModel_type, dataModelType));
					return NoneDataModelHandler.Factory;
			}
		}

		private ValueTask DoOperation(StateBagKey key, Func<ValueTask> func)
		{
			return IsPersistingEnabled ? DoOperationAsync() : func();

			async ValueTask DoOperationAsync()
			{
				var persistenceContext = _context.PersistenceContext;
				if (persistenceContext.GetState((int) key) == 0)
				{
					await func().ConfigureAwait(false);

					persistenceContext.SetState((int) key, value: 1);
				}
			}
		}

		private ValueTask DoOperation<TArg>(StateBagKey key, Func<TArg, ValueTask> func, TArg arg)
		{
			return IsPersistingEnabled ? DoOperationAsync() : func(arg);

			async ValueTask DoOperationAsync()
			{
				var persistenceContext = _context.PersistenceContext;
				if (persistenceContext.GetState((int) key) == 0)
				{
					await func(arg).ConfigureAwait(false);

					persistenceContext.SetState((int) key, value: 1);
				}
			}
		}

		private ValueTask DoOperation<TArg>(StateBagKey key, IEntity entity, Func<TArg, ValueTask> func, TArg arg)
		{
			return IsPersistingEnabled ? DoOperationAsync() : func(arg);

			async ValueTask DoOperationAsync()
			{
				var documentId = entity.As<IDocumentId>().DocumentId;

				var persistenceContext = _context.PersistenceContext;
				if (persistenceContext.GetState((int) key, documentId) == 0)
				{
					await func(arg).ConfigureAwait(false);

					persistenceContext.SetState((int) key, documentId, value: 1);
				}
			}
		}

		private void Complete(StateBagKey key)
		{
			if (IsPersistingEnabled)
			{
				_context.PersistenceContext.ClearState((int) key);
			}
		}

		private bool Capture(StateBagKey key, bool value)
		{
			if (IsPersistingEnabled)
			{
				var persistenceContext = _context.PersistenceContext;
				if (persistenceContext.GetState((int) key) == 1)
				{
					return persistenceContext.GetState((int) key, subKey: 0) == 1;
				}

				persistenceContext.SetState((int) key, subKey: 0, value ? 1 : 0);
				persistenceContext.SetState((int) key, value: 1);
			}

			return value;
		}

		private ValueTask<List<TransitionNode>> Capture(StateBagKey key, Func<ValueTask<List<TransitionNode>>> value)
		{
			return IsPersistingEnabled ? CaptureAsync() : value();

			async ValueTask<List<TransitionNode>> CaptureAsync()
			{
				var persistenceContext = _context.PersistenceContext;
				if (persistenceContext.GetState((int) key) == 0)
				{
					var list = await value().ConfigureAwait(false);
					persistenceContext.SetState((int) key, subKey: 0, list.Count);

					for (var i = 0; i < list.Count; i ++)
					{
						persistenceContext.SetState((int) key, i + 1, list[i].As<IDocumentId>().DocumentId);
					}

					persistenceContext.SetState((int) key, value: 1);

					return list;
				}

				var length = persistenceContext.GetState((int) key, subKey: 0);
				var capturedSet = new List<TransitionNode>(length);

				for (var i = 0; i < length; i ++)
				{
					var documentId = persistenceContext.GetState((int) key, i + 1);
					capturedSet.Add(_model.EntityMap[documentId].As<TransitionNode>());
				}

				return capturedSet;
			}
		}

		private ValueTask NotifyAccepted() => _notifyStateChanged?.OnChanged(StateMachineInterpreterState.Accepted) ?? default;
		private ValueTask NotifyStarted()  => _notifyStateChanged?.OnChanged(StateMachineInterpreterState.Started) ?? default;
		private ValueTask NotifyExited()   => _notifyStateChanged?.OnChanged(StateMachineInterpreterState.Exited) ?? default;
		private ValueTask NotifyWaiting()  => _notifyStateChanged?.OnChanged(StateMachineInterpreterState.Waiting) ?? default;

		private async ValueTask<DataModelValue> Run(IStateMachine? stateMachine)
		{
			_model = await BuildInterpreterModel(stateMachine).ConfigureAwait(false);
			_context = await CreateContext().ConfigureAwait(false);
			_anyTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_suspendToken, _destroyToken, _stopToken);
			await using var _ = _context.ConfigureAwait(false);
			try
			{
				await DoOperation(StateBagKey.NotifyAccepted, NotifyAccepted).ConfigureAwait(false);
				await DoOperation(StateBagKey.InitializeRootDataModel, InitializeRootDataModel).ConfigureAwait(false);
				await DoOperation(StateBagKey.EarlyInitializeDataModel, InitializeAllDataModels).ConfigureAwait(false);
				await DoOperation(StateBagKey.ExecuteGlobalScript, ExecuteGlobalScript).ConfigureAwait(false);
				await DoOperation(StateBagKey.NotifyStarted, NotifyStarted).ConfigureAwait(false);
				await DoOperation(StateBagKey.InitialEnterStates, InitialEnterStates).ConfigureAwait(false);
				await DoOperation(StateBagKey.MainEventLoop, MainEventLoop).ConfigureAwait(false);
				await DoOperation(StateBagKey.ExitInterpreter, ExitInterpreter).ConfigureAwait(false);
				await DoOperation(StateBagKey.NotifyExited, NotifyExited).ConfigureAwait(false);
			}
			catch (ChannelClosedException ex)
			{
				throw new StateMachineQueueClosedException(Resources.Exception_State_Machine_external_queue_has_been_closed, ex);
			}
			catch (StateMachineLiveLockException)
			{
				await CleanupPersistedData().ConfigureAwait(false);

				throw;
			}
			catch (OperationCanceledException ex) when (ex.CancellationToken == _stopToken || ex.CancellationToken == _anyTokenSource.Token && _stopToken.IsCancellationRequested)
			{
				throw new OperationCanceledException(Resources.Exception_State_Machine_has_been_halted, ex, _stopToken);
			}
			catch (OperationCanceledException ex) when (ex.CancellationToken == _destroyToken || ex.CancellationToken == _anyTokenSource.Token && _destroyToken.IsCancellationRequested)
			{
				await DoOperation(StateBagKey.ExitInterpreter, ExitInterpreter).ConfigureAwait(false);
				await DoOperation(StateBagKey.NotifyExited, NotifyExited).ConfigureAwait(false);
				await CleanupPersistedData().ConfigureAwait(false);

				throw new StateMachineDestroyedException(Resources.Exception_State_Machine_has_been_destroyed, ex, _destroyToken);
			}
			catch (OperationCanceledException ex) when (ex.CancellationToken == _suspendToken || ex.CancellationToken == _anyTokenSource.Token && _suspendToken.IsCancellationRequested)
			{
				throw new StateMachineSuspendedException(Resources.Exception_State_Machine_has_been_suspended, ex, _suspendToken);
			}

			await CleanupPersistedData().ConfigureAwait(false);

			return _doneData;
		}

		private async ValueTask CleanupPersistedData()
		{
			if (IsPersistingEnabled)
			{
				await _storageProvider.RemoveTransactionalStorage(partition: default, StateStorageKey, _stopToken).ConfigureAwait(false);
				await _storageProvider.RemoveTransactionalStorage(partition: default, StateMachineDefinitionStorageKey, _stopToken).ConfigureAwait(false);
			}
		}

		private async ValueTask InitializeAllDataModels()
		{
			if (_model.Root.Binding == BindingType.Early && !_model.DataModelList.IsDefaultOrEmpty)
			{
				foreach (var node in _model.DataModelList)
				{
					await DoOperation(StateBagKey.InitializeDataModel, node, InitializeDataModel, node).ConfigureAwait(false);
				}
			}
		}

		private ValueTask InitialEnterStates() => EnterStates(new List<TransitionNode>(1) { _model.Root.Initial.Transition });

		private async ValueTask MainEventLoop()
		{
			var exit = false;

			while (Capture(StateBagKey.Running, Running && !exit))
			{
				_anyTokenSource.Token.ThrowIfCancellationRequested();

				await DoOperation(StateBagKey.InternalQueueProcessing, InternalQueueProcessing).ConfigureAwait(false);

				exit = await ExternalQueueProcessing().ConfigureAwait(false);

				Complete(StateBagKey.InternalQueueProcessing);
				Complete(StateBagKey.Running);
			}

			Complete(StateBagKey.Running);
		}

		private async ValueTask InternalQueueProcessing()
		{
			var liveLockDetector = LiveLockDetector.Create();
			var exit = false;

			try
			{
				while (Capture(StateBagKey.Running, Running && !exit))
				{
					_anyTokenSource.Token.ThrowIfCancellationRequested();

					liveLockDetector.Iteration(_context.InternalQueue.Count);

					exit = await InternalQueueProcessingIteration().ConfigureAwait(false);

					Complete(StateBagKey.Running);
				}

				Complete(StateBagKey.Running);
			}
			finally
			{
				liveLockDetector.Dispose();
			}
		}

		private ValueTask<List<TransitionNode>> SelectInternalEventTransitions()
		{
			var internalEvent = _context.InternalQueue.Dequeue();

			_logger.ProcessingEvent(internalEvent);

			_context.DataModel.SetInternal(property: @"_event", new DataModelDescriptor(DataConverter.FromEvent(internalEvent), isReadOnly: true));

			return SelectTransitions(internalEvent);
		}

		private async ValueTask<bool> InternalQueueProcessMessage()
		{
			var exit = false;

			if (Capture(StateBagKey.InternalQueueNonEmpty, _context.InternalQueue.Count > 0))
			{
				var transitions = await Capture(StateBagKey.SelectInternalEventTransitions, SelectInternalEventTransitions).ConfigureAwait(false);

				if (transitions.Count > 0)
				{
					await Microstep(transitions).ConfigureAwait(false);

					await CheckPoint(PersistenceLevel.Transition).ConfigureAwait(false);
				}

				Complete(StateBagKey.SelectInternalEventTransitions);
			}
			else
			{
				exit = true;
			}

			Complete(StateBagKey.InternalQueueNonEmpty);

			return exit;
		}

		private async ValueTask<bool> InternalQueueProcessingIteration()
		{
			var exit = false;
			var transitions = await Capture(StateBagKey.EventlessTransitions, SelectEventlessTransitions).ConfigureAwait(false);

			if (transitions.Count > 0)
			{
				await Microstep(transitions).ConfigureAwait(false);

				await CheckPoint(PersistenceLevel.Transition).ConfigureAwait(false);
			}
			else
			{
				exit = await InternalQueueProcessMessage().ConfigureAwait(false);
			}

			Complete(StateBagKey.EventlessTransitions);

			return exit;
		}

		private async ValueTask<bool> ExternalQueueProcessing()
		{
			var exit = false;

			if (Capture(StateBagKey.Running2, Running))
			{
				foreach (var state in _context.StatesToInvoke.ToSortedList(StateEntityNode.EntryOrder))
				{
					foreach (var invoke in state.Invoke)
					{
						await DoOperation(StateBagKey.Invoke, invoke, Invoke, invoke).ConfigureAwait(false);
					}
				}

				_context.StatesToInvoke.Clear();
				Complete(StateBagKey.Invoke);

				if (Capture(StateBagKey.InternalQueueEmpty, _context.InternalQueue.Count == 0))
				{
					_anyTokenSource.Token.ThrowIfCancellationRequested();

					var transitions = await Capture(StateBagKey.ExternalEventTransitions, ExternalEventTransitions).ConfigureAwait(false);

					if (transitions.Count > 0)
					{
						await Microstep(transitions).ConfigureAwait(false);

						await CheckPoint(PersistenceLevel.Event).ConfigureAwait(false);
					}

					Complete(StateBagKey.ExternalEventTransitions);
				}

				Complete(StateBagKey.InternalQueueEmpty);
			}
			else
			{
				exit = true;
			}

			Complete(StateBagKey.Running2);

			return exit;
		}

		private async ValueTask<List<TransitionNode>> ExternalEventTransitions()
		{
			var externalEvent = await ReadExternalEvent().ConfigureAwait(false);

			_logger.ProcessingEvent(externalEvent);

			_context.DataModel.SetInternal(property: @"_event", new DataModelDescriptor(DataConverter.FromEvent(externalEvent), isReadOnly: true));

			foreach (var state in _context.Configuration)
			{
				foreach (var invoke in state.Invoke)
				{
					if (invoke.InvokeUniqueId == externalEvent.InvokeUniqueId)
					{
						await ApplyFinalize(invoke).ConfigureAwait(false);
					}

					if (invoke.AutoForward)
					{
						await ForwardEvent(invoke.InvokeId!, externalEvent).ConfigureAwait(false);
					}
				}
			}

			return await SelectTransitions(externalEvent).ConfigureAwait(false);
		}

		private async ValueTask CheckPoint(PersistenceLevel level)
		{
			if (!IsPersistingEnabled || _persistenceLevel < level)
			{
				return;
			}

			var persistenceContext = _context.PersistenceContext;
			await persistenceContext.CheckPoint((int) level, _stopToken).ConfigureAwait(false);

			if (level == PersistenceLevel.StableState)
			{
				await persistenceContext.Shrink(_stopToken).ConfigureAwait(false);
			}
		}

		private async ValueTask<IEvent> ReadExternalEvent()
		{
			while (true)
			{
				var evt = await ReadExternalEventUnfiltered().ConfigureAwait(false);

				if (evt.InvokeId == null)
				{
					return evt;
				}

				if (_externalCommunication.IsInvokeActive(evt.InvokeId, evt.InvokeUniqueId!))
				{
					return evt;
				}
			}
		}

		private async ValueTask<IEvent> ReadExternalEventUnfiltered()
		{
			var valueTask = _eventChannel.ReadAsync(_anyTokenSource.Token);

			if (valueTask.IsCompleted)
			{
				return await valueTask.ConfigureAwait(false);
			}

			await CheckPoint(PersistenceLevel.StableState).ConfigureAwait(false);

			await NotifyWaiting().ConfigureAwait(false);

			return await valueTask.ConfigureAwait(false);
		}

		private async ValueTask ExitInterpreter()
		{
			var statesToExit = _context.Configuration.ToSortedList(StateEntityNode.ExitOrder);

			foreach (var state in statesToExit)
			{
				foreach (var onExit in state.OnExit)
				{
					await DoOperation(StateBagKey.OnExit, onExit, RunExecutableEntity, onExit.ActionEvaluators).ConfigureAwait(false);
				}

				foreach (var invoke in state.Invoke)
				{
					await CancelInvoke(invoke).ConfigureAwait(false);
				}

				_context.Configuration.Delete(state);

				if (state is FinalNode final && final.Parent is StateMachineNode)
				{
					await DoOperation(StateBagKey.ReturnDoneEvent, state, EvaluateDoneData, final).ConfigureAwait(false);
				}
			}

			Complete(StateBagKey.ReturnDoneEvent);
			Complete(StateBagKey.OnExit);
		}

		private ValueTask<List<TransitionNode>> SelectEventlessTransitions() => SelectTransitions(evt: null);

		private async ValueTask<List<TransitionNode>> SelectTransitions(IEvent? evt)
		{
			var transitions = new List<TransitionNode>();

			foreach (var state in _context.Configuration.ToFilteredSortedList(s => s.IsAtomicState, StateEntityNode.EntryOrder))
			{
				await FindTransitionForState(state).ConfigureAwait(false);
			}

			return RemoveConflictingTransitions(transitions);

			async ValueTask FindTransitionForState(StateEntityNode state)
			{
				foreach (var transition in state.Transitions)
				{
					if (EventMatch(transition) && await ConditionMatch(transition).ConfigureAwait(false))
					{
						transitions.Add(transition);

						return;
					}
				}

				if (!(state.Parent is StateMachineNode))
				{
					await FindTransitionForState(state.Parent!).ConfigureAwait(false);
				}
			}

			bool EventMatch(TransitionNode transition)
			{
				var eventDescriptors = transition.EventDescriptors;

				if (evt == null)
				{
					return eventDescriptors == null;
				}

				return eventDescriptors != null && eventDescriptors.Any(d => d.IsEventMatch(evt));
			}

			async ValueTask<bool> ConditionMatch(TransitionNode transition)
			{
				var condition = transition.ConditionEvaluator;

				if (condition == null)
				{
					return true;
				}

				_stopToken.ThrowIfCancellationRequested();

				try
				{
					return await condition.EvaluateBoolean(_context.ExecutionContext, _stopToken).ConfigureAwait(false);
				}
				catch (Exception ex) when (IsError(ex))
				{
					await Error(transition, ex).ConfigureAwait(false);

					return false;
				}
			}
		}

		private List<TransitionNode> RemoveConflictingTransitions(List<TransitionNode> enabledTransitions)
		{
			var filteredTransitions = new List<TransitionNode>();
			List<TransitionNode>? transitionsToRemove = null;
			List<TransitionNode>? tr1 = null;
			List<TransitionNode>? tr2 = null;

			foreach (var t1 in enabledTransitions)
			{
				var t1Preempted = false;
				transitionsToRemove?.Clear();

				foreach (var t2 in filteredTransitions)
				{
					(tr1 ??= new List<TransitionNode>(1) { default! })[0] = t1;
					(tr2 ??= new List<TransitionNode>(1) { default! })[0] = t2;

					if (HasIntersection(ComputeExitSet(tr1), ComputeExitSet(tr2)))
					{
						if (IsDescendant(t1.Source, t2.Source))
						{
							(transitionsToRemove ??= new List<TransitionNode>()).Add(t2);
						}
						else
						{
							t1Preempted = true;
							break;
						}
					}
				}

				if (!t1Preempted)
				{
					if (transitionsToRemove != null)
					{
						foreach (var t3 in transitionsToRemove)
						{
							filteredTransitions.Remove(t3);
						}
					}

					filteredTransitions.Add(t1);
				}
			}

			return filteredTransitions;
		}

		private async ValueTask Microstep(List<TransitionNode> enabledTransitions)
		{
			await DoOperation(StateBagKey.ExitStates, ExitStates, enabledTransitions).ConfigureAwait(false);

			await DoOperation(StateBagKey.ExecuteTransitionContent, ExecuteTransitionContent, enabledTransitions).ConfigureAwait(false);

			await DoOperation(StateBagKey.EnterStates, EnterStates, enabledTransitions).ConfigureAwait(false);
		}

		private async ValueTask ExitStates(List<TransitionNode> enabledTransitions)
		{
			var statesToExit = ComputeExitSet(enabledTransitions);

			foreach (var state in statesToExit)
			{
				_context.StatesToInvoke.Delete(state);
			}

			var states = ToSortedList(statesToExit, StateEntityNode.ExitOrder);

			foreach (var state in states)
			{
				foreach (var history in state.HistoryStates)
				{
					var predicate = history.Type == HistoryType.Deep ? (Predicate<StateEntityNode>) Deep : Shallow;

					bool Deep(StateEntityNode node)    => node.IsAtomicState && IsDescendant(node, state);
					bool Shallow(StateEntityNode node) => node.Parent == state;

					_context.HistoryValue.Set(history.Id, _context.Configuration.ToFilteredList(predicate));
				}
			}

			foreach (var state in states)
			{
				_logger.ExitingState(state);

				foreach (var onExit in state.OnExit)
				{
					await DoOperation(StateBagKey.OnExit, onExit, RunExecutableEntity, onExit.ActionEvaluators).ConfigureAwait(false);
				}

				foreach (var invoke in state.Invoke)
				{
					await CancelInvoke(invoke).ConfigureAwait(false);
				}

				_context.Configuration.Delete(state);
			}

			Complete(StateBagKey.OnExit);
		}

		private static void AddIfNotExists<T>(List<T> list, T item)
		{
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}

		private static List<T> ToSortedList<T>(List<T> list, IComparer<T> comparer)
		{
			var result = new List<T>(list);
			result.Sort(comparer);

			return result;
		}

		private static bool HasIntersection<T>(List<T> list1, List<T> list2)
		{
			foreach (var item in list1)
			{
				if (list2.Contains(item))
				{
					return true;
				}
			}

			return false;
		}

		private async ValueTask EnterStates(List<TransitionNode> enabledTransitions)
		{
			var statesToEnter = new List<StateEntityNode>();
			var statesForDefaultEntry = new List<CompoundNode>();
			var defaultHistoryContent = new DefaultHistoryContent();

			ComputeEntrySet(enabledTransitions, statesToEnter, statesForDefaultEntry, defaultHistoryContent);

			foreach (var state in ToSortedList(statesToEnter, StateEntityNode.EntryOrder))
			{
				_context.Configuration.AddIfNotExists(state);
				_context.StatesToInvoke.AddIfNotExists(state);

				if (_model.Root.Binding == BindingType.Late && state.DataModel != null)
				{
					await DoOperation(StateBagKey.InitializeDataModel, state.DataModel, InitializeDataModel, state.DataModel).ConfigureAwait(false);
				}

				_logger.EnteringState(state);

				foreach (var onEntry in state.OnEntry)
				{
					await DoOperation(StateBagKey.OnEntry, onEntry, RunExecutableEntity, onEntry.ActionEvaluators).ConfigureAwait(false);
				}

				if (state is CompoundNode compound && statesForDefaultEntry.Contains(compound))
				{
					await DoOperation(StateBagKey.DefaultEntry, state, RunExecutableEntity, compound.Initial.Transition.ActionEvaluators).ConfigureAwait(false);
				}

				if (defaultHistoryContent.TryGetValue(state.Id, out var action))
				{
					await DoOperation(StateBagKey.DefaultHistoryContent, state, RunExecutableEntity, action).ConfigureAwait(false);
				}

				if (state is FinalNode final)
				{
					if (final.Parent is StateMachineNode)
					{
						Running = false;
					}
					else
					{
						var parent = final.Parent;
						var grandparent = parent!.Parent;

						DataModelValue doneData = default;
						if (final.DoneData != null)
						{
							doneData = await EvaluateDoneData(final.DoneData).ConfigureAwait(false);
						}

						_context.InternalQueue.Enqueue(new EventObject(EventType.Internal, EventName.GetDoneStateNameParts(parent.Id), doneData));

						if (grandparent is ParallelNode)
						{
							if (grandparent.States.All(IsInFinalState))
							{
								_context.InternalQueue.Enqueue(new EventObject(EventType.Internal, EventName.GetDoneStateNameParts(grandparent.Id)));
							}
						}
					}
				}
			}

			Complete(StateBagKey.OnEntry);
			Complete(StateBagKey.DefaultEntry);
			Complete(StateBagKey.DefaultHistoryContent);
		}

		private async ValueTask<DataModelValue> EvaluateDoneData(DoneDataNode doneData)
		{
			try
			{
				return await doneData.Evaluate(_context.ExecutionContext, _stopToken).ConfigureAwait(false);
			}
			catch (Exception ex) when (IsError(ex))
			{
				await Error(doneData, ex).ConfigureAwait(false);
			}

			return default;
		}

		private bool IsInFinalState(StateEntityNode state)
		{
			if (state is CompoundNode)
			{
				return state.States.Any(s => s is FinalNode && _context.Configuration.IsMember(s));
			}

			if (state is ParallelNode)
			{
				return state.States.All(IsInFinalState);
			}

			return false;
		}

		private void ComputeEntrySet(List<TransitionNode> transitions, List<StateEntityNode> statesToEnter, List<CompoundNode> statesForDefaultEntry,
									 DefaultHistoryContent defaultHistoryContent)
		{
			foreach (var transition in transitions)
			{
				foreach (var state in transition.TargetState)
				{
					AddDescendantStatesToEnter(state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
				}

				var ancestor = GetTransitionDomain(transition);

				foreach (var state in GetEffectiveTargetStates(transition))
				{
					AddAncestorStatesToEnter(state, ancestor, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
				}
			}
		}

		private List<StateEntityNode> ComputeExitSet(List<TransitionNode> transitions)
		{
			var statesToExit = new List<StateEntityNode>();
			foreach (var transition in transitions)
			{
				if (transition.Target != null)
				{
					var domain = GetTransitionDomain(transition);
					foreach (var state in _context.Configuration)
					{
						if (IsDescendant(state, domain))
						{
							AddIfNotExists(statesToExit, state);
						}
					}
				}
			}

			return statesToExit;
		}

		private void AddDescendantStatesToEnter(StateEntityNode state, List<StateEntityNode> statesToEnter, List<CompoundNode> statesForDefaultEntry,
												DefaultHistoryContent defaultHistoryContent)
		{
			if (state is HistoryNode history)
			{
				if (_context.HistoryValue.TryGetValue(history.Id, out var states))
				{
					foreach (var s in states)
					{
						AddDescendantStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
					}

					foreach (var s in states)
					{
						AddAncestorStatesToEnter(s, state.Parent, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
					}
				}
				else
				{
					defaultHistoryContent[state.Parent!.Id] = history.Transition.ActionEvaluators;

					foreach (var s in history.Transition.TargetState)
					{
						AddDescendantStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
					}

					foreach (var s in history.Transition.TargetState)
					{
						AddAncestorStatesToEnter(s, state.Parent, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
					}
				}
			}
			else
			{
				AddIfNotExists(statesToEnter, state);
				if (state is CompoundNode compound)
				{
					AddIfNotExists(statesForDefaultEntry, compound);

					foreach (var s in compound.Initial.Transition.TargetState)
					{
						AddDescendantStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
					}

					foreach (var s in compound.Initial.Transition.TargetState)
					{
						AddAncestorStatesToEnter(s, state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
					}
				}
				else
				{
					if (state is ParallelNode)
					{
						foreach (var child in state.States)
						{
							if (!statesToEnter.Exists(s => IsDescendant(s, child)))
							{
								AddDescendantStatesToEnter(child, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
							}
						}
					}
				}
			}
		}

		private void AddAncestorStatesToEnter(StateEntityNode state, StateEntityNode? ancestor, List<StateEntityNode> statesToEnter, List<CompoundNode> statesForDefaultEntry,
											  DefaultHistoryContent defaultHistoryContent)
		{
			var ancestors = GetProperAncestors(state, ancestor);

			if (ancestors == null)
			{
				return;
			}

			foreach (var anc in ancestors)
			{
				AddIfNotExists(statesToEnter, anc);

				if (anc is ParallelNode)
				{
					foreach (var child in anc.States)
					{
						if (!statesToEnter.Exists(s => IsDescendant(s, child)))
						{
							AddDescendantStatesToEnter(child, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
						}
					}
				}
			}
		}

		private static bool IsDescendant(StateEntityNode state1, StateEntityNode? state2)
		{
			for (var s = state1.Parent; s != null; s = s.Parent)
			{
				if (s == state2)
				{
					return true;
				}
			}

			return false;
		}

		private StateEntityNode? GetTransitionDomain(TransitionNode transition)
		{
			var tstates = GetEffectiveTargetStates(transition);

			if (tstates.Count == 0)
			{
				return null;
			}

			if (transition.Type == TransitionType.Internal && transition.Source is CompoundNode && tstates.TrueForAll(s => IsDescendant(s, transition.Source)))
			{
				return transition.Source;
			}

			return FindLcca(transition.Source, tstates);
		}

		private static StateEntityNode? FindLcca(StateEntityNode headState, List<StateEntityNode> tailStates)
		{
			var ancestors = GetProperAncestors(headState, state2: null);

			if (ancestors == null)
			{
				return null;
			}

			foreach (var anc in ancestors)
			{
				if (tailStates.TrueForAll(s => IsDescendant(s, anc)))
				{
					return anc;
				}
			}

			return null;
		}

		private static List<StateEntityNode>? GetProperAncestors(StateEntityNode state1, StateEntityNode? state2)
		{
			List<StateEntityNode>? states = null;

			for (var s = state1.Parent; s != null; s = s.Parent)
			{
				if (s == state2)
				{
					return states;
				}

				(states ??= new List<StateEntityNode>()).Add(s);
			}

			return state2 == null ? states : null;
		}

		private List<StateEntityNode> GetEffectiveTargetStates(TransitionNode transition)
		{
			var targets = new List<StateEntityNode>();

			foreach (var state in transition.TargetState)
			{
				if (state is HistoryNode history)
				{
					if (!_context.HistoryValue.TryGetValue(history.Id, out var values))
					{
						values = GetEffectiveTargetStates(history.Transition);
					}

					foreach (var s in values)
					{
						AddIfNotExists(targets, s);
					}
				}
				else
				{
					AddIfNotExists(targets, state);
				}
			}

			return targets;
		}

		private async ValueTask ExecuteTransitionContent(List<TransitionNode> transitions)
		{
			foreach (var transition in transitions)
			{
				_logger.PerformingTransition(transition);

				await DoOperation(StateBagKey.RunExecutableEntity, transition, RunExecutableEntity, transition.ActionEvaluators).ConfigureAwait(false);
			}

			Complete(StateBagKey.RunExecutableEntity);
		}

		private async ValueTask RunExecutableEntity(ImmutableArray<IExecEvaluator> action)
		{
			foreach (var executableEntity in action)
			{
				_stopToken.ThrowIfCancellationRequested();

				try
				{
					await executableEntity.Execute(_context.ExecutionContext, _stopToken).ConfigureAwait(false);
				}
				catch (Exception ex) when (IsError(ex))
				{
					await Error(executableEntity, ex).ConfigureAwait(false);

					break;
				}
			}

			await CheckPoint(PersistenceLevel.ExecutableAction).ConfigureAwait(false);
		}

		private bool IsOperationCancelled(Exception exception)
		{
			return exception switch
			{
					OperationCanceledException ex => ex.CancellationToken == _stopToken,
					_ => false
			};
		}

		private bool IsError(Exception ex) => !IsOperationCancelled(ex);

		private async ValueTask Error(object source, Exception exception, bool logLoggerErrors = true)
		{
			var sourceEntityId = (source as IEntity).Is(out IDebugEntityId? id) ? id.EntityId?.ToString(CultureInfo.InvariantCulture) : null;

			string? sendId = null;

			var errorType = _logger.IsPlatformError(exception)
					? ErrorType.Platform
					: _externalCommunication.IsCommunicationError(exception, out sendId)
							? ErrorType.Communication
							: ErrorType.Execution;

			var nameParts = errorType switch
			{
					ErrorType.Execution => EventName.ErrorExecution,
					ErrorType.Communication => EventName.ErrorCommunication,
					ErrorType.Platform => EventName.ErrorPlatform,
					_ => Infrastructure.UnexpectedValue<ImmutableArray<IIdentifier>>()
			};

			var eventObject = new EventObject(EventType.Platform, nameParts, DataConverter.FromException(exception), sendId);

			_context.InternalQueue.Enqueue(eventObject);

			try
			{
				await _logger.Error(errorType, _model.Root.Name, sourceEntityId, exception, _stopToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				if (logLoggerErrors)
				{
					try
					{
						await Error(source, ex, logLoggerErrors: false).ConfigureAwait(false);
					}
					catch
					{
						// ignored
					}
				}
			}
		}

		private async ValueTask ExecuteGlobalScript()
		{
			if (_model.Root.ScriptEvaluator != null)
			{
				try
				{
					await _model.Root.ScriptEvaluator.Execute(_context.ExecutionContext, _stopToken).ConfigureAwait(false);
				}
				catch (Exception ex) when (IsError(ex))
				{
					await Error(_model.Root.ScriptEvaluator, ex).ConfigureAwait(false);
				}
			}
		}

		private async ValueTask EvaluateDoneData(FinalNode final)
		{
			if (final.DoneData != null)
			{
				_doneData = await EvaluateDoneData(final.DoneData).ConfigureAwait(false);
			}
		}

		private ValueTask ForwardEvent(string invokeId, IEvent evt) => _externalCommunication.ForwardEvent(evt, invokeId, _stopToken);

		private ValueTask ApplyFinalize(InvokeNode invoke) => invoke.Finalize != null ? RunExecutableEntity(invoke.Finalize.ActionEvaluators) : default;

		private async ValueTask Invoke(InvokeNode invoke)
		{
			try
			{
				await invoke.Start(_context.ExecutionContext, _stopToken).ConfigureAwait(false);
			}
			catch (Exception ex) when (IsError(ex))
			{
				await Error(invoke, ex).ConfigureAwait(false);
			}
		}

		private async ValueTask CancelInvoke(InvokeNode invoke)
		{
			try
			{
				await invoke.Cancel(_context.ExecutionContext, _stopToken).ConfigureAwait(false);
			}
			catch (Exception ex) when (IsError(ex))
			{
				await Error(invoke, ex).ConfigureAwait(false);
			}
		}

		private async ValueTask InitializeRootDataModel()
		{
			var rootDataModel = _model.Root.DataModel;

			if (rootDataModel == null)
			{
				return;
			}

			if (_arguments.Type != DataModelValueType.Object)
			{
				await InitializeDataModel(rootDataModel).ConfigureAwait(false);

				return;
			}

			var dictionary = _arguments.AsObject();
			foreach (var node in rootDataModel.Data)
			{
				await InitializeData(node, dictionary[node.Id]).ConfigureAwait(false);
			}
		}

		private async ValueTask InitializeDataModel(DataModelNode dataModel)
		{
			foreach (var node in dataModel.Data)
			{
				await InitializeData(node).ConfigureAwait(false);
			}
		}

		private async ValueTask InitializeData(DataNode data, DataModelValue overrideValue = default)
		{
			try
			{
				_context.DataModel[data.Id] = await GetValue().ConfigureAwait(false);
			}
			catch (Exception ex) when (IsError(ex))
			{
				await Error(data, ex).ConfigureAwait(false);
			}

			async ValueTask<DataModelValue> GetValue()
			{
				if (!overrideValue.IsUndefined())
				{
					return overrideValue;
				}

				if (data.Source != null)
				{
					var resource = await Load().ConfigureAwait(false);

					return DataConverter.FromContent(resource.Content, resource.ContentType);
				}

				if (data.ExpressionEvaluator != null)
				{
					var obj = await data.ExpressionEvaluator.EvaluateObject(_context.ExecutionContext, _stopToken).ConfigureAwait(false);

					return DataModelValue.FromObject(obj.ToObject());
				}

				if (data.InlineContent != null)
				{
					return data.InlineContent;
				}

				return DataModelValue.Undefined;
			}

			async ValueTask<Resource> Load()
			{
				if (!_resourceLoaders.IsDefaultOrEmpty)
				{
					var uri = data.Source.Uri!;

					foreach (var resourceLoader in _resourceLoaders)
					{
						if (resourceLoader.CanHandle(uri))
						{
							return await resourceLoader.Request(uri, _stopToken).ConfigureAwait(false);
						}
					}
				}

				throw new StateMachineProcessorException(Resources.Exception_Cannot_find_ResourceLoader_to_load_external_resource);
			}
		}

		private async ValueTask<IStateMachineContext> CreateContext()
		{
			IStateMachineContext context;
			if (IsPersistingEnabled)
			{
				var storage = await _storageProvider.GetTransactionalStorage(partition: default, StateStorageKey, _stopToken).ConfigureAwait(false);
				context = new StateMachinePersistedContext(_model.Root.Name, _sessionId, _arguments, storage, _model.EntityMap, _logger, _externalCommunication);
			}
			else
			{
				context = new StateMachineContext(_model.Root.Name, _sessionId, _arguments, _logger, _externalCommunication);
			}

			PopulateInterpreterObject(context.InterpreterObject);

			if (_configuration != null)
			{
				PopulateConfigurationObject(_configuration, context.ConfigurationObject);
			}

			var dataModelVars = new Dictionary<string, string>();
			_dataModelHandler.ExecutionContextCreated(context.ExecutionContext, dataModelVars);

			PopulateDataModelHandlerObject(context.DataModelHandlerObject, dataModelVars);

			return context;
		}

		private void PopulateInterpreterObject(DataModelObject interpreterObject)
		{
			var type = GetType();
			var version = type.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

			interpreterObject.SetInternal(property: @"name", new DataModelDescriptor(new DataModelValue(type.FullName)));
			interpreterObject.SetInternal(property: @"version", new DataModelDescriptor(new DataModelValue(version)));
		}

		private static void PopulateConfigurationObject(IReadOnlyDictionary<string, string> configuration, DataModelObject configurationObject)
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

			foreach (var pair in configuration)
			{
				configurationObject.SetInternal(pair.Key, new DataModelDescriptor(new DataModelValue(pair.Value)));
			}
		}

		private void PopulateDataModelHandlerObject(DataModelObject dataModelHandlerObject, Dictionary<string, string> dataModelVars)
		{
			var type = _dataModelHandler.GetType();
			var version = type.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

			dataModelHandlerObject.SetInternal(property: @"name", new DataModelDescriptor(new DataModelValue(type.FullName)));
			dataModelHandlerObject.SetInternal(property: @"assembly", new DataModelDescriptor(new DataModelValue(type.Assembly.GetName().Name)));
			dataModelHandlerObject.SetInternal(property: @"version", new DataModelDescriptor(new DataModelValue(version)));

			var vars = new DataModelObject(true);
			foreach (var pair in dataModelVars)
			{
				vars.SetInternal(pair.Key, new DataModelDescriptor(new DataModelValue(pair.Value)));
			}

			dataModelHandlerObject.SetInternal(property: @"vars", new DataModelDescriptor(new DataModelValue(vars)));
		}

		private enum StateBagKey
		{
			Stop,
			Running,
			Running2,
			EventlessTransitions,
			InternalQueueNonEmpty,
			SelectInternalEventTransitions,
			InternalQueueEmpty,
			InitializeRootDataModel,
			EarlyInitializeDataModel,
			ExecuteGlobalScript,
			ExternalEventTransitions,
			InitialEnterStates,
			ExitInterpreter,
			InternalQueueProcessing,
			MainEventLoop,
			ExitStates,
			ExecuteTransitionContent,
			EnterStates,
			RunExecutableEntity,
			InitializeDataModel,
			OnExit,
			OnEntry,
			DefaultEntry,
			DefaultHistoryContent,
			Invoke,
			ReturnDoneEvent,
			NotifyAccepted,
			NotifyStarted,
			NotifyExited
		}

		private struct LiveLockDetector : IDisposable
		{
			private const int IterationCount = 36;

			private int[]? _data;
			private int    _index;
			private int    _internalQueueLength;
			private int    _sum;

		#region Interface IDisposable

			public void Dispose()
			{
				if (_data != null)
				{
					ArrayPool<int>.Shared.Return(_data);

					_data = null;
				}
			}

		#endregion

			public static LiveLockDetector Create() => new LiveLockDetector { _index = -1 };

			public void Iteration(int internalQueueCount)
			{
				if (_index == -1)
				{
					_internalQueueLength = internalQueueCount;
					_index = _sum = 0;

					return;
				}

				_data ??= ArrayPool<int>.Shared.Rent(IterationCount);

				if (_index >= IterationCount)
				{
					if (_sum >= 0)
					{
						throw new StateMachineLiveLockException();
					}

					_sum -= _data[_index % IterationCount];
				}

				var delta = internalQueueCount - _internalQueueLength;
				_internalQueueLength = internalQueueCount;
				_sum += delta;
				_data[_index ++ % IterationCount] = delta;
			}
		}
	}
}