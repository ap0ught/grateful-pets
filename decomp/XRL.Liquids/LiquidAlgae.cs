using System;
using System.Collections.Generic;
using System.Text;
using XRL.World;
using XRL.World.Parts;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
public class LiquidAlgae : BaseLiquid
{
	public new const string ID = "algae";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "g", "C" };

	public LiquidAlgae()
		: base("algae")
	{
		VaporTemperature = 200;
		ThermalConductivity = 20;
		Fluidity = 35;
		Evaporativity = 1;
		Staining = 1;
		StainOnlyWhenPure = true;
		Cleansing = 1;
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
		return "{{g|algae}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{g|algal}}";
	}

	public override string GetWaterRitualName()
	{
		return "algae";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		if (Liquid.IsMixed() && Liquid.Proportion("water") >= Liquid.Proportion("algae"))
		{
			return null;
		}
		return "{{g|algal}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{g|algae-covered}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{g|algae}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Target.HasPart<Stomach>())
		{
			Target.FireEvent(Event.New("AddFood", "Satiation", "Snack"));
			if (!Target.FireEvent(Event.New("AddWater", "Amount", -50 * Liquid.ComponentLiquids["algae"])))
			{
				return false;
			}
			Message.Compound("The brine stings your mouth and fills your breath with lake air.");
		}
		return true;
	}

	public override string GetPreparedCookingIngredient()
	{
		return "plantMinor";
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^g" + eRender.ColorString;
		}
	}

	public override void RenderBackgroundSecondary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString += "^C";
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			Liquid.ParentObject.Render.RenderString = "±";
		}
		Liquid.ParentObject.Render.ColorString = "&g^C";
		Liquid.ParentObject.Render.TileColor = "&g";
		Liquid.ParentObject.Render.DetailColor = "C";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		if (Liquid.GetPrimaryLiquidID() == "water")
		{
			Liquid.ParentObject.Render.ColorString = "&g^C";
			Liquid.ParentObject.Render.TileColor = "&g";
			Liquid.ParentObject.Render.DetailColor = "C";
		}
	}

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "Liquids/Paisley/";
		}
		return base.GetPaintAtlas(Liquid);
	}
}
