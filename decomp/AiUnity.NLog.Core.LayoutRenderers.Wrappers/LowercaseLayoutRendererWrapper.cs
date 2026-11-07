using System.ComponentModel;
using System.Globalization;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers.Wrappers;

[LayoutRenderer("lowercase", true)]
[AmbientProperty("Lowercase")]
[ThreadAgnostic]
[Preserve]
public sealed class LowercaseLayoutRendererWrapper : WrapperLayoutRendererBase
{
	[DefaultValue(true)]
	public bool Lowercase { get; set; }

	public CultureInfo Culture { get; set; }

	public LowercaseLayoutRendererWrapper()
	{
		Culture = CultureInfo.InvariantCulture;
		Lowercase = true;
	}

	protected override string Transform(string text)
	{
		if (!Lowercase)
		{
			return text;
		}
		return text.ToLower(Culture);
	}
}
