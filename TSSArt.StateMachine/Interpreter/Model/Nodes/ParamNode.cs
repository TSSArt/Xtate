﻿using System;
using System.Collections.Generic;

namespace TSSArt.StateMachine
{
	internal sealed class ParamNode : IParam, IStoreSupport, IAncestorProvider, IDocumentId, IDebugEntityId
	{
		private readonly LinkedListNode<int> _documentIdNode;
		private readonly ParamEntity         _param;

		public ParamNode(LinkedListNode<int> documentIdNode, in ParamEntity param)
		{
			Infrastructure.Assert(param.Name != null);

			_documentIdNode = documentIdNode;
			_param = param;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _param.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Name}(#{DocumentId})";

	#endregion

	#region Interface IDocumentId

		public int DocumentId => _documentIdNode.Value;

	#endregion

	#region Interface IParam

		public string Name => _param.Name!;

		public IValueExpression? Expression => _param.Expression;

		public ILocationExpression? Location => _param.Location;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ParamNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.Add(Key.Name, Name);
			bucket.AddEntity(Key.Expression, Expression);
			bucket.AddEntity(Key.Location, Location);
		}

	#endregion
	}
}