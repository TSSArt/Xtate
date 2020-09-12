﻿#region Copyright © 2019-2020 Sergii Artemenko

// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Service;

namespace Xtate
{
	public sealed partial class StateMachineHost : IServiceFactory, IServiceFactoryActivator
	{
		private static readonly Uri ServiceFactoryTypeId      = new Uri("http://www.w3.org/TR/scxml/");
		private static readonly Uri ServiceFactoryAliasTypeId = new Uri(uriString: "scxml", UriKind.Relative);

	#region Interface IServiceFactory

		ValueTask<IServiceFactoryActivator?> IServiceFactory.TryGetActivator(IFactoryContext factoryContext, Uri type, CancellationToken token) =>
				new ValueTask<IServiceFactoryActivator?>(CanHandle(type) ? this : null);

	#endregion

	#region Interface IServiceFactoryActivator

		async ValueTask<IService> IServiceFactoryActivator.StartService(IFactoryContext factoryContext, Uri? baseUri, InvokeData invokeData, IServiceCommunication serviceCommunication,
																		CancellationToken token)
		{
			Infrastructure.Assert(CanHandle(invokeData.Type));

			var sessionId = SessionId.New();
			var scxml = invokeData.RawContent ?? invokeData.Content.AsStringOrDefault();
			var parameters = invokeData.Parameters;
			var source = invokeData.Source;

			Infrastructure.Assert(scxml is { } || source is { });

			var origin = scxml is { } ? new StateMachineOrigin(scxml, baseUri) : new StateMachineOrigin(source!, baseUri);

			return await StartStateMachine(sessionId, origin, parameters, token).ConfigureAwait(false);
		}

	#endregion

		private static bool CanHandle(Uri type) => FullUriComparer.Instance.Equals(type, ServiceFactoryTypeId) || FullUriComparer.Instance.Equals(type, ServiceFactoryAliasTypeId);
	}
}