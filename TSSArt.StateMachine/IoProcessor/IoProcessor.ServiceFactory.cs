﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace TSSArt.StateMachine
{
	public sealed partial class IoProcessor : IServiceFactory
	{
		private static readonly Uri ServiceFactoryTypeId      = new Uri("http://www.w3.org/TR/scxml/");
		private static readonly Uri ServiceFactoryAliasTypeId = new Uri(uriString: "scxml", UriKind.Relative);

	#region Interface IServiceFactory

		async ValueTask<IService> IServiceFactory.StartService(Uri? source, string? rawContent, DataModelValue content, DataModelValue parameters,
															   IServiceCommunication serviceCommunication, CancellationToken token)
		{
			var sessionId = IdGenerator.NewSessionId();
			var scxml = rawContent ?? content.AsStringOrDefault();
			var context = GetCurrentContext();

			var errorProcessor = CreateErrorProcessor(sessionId, stateMachine: null, source, scxml);

			var service = await context.CreateAndAddStateMachine(sessionId, options: null, stateMachine: null, source, scxml, parameters, errorProcessor, token).ConfigureAwait(false);

			await service.StartAsync(token).ConfigureAwait(false);

			CompleteAsync();

			async void CompleteAsync()
			{
				try
				{
					await service.Result.ConfigureAwait(false);
				}
				finally
				{
					await context.DestroyStateMachine(sessionId).ConfigureAwait(false);
				}
			}

			return service;
		}

		Uri IServiceFactory.TypeId => ServiceFactoryTypeId;

		Uri IServiceFactory.AliasTypeId => ServiceFactoryAliasTypeId;

	#endregion
	}
}