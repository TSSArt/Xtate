﻿using System;
using System.Collections.Immutable;

namespace TSSArt.StateMachine
{
	public class DataModelBuilder : BuilderBase, IDataModelBuilder
	{
		private ImmutableArray<IData>.Builder? _dataList;

		public DataModelBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor)
		{ }

		public IDataModel Build() => new DataModelEntity { Ancestor = Ancestor, Data = _dataList?.ToImmutable() ?? default };

		public void AddData(IData data)
		{
			if (data == null) throw new ArgumentNullException(nameof(data));

			(_dataList ??= ImmutableArray.CreateBuilder<IData>()).Add(data);
		}
	}
}