﻿using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using TSSArt.StateMachine.Annotations;

namespace TSSArt.StateMachine
{
	[PublicAPI]
	public class ParallelFluentBuilder<TOuterBuilder> where TOuterBuilder : notnull
	{
		private readonly IParallelBuilder  _builder;
		private readonly Action<IParallel> _builtAction;
		private readonly IBuilderFactory   _factory;
		private readonly TOuterBuilder     _outerBuilder;

		public ParallelFluentBuilder(IBuilderFactory factory, TOuterBuilder outerBuilder, Action<IParallel> builtAction)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
			_builder = factory.CreateParallelBuilder(null);
			_outerBuilder = outerBuilder;
			_builtAction = builtAction;
		}

		[return: NotNull]
		public TOuterBuilder EndParallel()
		{
			_builtAction(_builder.Build());

			return _outerBuilder;
		}

		public ParallelFluentBuilder<TOuterBuilder> SetId(string id) => SetId((Identifier) id);

		public ParallelFluentBuilder<TOuterBuilder> SetId(IIdentifier id)
		{
			if (id == null) throw new ArgumentNullException(nameof(id));

			_builder.SetId(id);

			return this;
		}

		private ParallelFluentBuilder<TOuterBuilder> AddOnEntry(RuntimeAction action)
		{
			_builder.AddOnEntry(new OnEntryEntity { Action = ImmutableArray.Create<IExecutableEntity>(action) });

			return this;
		}

		public ParallelFluentBuilder<TOuterBuilder> AddOnEntry(ExecutableAction action) => AddOnEntry(new RuntimeAction(action));

		public ParallelFluentBuilder<TOuterBuilder> AddOnEntry(ExecutableTask task) => AddOnEntry(new RuntimeAction(task));

		public ParallelFluentBuilder<TOuterBuilder> AddOnEntry(ExecutableCancellableTask task) => AddOnEntry(new RuntimeAction(task));

		private ParallelFluentBuilder<TOuterBuilder> AddOnExit(RuntimeAction action)
		{
			_builder.AddOnExit(new OnExitEntity { Action = ImmutableArray.Create<IExecutableEntity>(action) });

			return this;
		}

		public ParallelFluentBuilder<TOuterBuilder> AddOnExit(ExecutableAction action) => AddOnExit(new RuntimeAction(action));

		public ParallelFluentBuilder<TOuterBuilder> AddOnExit(ExecutableTask task) => AddOnExit(new RuntimeAction(task));

		public ParallelFluentBuilder<TOuterBuilder> AddOnExit(ExecutableCancellableTask task) => AddOnExit(new RuntimeAction(task));

		public StateFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginState() => new StateFluentBuilder<ParallelFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddState);

		public ParallelFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginParallel() => new ParallelFluentBuilder<ParallelFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddParallel);

		public HistoryFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginHistory() => new HistoryFluentBuilder<ParallelFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddHistory);

		public StateFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginState(string id) => BeginState((Identifier) id);

		public StateFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginState(IIdentifier id) =>
				new StateFluentBuilder<ParallelFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddState).SetId(id);

		public ParallelFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginParallel(string id) => BeginParallel((Identifier) id);

		public ParallelFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginParallel(IIdentifier id) =>
				new ParallelFluentBuilder<ParallelFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddParallel).SetId(id);

		public HistoryFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginHistory(string id) => BeginHistory((Identifier) id);

		public HistoryFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginHistory(IIdentifier id) =>
				new HistoryFluentBuilder<ParallelFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddHistory).SetId(id);

		public TransitionFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginTransition() =>
				new TransitionFluentBuilder<ParallelFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddTransition);

		public ParallelFluentBuilder<TOuterBuilder> AddTransition(EventDescriptor eventDescriptor, string target) => AddTransition(eventDescriptor, (Identifier) target);

		public ParallelFluentBuilder<TOuterBuilder> AddTransition(EventDescriptor eventDescriptor, IIdentifier target) => BeginTransition().SetEvent(eventDescriptor).SetTarget(target).EndTransition();

		public ParallelFluentBuilder<TOuterBuilder> AddTransition(Predicate condition, string target) => AddTransition(condition, (Identifier) target);

		public ParallelFluentBuilder<TOuterBuilder> AddTransition(Predicate condition, IIdentifier target) => BeginTransition().SetCondition(condition).SetTarget(target).EndTransition();
	}
}