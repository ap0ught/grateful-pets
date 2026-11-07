using System;

namespace AiUnity.NLog.Core.Time;

[TimeSource("AccurateLocal")]
public class AccurateLocalTimeSource : TimeSource
{
	public override DateTime Time => DateTime.Now;
}
