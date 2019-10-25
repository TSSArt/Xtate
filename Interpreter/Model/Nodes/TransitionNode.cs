﻿using System;
using System.Collections.Generic;

namespace TSSArt.StateMachine
{
	public class TransitionNode : ITransition, IStoreSupport, IAncestorProvider, IDocumentId, IDebugEntityId
	{
		private readonly Transition          _transition;
		private readonly LinkedListNode<int> _documentIdNode;

		public TransitionNode(LinkedListNode<int> documentIdNode, in Transition transition, IReadOnlyList<StateEntityNode> target = null)
		{
			_transition = transition;
			_documentIdNode = documentIdNode;
			TargetState = target;
			ActionEvaluators = transition.Action.AsListOf<IExecEvaluator>() ?? Array.Empty<IExecEvaluator>();
			ConditionEvaluator = transition.Condition.As<IBooleanEvaluator>();
		}

		public IReadOnlyList<StateEntityNode> TargetState        { get; private set; }
		public StateEntityNode                Source             { get; private set; }
		public IReadOnlyList<IExecEvaluator>  ActionEvaluators   { get; }
		public IBooleanEvaluator              ConditionEvaluator { get; }

		object IAncestorProvider.Ancestor => _transition.Ancestor;

		public FormattableString EntityId => $"(#{DocumentId})";

		public int DocumentId => _documentIdNode.Value;

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.TransitionNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntityList(Key.Event, Event);
			bucket.AddEntity(Key.Condition, Condition);
			bucket.AddEntityList(Key.Target, Target);
			bucket.Add(Key.TransitionType, Type);
			bucket.AddEntityList(Key.Action, Action);
		}

		public IReadOnlyList<IEventDescriptor> Event => _transition.Event;

		public IExecutableEntity Condition => _transition.Condition;

		public IReadOnlyList<IIdentifier> Target => _transition.Target;

		public TransitionType Type => _transition.Type;

		public IReadOnlyList<IExecutableEntity> Action => _transition.Action;

		public void MapTarget(Dictionary<IIdentifier, StateEntityNode> idMap)
		{
			TargetState = StateEntityNodeList.Create(Target, id => idMap[id]);
		}

		public void SetSource(StateEntityNode source) => Source = source;

		private class StateEntityNodeList : ValidatedReadOnlyList<StateEntityNodeList, StateEntityNode>
		{
			protected override Options GetOptions() => Options.NullIfEmpty;
		}
	}
}