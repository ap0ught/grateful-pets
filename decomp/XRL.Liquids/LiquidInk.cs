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
public class LiquidInk : BaseLiquid
{
	public new const string ID = "ink";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "K", "y" };

	public LiquidInk()
		: base("ink")
	{
		FlameTemperature = 350;
		VaporTemperature = 1350;
		Combustibility = 30;
		PureElectricalConductivity = 40;
		MixedElectricalConductivity = 40;
		ThermalConductivity = 40;
		Fluidity = 10;
		Evaporativity = 1;
		Staining = 3;
		SlipperyWhenWet = true;
		SlipperyWhenFrozen = true;
		SlipperySaveTargetBase = 5;
		SlipperySaveTargetScale = 0.3;
		SlipperySaveVs = "Ink Slip Move";
		SlipperyMessage = "{{K|=subject.T= =verb:slip= on the ink!}}";
		SlipperyParticle = "&K\u001a";
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
		return "{{K|ink}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{K|inky}}";
	}

	public override string GetWaterRitualName()
	{
		return "ink";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{K|inky}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{K|inky}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{K|ink}}";
	}

	public override float GetValuePerDram()
	{
		return 1.5f;
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Message.Compound("It's very inky.");
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
			eRender.ColorString = "^k" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&y^k";
		Liquid.ParentObject.Render.TileColor = "&y";
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
			eRender.TileVariantColors("&y^k", "&y", "k");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&y^k", "&y", "k");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			render.ColorString = "&y^k";
			render.TileColor = "&y";
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
			eRender.ColorString += "&k";
		}
	}

	public override void StainElements(LiquidVolume Liquid, GetItemElementsEvent E)
	{
		E.Add("scholarship", 1);
	}
}
