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
public class LiquidPutrescence : BaseLiquid
{
	public new const string ID = "putrid";

	[NonSerialized]
	public static List<string> Colors = new List<string>(3) { "K", "g", "w" };

	public LiquidPutrescence()
		: base("putrid")
	{
		FlameTemperature = 600;
		VaporTemperature = 1600;
		Combustibility = 5;
		ThermalConductivity = 25;
		Fluidity = 15;
		Evaporativity = 1;
		Staining = 2;
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
		return "{{putrid|putrescence}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{putrid|putrid}}";
	}

	public override string GetWaterRitualName()
	{
		return "putrescence";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{putrid|putrid}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{putrid|putrid}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{putrid|putrescence}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Target.IsPlayer() && Message != null && !Message.Contains("It's disgusting!"))
		{
			Message.Compound("It's disgusting!");
		}
		InduceVomitingEvent.Send(Target, ref ExitInterface, Message);
		return true;
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

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString += "^K";
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&g^K";
		Liquid.ParentObject.Render.TileColor = "&g";
		Liquid.ParentObject.Render.DetailColor = "K";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&g";
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
			eRender.TileVariantColors("&G^K", "&G", "K");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
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
				render.RenderString = " ";
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

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "Liquids/Splotchy/";
		}
		return base.GetPaintAtlas(Liquid);
	}

	public override int GetNavigationWeight(LiquidVolume Liquid, GameObject GO, bool Smart, bool Slimewalking, bool FilthAffinity, ref bool Uncacheable)
	{
		if (!(FilthAffinity | Smart))
		{
			return 2;
		}
		return 0;
	}
}
