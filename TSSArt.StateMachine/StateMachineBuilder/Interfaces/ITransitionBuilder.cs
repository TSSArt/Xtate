﻿using System.Collections./**/Immutable;

namespace TSSArt.StateMachine
{
	public interface ITransitionBuilder
	{
		ITransition Build();

		void SetEvent(/**/ImmutableArray<IEventDescriptor> eventsDescriptor);
		void SetCondition(IExecutableEntity condition);
		void SetTarget(/**/ImmutableArray<IIdentifier> target);
		void SetType(TransitionType type);
		void AddAction(IExecutableEntity action);
	}
}