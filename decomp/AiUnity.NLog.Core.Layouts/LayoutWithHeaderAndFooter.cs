using AiUnity.NLog.Core.Config;

namespace AiUnity.NLog.Core.Layouts;

[Layout("LayoutWithHeaderAndFooter")]
[ThreadAgnostic]
public class LayoutWithHeaderAndFooter : Layout
{
	public Layout Layout { get; set; }

	public Layout Header { get; set; }

	public Layout Footer { get; set; }

	protected override string GetFormattedMessage(LogEventInfo logEvent)
	{
		return Layout.Render(logEvent);
	}
}
