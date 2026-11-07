using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Conditions;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Filters;

[Filter("when")]
[Preserve]
public class ConditionBasedFilter : Filter
{
	private static readonly object boxedTrue = true;

	[RequiredParameter]
	public ConditionExpression Condition { get; set; }

	protected override FilterResult Check(LogEventInfo logEvent)
	{
		object obj = Condition.Evaluate(logEvent);
		if (boxedTrue.Equals(obj))
		{
			return Action;
		}
		return FilterResult.Neutral;
	}
}
