using System;
using XRL.Rules;
using XRL.UI;
using XRL.World.Conversations.Parts;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ArkCore : IPart
{
	public bool Opened => The.Game.HasDelimitedGameState("EndExtra", ',', "OpenArk");

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != TookDamageEvent.ID && ID != PooledEvent<DefendMeleeHitEvent>.ID && ID != PooledEvent<DefenderMissileHitEvent>.ID && ID != BeforeDestroyObjectEvent.ID)
		{
			return ID == LeavingCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			TryOpen(E.Actor);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		ChimeAt(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DefendMeleeHitEvent E)
	{
		ChimeAt(E.Attacker);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DefenderMissileHitEvent E)
	{
		ChimeAt(E.Attacker);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeavingCellEvent E)
	{
		ChimeAt(The.Player);
		StartEnd(Broken: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		ChimeAt(The.Player);
		StartEnd(Broken: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (!Opened)
		{
			E.AddAction("Open", "open", "Open", null, 'o');
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Open" && TryOpen(E.Actor))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public bool TryOpen(GameObject Actor)
	{
		if (Opened)
		{
			return false;
		}
		if (ParentObject.CurrentZone.HasObjectWithTagOrProperty("CherubimLock"))
		{
			return Actor.ShowFailure("The protective force of the cherubim prevents you from opening the ark.");
		}
		string text = "OPEN ARK";
		if (Popup.AskString("Opening the ark will expose its nondeterministic core to the chamber's ambient normality and irrevocably damage Resheph. Continue?\n\nType " + text + " to confirm.", "", "Sounds/UI/ui_notification", null, text, text.Length, 0, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false).ToUpper() == text)
		{
			StartEnd();
		}
		return true;
	}

	public void ChimeAt(GameObject Actor)
	{
		if (GameObject.Validate(Actor))
		{
			AIHelpBroadcastEvent.Send(ParentObject, Actor, null, "Cherubim", 80, 100f, HelpCause.Trespass);
		}
	}

	public void StartEnd(bool Broken = false)
	{
		if (Opened)
		{
			return;
		}
		if (Broken)
		{
			Popup.Show("The ark was broken.");
		}
		The.Game.RequireSystem<ArkOpenedSystem>();
		if (NephalProperties.IsFoiled("Ehalcodon"))
		{
			The.Game.SetStringGameState("EndGrade", "Super");
		}
		The.Game.TryAddDelimitedGameState("EndExtra", ',', "OpenArk");
		CombatJuice.cameraShake(0.5f);
		try
		{
			Zone parentZone = currentCell.ParentZone;
			SoundManager.PlayUISound("Sounds/Abilities/sfx_ability_electromagnetic_pulse");
			foreach (GameObject item in parentZone.FindObjects((GameObject o) => o.GetBlueprint().DescendsFrom("BaseTriumHologram")))
			{
				item.Obliterate();
			}
			foreach (GameObject item2 in parentZone.FindObjects((GameObject o) => o.GetBlueprint().DescendsFrom("BaseHologramProjector")))
			{
				item2.ApplyEffect(new Broken());
				item2.SetIntProperty("NoRepair", 1);
			}
			The.Core.RenderDelay(500);
			SoundManager.PlayUISound("Sounds/Misc/sfx_spark");
			CombatJuice.cameraShake(0.4f);
			foreach (GameObject item3 in parentZone.FindObjectsWithPart("HologramMaterial"))
			{
				item3.Render.ColorString = ((Stat.RandomCosmetic(1, 100) < 50) ? "&R" : "&r");
				item3.GetPart<HologramMaterial>().ColorStrings = "&r,&R,&w,&r";
				item3.GetPart<HologramMaterial>().DetailColors = "r,w,c,R";
				item3.GetPart<HologramMaterial>().reset();
				if (Stat.RandomCosmetic(1, 100) < 50)
				{
					item3.Sparksplatter();
					item3.Obliterate();
				}
			}
			The.Core.RenderDelay(500);
			CombatJuice.cameraShake(0.3f);
			SoundManager.PlayUISound("Sounds/Grenade/sfx_grenade_highExplosive_explode");
			foreach (GameObject item4 in parentZone.FindObjectsWithPart("AnimatedWallGeneric"))
			{
				item4.Render.ColorString = ((Stat.RandomCosmetic(1, 100) < 50) ? "&y^R" : "&y^r");
				item4.Render.TileColor = ((Stat.RandomCosmetic(1, 100) < 50) ? "&y^R" : "&y^r");
				if (Stat.Random(1, 100) < 5)
				{
					item4.Render.TileColor = "&y+^" + Crayons.GetRandomColorAll();
				}
				AnimatedWallGeneric part = item4.GetPart<AnimatedWallGeneric>();
				part.FrameMS = Stat.Random(10, 50);
				part.RushingChance = Stat.Random(10, 20000);
				if (Stat.Random(1, 100) < 5)
				{
					item4.CurrentCell.AddObject("Firestarter");
					item4.Physics.Temperature += 5000;
				}
			}
			The.Core.RenderDelay(500);
			SoundManager.PlayUISound("Sounds/Abilities/sfx_ability_gas_breathe");
			CombatJuice.cameraShake(0.4f);
			foreach (GameObject item5 in parentZone.FindObjectsWithPart("ArkCore"))
			{
				item5.CurrentCell?.AddObject("CryoGas1000");
			}
			The.Core.RenderDelay(500);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("ArkCore Open", x);
		}
		if (!EndGame.CheckMarooned())
		{
			Popup.Show("The North Sheva superstrate evanesces, and the latches that prevent descent unlock.");
		}
	}
}
