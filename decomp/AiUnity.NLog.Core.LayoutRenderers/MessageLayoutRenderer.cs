using System;
using System.Text;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("message", false)]
[ThreadAgnostic]
[Preserve]
public class MessageLayoutRenderer : LayoutRenderer
{
	public bool WithException { get; set; }

	public string ExceptionSeparator { get; set; }

	public MessageLayoutRenderer()
	{
		ExceptionSeparator = Environment.NewLine;
	}

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		builder.Append(logEvent.FormattedMessage);
		if (WithException && logEvent.Exception != null)
		{
			builder.Append(ExceptionSeparator);
			builder.Append(logEvent.Exception.ToString());
		}
	}
}
