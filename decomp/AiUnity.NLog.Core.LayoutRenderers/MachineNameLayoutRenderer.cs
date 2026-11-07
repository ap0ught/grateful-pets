using System;
using System.Text;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Internal;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("machinename", false)]
[AppDomainFixedOutput]
[ThreadAgnostic]
[Preserve]
public class MachineNameLayoutRenderer : LayoutRenderer
{
	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	internal string MachineName { get; private set; }

	protected override void InitializeLayoutRenderer()
	{
		base.InitializeLayoutRenderer();
		try
		{
			MachineName = Environment.MachineName;
		}
		catch (Exception ex)
		{
			if (ex.MustBeRethrown())
			{
				throw;
			}
			Logger.Error("Error getting machine name {0}", ex);
			MachineName = string.Empty;
		}
	}

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		builder.Append(MachineName);
	}
}
