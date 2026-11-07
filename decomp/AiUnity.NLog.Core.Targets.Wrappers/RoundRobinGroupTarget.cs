using AiUnity.NLog.Core.Common;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets.Wrappers;

[Target("RoundRobinGroup", IsCompound = true, IsWrapper = true)]
[Preserve]
public class RoundRobinGroupTarget : CompoundTargetBase
{
	private int currentTarget;

	private object lockObject = new object();

	public RoundRobinGroupTarget()
		: this(new Target[0])
	{
	}

	public RoundRobinGroupTarget(params Target[] targets)
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
		lock (lockObject)
		{
			index = currentTarget;
			currentTarget = (currentTarget + 1) % Targets.Count;
		}
		Targets[index].WriteAsyncLogEvent(logEvent);
	}
}
