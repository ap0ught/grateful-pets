using System;
using AiUnity.Common.Attributes;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets.Wrappers;

[Target("FallbackGroup", IsCompound = true, IsWrapper = true)]
[Preserve]
public class FallbackGroupTarget : CompoundTargetBase
{
	private int currentTarget;

	private object lockObject = new object();

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	[Display("Return start", "Return to first target after any successful write.", false, 0)]
	public bool ReturnToFirstOnSuccess { get; set; }

	public FallbackGroupTarget()
		: this(new Target[0])
	{
	}

	public FallbackGroupTarget(params Target[] targets)
		: base(targets)
	{
	}

	protected override void Write(AsyncLogEventInfo logEvent)
	{
		AsyncContinuation continuation = null;
		int tryCounter = 0;
		int targetToInvoke;
		continuation = (Exception ex) =>
		{
			if (ex == null)
			{
				lock (lockObject)
				{
					if (currentTarget != 0 && ReturnToFirstOnSuccess)
					{
						Logger.Debug("Fallback: target '{0}' succeeded. Returning to the first one.", Targets[currentTarget]);
						currentTarget = 0;
					}
				}
				logEvent.Continuation(null);
			}
			else
			{
				lock (lockObject)
				{
					Logger.Warn("Fallback: target '{0}' failed. Proceeding to the next one. Error was: {1}", Targets[currentTarget], ex);
					currentTarget = (currentTarget + 1) % Targets.Count;
					tryCounter++;
					targetToInvoke = currentTarget;
					if (tryCounter >= Targets.Count)
					{
						targetToInvoke = -1;
					}
				}
				if (targetToInvoke >= 0)
				{
					Targets[targetToInvoke].WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(continuation));
				}
				else
				{
					logEvent.Continuation(ex);
				}
			}
		};
		lock (lockObject)
		{
			targetToInvoke = currentTarget;
		}
		Targets[targetToInvoke].WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(continuation));
	}
}
