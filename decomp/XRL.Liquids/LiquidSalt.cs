using System;
using System.Collections.Generic;
using System.Text;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
public class LiquidSalt : BaseLiquid
{
	public new const string ID = "salt";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "y", "Y" };

	public LiquidSalt()
		: base("salt")
	{
		VaporTemperature = 200;
		PureElectricalConductivity = 0;
		MixedElectricalConductivity = 100;
		ThermalConductivity = 20;
		Fluidity = 35;
		Staining = 1;
		StainOnlyWhenPure = true;
		Cleansing = 1;
		FreezeObject3 = "Halite";
		FreezeObjectThreshold3 = 500;
		FreezeObjectVerb3 = "solidify";
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "y";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{Y|salt}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		if (Liquid == null)
		{
			return "{{Y|salty}}";
		}
		int num = Liquid.ComponentLiquids["salt"];
		if (num >= 400)
		{
			return "{{Y|salty}}";
		}
		if (num > 0)
		{
			return "{{w|brackish}}";
		}
		return null;
	}

	public override string GetWaterRitualName()
	{
		return "salt";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		if (Liquid.IsMixed() && Liquid.Proportion("water") >= Liquid.Proportion("salt"))
		{
			return null;
		}
		return "{{Y|salty}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{Y|salt-encrusted}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{Y|salt}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Target.HasPart<Stomach>() && !Target.FireEvent(Event.New("AddWater", "Amount", -100 * Liquid.ComponentLiquids["salt"])))
		{
			return false;
		}
		if (!Target.HasPart<Amphibious>())
		{
			Message.Compound("Blech, it's salty!");
		}
		return true;
	}

	public override string GetPreparedCookingIngredient()
	{
		return "tastyMinor";
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^Y" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			Liquid.ParentObject.Render.RenderString = "±";
		}
		Liquid.ParentObject.Render.ColorString = "&Y^y";
		Liquid.ParentObject.Render.TileColor = "&Y";
		Liquid.ParentObject.Render.DetailColor = "y";
	}

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "Liquids/Speckle/";
		}
		return base.GetPaintAtlas(Liquid);
	}

	public override void StainElements(LiquidVolume Liquid, GetItemElementsEvent E)
	{
		E.Add("salt", 1);
	}
}
