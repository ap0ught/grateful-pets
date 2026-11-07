using System.Text;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("literal", false)]
[ThreadAgnostic]
[AppDomainFixedOutput]
[Preserve]
public class LiteralLayoutRenderer : LayoutRenderer
{
	public string Text { get; set; }

	public LiteralLayoutRenderer()
	{
	}

	public LiteralLayoutRenderer(string text)
	{
		Text = text;
	}

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		builder.Append(Text);
	}
}
