using System.ComponentModel;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers.Wrappers;

[LayoutRenderer("trim-whitespace", true)]
[AmbientProperty("TrimWhiteSpace")]
[ThreadAgnostic]
[Preserve]
public sealed class TrimWhiteSpaceLayoutRendererWrapper : WrapperLayoutRendererBase
{
	[DefaultValue(true)]
	public bool TrimWhiteSpace { get; set; }

	public TrimWhiteSpaceLayoutRendererWrapper()
	{
		TrimWhiteSpace = true;
	}

	protected override string Transform(string text)
	{
		if (!TrimWhiteSpace)
		{
			return text;
		}
		return text.Trim();
	}
}
