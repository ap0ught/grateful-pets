using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
public class LiquidCloning : BaseLiquid
{
	public const string REPLICATION_CONTEXT = "CloningDraught";

	public new const string ID = "cloning";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "Y", "M" };

	public LiquidCloning()
		: base("cloning")
	{
		Fluidity = 10;
		Evaporativity = 100;
		Glows = true;
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "Y";
	}

	public override float GetValuePerDram()
	{
		return 1250f;
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{cloning|cloning draught}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{cloning|homogenized}}";
	}

	public override string GetWaterRitualName()
	{
		return "draught";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{cloning|homogenized}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{cloning|homogenized}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{cloning|cloning-draught}}";
	}

	public override string GetPreparedCookingIngredient()
	{
		return "cloningMinor";
	}

	public void Clone(GameObject Target, GameObject Actor = null, int Amount = 1)
	{
		if (Cloning.CanBeCloned(Target, null, "CloningDraught"))
		{
			Target.ApplyEffect(new Budding(Actor, Amount, "CloningDraught"));
		}
	}

	public override void SmearOn(LiquidVolume Liquid, GameObject GO, GameObject By, bool FromCell)
	{
		if (Liquid.IsPureLiquid("cloning") && Liquid.Volume > 0 && (Liquid.ParentObject == null || !Liquid.ParentObject.IsTemporary))
		{
			Clone(GO, By);
		}
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Target.IsPlayer() && Cloning.CanBeCloned(Target, Target, "CloningDraught"))
		{
			Message.Compound("You feel unsettlingly ambivalent.");
		}
		if (Liquid.IsPureLiquid("cloning") && Volume > 0)
		{
			Clone(Target, Target, DrinkMagnifier.Magnify(Target, 1));
		}
		return true;
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
			eRender.ColorString = "^M" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&M^Y";
		Liquid.ParentObject.Render.TileColor = "&M";
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
			eRender.TileVariantColors("&M^Y", "&M", "Y");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&M^Y", "&M", "Y");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			render.ColorString = "&M^Y";
			render.TileColor = "&M";
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

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "Liquids/Paisley/";
		}
		return base.GetPaintAtlas(Liquid);
	}

	public override void StainElements(LiquidVolume Liquid, GetItemElementsEvent E)
	{
		E.Add("might", 1);
		E.Add("time", 1);
	}
}
