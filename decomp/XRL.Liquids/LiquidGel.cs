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
public class LiquidGel : BaseLiquid
{
	public new const string ID = "gel";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "Y", "y" };

	public LiquidGel()
		: base("gel")
	{
		PureElectricalConductivity = 100;
		MixedElectricalConductivity = 100;
		ThermalConductivity = 70;
		Fluidity = 5;
		Evaporativity = 1;
		Cleansing = 1;
		SlipperyWhenWet = true;
		SlipperyWhenFrozen = true;
		SlipperySaveTargetBase = 5;
		SlipperySaveTargetScale = 0.3;
		SlipperySaveVs = "Gel Slip Move";
		SlipperyMessage = "{{Y|=subject.T= =verb:slip= on the gel!}}";
		SlipperyParticle = "&Y\u001a";
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
		return "{{Y|gel}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{Y|unctuous}}";
	}

	public override string GetWaterRitualName()
	{
		return "gel";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Message.Compound("It's very greasy.");
		return true;
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{Y|unctuous}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{Y|unctuous}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{Y|gel}}";
	}

	public override float GetValuePerDram()
	{
		return 0.5f;
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				eRender.ColorString = "&Y";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
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
		Liquid.ParentObject.Render.ColorString = "&y^Y";
		Liquid.ParentObject.Render.TileColor = "&y";
		Liquid.ParentObject.Render.DetailColor = "Y";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&Y";
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
			eRender.TileVariantColors("&y^C", "&y", "C");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&y^C", "&y", "C");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			render.ColorString = "&y^Y";
			render.TileColor = "&y";
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
			eRender.ColorString += "&Y";
		}
	}
}
