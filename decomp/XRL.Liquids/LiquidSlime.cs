using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
public class LiquidSlime : BaseLiquid
{
	public new const string ID = "slime";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "g", "w" };

	public LiquidSlime()
		: base("slime")
	{
		FlameTemperature = 550;
		VaporTemperature = 1550;
		Combustibility = 8;
		ThermalConductivity = 35;
		Fluidity = 10;
		Evaporativity = 1;
		Staining = 1;
		ConversionProduct = "gel";
		SlipperyWhenWet = true;
		SlipperyWhenFrozen = true;
		SlipperySaveTargetBase = 5;
		SlipperySaveTargetScale = 0.3;
		SlipperySaveVs = "Slime Slip Move";
		SlipperyMessage = "{{slimy|=subject.T= =verb:slip= on the slime!}}";
		SlipperyParticle = "&g\u001a";
		CirculatoryLossTerm = "oozing";
		CirculatoryLossNoun = "ooze";
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "g";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{slimy|slime}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{slimy|slimy}}";
	}

	public override string GetWaterRitualName()
	{
		return "slime";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{slimy|slimy}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{slimy|slimy}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{g|slime}}";
	}

	public override string GetPreparedCookingIngredient()
	{
		return "slimeSpitting";
	}

	public override float GetValuePerDram()
	{
		return 0.1f;
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Message.Compound("It's disgustingly slimy!");
		if (Target.ApplyEffect(new Confused(Stat.Roll("3d6"), 5, 7)))
		{
			ExitInterface = true;
		}
		return true;
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				eRender.ColorString = "&g";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^g" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&w^g";
		Liquid.ParentObject.Render.TileColor = "&w";
		Liquid.ParentObject.Render.DetailColor = "g";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&g";
	}

	public override void RenderPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (!Liquid.IsWadingDepth() || Liquid == null || Liquid.ParentObject == null)
		{
			return;
		}
		if (Liquid.ParentObject.IsFrozen())
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&g^w", "&g", "w");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&g^w", "&g", "w");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			if (num < 15)
			{
				render.RenderString = "÷";
				render.ColorString = "&w^g";
				render.TileColor = "&w";
				render.DetailColor = "g";
			}
			else if (num < 30)
			{
				render.RenderString = "~";
				render.ColorString = "&g^w";
				render.TileColor = "&g";
				render.DetailColor = "w";
			}
			else if (num < 45)
			{
				render.RenderString = " ";
				render.ColorString = "&w^g";
				render.TileColor = "&w";
				render.DetailColor = "g";
			}
			else
			{
				render.RenderString = "~";
				render.ColorString = "&w^g";
				render.TileColor = "&w";
				render.DetailColor = "g";
			}
		}
	}

	public override void RenderSecondary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString += "&g";
		}
	}

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "Liquids/Splotchy/";
		}
		return base.GetPaintAtlas(Liquid);
	}
}
