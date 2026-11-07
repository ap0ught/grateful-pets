using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is not by default, the summoned wraith-knight receives a
/// Quickness bonus of ((power load - 100) / 10), i.e. 30 for the
/// standard overload power load of 400.
/// </remarks>
[Serializable]
public class TemplarPhylactery : IPoweredPart, IHackingSifrahHandler
{
	public static readonly string ACTIVATE_COMMAND_NAME = "ActivateTemplarPhylactery";

	public static readonly string DEACTIVATE_COMMAND_NAME = "DeactivateTemplarPhylactery";

	public static readonly string HACK_COMMAND_NAME = "HackTemplarPhylactery";

	public string wraithID;

	public GameObject wraith;

	public TemplarPhylactery()
	{
		ChargeUse = 1;
		IsPowerLoadSensitive = GlobalConfig.GetBoolSetting("EnablePhylacteryOverload");
		WorksOnHolder = true;
		MustBeUnderstood = true;
		NameForStatus = "ContinuityEngine";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetDefensiveItemListEvent.ID && ID != AIGetOffensiveItemListEvent.ID && ID != AfterObjectCreatedEvent.ID && ID != PooledEvent<CheckExistenceSupportEvent>.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != InventoryActionEvent.ID && ID != OnDestroyObjectEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveItemListEvent E)
	{
		if (wraith == null && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && WantsToUse(E.Actor) && GameObject.Validate(E.Target) && E.Target.Brain != null && !E.Target.HasPart<MentalShield>())
		{
			E.Add(ACTIVATE_COMMAND_NAME, 100, ParentObject, Inv: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetDefensiveItemListEvent E)
	{
		if (wraith != null && IsHacked() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && wraith.IsHostileTowards(E.Actor) && E.Actor.Stat("Intelligence") >= 7)
		{
			E.Add(DEACTIVATE_COMMAND_NAME, 1, ParentObject, Inv: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckExistenceSupportEvent E)
	{
		if (E.Object == wraith && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && wraith.InSameZone(E.Object))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		Despawn();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		Despawn();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Despawn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Despawn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (GameObject.Validate(ref wraith) && IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Despawn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (!IsHacked() && E.Actor.IsPlayer() && Options.SifrahHacking)
		{
			E.AddAction("Hack", "hack", HACK_COMMAND_NAME, null, 'h');
		}
		if (ParentObject.Equipped != null)
		{
			if (GameObject.Validate(ref wraith))
			{
				E.AddAction("Deactivate", "deactivate", DEACTIVATE_COMMAND_NAME, null, 'a');
			}
			else
			{
				E.AddAction("Activate", "activate", ACTIVATE_COMMAND_NAME, null, 'a');
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (IsHacked())
		{
			E.Infix.Compound(ParentObject.Does("have", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " been hacked.");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == ACTIVATE_COMMAND_NAME)
		{
			if (!E.Actor.CanMoveExtremities("Activate", ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return E.Actor.Fail(ParentObject.Does("are") + " unresponsive.");
			}
			Spawn();
			E.RequestInterfaceExit();
		}
		else if (E.Command == DEACTIVATE_COMMAND_NAME)
		{
			Despawn();
			E.RequestInterfaceExit();
		}
		else if (E.Command == HACK_COMMAND_NAME && !IsHacked() && E.Actor.IsPlayer() && Options.SifrahHacking)
		{
			if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: true, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return E.Actor.Fail(ParentObject.Does("are") + " " + GetStatusPhrase() + ".");
			}
			int techTier = ParentObject.GetTechTier();
			HackingSifrah hackingSifrah = new HackingSifrah(ParentObject, techTier, techTier, E.Actor.Stat("Intelligence"));
			hackingSifrah.HandlerID = ParentObject.ID;
			hackingSifrah.HandlerPartName = GetType().Name;
			hackingSifrah.Play(ParentObject);
			E.Actor.UseEnergy(1000, "Sifrah Hack TemplarPhylactery");
			if (!hackingSifrah.InterfaceExitRequested)
			{
				Statistic energy = E.Actor.Energy;
				if (energy == null || energy.Value >= 0)
				{
					goto IL_0225;
				}
			}
			E.RequestInterfaceExit();
		}
		goto IL_0225;
		IL_0225:
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (E.Context != "Sample" && E.Context != "Initialization" && E.Context != "GameStarted" && wraithID == null && E.ReplacementObject == null)
		{
			GameObject gameObject = HeroMaker.MakeHero(GameObject.Create("Wraith-Knight Templar"), null, "SpecialFactionHeroTemplate_TemplarWraith");
			gameObject.RequirePart<HologramMaterial>();
			gameObject.RequirePart<HologramInvulnerability>();
			gameObject.RequirePart<Unreplicable>();
			gameObject.ModIntProperty("IgnoresWalls", 1);
			foreach (GameObject item in gameObject.GetInventoryAndEquipment())
			{
				if (item.HasPropertyOrTag("MeleeWeapon"))
				{
					item.AddPart(new ModPsionic());
				}
				else
				{
					item.Obliterate();
				}
			}
			ParentObject.Render.DisplayName = "phylactery of " + gameObject.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: true, Short: true);
			wraithID = The.ZoneManager?.CacheObject(gameObject);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AdjustWeaponScore");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AdjustWeaponScore" && WantsToUse(E.GetGameObjectParameter("User")))
		{
			E.ModParameter("Score", 100);
		}
		return base.FireEvent(E);
	}

	public bool WantsToUse(GameObject Actor)
	{
		if (Actor != null && Actor.Stat("Intelligence") > 22)
		{
			return !WillBeHostileTowards(Actor);
		}
		return true;
	}

	public void Despawn()
	{
		if (GameObject.Validate(ref wraith))
		{
			wraith.Splatter("&M-");
			wraith.Splatter("&M.");
			wraith.Splatter("&M/");
			IComponent<GameObject>.XDidY(wraith, "disappear", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
			wraith.Obliterate();
			wraith = null;
		}
	}

	public void Spawn()
	{
		Despawn();
		GameObject equipped = ParentObject.Equipped;
		if (equipped == null || wraithID == null)
		{
			return;
		}
		Cell cell = equipped.GetCurrentCell();
		if (cell == null)
		{
			return;
		}
		cell = cell.GetEmptyAdjacentCells(1, 5).GetRandomElement();
		if (cell != null)
		{
			wraith = The.ZoneManager.peekCachedObject(wraithID).DeepCopy(CopyEffects: false, CopyID: true);
			int num = MyPowerLoadBonus(int.MinValue, 100, 10);
			if (num != 0 && wraith.HasStat("Speed"))
			{
				wraith.GetStat("Speed").BaseValue += num;
			}
			Temporary.AddHierarchically(wraith, 0, null, ParentObject, RootObjectValidateEveryTurn: true);
			if (WillTakeOnAttitudesOf(equipped))
			{
				wraith.TakeAllegiance<AllySummon>(equipped);
			}
			wraith.MakeActive();
			equipped.PlayWorldOrUISound("Sounds/Interact/sfx_interact_phylactery_on");
			cell.AddObject(wraith);
			wraith.TeleportSwirl(null, "&B", Voluntary: true);
			IComponent<GameObject>.XDidYToZ(equipped, "activate", wraith);
			IComponent<GameObject>.XDidY(wraith, "appear");
		}
	}

	public bool WillBeHostileTowards(GameObject Actor = null, GameObject Object = null)
	{
		if (Object == null)
		{
			Object = Actor;
		}
		if (GameObject.Validate(ref Object))
		{
			if (WillTakeOnAttitudesOf(Actor))
			{
				if (Actor == null && Object == null)
				{
					return false;
				}
				if (Actor == Object || !Actor.IsHostileTowards(Object))
				{
					return false;
				}
			}
			else if (wraithID != null && !The.ZoneManager.peekCachedObject(wraithID).IsHostileTowards(Object))
			{
				return false;
			}
		}
		return true;
	}

	public bool WillTakeOnAttitudesOf(GameObject Actor)
	{
		if (IsHacked())
		{
			return true;
		}
		if (GameObject.Validate(ref Actor) && Actor.HasProperty("PsychicHunter") && Actor.HasPart<Extradimensional>() && !Actor.IsPlayerControlled())
		{
			return true;
		}
		return false;
	}

	public bool IsHacked()
	{
		return ParentObject.GetIntProperty("SifrahTemplarPhylacteryHack") > 0;
	}

	public void HackingResultSuccess(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj != ParentObject)
		{
			return;
		}
		ParentObject.ModIntProperty("SifrahTemplarPhylacteryHack", 1);
		if (ParentObject.GetIntProperty("SifrahTemplarPhylacteryHack", 1) > 0)
		{
			ChargeUse = 100;
			if (who.IsPlayer())
			{
				Popup.Show("You hack " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + ".");
			}
		}
		else if (who.IsPlayer())
		{
			Popup.Show("You feel like you're making progress on hacking " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + ".");
		}
	}

	public void HackingResultExceptionalSuccess(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject)
		{
			ParentObject.SetIntProperty("SifrahTemplarPhylacteryHack", 1);
			ChargeUse = 10;
			if (who.IsPlayer())
			{
				Popup.Show("You hack " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + ", and find a way to reduce " + obj.its + " power consumption in the process!");
			}
		}
	}

	public void HackingResultPartialSuccess(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject && who.IsPlayer())
		{
			Popup.Show("You feel like you're making progress on hacking " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + ".");
		}
	}

	public void HackingResultFailure(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject)
		{
			ParentObject.ModIntProperty("SifrahTemplarPhylacteryHack", -1);
			if (who.IsPlayer())
			{
				Popup.Show("You cannot seem to work out how to hack " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + ".");
			}
		}
	}

	public void HackingResultCriticalFailure(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj != ParentObject)
		{
			return;
		}
		ParentObject.ModIntProperty("SifrahTemplarPhylacteryHack", -1);
		if (who.HasPart<Dystechnia>())
		{
			Dystechnia.CauseExplosion(ParentObject, who);
			game.RequestInterfaceExit();
			return;
		}
		if (who.IsPlayer())
		{
			Popup.Show("Your attempt to hack " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " has gone very wrong.");
		}
		List<Cell> localAdjacentCells = ParentObject.CurrentCell.GetLocalAdjacentCells();
		Cell cell = who.CurrentCell;
		ParentObject.UseCharge(Stat.Random(5000, 15000), LiveOnly: false, 0L);
		ParentObject.Discharge((cell != null && localAdjacentCells.Contains(cell)) ? cell : localAdjacentCells.GetRandomElement(), "3d8".RollCached(), 0, "2d4", null, who, obj);
	}
}
