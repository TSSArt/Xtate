﻿#region Copyright © 2019-2020 Sergii Artemenko

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
using System.Xml;
using Xtate.Annotations;

namespace Xtate.Scxml
{
	[PublicAPI]
	public class ScxmlDirectorOptions
	{
		private int _maxNestingLevel;

		public IErrorProcessor?        ErrorProcessor        { get; set; }
		public IXmlNamespaceResolver?  NamespaceResolver     { get; set; }
		public XmlReaderSettings?      XmlReaderSettings     { get; set; }
		public ScxmlXmlResolver?       XmlResolver           { get; set; }
		public IStateMachineValidator? StateMachineValidator { get; set; }
		public bool                    Async                 { get; set; }
		public bool                    XIncludeAllowed       { get; set; }

		public int MaxNestingLevel
		{
			get => _maxNestingLevel;
			set
			{
				if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), Resources.Exception_Value_must_be_non_negative_integer);

				_maxNestingLevel = value;
			}
		}
	}
}