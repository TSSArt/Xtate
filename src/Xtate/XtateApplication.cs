﻿// Copyright © 2019-2024 Sergii Artemenko
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
using System.Diagnostics;
using System.Threading.Tasks;
using Xtate.Builder;
using Xtate.Core;
using Xtate.IoC;
using IServiceProvider = Xtate.IoC.IServiceProvider;

namespace Xtate;

public class XtateApplicationBuilder
{
	private class TraceLogModule : Module
	{
		protected override void AddServices()
		{
			Services.AddImplementation<TraceLogWriter<Any>>().For<ILogWriter<Any>>();
		}
	}

	private class ConsoleLogModule : Module<TraceLogModule>
	{
		protected override void AddServices()
		{
			Services.AddForwarding<TraceListener>(_ => new TextWriterTraceListener(Console.Out));
		}
	}

	private readonly ServiceCollection _services = [];

	public XtateApplicationBuilder()
	{
		_services.AddModule<XtateModule>();
		//services.AddTransient<IServiceProviderDebugger>((provider) => new ServiceProviderDebugger(new StreamWriter(File.Create("C:\\tmp\\fff12.log"))));
		//services.AddTransient<IServiceProviderActions>((provider) => new ServiceProviderDebugger(Console.Out));
		//services.AddTransientDecorator<IServiceProviderActions>((provider, debugger) => new ServiceProviderDebuggerLimit(50, debugger));

	}

	public XtateApplicationBuilder AddServices(Action<IServiceCollection> addServices)
	{
		addServices(_services);

		return this;
	}

	public XtateApplicationBuilder LogToConsole()
	{
		_services.AddModule<ConsoleLogModule>();

		return this;
	}

	public XtateApplication Build()
	{
		return new XtateApplication(_services.BuildProvider());
	}
}

public class XtateApplication : IAsyncDisposable
{
	private readonly IServiceProvider _provider;

	internal XtateApplication(IServiceProvider provider) => _provider = provider;

#region Interface IAsyncDisposable

	public ValueTask DisposeAsync() => Disposer.DisposeAsync(_provider);

#endregion

	public static XtateApplication Create() => CreateBuilder().Build();

	public static XtateApplicationBuilder CreateBuilder() => new();

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

	public async ValueTask<DataModelValue> ExecuteStateMachine(IStateMachine stateMachine, DataModelValue arguments = default, SessionId? sessionId = default, Uri? location = default)
	{
		var stateMachineClass = new RuntimeStateMachine(stateMachine) { SessionId = sessionId!, Location = location!, Arguments = arguments };

		var host = await _provider.GetRequiredService<IHostController>().ConfigureAwait(false);

		return await host.ExecuteStateMachine(stateMachineClass, SecurityContextType.NewStateMachine).ConfigureAwait(false);
	}

	public async ValueTask StartStateMachine(IStateMachine stateMachine, DataModelValue arguments = default, SessionId? sessionId = default, Uri? location = default)
	{
		var stateMachineClass = new RuntimeStateMachine(stateMachine) { SessionId = sessionId!, Location = location!, Arguments = arguments };

		var host = await _provider.GetRequiredService<IHostController>().ConfigureAwait(false);

		await host.StartStateMachine(stateMachineClass, SecurityContextType.NewStateMachine).ConfigureAwait(false);
	}

	public async ValueTask<DataModelValue> ExecuteStateMachine(Uri location, DataModelValue arguments = default, SessionId? sessionId = default)
	{
		var stateMachineClass = new LocationStateMachine(location) { SessionId = sessionId!, Arguments = arguments };

		var host = await _provider.GetRequiredService<IHostController>().ConfigureAwait(false);

		return await host.ExecuteStateMachine(stateMachineClass, SecurityContextType.NewStateMachine).ConfigureAwait(false);
	}

	public async ValueTask StartStateMachine(Uri location, DataModelValue arguments = default, SessionId? sessionId = default)
	{
		var stateMachineClass = new LocationStateMachine(location) { SessionId = sessionId!, Arguments = arguments };

		var host = await _provider.GetRequiredService<IHostController>().ConfigureAwait(false);

		await host.StartStateMachine(stateMachineClass, SecurityContextType.NewStateMachine).ConfigureAwait(false);
	}
	
	public async ValueTask<DataModelValue> ExecuteStateMachine(string scxml, DataModelValue arguments = default, SessionId? sessionId = default, Uri? location = default)
	{
		var stateMachineClass = new ScxmlStringStateMachine(scxml) { SessionId = sessionId!, Location = location!, Arguments = arguments };

		var host = await _provider.GetRequiredService<IHostController>().ConfigureAwait(false);

		return await host.ExecuteStateMachine(stateMachineClass, SecurityContextType.NewStateMachine).ConfigureAwait(false);
	}

	public async ValueTask StartStateMachine(string scxml, DataModelValue arguments = default, SessionId? sessionId = default, Uri? location = default)
	{
		var stateMachineClass = new ScxmlStringStateMachine(scxml) { SessionId = sessionId!, Location = location!, Arguments = arguments };

		var host = await _provider.GetRequiredService<IHostController>().ConfigureAwait(false);

		await host.StartStateMachine(stateMachineClass, SecurityContextType.NewStateMachine).ConfigureAwait(false);
	}
}
