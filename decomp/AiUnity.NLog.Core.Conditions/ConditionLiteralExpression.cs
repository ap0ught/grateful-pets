using System;

namespace AiUnity.NLog.Core.Conditions;

internal sealed class ConditionLiteralExpression : ConditionExpression
{
	public object LiteralValue { get; private set; }

	public ConditionLiteralExpression(object literalValue)
	{
		LiteralValue = literalValue;
	}

	public override string ToString()
	{
		if (LiteralValue == null)
		{
			return "null";
		}
		return Convert.ToString(LiteralValue);
	}

	protected override object EvaluateNode(LogEventInfo context)
	{
		return LiteralValue;
	}
}
