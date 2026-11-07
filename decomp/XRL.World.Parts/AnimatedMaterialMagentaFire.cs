using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialMagentaFire : IPart
{
	public static readonly int ICON_COLOR_PRIORITY = 30;

	public int FrameOffset;

	public AnimatedMaterialMagentaFire()
	{
		FrameOffset = Stat.RandomCosmetic(0, 60);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool Render(RenderEvent E)
	{
		if (E.ColorsVisible)
		{
			int num = (XRLCore.CurrentFrame + FrameOffset) % 60;
			if (!Options.DisableTextAnimationEffects)
			{
				FrameOffset += Stat.RandomCosmetic(1, 5);
			}
			string text = null;
			string text2 = null;
			if (num < 15)
			{
				text2 = "m";
				text = "&m";
			}
			else if (num < 30)
			{
				text2 = "Y";
				text = "&Y";
			}
			else if (num < 45)
			{
				text2 = "M";
				text = "&M";
			}
			else
			{
				text2 = "m";
				text = "&m";
			}
			if (Options.UseTiles)
			{
				E.ApplyDetailColor(text2, ICON_COLOR_PRIORITY);
			}
			else
			{
				E.ApplyColors(text, ICON_COLOR_PRIORITY);
			}
		}
		return true;
	}
}
