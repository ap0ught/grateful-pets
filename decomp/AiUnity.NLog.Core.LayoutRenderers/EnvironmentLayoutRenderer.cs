using System;
using System.Text;
using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Layouts;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("environment", false)]
[Preserve]
public class EnvironmentLayoutRenderer : LayoutRenderer
{
	[RequiredParameter]
	[DefaultParameter]
	public string Variable { get; set; }

	public string Default { get; set; }

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		if (Variable != null)
		{
			string environmentVariable = Environment.GetEnvironmentVariable(Variable);
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				SimpleLayout simpleLayout = new SimpleLayout(environmentVariable);
				builder.Append(simpleLayout.Render(logEvent));
			}
			else if (Default != null)
			{
				SimpleLayout simpleLayout2 = new SimpleLayout(Default);
				builder.Append(simpleLayout2.Render(logEvent));
			}
		}
	}
}
