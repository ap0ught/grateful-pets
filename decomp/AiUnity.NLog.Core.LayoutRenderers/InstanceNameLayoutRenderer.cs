using System.Text;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("instancename", false)]
[AppDomainFixedOutput]
[ThreadAgnostic]
[Preserve]
public class InstanceNameLayoutRenderer : LayoutRenderer
{
	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	private static string InstanceName { get; set; }

	protected override void InitializeLayoutRenderer()
	{
		base.InitializeLayoutRenderer();
		if (InstanceName == null)
		{
			InstanceName = "unset";
		}
	}

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		builder.Append(InstanceName);
	}
}
