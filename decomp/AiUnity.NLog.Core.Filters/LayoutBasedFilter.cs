using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Layouts;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Filters;

[Preserve]
public abstract class LayoutBasedFilter : Filter
{
	[RequiredParameter]
	public Layout Layout { get; set; }
}
