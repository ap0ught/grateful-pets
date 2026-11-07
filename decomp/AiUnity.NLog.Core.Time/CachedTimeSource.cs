using System;

namespace AiUnity.NLog.Core.Time;

public abstract class CachedTimeSource : TimeSource
{
	private int lastTicks = -1;

	private DateTime lastTime = DateTime.MinValue;

	protected abstract DateTime FreshTime { get; }

	public override DateTime Time
	{
		get
		{
			int tickCount = Environment.TickCount;
			if (tickCount == lastTicks)
			{
				return lastTime;
			}
			DateTime freshTime = FreshTime;
			lastTicks = tickCount;
			lastTime = freshTime;
			return freshTime;
		}
	}
}
