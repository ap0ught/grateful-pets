using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Layouts;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[NLogConfigurationItem]
[Preserve]
public class NLogViewerParameterInfo
{
	[RequiredParameter]
	public string Name { get; set; }

	[RequiredParameter]
	public Layout Layout { get; set; }
}
