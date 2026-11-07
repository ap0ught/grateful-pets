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
public class LiquidNeutronFlux : BaseLiquid
{
	public new const string ID = "neutronflux";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "y", "Y" };

	public LiquidNeutronFlux()
		: base("neutronflux")
	{
		VaporTemperature = 10000;
		FreezeTemperature = -100;
		BrittleTemperature = -250;
		PureElectricalConductivity = 0;
		MixedElectricalConductivity = 0;
		ThermalConductivity = 0;
		Fluidity = 100;
		Evaporativity = 100;
		Cleansing = 100;
		Weight = 2.5;
		InterruptAutowalk = true;
		ConsiderDangerousToContact = true;
		ConsiderDangerousToDrink = true;
		Glows = true;
		CirculatoryLossTerm = "fluxing";
		CirculatoryLossNoun = "fluxation";
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "y";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{neutronic|neutron}} {{Y|flux}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{neutronic|neutronic}}";
	}

	public override string GetWaterRitualName()
	{
		return "flux";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{neutronic|neutronic}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{neutronic|neutral}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{neutronic|flux}}";
	}

	public override float GetValuePerDram()
	{
		return 1000f;
	}

	public override string GetPreparedCookingIngredient()
	{
		return "density";
	}

	public static bool Explode(GameObject WhatExplodes, GameObject Owner = null, int Phase = 0)
	{
		LiquidVolume liquidVolume = WhatExplodes.LiquidVolume;
		if (liquidVolume != null)
		{
			liquidVolume.Volume = 0;
			liquidVolume.ComponentLiquids.Clear();
		}
		return WhatExplodes.Explode(15000, Owner, "10d10+250", 1f, Neutron: true, SuppressDestroy: false, Indirect: false, Phase);
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Cell currentCell = Target.CurrentCell;
		int phase = Target.GetPhase();
		Physics.ApplyExplosion(currentCell, 15000, null, null, Local: true, Show: true, Target, "10d10+250", phase, Neutron: true);
		return true;
	}

	public override void SmearOnTick(LiquidVolume Liquid, GameObject Target, GameObject By, bool FromCell)
	{
		base.SmearOnTick(Liquid, Target, By, FromCell);
		Explode(Target, By);
	}

	public override void SmearOn(LiquidVolume Liquid, GameObject Target, GameObject By, bool FromCell)
	{
		base.SmearOn(Liquid, Target, By, FromCell);
		Explode(Target, By);
	}

	public override void BeforeRender(LiquidVolume Liquid)
	{
		if (!Liquid.Sealed || Liquid.LiquidVisibleWhenSealed)
		{
			Liquid.AddLight(0);
		}
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^Y" + eRender.ColorString;
		}
	}

	public override bool MixingWith(LiquidVolume Liquid, LiquidVolume NewLiquid, int Amount, GameObject PouredFrom, GameObject PouredTo, GameObject PouredBy, ref bool ExitInterface)
	{
		bool flag = base.MixingWith(Liquid, NewLiquid, Amount, PouredFrom, PouredTo, PouredBy, ref ExitInterface);
		if (!flag)
		{
			return false;
		}
		if (NewLiquid.ParentObject?.GetCurrentCell() != null)
		{
			NeutronFluxPourExplodesEvent.Check(out var Interrupt, PouredFrom, PouredTo, PouredBy, Liquid, Prospective: true);
			if (Interrupt)
			{
				flag = false;
			}
		}
		return flag;
	}

	public override void MixedWith(LiquidVolume Liquid, LiquidVolume NewLiquid, int Amount, GameObject PouredFrom, GameObject PouredTo, GameObject PouredBy, ref bool ExitInterface)
	{
		base.MixedWith(Liquid, NewLiquid, Amount, PouredFrom, PouredTo, PouredBy, ref ExitInterface);
		if (NewLiquid.ParentObject?.GetCurrentCell() != null && NeutronFluxPourExplodesEvent.Check(PouredFrom, PouredTo, PouredBy, Liquid))
		{
			int num = 0;
			if (Explode(Phase: (NewLiquid.ParentObject == null || !NewLiquid.ContainsLiquid("neutronflux")) ? Liquid.ParentObject.GetPhase() : (NewLiquid.ParentObject.GetObjectContext() ?? NewLiquid.ParentObject).GetPhase(), WhatExplodes: NewLiquid.ParentObject, Owner: PouredBy))
			{
				ExitInterface = true;
			}
		}
	}

	public override bool EnteredCell(LiquidVolume Liquid, EnteredCellEvent E)
	{
		bool flag = base.EnteredCell(Liquid, E);
		if (flag && Liquid.IsOpenVolume() && Explode(Liquid.ParentObject, E.Actor))
		{
			E.RequestInterfaceExit();
			flag = false;
		}
		return flag;
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&Y^y";
		Liquid.ParentObject.Render.TileColor = "&Y";
		Liquid.ParentObject.Render.DetailColor = "y";
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
			eRender.ColorString += "&Y";
		}
	}

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "Liquids/Paisley/";
		}
		return base.GetPaintAtlas(Liquid);
	}

	public override int GetNavigationWeight(LiquidVolume Liquid, GameObject GO, bool Smart, bool Slimewalking, bool FilthAffinity, ref bool Uncacheable)
	{
		return 99;
	}

	public override void StainElements(LiquidVolume Liquid, GetItemElementsEvent E)
	{
		E.Add("stars", 1);
	}
}
