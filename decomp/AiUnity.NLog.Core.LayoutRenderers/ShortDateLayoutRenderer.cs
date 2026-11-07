using System;
using System.ComponentModel;
using System.Text;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("shortdate", false)]
[ThreadAgnostic]
[Preserve]
public class ShortDateLayoutRenderer : LayoutRenderer
{
	[DefaultValue(false)]
	public bool UniversalTime { get; set; }

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		DateTime dateTime = logEvent.TimeStamp;
		if (UniversalTime)
		{
			dateTime = dateTime.ToUniversalTime();
		}
		builder.Append(dateTime.ToString("yyyy-MM-dd"));
	}
}
