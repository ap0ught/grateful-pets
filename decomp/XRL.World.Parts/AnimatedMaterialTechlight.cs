using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialTechlight : IPart
{
	public int nFrameOffset;

	public int FrameOffset;

	public int FlickerFrame;

	public string baseColor = "c";

	public AnimatedMaterialTechlight()
	{
		nFrameOffset = Stat.RandomCosmetic(0, 60);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool Render(RenderEvent E)
	{
		Render render = ParentObject.Render;
		int num = (XRLCore.CurrentFrame + FrameOffset) % 500;
		render.TileColor = baseColor;
		if (Stat.Random(1, 200) == 1 || FlickerFrame > 0)
		{
			render.ColorString = "&c";
			render.DetailColor = "c";
			if (FlickerFrame == 0)
			{
				FlickerFrame = 3;
			}
			FlickerFrame--;
		}
		if (num < 4)
		{
			render.ColorString = "&C";
			render.DetailColor = "C";
		}
		else if (num < 8)
		{
			render.ColorString = "&B";
			render.DetailColor = "B";
		}
		else if (num < 12)
		{
			render.ColorString = "&b";
			render.DetailColor = "b";
		}
		else
		{
			render.ColorString = "&Y";
			render.DetailColor = "Y";
		}
		if (!Options.DisableTextAnimationEffects)
		{
			FrameOffset += Stat.Random(0, 20);
		}
		if (Stat.Random(1, 400) == 1 || FlickerFrame > 0)
		{
			render.ColorString = "&b";
			render.DetailColor = "b";
		}
		return true;
	}
}
