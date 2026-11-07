using AiUnity.NLog.Core.LayoutRenderers;

namespace AiUnity.NLog.Core.Layouts;

[Layout("Log4JXmlEventLayout")]
public class Log4JXmlEventLayout : Layout
{
	public Log4JXmlEventLayoutRenderer Renderer { get; private set; }

	public Log4JXmlEventLayout()
	{
		Renderer = new Log4JXmlEventLayoutRenderer();
	}

	protected override string GetFormattedMessage(LogEventInfo logEvent)
	{
		if (logEvent.TryGetCachedLayoutValue(this, out var value))
		{
			return value;
		}
		return logEvent.AddCachedLayoutValue(this, Renderer.Render(logEvent));
	}
}
