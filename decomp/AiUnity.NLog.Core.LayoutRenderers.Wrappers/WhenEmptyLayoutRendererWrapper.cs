using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Layouts;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers.Wrappers;

[LayoutRenderer("whenEmpty", true)]
[AmbientProperty("WhenEmpty")]
[ThreadAgnostic]
[Preserve]
public sealed class WhenEmptyLayoutRendererWrapper : WrapperLayoutRendererBase
{
	[RequiredParameter]
	public Layout WhenEmpty { get; set; }

	protected override string Transform(string text)
	{
		return text;
	}

	protected override string RenderInner(LogEventInfo logEvent)
	{
		string text = base.RenderInner(logEvent);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return WhenEmpty.Render(logEvent);
	}
}
