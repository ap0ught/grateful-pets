using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Conditions;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers.Wrappers;

[LayoutRenderer("when", true)]
[AmbientProperty("When")]
[ThreadAgnostic]
[Preserve]
public sealed class WhenLayoutRendererWrapper : WrapperLayoutRendererBase
{
	[RequiredParameter]
	public ConditionExpression When { get; set; }

	protected override string Transform(string text)
	{
		return text;
	}

	protected override string RenderInner(LogEventInfo logEvent)
	{
		if (true.Equals(When.Evaluate(logEvent)))
		{
			return base.RenderInner(logEvent);
		}
		return string.Empty;
	}
}
