using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Conditions;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets.Wrappers;

[Target("FilteringWrapper", IsWrapper = true)]
[Preserve]
public class FilteringTargetWrapper : WrapperTargetBase
{
	private static readonly object boxedBooleanTrue = true;

	[RequiredParameter]
	[Display("Condition", "Condition must be met to forward Log events to wrapped target.", false, 0)]
	public ConditionExpression Condition { get; set; }

	public FilteringTargetWrapper()
	{
	}

	public FilteringTargetWrapper(Target wrappedTarget, ConditionExpression condition)
	{
		WrappedTarget = wrappedTarget;
		Condition = condition;
	}

	protected override void Write(AsyncLogEventInfo logEvent)
	{
		object obj = Condition.Evaluate(logEvent.LogEvent);
		if (boxedBooleanTrue.Equals(obj))
		{
			WrappedTarget.WriteAsyncLogEvent(logEvent);
		}
		else
		{
			logEvent.Continuation(null);
		}
	}
}
