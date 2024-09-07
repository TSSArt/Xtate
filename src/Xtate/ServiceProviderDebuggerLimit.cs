using System;
using Xtate.IoC;

namespace Xtate;

public class ServiceProviderDebuggerLimit(int limit, IServiceProviderDebugger? next) : IServiceProviderDebugger
{
	private int _level;

	public void RegisterService(ServiceEntry serviceEntry) => next?.RegisterService(serviceEntry);

	public void BeforeFactory(TypeKey serviceKey)
	{
		_level ++;

		if (_level >= limit)
		{
			throw new InvalidOperationException();
		}

		next?.BeforeFactory(serviceKey);
	}

	public void AfterFactory(TypeKey serviceKey)
	{
		_level --;

		next?.AfterFactory(serviceKey);
	}

	public void FactoryCalled(TypeKey serviceKey) => next?.FactoryCalled(serviceKey);
}