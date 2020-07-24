﻿using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public class DefaultInvokeEvaluator : IInvoke, IStartInvokeEvaluator, ICancelInvokeEvaluator, IAncestorProvider
	{
		private readonly InvokeEntity _invoke;

		public DefaultInvokeEvaluator(in InvokeEntity invoke)
		{
			_invoke = invoke;

			TypeExpressionEvaluator = invoke.TypeExpression?.As<IStringEvaluator>();
			SourceExpressionEvaluator = invoke.SourceExpression?.As<IStringEvaluator>();
			ContentExpressionEvaluator = invoke.Content?.Expression?.As<IObjectEvaluator>();
			ContentBodyEvaluator = invoke.Content?.Body?.As<IValueEvaluator>();
			IdLocationEvaluator = invoke.IdLocation?.As<ILocationEvaluator>();
			NameEvaluatorList = invoke.NameList.AsArrayOf<ILocationExpression, ILocationEvaluator>();
			ParameterList = invoke.Parameters.AsArrayOf<IParam, DefaultParam>();
		}

		public IObjectEvaluator?                  ContentExpressionEvaluator { get; }
		public IValueEvaluator?                   ContentBodyEvaluator       { get; }
		public ILocationEvaluator?                IdLocationEvaluator        { get; }
		public ImmutableArray<ILocationEvaluator> NameEvaluatorList          { get; }
		public ImmutableArray<DefaultParam>       ParameterList              { get; }
		public IStringEvaluator?                  SourceExpressionEvaluator  { get; }
		public IStringEvaluator?                  TypeExpressionEvaluator    { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _invoke.Ancestor;

	#endregion

	#region Interface ICancelInvokeEvaluator

		public virtual ValueTask Cancel(InvokeId invokeId, IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));
			if (invokeId == null) throw new ArgumentNullException(nameof(invokeId));

			return executionContext.CancelInvoke(invokeId, token);
		}

	#endregion

	#region Interface IInvoke

		public Uri?                                Type             => _invoke.Type;
		public IValueExpression?                   TypeExpression   => _invoke.TypeExpression;
		public Uri?                                Source           => _invoke.Source;
		public IValueExpression?                   SourceExpression => _invoke.SourceExpression;
		public string?                             Id               => _invoke.Id;
		public ILocationExpression?                IdLocation       => _invoke.IdLocation;
		public ImmutableArray<ILocationExpression> NameList         => _invoke.NameList;
		public bool                                AutoForward      => _invoke.AutoForward;
		public ImmutableArray<IParam>              Parameters       => _invoke.Parameters;
		public IFinalize?                          Finalize         => _invoke.Finalize;
		public IContent?                           Content          => _invoke.Content;

	#endregion

	#region Interface IStartInvokeEvaluator

		public virtual async ValueTask<InvokeId> Start(IIdentifier stateId, IExecutionContext executionContext, CancellationToken token)
		{
			if (stateId == null) throw new ArgumentNullException(nameof(stateId));
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			var invokeId = InvokeId.New(stateId, _invoke.Id);

			if (IdLocationEvaluator != null)
			{
				await IdLocationEvaluator.SetValue(invokeId, executionContext, token).ConfigureAwait(false);
			}

			var type = TypeExpressionEvaluator != null ? ToUri(await TypeExpressionEvaluator.EvaluateString(executionContext, token).ConfigureAwait(false)) : _invoke.Type;
			var source = SourceExpressionEvaluator != null ? ToUri(await SourceExpressionEvaluator.EvaluateString(executionContext, token).ConfigureAwait(false)) : _invoke.Source;

			var rawContent = ContentBodyEvaluator is IStringEvaluator rawContentEvaluator ? await rawContentEvaluator.EvaluateString(executionContext, token).ConfigureAwait(false) : null;
			var content = await DataConverter.GetContent(ContentBodyEvaluator, ContentExpressionEvaluator, executionContext, token).ConfigureAwait(false);
			var parameters = await DataConverter.GetParameters(NameEvaluatorList, ParameterList, executionContext, token).ConfigureAwait(false);

			Infrastructure.Assert(type != null);

			var invokeData = new InvokeData(invokeId, type, source, rawContent, content, parameters);

			await executionContext.StartInvoke(invokeData, token).ConfigureAwait(false);

			return invokeId;
		}

	#endregion

		private static Uri ToUri(string uri) => new Uri(uri, UriKind.RelativeOrAbsolute);
	}
}