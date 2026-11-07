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
public class LiquidSap : BaseLiquid
{
	public new const string ID = "sap";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "W", "Y" };

	public LiquidSap()
		: base("sap")
	{
		FlameTemperature = 250;
		VaporTemperature = 1250;
		Combustibility = 70;
		Adsorbence = 25;
		ThermalConductivity = 40;
		Fluidity = 3;
		Evaporativity = 1;
		Staining = 2;
		Weight = 0.5;
		InterruptAutowalk = true;
		StickyWhenWet = true;
		StickySaveTargetBase = 1;
		StickySaveTargetScale = 0.1;
		StickyDuration = 12;
		StickySaveVs = "Sap Stuck Restraint";
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "W";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{W|sap}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{Y|sugary}}";
	}

	public override string GetWaterRitualName()
	{
		return "sap";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{W|sappy}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{W|sappy}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{W|sap}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Target.HasPart<Stomach>())
		{
			Target.FireEvent(Event.New("AddFood", "Satiation", "Snack"));
			Message.Compound("It's sweet to the taste.");
		}
		return true;
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^W" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&W^Y";
		Liquid.ParentObject.Render.TileColor = "&W";
		Liquid.ParentObject.Render.DetailColor = "Y";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&W";
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
			eRender.TileVariantColors("&W^Y", "&W", "Y");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&W^Y", "&W", "Y");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			render.ColorString = "&W^Y";
			render.TileColor = "&W";
			render.DetailColor = "Y";
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
			eRender.ColorString += "&W";
		}
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				eRender.ColorString = "&W";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override float GetValuePerDram()
	{
		return 2f;
	}
}
