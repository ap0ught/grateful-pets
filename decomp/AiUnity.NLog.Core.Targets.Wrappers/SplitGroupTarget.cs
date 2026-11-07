using System;
using System.Collections.Generic;
using System.Threading;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets.Wrappers;

[Target("SplitGroup", IsCompound = true, IsWrapper = true)]
[Preserve]
public class SplitGroupTarget : CompoundTargetBase
{
	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	public SplitGroupTarget()
		: this(new Target[0])
	{
	}

	public SplitGroupTarget(params Target[] targets)
		: base(targets)
	{
	}

	protected override void Write(AsyncLogEventInfo logEvent)
	{
		AsyncHelpers.ForEachItemSequentially(Targets, logEvent.Continuation, (Target t, AsyncContinuation cont) =>
		{
			t.WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(cont));
		});
	}

	protected override void Write(AsyncLogEventInfo[] logEvents)
	{
		Logger.Trace("Writing {0} events", logEvents.Length);
		for (int i = 0; i < logEvents.Length; i++)
		{
			logEvents[i].Continuation = CountedWrap(logEvents[i].Continuation, Targets.Count);
		}
		foreach (Target target in Targets)
		{
			Logger.Trace("Sending {0} events to {1}", logEvents.Length, target);
			target.WriteAsyncLogEvents(logEvents);
		}
	}

	private static AsyncContinuation CountedWrap(AsyncContinuation originalContinuation, int counter)
	{
		if (counter == 1)
		{
			return originalContinuation;
		}
		List<Exception> exceptions = new List<Exception>();
		return (Exception ex) =>
		{
			if (ex != null)
			{
				lock (exceptions)
				{
					exceptions.Add(ex);
				}
			}
			int num = Interlocked.Decrement(ref counter);
			if (num == 0)
			{
				Exception combinedException = AsyncHelpers.GetCombinedException(exceptions);
				Logger.Trace("Combined exception: {0}", combinedException);
				originalContinuation(combinedException);
			}
			else
			{
				Logger.Trace("{0} remaining.", num);
			}
		};
	}
}
