using System.ComponentModel;
using System.Globalization;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers.Wrappers;

[LayoutRenderer("uppercase", true)]
[AmbientProperty("Uppercase")]
[ThreadAgnostic]
[Preserve]
public sealed class UppercaseLayoutRendererWrapper : WrapperLayoutRendererBase
{
	[DefaultValue(true)]
	public bool Uppercase { get; set; }

	public CultureInfo Culture { get; set; }

	public UppercaseLayoutRendererWrapper()
	{
		Culture = CultureInfo.InvariantCulture;
		Uppercase = true;
	}

	protected override string Transform(string text)
	{
		if (!Uppercase)
		{
			return text;
		}
		return text.ToUpper(Culture);
	}
}
