﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace TSSArt.StateMachine
{
	public delegate Task ExecutableTask(IExecutionContext executionContext, CancellationToken token);

	public delegate void ExecutableAction(IExecutionContext executionContext);

	public class RuntimeAction : IExecutableEntity, IExecEvaluator
	{
		private readonly ExecutableAction _action;
		private readonly ExecutableTask   _task;

		public RuntimeAction(ExecutableTask task) => _task = task ?? throw new ArgumentNullException(nameof(task));

		public RuntimeAction(ExecutableAction action) => _action = action ?? throw new ArgumentNullException(nameof(action));

		public Task Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (_task != null)
			{
				return _task(executionContext, token);
			}

			_action(executionContext);

			return Task.CompletedTask;
		}
	}
}