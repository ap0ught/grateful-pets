using System.Text;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Layouts;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers.Wrappers;

public abstract class WrapperLayoutRendererBase : LayoutRenderer
{
	[DefaultParameter]
	[Preserve]
	public Layout Inner { get; set; }

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		string text = RenderInner(logEvent);
		builder.Append(Transform(text));
	}

	protected abstract string Transform(string text);

	protected virtual string RenderInner(LogEventInfo logEvent)
	{
		return Inner.Render(logEvent);
	}
}
