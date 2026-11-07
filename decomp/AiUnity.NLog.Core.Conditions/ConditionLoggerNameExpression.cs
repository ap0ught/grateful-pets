namespace AiUnity.NLog.Core.Conditions;

internal sealed class ConditionLoggerNameExpression : ConditionExpression
{
	public override string ToString()
	{
		return "logger";
	}

	protected override object EvaluateNode(LogEventInfo context)
	{
		return context.LoggerName;
	}
}
