﻿#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Xtate
{
	internal struct DocumentIdRecord
	{
		private LinkedListNode<int>? _node;
		private int                  _value;

		public DocumentIdRecord(LinkedList<int> list)
		{
			_node = list.AddLast(-1);
			_value = -1;
		}

		public int Value
		{
			get
			{
				var node = _node;
				if (node == null)
				{
					return _value;
				}

				var value = node.Value;

				if (value >= 0)
				{
					_node = null;
					_value = value;
				}

				return value;
			}
		}

		[Pure]
		public DocumentIdRecord After()
		{
			Infrastructure.Assert(_node != null);

			return new DocumentIdRecord
				   {
						   _node = _node.List.AddAfter(_node, value: -1),
						   _value = -1
				   };
		}
	}
}