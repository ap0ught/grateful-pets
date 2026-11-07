using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialOverlandWater : IPart
{
	public int nFrameOffset;

	public bool Rushing;

	public bool Fresh;

	public bool Acid;

	public bool Bloody;

	public AnimatedMaterialOverlandWater()
	{
		nFrameOffset = Stat.RandomCosmetic(0, 60);
		Rushing = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool Render(RenderEvent E)
	{
		if (Globals.RenderMode == RenderModeType.Tiles)
		{
			return true;
		}
		if (ParentObject.Physics.IsFreezing())
		{
			E.RenderString = "~";
			E.TileVariantColors("&c^K", "&c", "K");
		}
		else
		{
			long num = (XRLCore.FrameTimer.ElapsedMilliseconds + nFrameOffset * 100) % 20000;
			if (Stat.RandomCosmetic(1, 60) == 1)
			{
				if (num < 5000)
				{
					ParentObject.Render.RenderString = "÷";
				}
				else if (num < 10000)
				{
					ParentObject.Render.RenderString = "~";
				}
				else if (num < 15000)
				{
					ParentObject.Render.RenderString = "÷";
				}
				else
				{
					ParentObject.Render.RenderString = "~";
				}
			}
			if (Stat.RandomCosmetic(1, 600) == 1)
			{
				E.RenderString = "~";
				E.TileVariantColors("&Y^k", "&Y", "k");
			}
		}
		return true;
	}
}
