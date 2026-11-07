using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Filters;

[NLogConfigurationItem]
[Preserve]
public abstract class Filter
{
	[RequiredParameter]
	public FilterResult Action { get; set; }

	protected Filter()
	{
		Action = FilterResult.Neutral;
	}

	internal FilterResult GetFilterResult(LogEventInfo logEvent)
	{
		return Check(logEvent);
	}

	protected abstract FilterResult Check(LogEventInfo logEvent);
}
