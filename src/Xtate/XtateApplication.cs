// Copyright © 2019-2024 Sergii Artemenko
// 
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

using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Builder;
using Xtate.Core;
using Xtate.IoC;

namespace Xtate;

public class XtateApplication : IDisposable, IAsyncDisposable
{
	private readonly ServiceProvider _provider;

	public XtateApplication()
	{
		var services = new ServiceCollection();
		services.AddModule<XtateModule>();
		_provider = new ServiceProvider(services);
	}

#region Interface IAsyncDisposable

	public  ValueTask DisposeAsync() => _provider.DisposeAsync();

#endregion

#region Interface IDisposable

	public void Dispose() => _provider.Dispose();

#endregion

	public static XtateApplication Create() => new();

	public StateMachineFluentBuilder CreateStateMachineBuilder() => _provider.GetRequiredServiceSync<StateMachineFluentBuilder>();

	public async ValueTask Start()
	{
		var host = await _provider.GetRequiredService<IHost>().ConfigureAwait(false);

		await host.StartHost().ConfigureAwait(false);
	}
	
	public async ValueTask Stop()
	{
		var host = await _provider.GetRequiredService<IHost>().ConfigureAwait(false);

		await host.StopHost().ConfigureAwait(false);
	}

	public async ValueTask<DataModelValue> ExecuteStateMachine(IStateMachine stateMachine, CancellationToken token = default)
	{
		var host = await _provider.GetRequiredService<IHost>().ConfigureAwait(false);

		var origin = new StateMachineOrigin(stateMachine);
		
		var controller = await host.StartStateMachineAsync(SessionId.New(), origin, default, SecurityContextType.NewStateMachine, token).ConfigureAwait(false);

		return await controller.GetResult(token).ConfigureAwait(false);
	}
}