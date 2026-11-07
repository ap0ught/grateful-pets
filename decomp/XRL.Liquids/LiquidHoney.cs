using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
public class LiquidHoney : BaseLiquid
{
	public new const string ID = "honey";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "w", "W" };

	public LiquidHoney()
		: base("honey")
	{
		FlameTemperature = 300;
		VaporTemperature = 1300;
		Combustibility = 60;
		Adsorbence = 25;
		PureElectricalConductivity = 40;
		MixedElectricalConductivity = 40;
		ThermalConductivity = 40;
		Fluidity = 10;
		Evaporativity = 1;
		Weight = 0.5;
		InterruptAutowalk = true;
		StickyWhenWet = true;
		StickySaveTargetBase = 1;
		StickySaveTargetScale = 0.1;
		StickyDuration = 12;
		StickySaveVs = "Honey Stuck Restraint";
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "w";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{w|honey}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{w|honeyed}}";
	}

	public override string GetWaterRitualName()
	{
		return "honey";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{w|sticky}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{w|sticky}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{w|honey}}";
	}

	public override string GetPreparedCookingIngredient()
	{
		return "medicinalMinor";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Target.HasPart<Stomach>())
		{
			Target.FireEvent(Event.New("AddFood", "Satiation", "Snack"));
			Message.Compound("Delicious!");
		}
		return true;
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^w" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&w^W";
		Liquid.ParentObject.Render.TileColor = "&w";
		Liquid.ParentObject.Render.DetailColor = "W";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&W";
	}

	public override void RenderPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (!Liquid.IsWadingDepth())
		{
			return;
		}
		if (Liquid.ParentObject.IsFrozen())
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&W^w", "&W", "w");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&W^w", "&W", "w");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			render.ColorString = "&W^w";
			render.TileColor = "&W";
			render.DetailColor = "w";
			if (num < 15)
			{
				render.RenderString = "÷";
			}
			else if (num < 30)
			{
				render.RenderString = "~";
			}
			else if (num < 45)
			{
				render.RenderString = "\t";
			}
			else
			{
				render.RenderString = "~";
			}
		}
	}

	public override void RenderSecondary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString += "&w";
		}
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				eRender.ColorString = "&w";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "Liquids/Gunk/";
		}
		return base.GetPaintAtlas(Liquid);
	}

	public override float GetValuePerDram()
	{
		return 2f;
	}
}
