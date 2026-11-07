using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Text;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
public class LiquidWater : BaseLiquid
{
	public new const string ID = "water";

	[NonSerialized]
	public static List<string> Colors = new List<string>(4) { "B", "y", "Y", "b" };

	public LiquidWater()
		: base("water")
	{
		Combustibility = -50;
		VaporObject = "SteamGas";
		Fluidity = 30;
		Evaporativity = 2;
		Cleansing = 5;
		PureElectricalConductivity = 0;
		MixedElectricalConductivity = 100;
		EnableCleaning = true;
		SlipperyWhenFrozen = true;
		SlipperySaveTargetBase = 5;
		SlipperySaveTargetScale = 0.3;
		SlipperySaveVs = "Ice Slip Move";
		SlipperyMessage = "{{C|=subject.T= =verb:slip= on the ice!}}";
		SlipperyParticle = "&C\u001a";
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "B";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		if (Liquid != null && Liquid.IsPureLiquid("water"))
		{
			return "{{B|fresh water}}";
		}
		return "{{B|water}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		if (Liquid == null)
		{
			return "{{Y|dilute}}";
		}
		if (Liquid.ComponentLiquids["water"] > 0)
		{
			return "{{Y|dilute}}";
		}
		return null;
	}

	public override string GetWaterRitualName()
	{
		return "water";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		if (Liquid.IsMixed())
		{
			if (Liquid.Proportion("salt", "blood") > Liquid.Proportion("water"))
			{
				return null;
			}
			if (Liquid.Primary != "oil" && Liquid.Primary != "lava" && Liquid.Primary != "wax")
			{
				return "{{Y|dilute}}";
			}
		}
		return "{{B|wet}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{B|wet}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{B|water}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Target.HasPart<Amphibious>())
		{
			Message?.Compound("You pour the water on " + Target.itself + "!");
		}
		Event obj = new Event("AddWater");
		obj.SetParameter("Amount", Volume * 10 * (Liquid?.ComponentLiquids["water"] ?? 1000));
		obj.SetParameter("MessageHolder", Message);
		try
		{
			if (!Target.FireEvent(obj))
			{
				return false;
			}
			if (Liquid.IsFreshWater())
			{
				Message?.Compound("Ahh, refreshing!");
			}
		}
		finally
		{
			if (obj.InterfaceExitRequested())
			{
				ExitInterface = true;
			}
		}
		return true;
	}

	public override void SmearOn(LiquidVolume Liquid, GameObject Target, GameObject By, bool FromCell)
	{
		if (Target.HasPart<Amphibious>() && Liquid.IsFreshWater())
		{
			Target.FireEvent(Event.New("AddWater", "Amount", 100000 * Liquid.Volume, "Forced", 1));
			Liquid.Empty();
		}
		base.SmearOn(Liquid, Target, By, FromCell);
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			using (Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder())
			{
				utf16ValueStringBuilder.Append("^b");
				utf16ValueStringBuilder.Append(eRender.ColorString);
				eRender.ColorString = utf16ValueStringBuilder.ToString();
			}
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&b^B";
		Liquid.ParentObject.Render.TileColor = "&b";
		Liquid.ParentObject.Render.DetailColor = "B";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&b";
	}

	public override void RenderPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (!Liquid.IsWadingDepth() || Liquid.Secondary == "algae" || Liquid.ParentObject == null)
		{
			return;
		}
		if (Liquid.ParentObject.IsFrozen())
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&c^C", "&c", "C");
			return;
		}
		if (Liquid.IsFreshWaterFast())
		{
			Render render = Liquid.ParentObject.Render;
			int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
			if (Stat.RandomCosmetic(1, 600) == 1)
			{
				eRender.RenderString = "÷";
				eRender.TileVariantColors("&Y^B", "&Y", "B");
			}
			if (render.ColorString == "&b" || Stat.RandomCosmetic(1, 60) == 1)
			{
				if (num < 15)
				{
					render.RenderString = "÷";
					render.ColorString = "&b^B";
					render.TileColor = "&b";
					render.DetailColor = "B";
				}
				else if (num < 30)
				{
					render.RenderString = " ";
					render.ColorString = "&Y^B";
					render.TileColor = "&Y";
					render.DetailColor = "B";
				}
				else if (num < 45)
				{
					render.RenderString = "÷";
					render.ColorString = "&b^B";
					render.TileColor = "&b";
					render.DetailColor = "B";
				}
				else
				{
					render.RenderString = "~";
					render.ColorString = "&y^B";
					render.TileColor = "&y";
					render.DetailColor = "B";
				}
			}
			return;
		}
		if (Liquid.Flowing)
		{
			int num2 = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
			if (num2 < 15)
			{
				eRender.RenderString = "~";
				eRender.TileVariantColors("&B^b", "&B", "b");
			}
			else if (num2 < 30)
			{
				eRender.RenderString = Liquid.ParentObject.Render.RenderString;
				eRender.TileVariantColors("&Y^b", "&Y", "b");
			}
			else if (num2 < 45)
			{
				eRender.RenderString = "~";
				eRender.TileVariantColors("&B^b", "&B", "b");
			}
			else
			{
				eRender.RenderString = Liquid.ParentObject.Render.RenderString;
				eRender.TileVariantColors("&B^b", "&B", "b");
			}
			return;
		}
		Render render2 = Liquid.ParentObject.Render;
		int num3 = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&Y^b", "&Y", "b");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			if (num3 < 15)
			{
				render2.RenderString = "÷";
				render2.ColorString = "&B^b";
				render2.TileColor = "&B";
				render2.DetailColor = "b";
			}
			else if (num3 < 30)
			{
				render2.RenderString = "~";
				render2.ColorString = "&B^b";
				render2.TileColor = "&B";
				render2.DetailColor = "b";
			}
			else if (num3 < 45)
			{
				render2.RenderString = " ";
				render2.ColorString = "&B^b";
				render2.TileColor = "&B";
				render2.DetailColor = "b";
			}
			else
			{
				render2.RenderString = "~";
				render2.ColorString = "&B^b";
				render2.TileColor = "&B";
				render2.DetailColor = "b";
			}
		}
	}

	public override void RenderSecondary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString += "&b";
		}
	}

	public override float GetValuePerDram()
	{
		return 0.01f;
	}

	public override float GetPureLiquidValueMultipler()
	{
		return 100f;
	}
}
