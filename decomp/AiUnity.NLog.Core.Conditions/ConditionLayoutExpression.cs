using AiUnity.NLog.Core.Layouts;

namespace AiUnity.NLog.Core.Conditions;

internal sealed class ConditionLayoutExpression : ConditionExpression
{
	public Layout Layout { get; private set; }

	public ConditionLayoutExpression(Layout layout)
	{
		Layout = layout;
	}

	public override string ToString()
	{
		return Layout.ToString();
	}

	protected override object EvaluateNode(LogEventInfo context)
	{
		return Layout.Render(context);
	}
}
