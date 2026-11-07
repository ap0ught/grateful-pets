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
public class LiquidConvalessence : BaseLiquid
{
	public new const string ID = "convalessence";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "C", "Y" };

	public LiquidConvalessence()
		: base("convalessence")
	{
		Temperature = 5;
		Combustibility = 5;
		PureElectricalConductivity = 10;
		MixedElectricalConductivity = 10;
		ThermalConductivity = 80;
		Fluidity = 20;
		Evaporativity = 15;
		Cleansing = 5;
		Glows = true;
		FreezeObject3 = "CryoGas";
		FreezeObjectThreshold3 = 1;
		FreezeObjectVerb3 = "sublimate";
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "C";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{C|convalessence}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{C|luminous}}";
	}

	public override string GetWaterRitualName()
	{
		return "convalessence";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{C|luminous}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{C|luminous}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{C|convalessence}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Message.Compound("It's effervescent!");
		return true;
	}

	public override string GetPreparedCookingIngredient()
	{
		return "coldMinor,regenLowtierMinor";
	}

	public override void BeforeRender(LiquidVolume Liquid)
	{
		if (!Liquid.Sealed || Liquid.LiquidVisibleWhenSealed)
		{
			Cell currentCell = Liquid.ParentObject.CurrentCell;
			if (currentCell != null)
			{
				int num = Liquid.Amount("convalessence");
				int r = ((num >= 1000) ? 3 : ((num < 500) ? 1 : 2));
				currentCell.ParentZone?.AddLight(currentCell.X, currentCell.Y, r, LightLevel.Light);
			}
		}
	}

	public override void BeforeRenderSecondary(LiquidVolume Liquid)
	{
		if (!Liquid.Sealed || Liquid.LiquidVisibleWhenSealed)
		{
			Liquid.AddLight(1);
		}
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				eRender.ColorString = "&C";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^C" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&Y^C";
		Liquid.ParentObject.Render.TileColor = "&Y";
		Liquid.ParentObject.Render.DetailColor = "C";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&C";
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
			eRender.TileVariantColors("&C^Y", "&C", "Y");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		render.ColorString = "&Y^C";
		render.TileColor = "&Y";
		render.DetailColor = "C";
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
			eRender.ColorString += "&C";
		}
	}

	public override void ObjectInCell(LiquidVolume Liquid, GameObject GO)
	{
		base.ObjectInCell(Liquid, GO);
		if (Liquid.IsOpenVolume() && GO.IsAlive)
		{
			GO.Heal(1, Message: false, FloatText: true, RandomMinimum: true);
		}
	}

	public override int GetHealingLocationValue(LiquidVolume Liquid, GameObject Actor)
	{
		if (Liquid.IsOpenVolume() && Actor.IsOrganic)
		{
			return 10;
		}
		return base.GetHealingLocationValue(Liquid, Actor);
	}

	public override void StainElements(LiquidVolume Liquid, GetItemElementsEvent E)
	{
		E.Add("ice", 1);
	}
}
