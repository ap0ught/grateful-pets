using System;

namespace AiUnity.NLog.Core.Time;

[TimeSource("FastUTC")]
public class FastUtcTimeSource : CachedTimeSource
{
	protected override DateTime FreshTime => DateTime.UtcNow;
}
