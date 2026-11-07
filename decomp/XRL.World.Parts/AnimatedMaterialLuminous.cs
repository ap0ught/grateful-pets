using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialLuminous : IPart
{
	public static readonly int ICON_COLOR_PRIORITY = 30;

	public int FrameOffset;

	public AnimatedMaterialLuminous()
	{
		FrameOffset = Stat.RandomCosmetic(0, 60);
	}

	public override bool Render(RenderEvent E)
	{
		if (E.ColorsVisible)
		{
			if ((XRLCore.CurrentFrame + FrameOffset) % 60 % 3 == 0)
			{
				E.ApplyColors("&C", ICON_COLOR_PRIORITY);
			}
			else
			{
				E.ApplyColors("&Y", ICON_COLOR_PRIORITY);
			}
		}
		return true;
	}
}
