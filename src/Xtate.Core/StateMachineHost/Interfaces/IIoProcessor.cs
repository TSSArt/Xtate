﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace TSSArt.StateMachine
{
	public interface IIoProcessor
	{
		Uri Id { get; }

		bool CanHandle(Uri? type, Uri? target);

		Uri GetTarget(SessionId sessionId);

		ValueTask Dispatch(SessionId sessionId, IOutgoingEvent evt, CancellationToken token);
	}
}