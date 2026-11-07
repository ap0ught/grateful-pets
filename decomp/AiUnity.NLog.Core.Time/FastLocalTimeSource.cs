using System;

namespace AiUnity.NLog.Core.Time;

[TimeSource("FastLocal")]
public class FastLocalTimeSource : CachedTimeSource
{
	protected override DateTime FreshTime => DateTime.Now;
}
