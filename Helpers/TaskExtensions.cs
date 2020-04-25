﻿using System.Threading;
using System.Threading.Tasks;
using TSSArt.StateMachine.Annotations;

namespace TSSArt.StateMachine
{
	[PublicAPI]
	internal static class TaskExtensions
	{
		public static Task<T> WaitAsync<T>(this Task<T> task, CancellationToken token)
		{
			if (task.IsCompleted || !token.CanBeCanceled)
			{
				return task;
			}

			if (token.IsCancellationRequested)
			{
				return Task.FromCanceled<T>(token);
			}

			return WaitAsyncLocal();

			async Task<T> WaitAsyncLocal()
			{
				await Task.WhenAny(task, Task.Delay(millisecondsDelay: -1, token)).ConfigureAwait(false);

				token.ThrowIfCancellationRequested();

				return await task.ConfigureAwait(false);
			}
		}

		public static ValueTask<T> WaitAsync<T>(this ValueTask<T> task, CancellationToken token)
		{
			if (task.IsCompleted || !token.CanBeCanceled)
			{
				return task;
			}

			if (token.IsCancellationRequested)
			{
				return new ValueTask<T>(Task.FromCanceled<T>(token));
			}

			return WaitAsyncLocal();

			async ValueTask<T> WaitAsyncLocal()
			{
				await Task.WhenAny(task.AsTask(), Task.Delay(millisecondsDelay: -1, token)).ConfigureAwait(false);

				token.ThrowIfCancellationRequested();

				return await task.ConfigureAwait(false);
			}
		}

		public static void Forget(this ValueTask valueTask)
		{
			if (!valueTask.IsCompleted)
			{
				valueTask.AsTask();
			}
		}

		public static void Forget<T>(this ValueTask<T> valueTask)
		{
			if (!valueTask.IsCompleted)
			{
				valueTask.AsTask();
			}
		}
	}
}