using System;
using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Common;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets.Wrappers;

[Preserve]
public abstract class WrapperTargetBase : Target
{
	[RequiredParameter]
	public Target WrappedTarget { get; set; }

	public override string ToString()
	{
		return base.ToString() + "(" + WrappedTarget?.ToString() + ")";
	}

	protected override void FlushAsync(AsyncContinuation asyncContinuation)
	{
		WrappedTarget.Flush(asyncContinuation);
	}

	protected sealed override void Write(LogEventInfo logEvent)
	{
		throw new NotSupportedException("This target must not be invoked in a synchronous way.");
	}
}
