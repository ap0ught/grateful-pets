using System;
using System.Text;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("newline", false)]
[Preserve]
public class NewLineLayoutRenderer : LayoutRenderer
{
	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		builder.Append(Environment.NewLine);
	}
}
