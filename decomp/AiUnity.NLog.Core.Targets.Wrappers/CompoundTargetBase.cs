using System;
using System.Collections.Generic;
using System.Text;
using AiUnity.NLog.Core.Common;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets.Wrappers;

[Preserve]
public abstract class CompoundTargetBase : Target
{
	public IList<Target> Targets { get; private set; }

	protected CompoundTargetBase(params Target[] targets)
	{
		Targets = new List<Target>(targets);
	}

	public override string ToString()
	{
		string value = string.Empty;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.ToString());
		stringBuilder.Append("(");
		foreach (Target target in Targets)
		{
			stringBuilder.Append(value);
			stringBuilder.Append(target.ToString());
			value = ", ";
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	protected override void Write(LogEventInfo logEvent)
	{
		throw new NotSupportedException("This target must not be invoked in a synchronous way.");
	}

	protected override void FlushAsync(AsyncContinuation asyncContinuation)
	{
		AsyncHelpers.ForEachItemInParallel(Targets, asyncContinuation, (Target t, AsyncContinuation c) =>
		{
			t.Flush(c);
		});
	}
}
