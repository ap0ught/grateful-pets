using System;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Internal;

namespace AiUnity.NLog.Core.Conditions;

[NLogConfigurationItem]
[ThreadAgnostic]
public abstract class ConditionExpression
{
	public static implicit operator ConditionExpression(string conditionExpressionText)
	{
		return ConditionParser.ParseExpression(conditionExpressionText);
	}

	public object Evaluate(LogEventInfo context)
	{
		try
		{
			return EvaluateNode(context);
		}
		catch (Exception ex)
		{
			if (ex.MustBeRethrown())
			{
				throw;
			}
			throw new ConditionEvaluationException("Exception occurred when evaluating condition", ex);
		}
	}

	public abstract override string ToString();

	protected abstract object EvaluateNode(LogEventInfo context);
}
