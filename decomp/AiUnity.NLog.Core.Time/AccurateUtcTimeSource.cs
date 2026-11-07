using System;

namespace AiUnity.NLog.Core.Time;

[TimeSource("AccurateUTC")]
public class AccurateUtcTimeSource : TimeSource
{
	public override DateTime Time => DateTime.UtcNow;
}
