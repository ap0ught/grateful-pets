using System;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialRealityStabilizationField : IPart
{
	public int FrameOffset;

	[NonSerialized]
	private static StringBuilder tileBuilder = new StringBuilder();

	[NonSerialized]
	private static string AltRender = "ú";

	public AnimatedMaterialRealityStabilizationField()
	{
		FrameOffset = Stat.RandomCosmetic(0, 2000);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool FinalRender(RenderEvent E)
	{
		if (!Visible() || !E.ColorsVisible)
		{
			return true;
		}
		if (E.Tile != null)
		{
			return true;
		}
		if (!Options.DisableImposters)
		{
			return true;
		}
		int num = (XRLCore.CurrentFrame10 + FrameOffset) % 2000;
		if (num >= 250 && num <= 750)
		{
			E.BackgroundString = "^k";
			E.DetailColor = "b";
		}
		else if (num >= 1250 && num <= 1750)
		{
			E.BackgroundString = "^k";
			E.DetailColor = "B";
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (!Options.DisableImposters)
		{
			return true;
		}
		E.Tile = "assets_content_textures_tiles_tile-dirt.png";
		int num = (XRLCore.CurrentFrame10 + FrameOffset) % 2000;
		if (num < 500)
		{
			E.RenderString = AltRender;
			if (E.ColorsVisible)
			{
				E.ColorString = "&y";
			}
		}
		else if (num < 1000)
		{
			E.RenderString = AltRender;
			if (E.ColorsVisible)
			{
				E.ColorString = "&K";
			}
		}
		else if (num < 1500)
		{
			E.RenderString = AltRender;
			if (E.ColorsVisible)
			{
				E.ColorString = "&K";
			}
		}
		else
		{
			E.RenderString = ParentObject.Render.RenderString;
			if (E.ColorsVisible)
			{
				E.ColorString = "&Y";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}
}
