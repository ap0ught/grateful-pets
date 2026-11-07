namespace AiUnity.NLog.Core.Conditions;

internal sealed class ConditionAndExpression : ConditionExpression
{
	private static readonly object boxedFalse = false;

	private static readonly object boxedTrue = true;

	public ConditionExpression Left { get; private set; }

	public ConditionExpression Right { get; private set; }

	public ConditionAndExpression(ConditionExpression left, ConditionExpression right)
	{
		Left = left;
		Right = right;
	}

	public override string ToString()
	{
		return "(" + Left?.ToString() + " and " + Right?.ToString() + ")";
	}

	protected override object EvaluateNode(LogEventInfo context)
	{
		if (!(bool)Left.Evaluate(context))
		{
			return boxedFalse;
		}
		if (!(bool)Right.Evaluate(context))
		{
			return boxedFalse;
		}
		return boxedTrue;
	}
}
