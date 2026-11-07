using System.Text;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("level", false)]
[ThreadAgnostic]
[Preserve]
public class LevelLayoutRenderer : LayoutRenderer
{
	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		builder.Append(logEvent.Level.ToString());
	}
}
