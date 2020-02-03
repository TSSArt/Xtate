﻿using System.Collections./**/Immutable;

namespace TSSArt.StateMachine
{
	public interface IStateMachine : IEntity
	{
		string                      Name          { get; }
		string                      DataModelType { get; }
		BindingType                 Binding       { get; }
		IInitial                    Initial       { get; }
		/**/ImmutableArray<IStateEntity> States        { get; }
		IDataModel                  DataModel     { get; }
		IExecutableEntity           Script        { get; }
	}
}