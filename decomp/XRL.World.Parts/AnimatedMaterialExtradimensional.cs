using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialExtradimensional : IPart
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
		int num = (XRLCore.CurrentFrame + FrameOffset) % 200;
		if (num < 2)
		{
			render.TileColor = "&o";
			render.ColorString = "&o";
			render.DetailColor = "o";
		}
		else if (num < 4)
		{
			render.TileColor = "&O";
			render.ColorString = "&O";
			render.DetailColor = "o";
		}
		else if (num < 6)
		{
			render.TileColor = "&O";
			render.ColorString = "&O";
			render.DetailColor = "O";
		}
		else if (num < 8)
		{
			render.TileColor = "&k";
			render.ColorString = "&k";
			render.DetailColor = "k";
		}
		else if (num < 10)
		{
			render.TileColor = "&o";
			render.ColorString = "&o";
			render.DetailColor = "O";
		}
		else
		{
			render.ColorString = ColorString;
			render.DetailColor = DetailColor;
			render.TileColor = TileColor;
		}
		if (!Options.DisableTextAnimationEffects)
		{
			FrameOffset += Stat.Random(1, 3);
		}
		if (2.in100() && !Options.DisableTextAnimationEffects)
		{
			FrameOffset += Stat.RandomCosmetic(0, 100);
		}
		return true;
	}
}
