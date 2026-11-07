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
public class LiquidWax : BaseLiquid
{
	public new const string ID = "wax";

	[NonSerialized]
	public static List<string> Colors = new List<string>(1) { "Y" };

	public LiquidWax()
		: base("wax")
	{
		FlameTemperature = 300;
		VaporTemperature = 2000;
		Temperature = 100;
		Combustibility = 65;
		Adsorbence = 25;
		PureElectricalConductivity = 0;
		MixedElectricalConductivity = 0;
		ThermalConductivity = 40;
		Fluidity = 7;
		Cleansing = 3;
		Weight = 0.5;
		InterruptAutowalk = true;
		FreezeObject1 = "Wax Nodule";
		FreezeObjectThreshold1 = 1;
		FreezeObjectVerb1 = "congeal";
		FreezeObject3 = "Wax Block";
		FreezeObjectThreshold3 = 500;
		FreezeObjectVerb3 = "solidify";
		StickyWhenWet = true;
		StickySaveTargetBase = 1;
		StickySaveTargetScale = 0.1;
		StickyDuration = 12;
		StickySaveVs = "Wax Stuck Restraint";
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "Y";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{Y|molten wax}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{Y|waxen}}";
	}

	public override string GetWaterRitualName()
	{
		return "wax";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{Y|waxy}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{Y|waxy}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{Y|wax}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Target.HasPart<Stomach>())
		{
			Message.Compound("It's hot and disgusting.");
			Target.TemperatureChange(100, Target);
		}
		return true;
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^y" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&y^Y";
		Liquid.ParentObject.Render.TileColor = "&y";
		Liquid.ParentObject.Render.DetailColor = "Y";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&y";
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
			eRender.TileVariantColors("&Y^y", "&Y", "y");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&Y^y", "&Y", "y");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			render.ColorString = "&Y^y";
			render.TileColor = "&Y";
			render.DetailColor = "y";
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
			eRender.ColorString += "&y";
		}
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				eRender.ColorString = "&y";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "Liquids/Splotchy/";
		}
		return base.GetPaintAtlas(Liquid);
	}

	public override float GetValuePerDram()
	{
		return 0f;
	}
}
