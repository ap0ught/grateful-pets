using System.Collections.Generic;
using AiUnity.NLog.Core.Common;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[Target("Memory")]
[Preserve]
public sealed class MemoryTarget : TargetWithLayout
{
	public IList<string> Logs { get; private set; }

	public MemoryTarget()
	{
		Logs = new List<string>();
	}

	protected override void Write(LogEventInfo logEvent)
	{
		string item = Layout.Render(logEvent);
		Logs.Add(item);
	}
}
