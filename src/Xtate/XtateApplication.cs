// Copyright © 2019-2025 Sergii Artemenko
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
using System.Threading.Tasks;
using Xtate.Builder;
using Xtate.Core;
using Xtate.IoC;
using IServiceProvider = Xtate.IoC.IServiceProvider;

namespace Xtate;

public class XtateApplicationOptions
{
	//public bool EnableIocDebug { get; set; } = false;
}

public class XtateApplicationBuilder
{
    private readonly ServiceCollection _services = [];

    private readonly XtateApplicationOptions _options = new();

    public XtateApplicationBuilder()
    {
        _services.AddModule<XtateModule>();
    }

    public XtateApplicationBuilder AddServices(Action<IServiceCollection> addServices)
    {
        addServices(_services);

        return this;
    }

    public XtateApplicationBuilder AddModule<TModule>() where TModule : IModule, new()
    {
        _services.AddModule<TModule>();

        return this;
    }

    public XtateApplicationBuilder Configure<TOptions>(Action<TOptions> setOptions)
    {
        _services.Configure(setOptions);

        return this;
    }

	public XtateApplicationBuilder SetOptions(Action<XtateApplicationOptions> setOptions)
    {
        setOptions(_options);

        return this;
    }

    public XtateApplication Build()
    {
       /* if (_options.EnableIocDebug)
        {
            _services.AddModule<IocDebugModule>();
        }*/

        return new XtateApplication(_services.BuildProvider());
    }
}

public class XtateApplication : IAsyncDisposable
{
    private readonly IServiceProvider _serviceProvider;

    internal XtateApplication(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

#region Interface IAsyncDisposable

    public async ValueTask DisposeAsync() => await Disposer.DisposeAsync(_serviceProvider).ConfigureAwait(false);

#endregion

    public static XtateApplication Create() => CreateBuilder().Build();

    public static XtateApplicationBuilder CreateBuilder() => new();

    public StateMachineFluentBuilder CreateStateMachineBuilder() => _serviceProvider.GetRequiredServiceSync<StateMachineFluentBuilder>();

    public async ValueTask Start()
    {
        //var host = await _serviceProvider.GetRequiredService<IHostController>().ConfigureAwait(false);

        //await host.StartHost().ConfigureAwait(false);
    }

    public async ValueTask Stop()
    {
        //var host = await _serviceProvider.GetRequiredService<IHostController>().ConfigureAwait(false);

        //await host.StopHost().ConfigureAwait(false);
    }

    private ValueTask<IStateMachineScopeManager> GetStateMachineScopeManager() => _serviceProvider.GetRequiredService<IStateMachineScopeManager>();

    public async ValueTask<DataModelValue> ExecuteStateMachine(IStateMachine stateMachine,
                                                               DataModelValue arguments = default,
                                                               SessionId? sessionId = null,
                                                               Uri? location = null)
    {
        var stateMachineScopeManager = await GetStateMachineScopeManager().ConfigureAwait(false);

        var stateMachineClass = new RuntimeStateMachine(stateMachine) { SessionId = sessionId!, Location = location!, Arguments = arguments };

        return await stateMachineScopeManager.Execute(stateMachineClass, SecurityContextType.NewStateMachine).ConfigureAwait(false);
    }

    public async ValueTask StartStateMachine(IStateMachine stateMachine,
                                             DataModelValue arguments = default,
                                             SessionId? sessionId = null,
                                             Uri? location = null)
    {
        var stateMachineScopeManager = await GetStateMachineScopeManager().ConfigureAwait(false);

        var stateMachineClass = new RuntimeStateMachine(stateMachine) { SessionId = sessionId!, Location = location!, Arguments = arguments };

        await stateMachineScopeManager.Start(stateMachineClass, SecurityContextType.NewStateMachine).ConfigureAwait(false);
    }

    public async ValueTask<DataModelValue> ExecuteStateMachine(Uri location, DataModelValue arguments = default, SessionId? sessionId = null)
    {
        var stateMachineScopeManager = await GetStateMachineScopeManager().ConfigureAwait(false);

        var stateMachineClass = new LocationStateMachine(location) { SessionId = sessionId!, Arguments = arguments };

        return await stateMachineScopeManager.Execute(stateMachineClass, SecurityContextType.NewStateMachine).ConfigureAwait(false);
    }

    public async ValueTask StartStateMachine(Uri location, DataModelValue arguments = default, SessionId? sessionId = null)
    {
        var stateMachineScopeManager = await GetStateMachineScopeManager().ConfigureAwait(false);

        var stateMachineClass = new LocationStateMachine(location) { SessionId = sessionId!, Arguments = arguments };

        await stateMachineScopeManager.Start(stateMachineClass, SecurityContextType.NewStateMachine).ConfigureAwait(false);
    }

    public async ValueTask<DataModelValue> ExecuteStateMachine(string scxml,
                                                               DataModelValue arguments = default,
                                                               SessionId? sessionId = null,
                                                               Uri? location = null)
    {
        var stateMachineScopeManager = await GetStateMachineScopeManager().ConfigureAwait(false);

        var stateMachineClass = new ScxmlStringStateMachine(scxml) { SessionId = sessionId!, Location = location!, Arguments = arguments };

        return await stateMachineScopeManager.Execute(stateMachineClass, SecurityContextType.NewStateMachine).ConfigureAwait(false);
    }

    public async ValueTask StartStateMachine(string scxml,
                                             DataModelValue arguments = default,
                                             SessionId? sessionId = null,
                                             Uri? location = null)
    {
        var stateMachineScopeManager = await GetStateMachineScopeManager().ConfigureAwait(false);

        var stateMachineClass = new ScxmlStringStateMachine(scxml) { SessionId = sessionId!, Location = location!, Arguments = arguments };

        await stateMachineScopeManager.Start(stateMachineClass, SecurityContextType.NewStateMachine).ConfigureAwait(false);
    }

	public async ValueTask DestroyStateMachine(SessionId sessionId)
	{
		var stateMachineScopeManager = await GetStateMachineScopeManager().ConfigureAwait(false);

		await stateMachineScopeManager.Destroy(sessionId).ConfigureAwait(false);
	}
}