﻿using System;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace TSSArt.StateMachine
{
	[PublicAPI]
	public class TransitionFluentBuilder<TOuterBuilder>
	{
		private readonly ITransitionBuilder  _builder;
		private readonly Action<ITransition> _builtAction;
		private readonly TOuterBuilder       _outerBuilder;

		public TransitionFluentBuilder(IBuilderFactory factory, TOuterBuilder outerBuilder, Action<ITransition> builtAction)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));

			_builder = factory.CreateTransitionBuilder(null);
			_outerBuilder = outerBuilder;
			_builtAction = builtAction;
		}

		public TOuterBuilder EndTransition()
		{
			_builtAction(_builder.Build());

			return _outerBuilder;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetEvent(params IEventDescriptor[] eventsDescriptor)
		{
			if (eventsDescriptor == null) throw new ArgumentNullException(nameof(eventsDescriptor));
			if (eventsDescriptor.Length == 0) throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyCollection, nameof(eventsDescriptor));

			_builder.SetEvent(eventsDescriptor.ToImmutableArray());

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetEvent(ImmutableArray<IEventDescriptor> eventsDescriptor)
		{
			if (eventsDescriptor.IsDefaultOrEmpty) throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyList, nameof(eventsDescriptor));

			_builder.SetEvent(eventsDescriptor);

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetCondition(PredicateCancellableTask predicate)
		{
			_builder.SetCondition(new RuntimePredicate(predicate));

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetCondition(Predicate predicate)
		{
			_builder.SetCondition(new RuntimePredicate(predicate));

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetTarget(params string[] target)
		{
			if (target == null) throw new ArgumentNullException(nameof(target));
			if (target.Length == 0) throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyCollection, nameof(target));

			var builder = ImmutableArray.CreateBuilder<IIdentifier>(target.Length);

			foreach (var s in target)
			{
				builder.Add((Identifier) s);
			}

			_builder.SetTarget(builder.MoveToImmutable());

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetTarget(params IIdentifier[] target)
		{
			if (target == null) throw new ArgumentNullException(nameof(target));
			if (target.Length == 0) throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyCollection, nameof(target));

			_builder.SetTarget(target.ToImmutableArray());

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetTarget(ImmutableArray<string> target)
		{
			if (target.IsDefaultOrEmpty) throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyList, nameof(target));

			_builder.SetTarget(ImmutableArray.CreateRange<string, IIdentifier>(target, id => (Identifier) id));

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetTarget(ImmutableArray<IIdentifier> target)
		{
			if (target.IsDefaultOrEmpty) throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyList, nameof(target));

			_builder.SetTarget(target);

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> SetType(TransitionType type)
		{
			_builder.SetType(type);

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> AddOnTransition(ExecutableAction action)
		{
			_builder.AddAction(new RuntimeAction(action));

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> AddOnTransition(ExecutableTask task)
		{
			_builder.AddAction(new RuntimeAction(task));

			return this;
		}

		public TransitionFluentBuilder<TOuterBuilder> AddOnTransition(ExecutableCancellableTask task)
		{
			_builder.AddAction(new RuntimeAction(task));

			return this;
		}
	}
}