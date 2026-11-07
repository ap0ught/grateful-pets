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
public class LiquidWine : BaseLiquid
{
	public new const string ID = "wine";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "m", "r" };

	public LiquidWine()
		: base("wine")
	{
		FlameTemperature = 620;
		VaporTemperature = 1620;
		Combustibility = 15;
		ThermalConductivity = 45;
		Fluidity = 30;
		Evaporativity = 3;
		Staining = 2;
		Cleansing = 1;
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "m";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{m|wine}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{m|lush}}";
	}

	public override string GetWaterRitualName()
	{
		return "wine";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{m|lush}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{m|lush}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{m|wine}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		long turns = XRLCore.Core.Game.Turns;
		if (Target == null)
		{
			return true;
		}
		if (Target.HasPart<Stomach>())
		{
			Target.FireEvent(Event.New("AddWater", "Amount", 2 * Volume, "Forced", 1));
			Message.Compound("You flush with the warming draught!");
		}
		if (!Target.HasProperty("ConfuseOnEatTurnWine"))
		{
			Target.SetLongProperty("ConfuseOnEatTurnWine", XRLCore.Core.Game.Turns);
		}
		long longProperty = Target.GetLongProperty("ConfuseOnEatTurnWine", 0L);
		int num = Target.GetIntProperty("ConfuseOnEatAmountWine");
		if (turns - longProperty > 80)
		{
			num = 0;
		}
		if (num > Math.Max(1, Target.StatMod("Toughness") * 2) && Target.ApplyEffect(new Confused(Stat.Roll("5d5"), 1, 3)))
		{
			ExitInterface = true;
		}
		Target.SetLongProperty("ConfuseOnEatTurnWine", turns);
		num++;
		Target.SetIntProperty("ConfuseOnEatAmountWine", num);
		return true;
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^m" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&m^r";
		Liquid.ParentObject.Render.TileColor = "&m";
		Liquid.ParentObject.Render.DetailColor = "r";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&m";
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
			eRender.TileVariantColors("&M^b", "&M", "b");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&r^m", "&r", "m");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			render.ColorString = "&m^r";
			render.TileColor = "&m";
			render.DetailColor = "r";
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
			eRender.ColorString += "&m";
		}
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				eRender.ColorString = "&m";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override float GetValuePerDram()
	{
		return 4f;
	}
}
