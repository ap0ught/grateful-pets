using System;
using System.Text;
using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("event-properties", false)]
[Preserve]
public class EventPropertiesLayoutRenderer : LayoutRenderer
{
	[RequiredParameter]
	[DefaultParameter]
	public string Item { get; set; }

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		if (logEvent.Properties.TryGetValue(Item, out var value))
		{
			builder.Append(Convert.ToString(value));
		}
	}
}
