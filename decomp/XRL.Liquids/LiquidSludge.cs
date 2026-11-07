using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
public class LiquidSludge : BaseLiquid
{
	public new const string ID = "sludge";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "w", "Y" };

	public LiquidSludge()
		: base("sludge")
	{
		FlameTemperature = 575;
		VaporTemperature = 1575;
		Combustibility = 7;
		ThermalConductivity = 30;
		Fluidity = 10;
		Evaporativity = 1;
		Staining = 1;
		CirculatoryLossTerm = "oozing";
		CirculatoryLossNoun = "ooze";
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "w";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{w|brown sludge}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{w|sludgy}}";
	}

	public override string GetWaterRitualName()
	{
		return "sludge";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{w|sludgy}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{w|sludgy}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{w|sludge}}";
	}

	public override string GetPreparedCookingIngredient()
	{
		return "selfPoison";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Message.Compound("It's horrifying!");
		if (Target.ApplyEffect(new Poisoned(Stat.Roll("1d4+4"), Stat.Roll("1d2+2") + "d2", 10)))
		{
			Message.Compound("You feel sick!");
			ExitInterface = true;
		}
		return true;
	}

	public override void ObjectGoingProne(LiquidVolume Liquid, GameObject GO, bool UsePopups)
	{
		if (!Liquid.IsWadingDepth())
		{
			return;
		}
		if (GO.IsPlayer())
		{
			IComponent<GameObject>.EmitMessage(GO, "Brown sludge splashes into your mouth. You wince at the metallic taste.", ' ', FromDialog: false, UsePopups, AlwaysVisible: false, null, GO);
		}
		GO.Splatter("&w.");
		for (int i = 0; i < 3; i++)
		{
			List<GameObject> list = Event.NewGameObjectList();
			Inventory inventory = GO.Inventory;
			if (inventory != null)
			{
				foreach (GameObject item in inventory.GetObjectsDirect())
				{
					if (item.HasPart<Metal>())
					{
						list.Add(item);
					}
				}
			}
			Body body = GO.Body;
			if (body != null)
			{
				foreach (GameObject equippedObject in body.GetEquippedObjects())
				{
					if (equippedObject.HasPart<Metal>())
					{
						list.Add(equippedObject);
					}
				}
			}
			if (list.Count > 0 && 75.in100())
			{
				list.GetRandomElement().ApplyEffect(new Rusted(UsePopups));
			}
		}
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				eRender.ColorString = "&w";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^w" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString = "&Y^w";
		Liquid.ParentObject.Render.TileColor = "&Y";
		Liquid.ParentObject.Render.DetailColor = "w";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.Render.ColorString += "&w";
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
			eRender.TileVariantColors("&Y^w", "&Y", "w");
			return;
		}
		Render render = Liquid.ParentObject.Render;
		int num = (XRLCore.CurrentFrame + Liquid.FrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&Y^w", "&Y", "w");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			render.ColorString = "&Y^w";
			render.TileColor = "&Y";
			render.DetailColor = "w";
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
			eRender.ColorString += "&w";
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
		if (!FilthAffinity)
		{
			if (!Smart)
			{
				return 2;
			}
			return 3;
		}
		return 0;
	}
}
