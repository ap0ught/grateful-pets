using System;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: the player's effective level for dominating the
/// target is increased by the standard power load bonus, i.e. 2 for the
/// standard overload power load of 400, and charge use is increased
/// treating the power load as a percentage.
/// </remarks>
[Serializable]
public class Teleprojector : IPart, IHackingSifrahHandler
{
	public int InitialChargeUse = 1000;

	public int MaintainChargeUse = 10;

	public Guid ActivatedAbilityID = Guid.Empty;

	public GameObject Target;

	[NonSerialized]
	private int HackPerformance;

	public override bool SameAs(IPart p)
	{
		Teleprojector teleprojector = p as Teleprojector;
		if (teleprojector.InitialChargeUse != InitialChargeUse)
		{
			return false;
		}
		if (teleprojector.MaintainChargeUse != MaintainChargeUse)
		{
			return false;
		}
		if (teleprojector.ActivatedAbilityID != ActivatedAbilityID)
		{
			return false;
		}
		if (teleprojector.Target != Target)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public int GetCooldown()
	{
		return 200;
	}

	public int GetDuration()
	{
		return 400;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		if (ParentObject.Equipped != null)
		{
			ActivatedAbilityEntry ability = MyActivatedAbility(ActivatedAbilityID, ParentObject.Equipped);
			int num = MyPowerLoadBonus();
			if (num > 0)
			{
				stats.postfix += $"\nAttack roll increased by {num} due to high power load.";
			}
			if (num < 0)
			{
				stats.postfix += $"\nAttack roll reduced by {-num} due to low power load.";
			}
			int num2 = GetAvailableComputePowerEvent.GetFor(ParentObject.Equipped) / 5;
			stats.AddComputePowerPostfix("Attack roll", num2);
			int num3 = ParentObject.Equipped.GetStat("Level").Value + 4;
			int num4 = num3 + num + num2;
			stats.Set("Attack", "1d8+" + num4, num4 != num3, num4 - num3);
			int num5 = stats.CollectComputePowerAdjustUp(ability, "Duration", GetDuration());
			stats.Set("Duration", num5, num5 != GetDuration(), num5 - GetDuration());
			stats.CollectCooldownTurns(ability, GetCooldown());
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BootSequenceDoneEvent.ID && ID != BootSequenceInitializedEvent.ID && ID != EquippedEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != InventoryActionEvent.ID && ID != PooledEvent<IsOverloadableEvent>.ID && ID != UnequippedEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject.Equipped);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ActivatedAbilityID != Guid.Empty)
		{
			E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "ActivateTeleprojector");
		E.Actor.RegisterPartEvent(this, "BeginTakeAction");
		E.Actor.RegisterPartEvent(this, "ChainInterruptDomination");
		E.Actor.RegisterPartEvent(this, "DominationBroken");
		E.Actor.RegisterPartEvent(this, "EarlyBeforeDeathRemoval");
		E.Actor.RegisterPartEvent(this, "TookDamage");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		EndDomination(E.Actor);
		E.Actor.UnregisterPartEvent(this, "ActivateTeleprojector");
		E.Actor.UnregisterPartEvent(this, "BeginTakeAction");
		E.Actor.UnregisterPartEvent(this, "ChainInterruptDomination");
		E.Actor.UnregisterPartEvent(this, "DominationBroken");
		E.Actor.UnregisterPartEvent(this, "EarlyBeforeDeathRemoval");
		RemoveAbility(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceDoneEvent E)
	{
		if (ParentObject.Equipped != null)
		{
			if (ParentObject.Equipped.IsPlayer())
			{
				Popup.Show(ParentObject.Does("attune") + " to your physiology.");
			}
			ActivatedAbilityID = ParentObject.Equipped.AddActivatedAbility("Activate " + Grammar.MakeTitleCase(ParentObject.BaseDisplayNameStripped), "ActivateTeleprojector", "Items", null, "÷");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceInitializedEvent E)
	{
		RemoveAbility();
		EndDomination();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsOverloadableEvent E)
	{
		return false;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IsActive())
		{
			E.AddAction("Activate", "activate", "ActivateTeleprojector", null, 'a', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: false);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateTeleprojector" && ActivateTeleprojector())
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ActivateTeleprojector");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DominationBroken")
		{
			EndDomination();
		}
		else if (E.ID == "ActivateTeleprojector")
		{
			if (ActivateTeleprojector())
			{
				E.RequestInterfaceExit();
			}
		}
		else if (E.ID == "BeginTakeAction")
		{
			if (GameObject.Validate(ref Target) && MaintainChargeUse > 0 && !ParentObject.UseCharge(MaintainChargeUse * MyPowerLoadLevel() / 100, LiveOnly: false, 0L))
			{
				InterruptDomination();
			}
		}
		else if (E.ID == "ChainInterruptDomination")
		{
			if (GameObject.Validate(ref Target) && !Target.FireEvent("InterruptDomination"))
			{
				return false;
			}
		}
		else if (E.ID == "EarlyBeforeDeathRemoval")
		{
			PerformMetempsychosis();
		}
		return base.FireEvent(E);
	}

	private bool ActivateTeleprojector()
	{
		if (!IsActive())
		{
			return false;
		}
		GameObject actor = ParentObject.Equipped;
		if (!actor.IsPlayer())
		{
			return false;
		}
		if (!actor.IsActivatedAbilityUsable(ActivatedAbilityID))
		{
			return actor.Fail(ParentObject.Does("are") + " still cooling down.");
		}
		Cell cell = PickDirection(ForAttack: true, "Activate Teleprojector");
		if (cell == null)
		{
			return false;
		}
		actor?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_mental_generic_activate");
		bool flag = false;
		foreach (GameObject item in cell.GetObjectsWithPart("Robot"))
		{
			if (!item.HasStat("Level"))
			{
				continue;
			}
			flag = true;
			if (item.Brain == null)
			{
				return actor.Fail("There seems to be no digital mind in " + item.t() + " to dominate.");
			}
			if (item.HasCopyRelationship(actor))
			{
				return actor.Fail("You can't dominate " + actor.itself + "!");
			}
			if (item.GetEffect((Dominated e) => e.Dominator == actor) != null)
			{
				return actor.Fail("You can't dominate someone you are already dominating.");
			}
			if (item.HasEffect<Dominated>())
			{
				return actor.Fail("You can't do that.");
			}
			if (!item.FireEvent("CanApplyRoboDomination") || !CanApplyEffectEvent.Check(item, "RoboDomination"))
			{
				return actor.Fail(item.Does("do") + " not have a digital mind you can make electronic contact with.");
			}
			if (!item.CheckInfluence("RoboDomination", actor))
			{
				return false;
			}
			int num = MyPowerLoadLevel();
			if (ParentObject.UseCharge(InitialChargeUse * num / 100, LiveOnly: false, 0L, IncludeTransient: true, IncludeBiological: true, num))
			{
				item.AddOpinion<OpinionDominate>(actor);
				int num2 = IComponent<GameObject>.PowerLoadBonus(num) + actor.Stat("Level") + GetAvailableComputePowerEvent.GetFor(actor) / 5;
				int num3 = item.Stat("Level");
				if (Options.SifrahHacking)
				{
					HackingSifrah hackingSifrah = new HackingSifrah(item, num3 / 5, Stats.GetCombatMA(item) / 2, num2);
					hackingSifrah.HandlerID = ParentObject.ID;
					hackingSifrah.HandlerPartName = "Teleprojector";
					hackingSifrah.Play(item);
					if (hackingSifrah.InterfaceExitRequested)
					{
						return false;
					}
					num2 = num2 * HackPerformance / 100;
				}
				PerformMentalAttack(RoboDom, actor, item, null, "Domination Teleprojector", "1d8+4", 0, GetDuration(), int.MinValue, num2, num3);
				actor.CooldownActivatedAbility(ActivatedAbilityID, GetCooldown());
				return true;
			}
			return actor.Fail(ParentObject.Does("don't") + " have enough charge to function.");
		}
		if (!flag)
		{
			Popup.ShowFail("There is nothing there that " + ParentObject.t() + " can uplink with.");
		}
		return false;
	}

	public bool RoboDom(MentalAttackEvent E)
	{
		GameObject attacker = E.Attacker;
		GameObject defender = E.Defender;
		if (E.Penetrations > 0)
		{
			int duration = GetAvailableComputePowerEvent.AdjustUp(attacker, E.Magnitude);
			Dominated e = new Dominated(attacker, duration, RoboDom: true);
			if (defender.ApplyEffect(e))
			{
				Target = defender;
				defender.Sparksplatter();
				Popup.Show("You take control of " + defender.t() + "!");
				attacker.Brain.PushGoal(new Dormant(-1));
				XRLCore.Core.Game.Player.Body = defender;
				IComponent<GameObject>.ThePlayer.Target = null;
				return true;
			}
		}
		IComponent<GameObject>.XDidYToZ(defender, "resist", attacker, "domination", "!", null, null, defender, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		return false;
	}

	private bool IsActive()
	{
		if (ParentObject.Equipped == null)
		{
			return false;
		}
		BootSequence part = ParentObject.GetPart<BootSequence>();
		if (part != null && part.BootTimeLeft > 0)
		{
			return false;
		}
		return true;
	}

	private void RemoveAbility(GameObject GO = null)
	{
		if (GO == null)
		{
			GO = ParentObject.Equipped;
		}
		GO?.RemoveActivatedAbility(ref ActivatedAbilityID);
		ActivatedAbilityID = Guid.Empty;
	}

	private bool EndDomination(GameObject actor = null)
	{
		if (!GameObject.Validate(ParentObject))
		{
			return false;
		}
		if (!GameObject.Validate(ref Target))
		{
			return false;
		}
		if (actor == null)
		{
			actor = ParentObject.Equipped;
		}
		Dominated effect = Target.GetEffect((Dominated e) => e.Dominator == actor);
		if (effect != null && !effect.BeingRemovedBySource)
		{
			effect.BeingRemovedBySource = true;
			Target.RemoveEffect(effect);
		}
		if (Target.OnWorldMap())
		{
			Target.PullDown();
		}
		Target = null;
		if (actor != null)
		{
			XRLCore.Core.Game.Player.Body = actor;
			IComponent<GameObject>.ThePlayer.Target = null;
			if (actor.IsPlayer())
			{
				Popup.Show("{{r|Your domination is broken!}}");
			}
			actor.Sparksplatter();
			actor.Brain.Goals.Clear();
		}
		Sidebar.UpdateState();
		return true;
	}

	public void InterruptDomination()
	{
		if (GameObject.Validate(ref Target))
		{
			Target.FireEvent("InterruptDomination");
		}
	}

	public bool PerformMetempsychosis(GameObject Actor = null)
	{
		if (!GameObject.Validate(ref Target))
		{
			return false;
		}
		if (Actor == null)
		{
			Actor = ParentObject.Equipped;
		}
		Dominated effect = Target.GetEffect((Dominated e) => e.Dominator == Actor);
		if (effect != null && !effect.BeingRemovedBySource)
		{
			effect.BeingRemovedBySource = true;
			effect.Metempsychosis = true;
			Target.RemoveEffect(effect);
			Domination.Metempsychosis(Target, effect.FromOriginalPlayerBody);
		}
		Target = null;
		return true;
	}

	public void HackingResultSuccess(GameObject Actor, GameObject Object, HackingSifrah Game)
	{
		HackPerformance = 150;
	}

	public void HackingResultExceptionalSuccess(GameObject Actor, GameObject Object, HackingSifrah Game)
	{
		HackPerformance = 300;
	}

	public void HackingResultPartialSuccess(GameObject Actor, GameObject Object, HackingSifrah Game)
	{
		HackPerformance = 100;
	}

	public void HackingResultFailure(GameObject Actor, GameObject Object, HackingSifrah Game)
	{
		HackPerformance = 50;
	}

	public void HackingResultCriticalFailure(GameObject Actor, GameObject Object, HackingSifrah Game)
	{
		HackPerformance = 0;
	}
}
