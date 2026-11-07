using System;
using AiUnity.NLog.Core.Config;

namespace AiUnity.NLog.Core.Time;

[NLogConfigurationItem]
public abstract class TimeSource
{
	private static TimeSource currentSource = new FastLocalTimeSource();

	public abstract DateTime Time { get; }

	public static TimeSource Current
	{
		get
		{
			return currentSource;
		}
		set
		{
			currentSource = value;
		}
	}

	public override string ToString()
	{
		TimeSourceAttribute timeSourceAttribute = (TimeSourceAttribute)Attribute.GetCustomAttribute(GetType(), typeof(TimeSourceAttribute));
		if (timeSourceAttribute != null)
		{
			return timeSourceAttribute.DisplayName + " (time source)";
		}
		return GetType().Name;
	}
}
