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
using Xtate.IoC;

namespace Xtate;

public class ServiceProviderDebuggerLimit(int limit, IServiceProviderActions next) : IServiceProviderActions
{
	private int _level;

#region Interface IServiceProviderActions

	public IServiceProviderDataActions? RegisterServices() => next.RegisterServices();

	public IServiceProviderDataActions? ServiceRequesting(TypeKey typeKey) => next.ServiceRequesting(typeKey);

	public IServiceProviderDataActions? ServiceRequested(TypeKey typeKey) => next.ServiceRequested(typeKey);

	public IServiceProviderDataActions? FactoryCalling(TypeKey typeKey)
	{
		_level ++;

		if (_level >= limit)
		{
			throw new InvalidOperationException();
		}

		return next.FactoryCalling(typeKey);
	}

	public IServiceProviderDataActions? FactoryCalled(TypeKey typeKey)
	{
		_level --;

		return next.FactoryCalled(typeKey);
	}

#endregion
}