using System;
using XRL.Rules;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Chair : IActivePart
{
	public int Level;

	public int LevelWhenDisabled = -1;

	public string DamageAttributes;

	public bool NoSmartUse;

	public bool BreakIfGiganticOnNonGigantic = true;

	public bool Securing;

	public long LastIdleUsed;

	[NonSerialized]
	private GameObject Seated;

	public Chair()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		Chair chair = p as Chair;
		if (chair.Level != Level)
		{
			return false;
		}
		if (chair.LevelWhenDisabled != LevelWhenDisabled)
		{
			return false;
		}
		if (chair.DamageAttributes != DamageAttributes)
		{
			return false;
		}
		if (chair.NoSmartUse != NoSmartUse)
		{
			return false;
		}
		if (chair.BreakIfGiganticOnNonGigantic != BreakIfGiganticOnNonGigantic)
		{
			return false;
		}
		if (chair.Securing != Securing)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AddedToInventoryEvent.ID && ID != BeforeDestroyObjectEvent.ID && (ID != CanSmartUseEvent.ID || NoSmartUse) && (ID != CommandSmartUseEvent.ID || NoSmartUse) && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<IdleQueryEvent>.ID && ID != InventoryActionEvent.ID && ID != LeftCellEvent.ID && (ID != EnteredCellEvent.ID || !Securing) && ID != PooledEvent<PollForHealingLocationEvent>.ID)
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
		if (Securing)
		{
			Seated = E.Cell?.GetFirstObject(IsSeated);
		}
		else
		{
			DetachEffects(E.Cell, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (Securing && GameObject.Validate(ref Seated))
		{
			MoveSeated(Seated, E.Cell, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		SyncEffects();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		SyncEffects();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (ChargeUse > 0)
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null && cell.HasObjectWithEffect("Sitting", IsSittingOnThis))
			{
				ConsumeChargeIfOperational();
				SyncEffects();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.Understood())
		{
			if (E.Actor.HasEffect((Sitting fx) => fx.SittingOn == ParentObject))
			{
				E.AddAction("Stand", "stand", "StandUpFromChair", null, 's', FireOnActor: false, 5);
			}
			else
			{
				E.AddAction("Sit", "sit", "SitOnChair", null, 's', FireOnActor: false, 5);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "SitOnChair")
		{
			SitDown(E.Actor, E);
		}
		else if (E.Command == "StandUpFromChair")
		{
			StandUp(E.Actor, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (!NoSmartUse && E.Actor != ParentObject && ParentObject.Understood() && E.Actor.PhaseMatches(ParentObject) && E.Actor.FlightCanReach(ParentObject))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (!NoSmartUse && E.Actor != ParentObject && ParentObject.Understood() && E.Actor.PhaseMatches(ParentObject) && E.Actor.FlightCanReach(ParentObject))
		{
			if (E.Actor.HasEffect((Sitting fx) => fx.SittingOn == ParentObject))
			{
				StandUp(E.Actor, E);
			}
			else
			{
				SitDown(E.Actor, E);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (Level > -10 && E.Actor != ParentObject)
		{
			string owner = ParentObject.Owner;
			if ((owner.IsNullOrEmpty() || E.Actor.HasTagOrProperty(owner) || E.Actor.DisplayNameOnly == owner || E.Actor.BelongsToFaction(owner)) && 1.in100())
			{
				Sitting effect = E.Actor.GetEffect<Sitting>();
				if (effect != null)
				{
					if (effect.SittingOn == ParentObject && StandUp(E.Actor))
					{
						return false;
					}
				}
				else if (IComponent<GameObject>.currentTurn - LastIdleUsed >= 50)
				{
					int num = ParentObject.DistanceTo(E.Actor);
					if (num <= 1)
					{
						LastIdleUsed = IComponent<GameObject>.currentTurn;
						if (SitDown(E.Actor))
						{
							return false;
						}
					}
					else if (Stat.Random(1, 40) > num)
					{
						LastIdleUsed = IComponent<GameObject>.currentTurn;
						GameObject who = E.Actor;
						who.Brain.PushGoal(new DelegateGoal((GoalHandler h) =>
						{
							if (who.CurrentCell.Objects.Contains(ParentObject))
							{
								SitDown(who);
							}
							h.FailToParent();
						}));
						who.Brain.PushGoal(new MoveTo(ParentObject));
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PollForHealingLocationEvent E)
	{
		if (E.Actor != ParentObject && E.Actor.PhaseMatches(ParentObject) && E.Actor.FlightCanReach(ParentObject))
		{
			int num = EffectiveLevel();
			if (num > -10)
			{
				E.Value = Math.Max(E.Value, Math.Min(Math.Max(1, num), 9));
				if (E.First)
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UseHealingLocationEvent E)
	{
		if (EffectiveLevel() > -10 && !E.Actor.HasEffect<Sitting>())
		{
			SitDown(E.Actor, E);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public int EffectiveLevel()
	{
		if (!IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return Level;
		}
		return LevelWhenDisabled;
	}

	private void SyncObjectEffect(GameObject GO)
	{
		Sitting effect = GO.GetEffect((Sitting fx) => fx.SittingOn == ParentObject);
		if (effect != null)
		{
			effect.Level = EffectiveLevel();
			effect.DamageAttributes = DamageAttributes;
		}
	}

	public void SyncEffects()
	{
		ParentObject.CurrentCell?.ForeachObject((Action<GameObject>)SyncObjectEffect);
	}

	private void DetachEffects(GameObject GO, IEvent Event)
	{
		Sitting effect = GO.GetEffect((Sitting fx) => fx.SittingOn == ParentObject);
		if (effect != null)
		{
			GO.RemoveEffect(effect);
			GO.ApplyEffect(new Prone());
			Event?.RequestInterfaceExit();
		}
	}

	private void DetachEffects(Cell Cell = null, IEvent Event = null)
	{
		if (Cell == null)
		{
			Cell = ParentObject.GetCurrentCell();
		}
		Cell?.SafeForeachObject((GameObject o) =>
		{
			DetachEffects(o, Event);
		});
	}

	private bool IsSeated(GameObject Object)
	{
		if (Object.TryGetEffect<Sitting>(out var Effect))
		{
			return Effect.SittingOn == ParentObject;
		}
		return false;
	}

	public bool IsOccupiedFor(GameObject Object)
	{
		foreach (GameObject @object in ParentObject.CurrentCell.Objects)
		{
			if (@object.IsCombatObject() && @object != Object && (@object != ParentObject || !@object.IsAlliedTowards(Object)))
			{
				return true;
			}
		}
		return false;
	}

	private void MoveSeated(GameObject Object, Cell Target, IEvent Event)
	{
		if (Object.DistanceTo(Target) > 0)
		{
			if (!Object.SystemMoveTo(Target, 0))
			{
				DetachEffects(Object, Event);
			}
			else
			{
				Event?.RequestInterfaceExit();
			}
		}
	}

	public bool SitDown(GameObject Actor, IEvent FromEvent = null)
	{
		if (Actor == ParentObject)
		{
			return Actor.Fail("You cannot sit on " + Actor.itself + ".");
		}
		switch (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: true, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: true, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
		case ActivePartStatus.Broken:
			return Actor.Fail(ParentObject.Itis + " broken.");
		default:
			return Actor.Fail(ParentObject.Does("don't") + " seem to be working.");
		case ActivePartStatus.Operational:
		{
			if (Actor.HasEffect<Sitting>())
			{
				return Actor.Fail("You are already sitting down.");
			}
			if (Actor.HasEffect<Enclosed>())
			{
				return Actor.Fail("You cannot do that while enclosed.");
			}
			if (Actor.HasEffect<Burrowed>())
			{
				return Actor.Fail("You cannot do that while burrowed.");
			}
			if (!Actor.CanChangeBodyPosition("Sitting", ShowMessage: true))
			{
				return false;
			}
			if (!Actor.FlightCanReach(ParentObject))
			{
				return Actor.Fail("You cannot reach " + ParentObject.t() + ".");
			}
			if (!Actor.PhaseMatches(ParentObject))
			{
				return Actor.Fail("You are out of phase with " + ParentObject.t() + ".");
			}
			Cell cell = ParentObject.CurrentCell;
			if (cell == null)
			{
				cell = Actor.CurrentCell;
				if (cell == null)
				{
					return false;
				}
				if (ParentObject.Equipped == Actor)
				{
					Event obj = Event.New("CommandUnequipObject");
					obj.SetParameter("Object", ParentObject);
					obj.SetFlag("NoStack", State: true);
					if (!Actor.FireEvent(obj))
					{
						return Actor.Fail("You cannot unequip " + ParentObject.t() + ".");
					}
				}
				if (ParentObject.InInventory == Actor)
				{
					ParentObject.SplitFromStack();
					if (!InventoryActionEvent.Check(Actor, Actor, ParentObject, "CommandDropObject"))
					{
						return Actor.Fail("You cannot set " + ParentObject.t() + " down!");
					}
				}
				if (ParentObject.CurrentCell != cell)
				{
					return false;
				}
			}
			else
			{
				ParentObject.SplitFromStack();
			}
			if (Actor.CurrentCell != cell && (!Actor.Move(Actor.GetDirectionToward(ParentObject), Forced: false, System: false, IgnoreGravity: false, NoStack: false, AllowDashing: false, DoConfirmations: false, null, null, NearestAvailable: false, null, null, null, Peaceful: true) || Actor.CurrentCell != cell))
			{
				FromEvent?.RequestInterfaceExit();
				ParentObject.CheckStack();
				return false;
			}
			if (BreakIfGiganticOnNonGigantic && Actor.IsGiganticCreature && !ParentObject.IsGiganticEquipment && ParentObject.ApplyEffect(new Broken()) && IsBroken())
			{
				Actor.Fail("You think you broke " + ParentObject.them + "...");
				ParentObject.CheckStack();
				return true;
			}
			IComponent<GameObject>.XDidYToZ(Actor, "sit", "down on", ParentObject);
			Actor.ApplyEffect(new Sitting(ParentObject, EffectiveLevel(), DamageAttributes));
			if (Actor.IsPlayer())
			{
				ParentObject.SetIntProperty("DroppedByPlayer", 1);
				MetricsManager.LogEvent("ChairsSat");
			}
			Actor.UseEnergy(1000, "Position");
			FromEvent?.RequestInterfaceExit();
			Actor.FireEvent(Event.New("SatIn", "Object", ParentObject));
			ParentObject.FireEvent(Event.New("BeingSatOn", "Object", Actor));
			return true;
		}
		}
	}

	public bool StandUp(GameObject Actor, IEvent FromEvent = null, Sitting S = null)
	{
		if (S == null)
		{
			S = Actor.GetEffect<Sitting>();
			if (S == null)
			{
				return Actor.Fail("You are not sitting down.");
			}
		}
		if (S.SittingOn != ParentObject)
		{
			return Actor.Fail("It is not " + ParentObject.t() + " that you are sitting on.");
		}
		IComponent<GameObject>.XDidY(Actor, "stand", "up");
		Actor.RemoveEffect(S);
		Actor.UseEnergy(1000, "Position");
		FromEvent?.RequestInterfaceExit();
		return true;
	}

	private bool IsSittingOnThis(GameObject obj)
	{
		Sitting effect = obj.GetEffect<Sitting>();
		if (effect != null)
		{
			return effect.SittingOn == ParentObject;
		}
		return false;
	}
}
