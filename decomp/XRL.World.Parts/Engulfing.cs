using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Engulfing : IPoweredPart
{
	public static readonly string COMMAND_NAME = "CommandEngulf";

	public int AVBonus;

	public int DVPenalty;

	public int EnterSaveTarget;

	public int ExitSaveTarget;

	public int DamageChance;

	public int EnterDamageChance;

	public int ExitDamageChance;

	public int DamageBloodSplatterChance = 50;

	public int PeriodicEventTurns1;

	public int PeriodicEventTurns2;

	public int PeriodicEventTurns3;

	public string _AffectedProperties;

	public string EnterSaveStat = "Agility";

	public string ExitSaveStat = "Strength";

	public string Damage;

	public string DamageAttributes;

	public string EnterEventSelf;

	public string EnterEventUser;

	public string ExitEventSelf;

	public string ExitEventUser;

	public string ApplyChangesEventSelf;

	public string ApplyChangesEventUser;

	public string UnapplyChangesEventSelf;

	public string UnapplyChangesEventUser;

	public string EffectDescriptionPrefix;

	public string EffectDescriptionPostfix;

	public string PeriodicEvent1;

	public string PeriodicEvent2;

	public string PeriodicEvent3;

	public bool SizeInsensitive = true;

	public bool NoDamageWhenDisabled;

	public bool EnterDamageFailOnly;

	public bool ExitDamageFailOnly;

	public bool Pull;

	public GameObject Engulfed;

	public Guid ActivatedAbilityID;

	private Dictionary<string, int> _PropertyMap;

	public string AffectedProperties
	{
		get
		{
			return _AffectedProperties;
		}
		set
		{
			_AffectedProperties = value;
			_PropertyMap = null;
		}
	}

	public Dictionary<string, int> PropertyMap
	{
		get
		{
			if (_PropertyMap == null)
			{
				_PropertyMap = IComponent<GameObject>.MapFromString(_AffectedProperties);
			}
			return _PropertyMap;
		}
	}

	public Engulfing()
	{
		ChargeUse = 0;
		IsEMPSensitive = false;
		WorksOnSelf = true;
		IsBioScannable = true;
	}

	public override bool SameAs(IPart p)
	{
		Engulfing engulfing = p as Engulfing;
		if (engulfing.AVBonus != AVBonus)
		{
			return false;
		}
		if (engulfing.DVPenalty != DVPenalty)
		{
			return false;
		}
		if (engulfing.EnterSaveTarget != EnterSaveTarget)
		{
			return false;
		}
		if (engulfing.ExitSaveTarget != ExitSaveTarget)
		{
			return false;
		}
		if (engulfing.DamageChance != DamageChance)
		{
			return false;
		}
		if (engulfing.EnterDamageChance != EnterDamageChance)
		{
			return false;
		}
		if (engulfing.ExitDamageChance != ExitDamageChance)
		{
			return false;
		}
		if (engulfing.DamageBloodSplatterChance != DamageBloodSplatterChance)
		{
			return false;
		}
		if (engulfing.PeriodicEventTurns1 != PeriodicEventTurns1)
		{
			return false;
		}
		if (engulfing.PeriodicEventTurns2 != PeriodicEventTurns2)
		{
			return false;
		}
		if (engulfing.PeriodicEventTurns3 != PeriodicEventTurns3)
		{
			return false;
		}
		if (engulfing.AffectedProperties != AffectedProperties)
		{
			return false;
		}
		if (engulfing.EnterSaveStat != EnterSaveStat)
		{
			return false;
		}
		if (engulfing.ExitSaveStat != ExitSaveStat)
		{
			return false;
		}
		if (engulfing.Damage != Damage)
		{
			return false;
		}
		if (engulfing.DamageAttributes != DamageAttributes)
		{
			return false;
		}
		if (engulfing.EnterEventSelf != EnterEventSelf)
		{
			return false;
		}
		if (engulfing.EnterEventUser != EnterEventUser)
		{
			return false;
		}
		if (engulfing.ExitEventSelf != ExitEventSelf)
		{
			return false;
		}
		if (engulfing.ExitEventUser != ExitEventUser)
		{
			return false;
		}
		if (engulfing.ApplyChangesEventSelf != ApplyChangesEventSelf)
		{
			return false;
		}
		if (engulfing.ApplyChangesEventUser != ApplyChangesEventUser)
		{
			return false;
		}
		if (engulfing.UnapplyChangesEventSelf != UnapplyChangesEventSelf)
		{
			return false;
		}
		if (engulfing.UnapplyChangesEventUser != UnapplyChangesEventUser)
		{
			return false;
		}
		if (engulfing.EffectDescriptionPrefix != EffectDescriptionPrefix)
		{
			return false;
		}
		if (engulfing.EffectDescriptionPostfix != EffectDescriptionPostfix)
		{
			return false;
		}
		if (engulfing.PeriodicEvent1 != PeriodicEvent1)
		{
			return false;
		}
		if (engulfing.PeriodicEvent2 != PeriodicEvent2)
		{
			return false;
		}
		if (engulfing.PeriodicEvent3 != PeriodicEvent3)
		{
			return false;
		}
		if (engulfing.SizeInsensitive != SizeInsensitive)
		{
			return false;
		}
		if (engulfing.NoDamageWhenDisabled != NoDamageWhenDisabled)
		{
			return false;
		}
		if (engulfing.EnterDamageFailOnly != EnterDamageFailOnly)
		{
			return false;
		}
		if (engulfing.ExitDamageFailOnly != ExitDamageFailOnly)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public virtual void CollectStats(Templates.StatCollector stats)
	{
		string text = "";
		if (ParentObject.HasPart("EngulfingDamage"))
		{
			text += "Hey you do damage!";
		}
		stats.Set("Postfix", text);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != PooledEvent<CanChangeMovementModeEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != EnteredCellEvent.ID && ID != OnDestroyObjectEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			Cell cell = E.TargetCell ?? PickDirection("Engulf");
			if (cell == null)
			{
				return false;
			}
			GameObject combatTarget = cell.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: false);
			if (combatTarget == null || combatTarget == ParentObject)
			{
				return false;
			}
			if (!Engulf(combatTarget))
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanChangeMovementModeEvent E)
	{
		if (E.Object == ParentObject && E.To == "Flying" && CheckEngulfed())
		{
			if (E.ShowMessage)
			{
				E.Object.Fail("You cannot fly while engulfing " + Engulfed.t() + ".");
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (!CheckEngulfed() && E.Distance == 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.Target.IsCombatObject() && E.Actor.PhaseAndFlightMatches(E.Target) && !E.Actor.IsInStasis())
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (CheckEngulfed())
		{
			if (ParentObject.CurrentCell != null)
			{
				if (Engulfed.CurrentCell != null && Engulfed.CurrentCell != ParentObject.CurrentCell)
				{
					Engulfed.CurrentCell.RemoveObject(Engulfed);
				}
				if (!ParentObject.CurrentCell.Objects.Contains(Engulfed))
				{
					ParentObject.CurrentCell.AddObject(Engulfed);
				}
			}
			Engulfed.FireEvent(Event.New("EngulfDragged", "Object", ParentObject));
			ParentObject.FireEvent(Event.New("EngulferDragged", "Object", Engulfed));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (CheckEngulfed() && !ParentObject.Brain.HasGoal("FleeLocation"))
		{
			ParentObject.Brain.Goals.Clear();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		EndAllEngulfment();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginBeingTaken");
		Registrar.Register("ObjectEnteredAdjacentCell");
		base.Register(Object, Registrar);
	}

	public override void Initialize()
	{
		base.Initialize();
		ActivatedAbilityID = AddMyActivatedAbility("Engulf", COMMAND_NAME, "Physical Mutations", null, "@");
	}

	public bool PerformDamage(GameObject who)
	{
		if (Damage.IsNullOrEmpty())
		{
			return false;
		}
		bool flag = who.TakeDamage(Damage.RollCached(), "from %O!", DamageAttributes, null, null, ParentObject);
		if (flag && DamageBloodSplatterChance.in100())
		{
			who.Bloodsplatter();
		}
		return flag;
	}

	public bool CheckEnterDamage(GameObject who, bool Failed)
	{
		if (EnterDamageChance <= 0)
		{
			return false;
		}
		if (EnterDamageFailOnly && !Failed)
		{
			return false;
		}
		if (NoDamageWhenDisabled && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		if (!EnterDamageChance.in100())
		{
			return false;
		}
		return PerformDamage(who);
	}

	public bool CheckExitDamage(GameObject who, bool Failed)
	{
		if (ExitDamageChance <= 0)
		{
			return false;
		}
		if (ExitDamageFailOnly && !Failed)
		{
			return false;
		}
		if (NoDamageWhenDisabled && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		if (!ExitDamageChance.in100())
		{
			return false;
		}
		return PerformDamage(who);
	}

	public bool CheckPeriodicDamage(GameObject who)
	{
		if (DamageChance <= 0)
		{
			return false;
		}
		if (NoDamageWhenDisabled && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		if (!DamageChance.in100())
		{
			return false;
		}
		return PerformDamage(who);
	}

	public override bool Render(RenderEvent E)
	{
		if (GameObject.Validate(ref Engulfed))
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num >= 31 && num <= 60)
			{
				E.ColorString = Engulfed.Render.ColorString;
				E.DetailColor = Engulfed.Render.DetailColor;
				E.Tile = Engulfed.Render.Tile;
				E.RenderString = Engulfed.Render.RenderString;
			}
		}
		return base.Render(E);
	}

	public bool Engulf(GameObject who, Event E = null)
	{
		if (CheckEngulfed())
		{
			EndEngulfment(Engulfed);
			if (Engulfed != null)
			{
				return false;
			}
		}
		if (who.SameAs(ParentObject))
		{
			return false;
		}
		if (!SizeInsensitive && who.IsGiganticCreature && !ParentObject.IsGiganticCreature)
		{
			ParentObject.ShowFailure(who.Does("are") + " too large for you to engulf.");
			return false;
		}
		if (!who.CanChangeMovementMode("Engulfed", ShowMessage: false, Involuntary: true, AllowTelekinetic: false, FrozenOkay: true) || !who.CanChangeBodyPosition("Engulfed", ShowMessage: false, Involuntary: true, AllowTelekinetic: false, FrozenOkay: true))
		{
			ParentObject.ShowFailure("You cannot do that while " + who.does("are") + " in " + who.its + " present situation.");
			return false;
		}
		if (!who.PhaseAndFlightMatches(ParentObject))
		{
			return false;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		if (EnterSaveTarget > 0 && who.MakeSave(EnterSaveStat, EnterSaveTarget, ParentObject, "Strength", "Engulfment"))
		{
			if (ParentObject.IsPlayer())
			{
				ParentObject.Fail("You fail to engulf " + who.t() + ".");
			}
			else if (who.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("try") + " to engulf you, but" + ParentObject.GetVerb("fail") + ".");
			}
			else if (IComponent<GameObject>.Visible(ParentObject))
			{
				IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("try") + " to engulf " + who.t() + ", but" + ParentObject.GetVerb("fail") + ".");
			}
			CheckEnterDamage(who, Failed: true);
			ParentObject.UseEnergy(1000, "Position");
			E?.RequestInterfaceExit();
			return false;
		}
		if (Pull)
		{
			if (who.CurrentCell != ParentObject.CurrentCell && !who.DirectMoveTo(ParentObject.CurrentCell, 0, Forced: false, IgnoreCombat: true))
			{
				E?.RequestInterfaceExit();
				return false;
			}
		}
		else if (who.CurrentCell != cell && !ParentObject.DirectMoveTo(who.CurrentCell, 0, Forced: false, IgnoreCombat: true))
		{
			E?.RequestInterfaceExit();
			return false;
		}
		CheckEnterDamage(who, Failed: false);
		IComponent<GameObject>.XDidYToZ(who, "are", "engulfed by", ParentObject, null, null, null, null, null, who, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
		if (!EnterEventSelf.IsNullOrEmpty())
		{
			ParentObject.FireEvent(EnterEventSelf);
		}
		if (!EnterEventUser.IsNullOrEmpty())
		{
			who.FireEvent(EnterEventUser);
		}
		Engulfed = who;
		who.ApplyEffect(new Engulfed(ParentObject));
		ParentObject.UseEnergy(1000, "Position");
		ParentObject.FireEvent(Event.New("Engulfed", "Object", who));
		who.FireEvent(Event.New("ObjectEngulfed", "Object", ParentObject));
		E?.RequestInterfaceExit();
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectEnteredAdjacentCell")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (gameObjectParameter != null && ParentObject.IsHostileTowards(gameObjectParameter) && CheckEngulfed() && !gameObjectParameter.MakeSave("Strength", 20, null, null, "Engulfment"))
			{
				Engulf(gameObjectParameter);
			}
		}
		else if (E.ID == "BeginBeingTaken")
		{
			EndAllEngulfment();
		}
		return base.FireEvent(E);
	}

	public bool EndEngulfment(GameObject who, Event E = null, Engulfed enc = null)
	{
		if (who == null)
		{
			Engulfed = null;
			return true;
		}
		if (enc == null)
		{
			enc = who.GetEffect<Engulfed>();
			if (enc == null)
			{
				Engulfed = null;
				return true;
			}
		}
		if (enc.EngulfedBy != ParentObject)
		{
			Engulfed = null;
			return true;
		}
		CheckExitDamage(who, Failed: true);
		if (!ExitEventSelf.IsNullOrEmpty())
		{
			ParentObject.FireEvent(ExitEventSelf);
		}
		if (!ExitEventUser.IsNullOrEmpty())
		{
			who.FireEvent(ExitEventUser);
		}
		who.RemoveEffect(enc);
		who.UseEnergy(1000, "Position");
		who.FireEvent(Event.New("Exited", "Object", ParentObject));
		ParentObject.FireEvent(Event.New("ObjectExited", "Object", who));
		E?.RequestInterfaceExit();
		Engulfed = null;
		return true;
	}

	public void EndAllEngulfment()
	{
		ParentObject.CurrentCell?.ForeachObject((GameObject obj) =>
		{
			Engulfed effect = obj.GetEffect<Engulfed>();
			if (effect != null && effect.EngulfedBy == ParentObject)
			{
				obj.RemoveEffect(effect);
			}
		});
	}

	public void ProcessTurnEngulfed(GameObject who, int TurnsEngulfed)
	{
		bool flag = IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (!ParentObject.IsPlayer() && who.InSamePartyAs(ParentObject))
		{
			EndAllEngulfment();
			return;
		}
		who.FireEvent(Event.New("StartTurnEngulfed", "Object", ParentObject, "Disabled", flag ? 1 : 0));
		ParentObject.FireEvent(Event.New("StartTurnEngulfing", "Object", who, "Disabled", flag ? 1 : 0));
		CheckPeriodicDamage(who);
		if (!flag)
		{
			if (!PeriodicEvent1.IsNullOrEmpty() && PeriodicEventTurns1 > 0 && TurnsEngulfed % PeriodicEventTurns1 == 0)
			{
				who.FireEvent(PeriodicEvent1);
			}
			if (!PeriodicEvent2.IsNullOrEmpty() && PeriodicEventTurns2 > 0 && TurnsEngulfed % PeriodicEventTurns2 == 0)
			{
				who.FireEvent(PeriodicEvent2);
			}
			if (!PeriodicEvent3.IsNullOrEmpty() && PeriodicEventTurns3 > 0 && TurnsEngulfed % PeriodicEventTurns3 == 0)
			{
				who.FireEvent(PeriodicEvent3);
			}
		}
		who.FireEvent(Event.New("EndTurnEngulfed", "Object", ParentObject, "Disabled", flag ? 1 : 0));
		ParentObject.FireEvent(Event.New("EndTurnEngulfing", "Object", who, "Disabled", flag ? 1 : 0));
	}

	public bool IsOurEffect(Effect FX)
	{
		if (FX is Engulfed engulfed)
		{
			return engulfed.EngulfedBy == ParentObject;
		}
		return false;
	}

	public bool CheckEngulfed()
	{
		if (GameObject.Validate(ref Engulfed) && !Engulfed.HasEffect(typeof(Engulfed), IsOurEffect))
		{
			Engulfed = null;
		}
		return Engulfed != null;
	}
}
