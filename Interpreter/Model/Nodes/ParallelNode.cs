﻿using System;
using System.Collections.Immutable;

namespace Xtate
{
	internal sealed class ParallelNode : StateEntityNode, IParallel, IAncestorProvider, IDebugEntityId
	{
		private readonly ParallelEntity _parallel;

		public ParallelNode(in DocumentIdRecord documentIdNode, in ParallelEntity parallel) : base(documentIdNode, GetChildNodes(initial: null, parallel.States, parallel.HistoryStates))
		{
			_parallel = parallel;

			var id = parallel.Id ?? new IdentifierNode(Identifier.New());
			var transitions = parallel.Transitions.AsArrayOf<ITransition, TransitionNode>(true);
			var invokeList = parallel.Invoke.AsArrayOf<IInvoke, InvokeNode>(true);

			Id = id;
			States = parallel.States.AsArrayOf<IStateEntity, StateEntityNode>();
			HistoryStates = parallel.HistoryStates.AsArrayOf<IHistory, HistoryNode>(true);
			Transitions = transitions;
			OnEntry = parallel.OnEntry.AsArrayOf<IOnEntry, OnEntryNode>(true);
			OnExit = parallel.OnExit.AsArrayOf<IOnExit, OnExitNode>(true);
			Invoke = invokeList;
			DataModel = parallel.DataModel?.As<DataModelNode>();

			foreach (var transition in transitions)
			{
				transition.SetSource(this);
			}

			foreach (var invoke in invokeList)
			{
				invoke.SetStateId(id);
			}
		}

		public override bool                            IsAtomicState => false;
		public override ImmutableArray<InvokeNode>      Invoke        { get; }
		public override ImmutableArray<TransitionNode>  Transitions   { get; }
		public override ImmutableArray<HistoryNode>     HistoryStates { get; }
		public override ImmutableArray<StateEntityNode> States        { get; }
		public override ImmutableArray<OnEntryNode>     OnEntry       { get; }
		public override ImmutableArray<OnExitNode>      OnExit        { get; }
		public override DataModelNode?                  DataModel     { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _parallel.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}(#{DocumentId})";

	#endregion

	#region Interface IParallel

		public override IIdentifier Id { get; }

		IDataModel? IParallel.                 DataModel     => DataModel;
		ImmutableArray<IInvoke> IParallel.     Invoke        => ImmutableArray<IInvoke>.CastUp(Invoke);
		ImmutableArray<IStateEntity> IParallel.States        => ImmutableArray<IStateEntity>.CastUp(States);
		ImmutableArray<IHistory> IParallel.    HistoryStates => ImmutableArray<IHistory>.CastUp(HistoryStates);
		ImmutableArray<ITransition> IParallel. Transitions   => ImmutableArray<ITransition>.CastUp(Transitions);
		ImmutableArray<IOnEntry> IParallel.    OnEntry       => ImmutableArray<IOnEntry>.CastUp(OnEntry);
		ImmutableArray<IOnExit> IParallel.     OnExit        => ImmutableArray<IOnExit>.CastUp(OnExit);

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ParallelNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntity(Key.Id, Id);
			bucket.AddEntity(Key.DataModel, DataModel);
			bucket.AddEntityList(Key.States, States);
			bucket.AddEntityList(Key.HistoryStates, HistoryStates);
			bucket.AddEntityList(Key.Transitions, Transitions);
			bucket.AddEntityList(Key.OnEntry, OnEntry);
			bucket.AddEntityList(Key.OnExit, OnExit);
			bucket.AddEntityList(Key.Invoke, Invoke);
		}
	}
}