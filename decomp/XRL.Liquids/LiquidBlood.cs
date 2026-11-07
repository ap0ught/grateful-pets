using System;
using System.Collections.Generic;
using System.Text;
using XRL.World;
using XRL.World.Parts;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
public class LiquidBlood : BaseLiquid
{
	public new const string ID = "blood";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "r", "K" };

	public LiquidBlood()
		: base("blood")
	{
		FlameTemperature = 400;
		VaporTemperature = 1200;
		Combustibility = 2;
		Fluidity = 5;
		Evaporativity = 1;
		Staining = 2;
		ThermalConductivity = 35;
		CirculatoryLossTerm = "bleeding";
		CirculatoryLossNoun = "bleed";
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "r";
	}

	public override float GetValuePerDram()
	{
		return 0.25f;
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Message.Compound("It has a metallic taste.");
		return true;
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{r|bloody}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{r|bloody}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{r|blood}}";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{r|blood}}";
	}

	public override string GetWaterRitualName()
	{
		return "blood";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{r|bloody}}";
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^r" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&r^k";
		Liquid.ParentObject.Render.TileColor = "&r";
		Liquid.ParentObject.Render.DetailColor = "k";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&r";
		if (Liquid.ContainsLiquid("algae"))
		{
			Liquid.ParentObject.Render.ColorString += "^C";
		}
	}

	public override void RenderPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "&r";
		}
	}

	public override void RenderSecondary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString += "&r";
			if (Liquid.ContainsLiquid("algae"))
			{
				eRender.DetailColor += "C";
			}
		}
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "&r";
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override void StainElements(LiquidVolume Liquid, GetItemElementsEvent E)
	{
		E.Add("might", 1);
	}
}
