using System;
using AiUnity.NLog.Core.Common;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets.Wrappers;

[Target("RandomizeGroup", IsCompound = true, IsWrapper = true)]
[Preserve]
public class RandomizeGroupTarget : CompoundTargetBase
{
	private readonly Random random = new Random();

	public RandomizeGroupTarget()
		: this(new Target[0])
	{
	}

	public RandomizeGroupTarget(params Target[] targets)
		: base(targets)
	{
	}

	protected override void Write(AsyncLogEventInfo logEvent)
	{
		if (Targets.Count == 0)
		{
			logEvent.Continuation(null);
			return;
		}
		int index;
		lock (random)
		{
			index = random.Next(Targets.Count);
		}
		Targets[index].WriteAsyncLogEvent(logEvent);
	}
}
