using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("date", false)]
[ThreadAgnostic]
[Preserve]
public class DateLayoutRenderer : LayoutRenderer
{
	public CultureInfo Culture { get; set; }

	[DefaultParameter]
	public string Format { get; set; }

	[DefaultValue(false)]
	public bool UniversalTime { get; set; }

	public DateLayoutRenderer()
	{
		Format = "yyyy/MM/dd HH:mm:ss.fff";
		Culture = CultureInfo.InvariantCulture;
	}

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		DateTime dateTime = logEvent.TimeStamp;
		if (UniversalTime)
		{
			dateTime = dateTime.ToUniversalTime();
		}
		builder.Append(dateTime.ToString(Format, Culture));
	}
}
