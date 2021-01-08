﻿#region Copyright © 2019-2021 Sergii Artemenko

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

namespace Xtate.Service
{
	public class InputServiceFactory : ServiceFactoryBase
	{
		public static IServiceFactory Instance { get; } = new InputServiceFactory();

		protected override void Register(IServiceCatalog catalog)
		{
			if (catalog is null) throw new ArgumentNullException(nameof(catalog));

			catalog.Register(type: "http://xtate.net/scxml/service/#Input", () => new InputService());
			catalog.Register(type: "input", () => new InputService());
		}
	}
}