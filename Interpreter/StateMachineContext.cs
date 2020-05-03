﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace TSSArt.StateMachine
{
	internal class StateMachineContext : IStateMachineContext, IExecutionContext
	{
		private static readonly Uri InternalTarget = new Uri(uriString: "_internal", UriKind.Relative);

		private readonly ExternalCommunicationWrapper _externalCommunication;
		private readonly LoggerWrapper                _logger;
		private readonly string?                      _stateMachineName;

		public StateMachineContext(string? stateMachineName, string sessionId, DataModelValue arguments, LoggerWrapper logger,
								   ExternalCommunicationWrapper externalCommunication, ImmutableDictionary<object, object> contextRuntimeItems)
		{
			_stateMachineName = stateMachineName;
			_logger = logger;
			_externalCommunication = externalCommunication;

			DataModel = CreateDataModel(stateMachineName, sessionId, arguments);
			RuntimeItems = new ContextItems(contextRuntimeItems);
		}

	#region Interface IAsyncDisposable

		public virtual ValueTask DisposeAsync() => default;

	#endregion

	#region Interface IExecutionContext

		public bool InState(IIdentifier id)
		{
			foreach (var state in Configuration)
			{
				if (IdentifierEqualityComparer.Instance.Equals(id, state.Id))
				{
					return true;
				}
			}

			return false;
		}

		public async ValueTask Send(IOutgoingEvent evt, CancellationToken token)
		{
			if (IsInternalEvent(evt) || await _externalCommunication.TrySendEvent(evt, token).ConfigureAwait(false) == SendStatus.ToInternalQueue)
			{
				InternalQueue.Enqueue(new EventObject(EventType.Internal, evt));
			}
		}

		public ValueTask Cancel(string sendId, CancellationToken token) => _externalCommunication.CancelEvent(sendId, token);

		public ValueTask Log(string? label, DataModelValue arguments, CancellationToken token) => _logger.Log(_stateMachineName, label, arguments, token);

		public ValueTask StartInvoke(InvokeData invokeData, CancellationToken token = default) => _externalCommunication.StartInvoke(invokeData, token);

		public ValueTask CancelInvoke(string invokeId, CancellationToken token) => _externalCommunication.CancelInvoke(invokeId, token);

		public IContextItems RuntimeItems { get; }

	#endregion

	#region Interface IStateMachineContext

		public DataModelObject DataModel { get; }

		public OrderedSet<StateEntityNode> Configuration { get; } = new OrderedSet<StateEntityNode>();

		public DataModelObject DataModelHandlerObject { get; } = new DataModelObject(isReadOnly: true, capacity: 0);

		public IExecutionContext ExecutionContext => this;

		public KeyList<StateEntityNode> HistoryValue { get; } = new KeyList<StateEntityNode>();

		public EntityQueue<IEvent> InternalQueue { get; } = new EntityQueue<IEvent>();

		public DataModelObject InterpreterObject { get; } = new DataModelObject(isReadOnly: true, capacity: 0);

		public DataModelObject ConfigurationObject { get; } = new DataModelObject(isReadOnly: true, capacity: 0);

		public DataModelObject HostObject { get; } = new DataModelObject(isReadOnly: true, capacity: 0);

		public OrderedSet<StateEntityNode> StatesToInvoke { get; } = new OrderedSet<StateEntityNode>();

		public virtual IPersistenceContext PersistenceContext => throw new NotSupportedException();

	#endregion

		private static bool IsInternalEvent(IOutgoingEvent evt)
		{
			if (evt.Target != InternalTarget || evt.Type != null)
			{
				return false;
			}

			if (evt.DelayMs != 0)
			{
				throw new StateMachineExecutionException(Resources.Exception_Internal_events_can_t_be_delayed);
			}

			return true;
		}

		private DataModelObject CreateDataModel(string? stateMachineName, string sessionId, DataModelValue arguments)
		{
			var platform = new DataModelObject(capacity: 5)
						   {
								   [@"interpreter"] = new DataModelValue(InterpreterObject),
								   [@"datamodel"] = new DataModelValue(DataModelHandlerObject),
								   [@"configuration"] = new DataModelValue(ConfigurationObject),
								   [@"host"] = new DataModelValue(HostObject),
								   [@"args"] = arguments
						   };

			platform.MakeReadOnly();

			var dataModel = new DataModelObject(capacity: 5);

			dataModel.SetInternal(property: @"_name", new DataModelDescriptor(new DataModelValue(stateMachineName), isReadOnly: true));
			dataModel.SetInternal(property: @"_sessionid", new DataModelDescriptor(new DataModelValue(sessionId), isReadOnly: true));
			dataModel.SetInternal(property: @"_event", new DataModelDescriptor(value: default, isReadOnly: true));
			dataModel.SetInternal(property: @"_ioprocessors", new DataModelDescriptor(new DataModelValue(GetIoProcessors()), isReadOnly: true));
			dataModel.SetInternal(property: @"_x", new DataModelDescriptor(new DataModelValue(platform), isReadOnly: true));

			return dataModel;

			DataModelObject GetIoProcessors()
			{
				var ioProcessors = _externalCommunication.GetIoProcessors();

				if (ioProcessors.IsDefaultOrEmpty)
				{
					return DataModelObject.Empty;
				}

				var dictionary = new DataModelObject(capacity: ioProcessors.Length);

				foreach (var ioProcessor in ioProcessors)
				{
					dictionary[ioProcessor.Id.ToString()] = new DataModelValue(new DataModelObject(capacity: 1) { [@"location"] = new DataModelValue(ioProcessor.GetTarget(sessionId).ToString()) });
				}

				dictionary.MakeDeepConstant();

				return dictionary;
			}
		}

		private class ContextItems : IContextItems
		{
			private readonly Dictionary<object, object>          _items = new Dictionary<object, object>();
			private readonly ImmutableDictionary<object, object> _permanentItems;

			public ContextItems(ImmutableDictionary<object, object> permanentItems) => _permanentItems = permanentItems;

		#region Interface IContextItems

			public object? this[object key]
			{
				get => _permanentItems.TryGetValue(key, out var value) ? value : _items.TryGetValue(key, out value) ? value : null;
				set
				{
					if (value != null && !_permanentItems.ContainsKey(key))
					{
						_items[key] = value;
					}
				}
			}

		#endregion
		}
	}
}