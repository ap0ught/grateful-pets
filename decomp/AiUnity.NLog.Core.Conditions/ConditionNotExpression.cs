namespace AiUnity.NLog.Core.Conditions;

internal sealed class ConditionNotExpression : ConditionExpression
{
	public ConditionExpression Expression { get; private set; }

	public ConditionNotExpression(ConditionExpression expression)
	{
		Expression = expression;
	}

	public override string ToString()
	{
		return "(not " + Expression?.ToString() + ")";
	}

	protected override object EvaluateNode(LogEventInfo context)
	{
		return !(bool)Expression.Evaluate(context);
	}
}
