using System;
using System.ComponentModel;
using System.Text;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("longdate", false)]
[ThreadAgnostic]
[Preserve]
public class LongDateLayoutRenderer : LayoutRenderer
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
		builder.Append(dateTime.Year);
		builder.Append('-');
		Append2DigitsZeroPadded(builder, dateTime.Month);
		builder.Append('-');
		Append2DigitsZeroPadded(builder, dateTime.Day);
		builder.Append(' ');
		Append2DigitsZeroPadded(builder, dateTime.Hour);
		builder.Append(':');
		Append2DigitsZeroPadded(builder, dateTime.Minute);
		builder.Append(':');
		Append2DigitsZeroPadded(builder, dateTime.Second);
		builder.Append('.');
		Append4DigitsZeroPadded(builder, (int)(dateTime.Ticks % 10000000) / 1000);
	}

	private static void Append2DigitsZeroPadded(StringBuilder builder, int number)
	{
		builder.Append((char)(number / 10 + 48));
		builder.Append((char)(number % 10 + 48));
	}

	private static void Append4DigitsZeroPadded(StringBuilder builder, int number)
	{
		builder.Append((char)(number / 1000 % 10 + 48));
		builder.Append((char)(number / 100 % 10 + 48));
		builder.Append((char)(number / 10 % 10 + 48));
		builder.Append((char)(number / 1 % 10 + 48));
	}
}
