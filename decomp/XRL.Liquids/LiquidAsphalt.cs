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
public class LiquidAsphalt : BaseLiquid
{
	public new const string ID = "asphalt";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "K", "y" };

	public LiquidAsphalt()
		: base("asphalt")
	{
		FlameTemperature = 240;
		VaporTemperature = 1240;
		Combustibility = 75;
		Adsorbence = 25;
		PureElectricalConductivity = 10;
		MixedElectricalConductivity = 10;
		ThermalConductivity = 35;
		Fluidity = 1;
		Staining = 1;
		Weight = 0.5;
		InterruptAutowalk = true;
		StickyWhenWet = true;
		StickySaveTargetBase = 4;
		StickySaveTargetScale = 0.1;
		StickyDuration = 12;
		StickySaveVs = "Asphalt Stuck Restraint";
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "K";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{K|asphalt}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{K|tarry}}";
	}

	public override string GetWaterRitualName()
	{
		return "tar";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{K|tarry}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{K|tarred}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{K|tar}}";
	}

	public override string GetPreparedCookingIngredient()
	{
		return "stabilityMinor";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Message.Compound("{{K|It burns!}}");
		Target.TemperatureChange(500, Target);
		Damage value = new Damage(Stat.Roll(Liquid.Proportion("asphalt") / 100 + 1 + "d6"));
		Event obj = Event.New("TakeDamage");
		obj.AddParameter("Damage", value);
		obj.AddParameter("Owner", Liquid);
		obj.AddParameter("Attacker", Liquid);
		obj.AddParameter("Message", "from {{K|drinking asphalt}}!");
		Target.FireEvent(obj);
		ExitInterface = true;
		return true;
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^K" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&k^K";
		Liquid.ParentObject.Render.TileColor = "&k";
		Liquid.ParentObject.Render.DetailColor = "K";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&K";
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
			eRender.TileVariantColors("&k^K", "&k", "K");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&k^K", "&k", "K");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			render.ColorString = "&k^K";
			render.TileColor = "&k";
			render.DetailColor = "K";
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
			eRender.ColorString += "&K";
		}
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				eRender.ColorString = "&K";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "Liquids/Speckle/";
		}
		return base.GetPaintAtlas(Liquid);
	}
}
