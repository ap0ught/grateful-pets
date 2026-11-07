using System;
using System.Collections.Generic;
using System.Text;
using Qud.API;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
public class LiquidLava : BaseLiquid
{
	public new const string ID = "lava";

	[NonSerialized]
	public static List<string> Colors = new List<string>(3) { "R", "r", "W" };

	public LiquidLava()
		: base("lava")
	{
		VaporTemperature = 10000;
		Temperature = 1000;
		Fluidity = 15;
		Cleansing = 20;
		Weight = 0.5;
		PureElectricalConductivity = 90;
		MixedElectricalConductivity = 90;
		InterruptAutowalk = true;
		ConsiderDangerousToContact = true;
		ConsiderDangerousToDrink = true;
		Glows = true;
		FreezeObject1 = "SmallBoulder";
		FreezeObjectThreshold1 = 1;
		FreezeObjectVerb1 = "solidify";
		FreezeObject2 = "MediumBoulder";
		FreezeObjectThreshold2 = 100;
		FreezeObjectVerb2 = "solidify";
		FreezeObject3 = "Shale";
		FreezeObjectThreshold3 = 400;
		FreezeObjectVerb3 = "solidify";
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "R";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{lava|lava}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{lava|magmatic}}";
	}

	public override string GetWaterRitualName()
	{
		return "lava";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{lava|magmatic}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{lava|lava-covered}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{lava|lava}}";
	}

	public override string GetPreparedCookingIngredient()
	{
		return "heatMinor";
	}

	public override bool SafeContainer(GameObject GO)
	{
		if (GO.Physics != null)
		{
			return GO.Physics.FlameTemperature > Temperature;
		}
		return true;
	}

	public override float GetValuePerDram()
	{
		return 50f;
	}

	public override bool Vaporized(LiquidVolume Liquid, GameObject gameObject)
	{
		return false;
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Achievement.DRINK_LAVA.Unlock();
		JournalAPI.AddAccomplishment("Inexplicably, you drank lava.", "For reasons kept secret by the sphinx of subjective experience, =name= drank the forbidden tea.", "While traveling through " + The.Player.CurrentZone.GetTerrainDisplayName() + ", =name= stopped at a tavern in " + JournalAPI.GetLandmarkNearestPlayer().Text + ". There " + The.Player.GetPronounProvider().Subjective + " lost a bet and bashfully drank a dram of lava. Miraculously, " + The.Player.GetPronounProvider().Subjective + " left the tavern unharmed.", null, "general", MuralCategory.BodyExperienceNeutral, MuralWeight.Medium, null, -1L);
		Message.Compound("{{lava|IT BURNS!}}");
		Target.TemperatureChange(Temperature, Target);
		string dice = Liquid.Proportion("lava") / 100 + 1 + "d100";
		Target.TakeDamage(dice.Roll(), "from {{lava|drinking lava}}!", "Heat", null, null, Target, Liquid.ParentObject);
		ExitInterface = true;
		return true;
	}

	public override void BeforeRender(LiquidVolume Liquid)
	{
		if ((!Liquid.Sealed || Liquid.LiquidVisibleWhenSealed) && (Liquid.Primary == "lava" || Liquid.Secondary == "lava" || Liquid.IsPureLiquid("lava")))
		{
			Liquid.AddLight(0);
		}
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^R" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&W^R";
		Liquid.ParentObject.Render.TileColor = "&W";
		Liquid.ParentObject.Render.DetailColor = "R";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&R";
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
			eRender.TileVariantColors("&W^R", "&W", "R");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&W^R", "&W", "R");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			if (num < 15)
			{
				render.RenderString = "÷";
				render.ColorString = "&W^R";
				render.TileColor = "&W";
				render.DetailColor = "R";
			}
			else if (num < 30)
			{
				render.RenderString = "~";
				render.ColorString = "&W^r";
				render.TileColor = "&W";
				render.DetailColor = "r";
			}
			else if (num < 45)
			{
				render.RenderString = "\t";
				render.ColorString = "&W^R";
				render.TileColor = "&W";
				render.DetailColor = "R";
			}
			else
			{
				render.RenderString = "~";
				render.ColorString = "&W^R";
				render.TileColor = "&W";
				render.DetailColor = "R";
			}
		}
	}

	public override void RenderSecondary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString += "&r";
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
		if (Smart && GO != null)
		{
			Uncacheable = true;
			int num = GO.Stat("HeatResistance");
			if (num > 0 && ((!Liquid.IsSwimmingDepth()) ? (!HasFlammableEquipmentEvent.Check(GO, Temperature)) : (!HasFlammableEquipmentOrInventoryEvent.Check(GO, Temperature))))
			{
				if (num >= 100)
				{
					return 0;
				}
				return Math.Min(Math.Max(40 + 59 * (100 - num) / 100, 0), 99);
			}
		}
		return 99;
	}

	public override void StainElements(LiquidVolume Liquid, GetItemElementsEvent E)
	{
		E.Add("might", 1);
	}
}
