﻿#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System.Collections.Immutable;
using Xtate.DataModel;
using Xtate.IoProcessor;
using Xtate.Service;

namespace Xtate.Test
{
	public static class StateMachineHostOptionsTestBuilder
	{
		public delegate void StateMachineHostOptionsSetup(StateMachineHostOptions options);

		public static StateMachineHostOptions Create(StateMachineHostOptionsSetup build)
		{
			var options = new StateMachineHostOptions
						  {
								  DataModelHandlerFactories = ImmutableArray<IDataModelHandlerFactory>.Empty,
								  IoProcessorFactories = ImmutableArray<IIoProcessorFactory>.Empty,
								  ServiceFactories = ImmutableArray<IServiceFactory>.Empty
						  };

			build(options);

			return options;
		}
	}
}