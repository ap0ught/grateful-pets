using System.ComponentModel;
using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Common;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets.Wrappers;

[Target("RepeatingWrapper", IsWrapper = true)]
[Preserve]
public class RepeatingTargetWrapper : WrapperTargetBase
{
	[DefaultValue(3)]
	[Display("Repeat Count", "Number of times to call target for a given log message.", false, 0)]
	public int RepeatCount { get; set; }

	public RepeatingTargetWrapper()
		: this(null, 3)
	{
	}

	public RepeatingTargetWrapper(Target wrappedTarget, int repeatCount)
	{
		WrappedTarget = wrappedTarget;
		RepeatCount = repeatCount;
	}

	protected override void Write(AsyncLogEventInfo logEvent)
	{
		AsyncHelpers.Repeat(RepeatCount, logEvent.Continuation, (AsyncContinuation cont) =>
		{
			WrappedTarget.WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(cont));
		});
	}
}
