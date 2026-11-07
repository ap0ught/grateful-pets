namespace AiUnity.NLog.Core.Conditions;

internal sealed class ConditionOrExpression : ConditionExpression
{
	private static readonly object boxedFalse = false;

	private static readonly object boxedTrue = true;

	public ConditionExpression LeftExpression { get; private set; }

	public ConditionExpression RightExpression { get; private set; }

	public ConditionOrExpression(ConditionExpression left, ConditionExpression right)
	{
		LeftExpression = left;
		RightExpression = right;
	}

	public override string ToString()
	{
		return "(" + LeftExpression?.ToString() + " or " + RightExpression?.ToString() + ")";
	}

	protected override object EvaluateNode(LogEventInfo context)
	{
		if ((bool)LeftExpression.Evaluate(context))
		{
			return boxedTrue;
		}
		if ((bool)RightExpression.Evaluate(context))
		{
			return boxedTrue;
		}
		return boxedFalse;
	}
}
