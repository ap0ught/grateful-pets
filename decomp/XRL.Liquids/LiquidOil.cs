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
public class LiquidOil : BaseLiquid
{
	public new const string ID = "oil";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "K", "Y" };

	public LiquidOil()
		: base("oil")
	{
		FlameTemperature = 250;
		VaporTemperature = 2000;
		Combustibility = 90;
		PureElectricalConductivity = 0;
		MixedElectricalConductivity = 0;
		ThermalConductivity = 40;
		Fluidity = 25;
		Staining = 1;
		Cleansing = 1;
		SlipperyWhenWet = true;
		SlipperyWhenFrozen = true;
		SlipperySaveTargetBase = 5;
		SlipperySaveTargetScale = 0.3;
		SlipperySaveVs = "Oil Slip Move";
		SlipperyMessage = "{{C|=subject.T= =verb:slip= on the oil!}}";
		SlipperyParticle = "&C\u001a";
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
		return "{{K|oil}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{K|oily}}";
	}

	public override string GetWaterRitualName()
	{
		return "oil";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{K|oily}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{K|oily}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{K|oil}}";
	}

	public override float GetValuePerDram()
	{
		return 3f;
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Message.Compound("{{K|Disgusting!}}");
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
		Liquid.ParentObject.Render.ColorString = "&Y^k";
		Liquid.ParentObject.Render.TileColor = "&Y";
		Liquid.ParentObject.Render.DetailColor = "k";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&k";
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
			eRender.TileVariantColors("&Y^k", "&Y", "k");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			render.ColorString = "&Y^k";
			render.TileColor = "&Y";
			render.DetailColor = "k";
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
				render.RenderString = "÷";
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
}
