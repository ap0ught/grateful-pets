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
public class LiquidCider : BaseLiquid
{
	public new const string ID = "cider";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "w", "r" };

	public LiquidCider()
		: base("cider")
	{
		FlameTemperature = 500;
		VaporTemperature = 800;
		Combustibility = 5;
		ThermalConductivity = 40;
		Evaporativity = 5;
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
		return "{{cider|cider}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{cider|spiced}}";
	}

	public override string GetWaterRitualName()
	{
		return "cider";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{cider|fragrant}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{cider|fragrant}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{cider|cider}}";
	}

	public override string GetPreparedCookingIngredient()
	{
		return "quicknessMinor";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		long turns = XRLCore.Core.Game.Turns;
		if (Target.HasPart<Stomach>())
		{
			Target.FireEvent(Event.New("AddWater", "Amount", Volume * 2, "Forced", 1));
			Target.FireEvent(Event.New("AddFood", "Satiation", "Snack"));
			Message.Compound("It is spicy and delicious.");
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
			eRender.ColorString = "^w" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&w^r";
		Liquid.ParentObject.Render.TileColor = "&w";
		Liquid.ParentObject.Render.DetailColor = "r";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&w";
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
			eRender.TileVariantColors("&r^w", "&r", "w");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&r^w", "&r", "w");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
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
			render.ColorString = "&w^r";
			render.TileColor = "&w";
			render.DetailColor = "r";
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

	public override float GetValuePerDram()
	{
		return 3.8f;
	}
}
