using System;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AIWallPhaser : AIBehaviorPart
{
	[NonSerialized]
	private long LastTick = -1L;

	[NonSerialized]
	private int LastPhase;

	public int GetPhase()
	{
		long timeTicks = The.Game.TimeTicks;
		if (timeTicks != LastTick)
		{
			LastTick = timeTicks;
			LastPhase = ParentObject.GetPhase();
		}
		return LastPhase;
	}

	public bool TryMoveNearestWall(Cell Origin = null, bool IgnoreCombat = false, bool ClearMovement = false)
	{
		if (GetPhase() != 2)
		{
			return false;
		}
		if (Origin == null)
		{
			Origin = ParentObject.CurrentCell;
		}
		Cell cell = Origin;
		int num = int.MaxValue;
		Zone.ObjectEnumerator enumerator = Origin.ParentZone.IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (current.IsWall() && !ParentObject.PhaseMatches(current))
			{
				Cell cell2 = current.CurrentCell;
				int num2 = Origin.DistanceTo(cell2);
				if (num2 > 0 && num2 < num)
				{
					cell = cell2;
					num = num2;
				}
			}
		}
		if (cell != Origin && cell != ParentObject.CurrentCell)
		{
			Brain brain = ParentObject.Brain;
			if (ClearMovement)
			{
				brain.RemoveGoalsDescendedFrom<IMovementGoal>();
			}
			brain.PushGoal(new MoveTo(cell, careful: false, IgnoreCombat));
			return true;
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIBoredEvent>.ID)
		{
			return ID == ActorGetNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (TryMoveNearestWall())
		{
			return false;
		}
		if (ParentObject.CurrentCell.HasWall())
		{
			ParentObject.Brain.PushGoal(new Wait(Stat.Random(5, 10), "I like walls"));
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ActorGetNavigationWeightEvent E)
	{
		if (E.Weight < 3 && GetPhase() == 2 && !E.Cell.HasWall())
		{
			E.Weight = 3;
			E.Uncacheable = true;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AICantAttackRange");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AICantAttackRange" && ParentObject.Target != null && (TryMoveNearestWall(ParentObject.Target.CurrentCell, IgnoreCombat: false, ClearMovement: true) || ParentObject.CurrentCell.HasWall()))
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
