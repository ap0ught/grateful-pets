using System.ComponentModel;
using System.Text;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("logger", false)]
[ThreadAgnostic]
[Preserve]
public class LoggerNameLayoutRenderer : LayoutRenderer
{
	[DefaultValue(false)]
	public bool ShortName { get; set; }

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		if (ShortName)
		{
			int num = logEvent.LoggerName.LastIndexOf('.');
			if (num < 0)
			{
				builder.Append(logEvent.LoggerName);
			}
			else
			{
				builder.Append(logEvent.LoggerName.Substring(num + 1));
			}
		}
		else
		{
			builder.Append(logEvent.LoggerName);
		}
	}
}
