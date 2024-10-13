using System;
using Xtate.IoC;

namespace Xtate;

public class ServiceProviderDebuggerLimit(int limit, IServiceProviderActions next) : IServiceProviderActions
{
	private int _level;
	
	public IServiceProviderDataActions? RegisterServices() => next.RegisterServices();

	public IServiceProviderDataActions? ServiceRequesting(TypeKey typeKey) => next.ServiceRequesting(typeKey);

	public IServiceProviderDataActions? ServiceRequested(TypeKey typeKey) => next.ServiceRequested(typeKey);

	public IServiceProviderDataActions? FactoryCalling(TypeKey typeKey)
	{
		_level++;

		if (_level >= limit)
		{
			throw new InvalidOperationException();
		}

		return next.FactoryCalling(typeKey);
	}

	public IServiceProviderDataActions? FactoryCalled(TypeKey typeKey)
	{
		_level--;

		return next.FactoryCalled(typeKey);
	}
}