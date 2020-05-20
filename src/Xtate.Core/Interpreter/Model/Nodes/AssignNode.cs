﻿using System;

namespace Xtate
{
	internal sealed class AssignNode : ExecutableEntityNode, IAssign, IAncestorProvider, IDebugEntityId
	{
		private readonly AssignEntity _entity;

		public AssignNode(in DocumentIdRecord documentIdNode, in AssignEntity entity) : base(documentIdNode, (IAssign?) entity.Ancestor) => _entity = entity;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _entity.Ancestor;

	#endregion

	#region Interface IAssign

		public ILocationExpression? Location => _entity.Location;

		public IValueExpression? Expression => _entity.Expression;

		public string? InlineContent => _entity.InlineContent;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.AssignNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntity(Key.Location, Location);
			bucket.AddEntity(Key.Expression, Expression);
			bucket.Add(Key.InlineContent, InlineContent);
		}
	}
}