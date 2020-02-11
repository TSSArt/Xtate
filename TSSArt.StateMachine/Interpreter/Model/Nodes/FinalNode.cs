﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TSSArt.StateMachine
{
	internal sealed class FinalNode : StateEntityNode, IFinal, IAncestorProvider, IDebugEntityId
	{
		private readonly Final _final;

		public FinalNode(LinkedListNode<int> documentIdNode, in Final final) : base(documentIdNode, children: null)
		{
			_final = final;

			Id = final.Id ?? new IdentifierNode(new RuntimeIdentifier());
			OnEntry = final.OnEntry.AsArrayOf<IOnEntry, OnEntryNode>(true);
			OnExit = final.OnExit.AsArrayOf<IOnExit, OnExitNode>(true);
			DoneData = final.DoneData.As<DoneDataNode>();
		}

		public override bool                           IsAtomicState => true;
		public override ImmutableArray<TransitionNode> Transitions   => ImmutableArray<TransitionNode>.Empty;
		public override ImmutableArray<HistoryNode>    HistoryStates => ImmutableArray<HistoryNode>.Empty;
		public override ImmutableArray<InvokeNode>     Invoke        => ImmutableArray<InvokeNode>.Empty;
		public override ImmutableArray<OnEntryNode>    OnEntry       { get; }
		public override ImmutableArray<OnExitNode>     OnExit        { get; }
		public          DoneDataNode                   DoneData      { get; }

		object IAncestorProvider.Ancestor => _final.Ancestor;

		FormattableString IDebugEntityId.EntityId => $"{Id}(${DocumentId})";

		public override IIdentifier Id { get; }

		ImmutableArray<IOnEntry> IFinal.OnEntry  => ImmutableArray<IOnEntry>.CastUp(OnEntry);
		ImmutableArray<IOnExit> IFinal. OnExit   => ImmutableArray<IOnExit>.CastUp(OnExit);
		IDoneData IFinal.               DoneData => DoneData;

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.FinalNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntity(Key.Id, Id);
			bucket.AddEntityList(Key.OnEntry, OnEntry);
			bucket.AddEntityList(Key.OnExit, OnExit);
			bucket.AddEntity(Key.DoneData, DoneData);
		}
	}
}