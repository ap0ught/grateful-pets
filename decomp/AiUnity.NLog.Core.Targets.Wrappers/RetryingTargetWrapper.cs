using System;
using System.ComponentModel;
using System.Threading;
using AiUnity.Common.Attributes;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets.Wrappers;

[Target("RetryingWrapper", IsWrapper = true)]
[Preserve]
public class RetryingTargetWrapper : WrapperTargetBase
{
	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	[DefaultValue(3)]
	[Display("Retry Count", "Number of retry attempts on wrapped target.", false, 0)]
	public int RetryCount { get; set; }

	[DefaultValue(100)]
	[Display("Retry Count", "Time to wait between retires in milliseconds.", false, 0)]
	public int RetryDelayMilliseconds { get; set; }

	public RetryingTargetWrapper()
		: this(null, 3, 100)
	{
	}

	public RetryingTargetWrapper(Target wrappedTarget, int retryCount, int retryDelayMilliseconds)
	{
		WrappedTarget = wrappedTarget;
		RetryCount = retryCount;
		RetryDelayMilliseconds = retryDelayMilliseconds;
	}

	protected override void Write(AsyncLogEventInfo logEvent)
	{
		AsyncContinuation continuation = null;
		int counter = 0;
		continuation = (Exception ex) =>
		{
			if (ex == null)
			{
				logEvent.Continuation(null);
			}
			else
			{
				int num = Interlocked.Increment(ref counter);
				Logger.Warn("Error while writing to '{0}': {1}. Try {2}/{3}", WrappedTarget, ex, num, RetryCount);
				if (num >= RetryCount)
				{
					Logger.Warn("Too many retries. Aborting.");
					logEvent.Continuation(ex);
				}
				else
				{
					Thread.Sleep(RetryDelayMilliseconds);
					WrappedTarget.WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(continuation));
				}
			}
		};
		WrappedTarget.WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(continuation));
	}
}
