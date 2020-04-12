﻿using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace TSSArt.StateMachine
{
	internal static class DataConverter
	{
		public static ValueTask<DataModelValue> GetData(IValueEvaluator? contentBodyEvaluator, IObjectEvaluator? contentExpressionEvaluator, ImmutableArray<ILocationEvaluator> nameEvaluatorList,
														ImmutableArray<DefaultParam> parameterList, IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			if (nameEvaluatorList.IsDefaultOrEmpty && parameterList.IsDefaultOrEmpty)
			{
				return GetContent(contentBodyEvaluator, contentExpressionEvaluator, executionContext, token);
			}

			return GetParameters(nameEvaluatorList, parameterList, executionContext, token);
		}

		public static async ValueTask<DataModelValue> GetContent(IValueEvaluator? contentBodyEvaluator, IObjectEvaluator? contentExpressionEvaluator,
																 IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			if (contentExpressionEvaluator != null)
			{
				var obj = await contentExpressionEvaluator.EvaluateObject(executionContext, token).ConfigureAwait(false);

				return DataModelValue.FromObject(obj.ToObject()).AsConstant();
			}

			if (contentBodyEvaluator is IObjectEvaluator objectEvaluator)
			{
				var obj = await objectEvaluator.EvaluateObject(executionContext, token).ConfigureAwait(false);

				return DataModelValue.FromObject(obj.ToObject()).AsConstant();
			}

			if (contentBodyEvaluator is IStringEvaluator stringEvaluator)
			{
				var str = await stringEvaluator.EvaluateString(executionContext, token).ConfigureAwait(false);

				return new DataModelValue(str);
			}

			return DataModelValue.Undefined;
		}

		public static async ValueTask<DataModelValue> GetParameters(ImmutableArray<ILocationEvaluator> nameEvaluatorList, ImmutableArray<DefaultParam> parameterList,
																	IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			if (nameEvaluatorList.IsDefaultOrEmpty && parameterList.IsDefaultOrEmpty)
			{
				return DataModelValue.Undefined;
			}

			var attributes = new DataModelObject();

			if (!nameEvaluatorList.IsDefaultOrEmpty)
			{
				foreach (var locationEvaluator in nameEvaluatorList)
				{
					var name = locationEvaluator.GetName(executionContext);
					var value = locationEvaluator.GetValue(executionContext).ToObject();

					attributes[name] = DataModelValue.FromObject(value).AsConstant();
				}
			}

			if (!parameterList.IsDefaultOrEmpty)
			{
				foreach (var param in parameterList)
				{
					var name = param.Name;
					object? value = null;

					if (param.ExpressionEvaluator != null)
					{
						value = (await param.ExpressionEvaluator.EvaluateObject(executionContext, token).ConfigureAwait(false)).ToObject();
					}
					else if (param.LocationEvaluator != null)
					{
						value = param.LocationEvaluator.GetValue(executionContext).ToObject();
					}

					attributes[name] = DataModelValue.FromObject(value).AsConstant();
				}
			}

			return new DataModelValue(attributes);
		}
	}
}