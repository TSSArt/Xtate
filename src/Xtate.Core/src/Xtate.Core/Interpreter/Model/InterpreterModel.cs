﻿using System.Collections.Immutable;
using TSSArt.StateMachine.Annotations;

namespace TSSArt.StateMachine
{
	[PublicAPI]
	internal class InterpreterModel
	{
		public InterpreterModel(StateMachineNode root, int maxConfigurationLength, ImmutableDictionary<int, IEntity> entityMap, ImmutableArray<DataModelNode> dataModelList)
		{
			Root = root;
			MaxConfigurationLength = maxConfigurationLength;
			EntityMap = entityMap;
			DataModelList = dataModelList;
		}

		public StateMachineNode Root { get; }

		public int MaxConfigurationLength { get; }

		public ImmutableDictionary<int, IEntity> EntityMap { get; }

		public ImmutableArray<DataModelNode> DataModelList { get; }
	}
}