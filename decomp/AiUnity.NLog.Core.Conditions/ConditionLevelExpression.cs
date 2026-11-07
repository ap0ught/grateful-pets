namespace AiUnity.NLog.Core.Conditions;

internal sealed class ConditionLevelExpression : ConditionExpression
{
	public override string ToString()
	{
		return "level";
	}

	protected override object EvaluateNode(LogEventInfo context)
	{
		return context.Level;
	}
}
