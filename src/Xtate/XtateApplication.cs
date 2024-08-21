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

public class XtateApplication : IAsyncDisposable
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

	public static XtateApplication Create() => new();

	public StateMachineFluentBuilder CreateStateMachineBuilder() => _provider.GetRequiredServiceSync<StateMachineFluentBuilder>();

	public async ValueTask Start()
	{
		var host = await _provider.GetRequiredService<IHostController>().ConfigureAwait(false);

		await host.StartHost().ConfigureAwait(false);
	}
	
	public async ValueTask Stop()
	{
		var host = await _provider.GetRequiredService<IHostController>().ConfigureAwait(false);

		await host.StopHost().ConfigureAwait(false);
	}

	public async ValueTask<DataModelValue> ExecuteStateMachine(IStateMachine stateMachine, DataModelValue arguments = default, CancellationToken token = default)
	{
		/*
		var serviceScopeFactory = _provider.GetRequiredServiceSync<IServiceScopeFactory>();

		var serviceScope = serviceScopeFactory.CreateScope(services => services.AddConstant(stateMachine));

		var host = await _provider.GetRequiredService<IHostController>().ConfigureAwait(false);

		var origin = new StateMachineOrigin(stateMachine);
		
		var controller = await host.StartStateMachine(SessionId.New(), origin, parameters, SecurityContextType.NoAccess, token).ConfigureAwait(false);

		return await controller.GetResult(token).ConfigureAwait(false);*/

		var module = new RuntimeStateMachineModule(stateMachine, arguments);

		return await ExecuteStateMachine(module, token).ConfigureAwait(false);
	}

	public async ValueTask<DataModelValue> ExecuteStateMachine(StateMachineModule stateMachineModule, CancellationToken token)
	{
		var serviceScopeFactory = _provider.GetRequiredServiceSync<IServiceScopeFactory>();
		var serviceScope = serviceScopeFactory.CreateScope(stateMachineModule.AddServices);

		await using (serviceScope.ConfigureAwait(false))
		{
			var stateMachineController = await serviceScope.ServiceProvider.GetRequiredService<IStateMachineController>().ConfigureAwait(false);

			await stateMachineController.StartAsync(token).ConfigureAwait(false);
			return await stateMachineController.GetResult(token).ConfigureAwait(false);
		}
	}
}

public class RuntimeStateMachineModule(IStateMachine stateMachine, DataModelValue arguments) : StateMachineModule
{
	public override void AddServices(IServiceCollection services)
	{
		services.AddConstant(stateMachine);
		services.AddConstant<IStateMachineArguments>(new StateMachineArguments(arguments));
	}
}

public abstract class StateMachineModule
{
	public abstract void AddServices(IServiceCollection services);
}