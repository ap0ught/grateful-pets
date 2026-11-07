using AiUnity.NLog.Core.Common;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[Target("Null")]
[Preserve]
public sealed class NullTarget : Target
{
	protected override void Write(LogEventInfo logEvent)
	{
	}
}
