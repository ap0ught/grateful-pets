namespace AiUnity.NLog.Core.Internal;

internal interface IRenderable
{
	string Render(LogEventInfo logEvent);
}
