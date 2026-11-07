using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialHighlyEntropic : IPart
{
	public string ColorString;

	public string TileColor;

	public string DetailColor;

	public int FrameOffset;

	public override bool Render(RenderEvent E)
	{
		Render render = ParentObject.Render;
		if (ColorString == null)
		{
			ColorString = ParentObject.Render.ColorString;
		}
		if (TileColor == null)
		{
			TileColor = ParentObject.Render.TileColor;
		}
		if (DetailColor == null)
		{
			DetailColor = ParentObject.Render.DetailColor;
		}
		int num = (XRLCore.CurrentFrame + FrameOffset) % 150;
		if (num < 3)
		{
			render.TileColor = "&k";
			render.ColorString = "&k";
		}
		else if (num < 6)
		{
			render.DetailColor = "K";
		}
		else if (num < 9)
		{
			render.TileColor = "&m";
			render.ColorString = "&m";
		}
		else if (num < 12)
		{
			render.DetailColor = "m";
		}
		else if (num < 15)
		{
			render.TileColor = "&K";
			render.ColorString = "&K";
			render.DetailColor = "y";
		}
		else if (num < 18)
		{
			render.TileColor = "&m";
			render.ColorString = "&m";
			render.DetailColor = "K";
		}
		else
		{
			render.ColorString = ColorString;
			render.DetailColor = DetailColor;
			render.TileColor = TileColor;
		}
		if (!Options.DisableTextAnimationEffects)
		{
			FrameOffset += (50.in100() ? 1 : Stat.Random(1, 3));
		}
		if (2.in100() && !Options.DisableTextAnimationEffects)
		{
			FrameOffset += Stat.RandomCosmetic(0, 100);
		}
		return true;
	}
}
