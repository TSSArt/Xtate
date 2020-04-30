﻿using System;
using System.Collections.Immutable;
using System.ComponentModel;

namespace TSSArt.StateMachine
{
	public class StateMachineBuilder : BuilderBase, IStateMachineBuilder
	{
		private BindingType                           _bindingType;
		private IDataModel?                           _dataModel;
		private string?                               _dataModelType;
		private ImmutableArray<IIdentifier>           _initialId;
		private bool                                  _injectOptions;
		private string?                               _name;
		private StateMachineOptions                   _options;
		private IScript?                              _script;
		private ImmutableArray<IStateEntity>.Builder? _states;

		public StateMachineBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IStateMachineBuilder

		public IStateMachine Build()
		{
			var initial = _initialId != null ? (IInitial) new InitialEntity { Transition = new TransitionEntity { Target = _initialId } } : null;

			var ancestor = _injectOptions ? new AncestorContainer(_options, Ancestor) : Ancestor;

			return new StateMachineEntity
				   {
						   Ancestor = ancestor, Name = _name, Initial = initial, DataModelType = _dataModelType,
						   Binding = _bindingType, States = _states?.ToImmutable() ?? default, DataModel = _dataModel, Script = _script
				   };
		}

		public void SetInitial(ImmutableArray<IIdentifier> initialId)
		{
			if (initialId.IsDefaultOrEmpty) throw new ArgumentException(Resources.Exception_ValueCannotBeEmptyList, nameof(initialId));

			_initialId = initialId;
		}

		public void SetName(string name)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));

			_name = name;
			_options.Name = name;
			_injectOptions = true;
		}

		public void SetBindingType(BindingType bindingType) => _bindingType = bindingType;

		public void AddState(IState state)
		{
			if (state == null) throw new ArgumentNullException(nameof(state));

			(_states ??= ImmutableArray.CreateBuilder<IStateEntity>()).Add(state);
		}

		public void AddParallel(IParallel parallel)
		{
			if (parallel == null) throw new ArgumentNullException(nameof(parallel));

			(_states ??= ImmutableArray.CreateBuilder<IStateEntity>()).Add(parallel);
		}

		public void AddFinal(IFinal final)
		{
			if (final == null) throw new ArgumentNullException(nameof(final));

			(_states ??= ImmutableArray.CreateBuilder<IStateEntity>()).Add(final);
		}

		public void SetDataModel(IDataModel dataModel) => _dataModel = dataModel ?? throw new ArgumentNullException(nameof(dataModel));

		public void SetScript(IScript script) => _script = script ?? throw new ArgumentNullException(nameof(script));

		public void SetDataModelType(string dataModelType) => _dataModelType = dataModelType ?? throw new ArgumentNullException(nameof(dataModelType));

		public void SetPersistenceLevel(PersistenceLevel persistenceLevel)
		{
			if (!Enum.IsDefined(typeof(PersistenceLevel), persistenceLevel)) throw new InvalidEnumArgumentException(nameof(persistenceLevel), (int) persistenceLevel, typeof(PersistenceLevel));

			_options.PersistenceLevel = persistenceLevel;
			_injectOptions = true;
		}

		public void SetSynchronousEventProcessing(bool value)
		{
			_options.SynchronousEventProcessing = value;
			_injectOptions = true;
		}

		public void SetExternalQueueSize(int size)
		{
			if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));

			_options.ExternalQueueSize = size;
			_injectOptions = true;
		}

	#endregion
	}
}