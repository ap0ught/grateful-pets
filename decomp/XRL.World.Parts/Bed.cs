using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Bed : IActivePart
{
	public string SleepEvent1;

	public string SleepEvent2;

	public string SleepEvent3;

	public int SleepEventTurns1;

	public int SleepEventTurns2;

	public int SleepEventTurns3;

	public int SleepEventLevel1;

	public int SleepEventLevel2;

	public int SleepEventLevel3;

	public bool NoSmartUse;

	public bool SleepEventSendSource1;

	public bool SleepEventSendSource2;

	public bool SleepEventSendSource3;

	public bool BreakIfGiganticOnNonGigantic = true;

	public string BreakIfUserHasPart;

	public int HealCounter;

	public long lastIdleUsed;

	public Bed()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		Bed bed = p as Bed;
		if (bed.SleepEvent1 != SleepEvent1)
		{
			return false;
		}
		if (bed.SleepEvent2 != SleepEvent2)
		{
			return false;
		}
		if (bed.SleepEvent3 != SleepEvent3)
		{
			return false;
		}
		if (bed.SleepEventTurns1 != SleepEventTurns1)
		{
			return false;
		}
		if (bed.SleepEventTurns2 != SleepEventTurns2)
		{
			return false;
		}
		if (bed.SleepEventTurns3 != SleepEventTurns3)
		{
			return false;
		}
		if (bed.SleepEventLevel1 != SleepEventLevel1)
		{
			return false;
		}
		if (bed.SleepEventLevel2 != SleepEventLevel2)
		{
			return false;
		}
		if (bed.SleepEventLevel3 != SleepEventLevel3)
		{
			return false;
		}
		if (bed.NoSmartUse != NoSmartUse)
		{
			return false;
		}
		if (bed.SleepEventSendSource1 != SleepEventSendSource1)
		{
			return false;
		}
		if (bed.SleepEventSendSource2 != SleepEventSendSource2)
		{
			return false;
		}
		if (bed.SleepEventSendSource3 != SleepEventSendSource3)
		{
			return false;
		}
		if (bed.BreakIfGiganticOnNonGigantic != BreakIfGiganticOnNonGigantic)
		{
			return false;
		}
		if (bed.BreakIfUserHasPart != BreakIfUserHasPart)
		{
			return false;
		}
		if (bed.HealCounter != HealCounter)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AddedToInventoryEvent.ID && ID != BeforeDestroyObjectEvent.ID && (ID != CanSmartUseEvent.ID || NoSmartUse) && (ID != CommandSmartUseEvent.ID || NoSmartUse) && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<IdleQueryEvent>.ID && ID != InventoryActionEvent.ID && ID != LeftCellEvent.ID && ID != PooledEvent<PollForHealingLocationEvent>.ID)
		{
			return ID == PooledEvent<UseHealingLocationEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		DetachEffects((Cell)null, (IEvent)E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AddedToInventoryEvent E)
	{
		DetachEffects((Cell)null, (IEvent)E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		DetachEffects(E.Cell, E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (!NoSmartUse && E.Actor != ParentObject && E.Actor.PhaseMatches(ParentObject) && E.Actor.FlightCanReach(ParentObject))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (!NoSmartUse)
		{
			AttemptSleep(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.Understood() && E.Actor.PhaseMatches(ParentObject) && E.Actor.FlightCanReach(ParentObject))
		{
			E.AddAction("Sleep", "sleep", "SleepOnBed", null, 's', FireOnActor: false, 10);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "SleepOnBed" && AttemptSleep(E.Actor))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (ParentObject.GetIntProperty("DroppedByPlayer") != 0)
		{
			return base.HandleEvent(E);
		}
		if (E.Actor == ParentObject)
		{
			return base.HandleEvent(E);
		}
		string owner = ParentObject.Owner;
		if (!owner.IsNullOrEmpty() && !E.Actor.HasTagOrProperty(owner) && E.Actor.DisplayNameOnly != owner && !E.Actor.BelongsToFaction(owner))
		{
			return base.HandleEvent(E);
		}
		bool flag = E.Actor.HasTag("Nocturnal");
		if ((!flag && IsNight()) || (flag && IsDay()))
		{
			Cell cell = ParentObject.CurrentCell;
			if (E.Actor.Brain != null && E.Actor.HasTagOrProperty("SleepOnBed") && !E.Actor.HasTagOrProperty("NoSleep") && !E.Actor.HasPart<Robot>() && !E.Actor.IsFlying && cell.IsEmptyOfSolid() && cell.GetNavigationWeightFor(E.Actor) < 30)
			{
				if (IComponent<GameObject>.currentTurn - lastIdleUsed < 50)
				{
					return base.HandleEvent(E);
				}
				lastIdleUsed = IComponent<GameObject>.currentTurn;
				GameObject who = E.Actor;
				who.Brain.PushGoal(new DelegateGoal((GoalHandler h) =>
				{
					if (who.CurrentCell.Objects.Contains(ParentObject))
					{
						AttemptSleep(who);
					}
					h.FailToParent();
				}));
				who.Brain.PushGoal(new MoveTo(ParentObject));
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PollForHealingLocationEvent E)
	{
		if (E.Actor != ParentObject && E.Actor.PhaseMatches(ParentObject) && E.Actor.FlightCanReach(ParentObject))
		{
			E.Value = Math.Max(E.Value, Tier.Constrain(ParentObject.GetTier() / 2));
			if (E.First)
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UseHealingLocationEvent E)
	{
		if (E.Actor != ParentObject && !E.Actor.HasEffect<Asleep>() && AttemptSleep(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool AttemptSleep(GameObject who)
	{
		AttemptSleep(who, out var SleepSuccessful, out var MoveFailed, out var Broke);
		return SleepSuccessful | MoveFailed | Broke;
	}

	public void AttemptSleep(GameObject Actor, out bool SleepSuccessful, out bool MoveFailed, out bool Broke)
	{
		SleepSuccessful = false;
		MoveFailed = false;
		Broke = false;
		if (Actor == ParentObject)
		{
			Actor.ShowFailure("You cannot sleep on " + Actor.itself + ".");
			return;
		}
		if (ParentObject.CurrentCell == null)
		{
			Actor.ShowFailure(ParentObject.T() + " must be laid out before you can do that.");
			return;
		}
		switch (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: true, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: true, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
		case ActivePartStatus.Broken:
			Actor.ShowFailure(ParentObject.Itis + " broken.");
			break;
		default:
			Actor.ShowFailure(ParentObject.Does("don't") + " seem to be working.");
			break;
		case ActivePartStatus.Operational:
		{
			Hangable part = ParentObject.GetPart<Hangable>();
			if (part != null && !part.Hanging)
			{
				Actor.ShowFailure(ParentObject.T() + " must be hung up before you can do that.");
			}
			else if (Actor.HasEffect<Sitting>())
			{
				Actor.ShowFailure("You cannot do that while sitting.");
			}
			else if (Actor.HasEffect<Burrowed>())
			{
				Actor.ShowFailure("You cannot do that while burrowed.");
			}
			else
			{
				if (!Actor.CanChangeBodyPosition("Prone", ShowMessage: true))
				{
					break;
				}
				if (Actor.IsFlying)
				{
					Actor.ShowFailure("You cannot do that while flying.");
					break;
				}
				if (!Actor.FlightCanReach(ParentObject))
				{
					Actor.ShowFailure("You cannot reach " + ParentObject.t() + ".");
					break;
				}
				if (!Actor.PhaseMatches(ParentObject))
				{
					Actor.ShowFailure("You are out of phase with " + ParentObject.t() + ".");
					break;
				}
				if (!Actor.FireEvent("CanApplySleep"))
				{
					Actor.ShowFailure("You can't go to sleep right now.");
					break;
				}
				if (Actor.IsPlayer())
				{
					ParentObject.SetIntProperty("DroppedByPlayer", 1);
				}
				Cell cell = ParentObject.CurrentCell;
				if (Actor.CurrentCell == cell || (Actor.Move(Actor.GetDirectionToward(ParentObject), Forced: false, System: false, IgnoreGravity: false, NoStack: false, AllowDashing: false, DoConfirmations: false, null, null, NearestAvailable: false, null, null, null, Peaceful: true) && Actor.CurrentCell == cell))
				{
					int num;
					if (Actor.IsPlayer())
					{
						List<string> list = new List<string>();
						list.Add("Until " + Calendar.GetTime((int)(Calendar.TotalTimeTicks + 150) % 1200));
						list.Add("Until " + Calendar.GetTime((int)(Calendar.TotalTimeTicks + 375) % 1200));
						list.Add("Until " + Calendar.GetTime((int)(Calendar.TotalTimeTicks + 600) % 1200));
						num = Popup.PickOption("How long would you like to sleep?", null, "", "Sounds/UI/ui_notification", list.ToArray(), null, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
					}
					else
					{
						num = Stat.Random(0, 2);
					}
					if (num < 0)
					{
						break;
					}
					int duration = 0;
					switch (num)
					{
					case 0:
						duration = 150;
						break;
					case 1:
						duration = 375;
						break;
					case 2:
						duration = 600;
						break;
					}
					try
					{
						if (((!BreakIfUserHasPart.IsNullOrEmpty() && Actor.HasPart(BreakIfUserHasPart)) || (BreakIfGiganticOnNonGigantic && Actor.IsGiganticCreature && !ParentObject.IsGiganticEquipment)) && ParentObject.ApplyEffect(new Broken()) && IsBroken())
						{
							Broke = true;
							Actor.ShowFailure("You think you broke " + ParentObject.them + "...");
							break;
						}
						Actor.ForceApplyEffect(new Asleep(ParentObject, duration, forced: true, quicksleep: true, Voluntary: true));
						if (Actor.IsPlayer() && ParentObject.Blueprint == "Bedroll")
						{
							MetricsManager.LogEvent("BedrollsSlept");
						}
						Event e = Event.New("SleptIn", "Actor", Actor, "Object", ParentObject);
						ParentObject.FireEvent(e);
						Actor.FireEvent(e);
						SleepSuccessful = true;
						break;
					}
					catch (Exception message)
					{
						MetricsManager.LogError(message);
						break;
					}
				}
				MoveFailed = true;
			}
			break;
		}
		}
	}

	private void SendEvent(GameObject Actor, string ID, int Level, bool SendSource)
	{
		if (SendSource || Level != 0)
		{
			Event obj = Event.New(ID);
			if (SendSource)
			{
				obj.SetParameter("Source", ParentObject);
			}
			if (Level != 0)
			{
				obj.SetParameter("Level", Level);
			}
			Actor.FireEvent(obj);
		}
		else
		{
			Actor.FireEvent(ID);
		}
	}

	public void ProcessTurnAsleep(GameObject Actor, int TurnsAsleep)
	{
		if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (++HealCounter >= 10 - ParentObject.GetTier())
			{
				Actor.Heal(1);
				HealCounter = 0;
			}
			if (!SleepEvent1.IsNullOrEmpty() && SleepEventTurns1 > 0 && TurnsAsleep % SleepEventTurns1 == 0)
			{
				SendEvent(Actor, SleepEvent1, SleepEventLevel1, SleepEventSendSource1);
			}
			if (!SleepEvent2.IsNullOrEmpty() && SleepEventTurns2 > 0 && TurnsAsleep % SleepEventTurns2 == 0)
			{
				SendEvent(Actor, SleepEvent2, SleepEventLevel2, SleepEventSendSource2);
			}
			if (!SleepEvent3.IsNullOrEmpty() && SleepEventTurns3 > 0 && TurnsAsleep % SleepEventTurns3 == 0)
			{
				SendEvent(Actor, SleepEvent3, SleepEventLevel3, SleepEventSendSource3);
			}
		}
	}

	private void DetachEffects(GameObject GO, IEvent Event)
	{
		Asleep effect = GO.GetEffect<Asleep>();
		if (effect != null && effect.AsleepOn == ParentObject)
		{
			GO.FireEvent("WakeUp");
			GO.RemoveEffect(effect);
			Event?.RequestInterfaceExit();
		}
	}

	private void DetachEffects(Cell C = null, IEvent Event = null)
	{
		if (C == null)
		{
			C = ParentObject.GetCurrentCell();
		}
		C?.SafeForeachObject((GameObject o) =>
		{
			DetachEffects(o, Event);
		});
	}
}
