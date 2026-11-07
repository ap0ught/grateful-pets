using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialFire : IPart
{
	public static readonly int ICON_COLOR_PRIORITY = 30;

	public int FrameOffset;

	public AnimatedMaterialFire()
	{
		FrameOffset = Stat.RandomCosmetic(0, 60);
	}

	public override bool Render(RenderEvent E)
	{
		if (E.ColorsVisible)
		{
			int num = (XRLCore.CurrentFrame + FrameOffset) % 60;
			string text = null;
			if (!Options.DisableTextAnimationEffects)
			{
				FrameOffset += Stat.RandomCosmetic(1, 5);
			}
			if (num < 15)
			{
				text = "&R";
			}
			else if (num < 30)
			{
				text = "&W";
			}
			else
			{
				text = ((num >= 45) ? "&W" : "&r");
			}
			E.ApplyColors(text, ICON_COLOR_PRIORITY);
		}
		return base.Render(E);
	}
}
