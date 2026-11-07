namespace AiUnity.NLog.Core.Conditions;

internal sealed class ConditionMessageExpression : ConditionExpression
{
	public override string ToString()
	{
		return "message";
	}

	protected override object EvaluateNode(LogEventInfo context)
	{
		return context.FormattedMessage;
	}
}
