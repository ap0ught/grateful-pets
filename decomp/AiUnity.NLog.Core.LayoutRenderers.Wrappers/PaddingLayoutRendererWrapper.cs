using System.ComponentModel;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers.Wrappers;

[LayoutRenderer("pad", true)]
[AmbientProperty("Padding")]
[AmbientProperty("PadCharacter")]
[AmbientProperty("FixedLength")]
[ThreadAgnostic]
[Preserve]
public sealed class PaddingLayoutRendererWrapper : WrapperLayoutRendererBase
{
	public int Padding { get; set; }

	[DefaultValue(' ')]
	public char PadCharacter { get; set; }

	[DefaultValue(false)]
	public bool FixedLength { get; set; }

	public PaddingLayoutRendererWrapper()
	{
		PadCharacter = ' ';
	}

	protected override string Transform(string text)
	{
		string text2 = text ?? string.Empty;
		if (Padding != 0)
		{
			text2 = ((Padding <= 0) ? text2.PadRight(-Padding, PadCharacter) : text2.PadLeft(Padding, PadCharacter));
			int num = Padding;
			if (num < 0)
			{
				num = -num;
			}
			if (FixedLength && text2.Length > num)
			{
				text2 = text2.Substring(0, num);
			}
		}
		return text2;
	}
}
