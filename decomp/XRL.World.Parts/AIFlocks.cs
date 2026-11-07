using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AIFlocks : AIBehaviorPart
{
	public string FlocksWith = "Pig";

	public int MinDistance = 1;

	public int MaxDistance = 3;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<AIBoredEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		Flock();
		return base.HandleEvent(E);
	}

	public bool Flock()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		List<GameObject> list = Event.NewGameObjectList();
		foreach (GameObject item in cell.ParentZone.FastCombatSquareVisibility(cell.X, cell.Y, 8, ParentObject))
		{
			if (item != ParentObject && item.Blueprint == FlocksWith && item.CurrentCell != null)
			{
				if (item.CurrentCell.PathDistanceTo(cell) <= MinDistance)
				{
					ParentObject.Brain.Think("I'm too close to " + FlocksWith + ".");
					ParentObject.Brain.PushGoal(new Step(Directions.GetRandomDirection()));
					ParentObject.Brain.PushGoal(new Step(Directions.GetRandomDirection()));
					return false;
				}
				list.Add(item);
			}
		}
		if (list.Count <= 0)
		{
			ParentObject.Brain.Think("I can't find " + Grammar.A(FlocksWith) + " to flock with.");
			return false;
		}
		int num = 0;
		int num2 = 0;
		foreach (GameObject item2 in list)
		{
			num += item2.CurrentCell.X;
			num2 += item2.CurrentCell.Y;
		}
		num /= list.Count;
		num2 /= list.Count;
		Cell cell2 = ParentObject.CurrentCell.ParentZone.GetCell(num, num2);
		if (cell2.PathDistanceTo(ParentObject.CurrentCell) > MaxDistance)
		{
			ParentObject.Brain.PushGoal(new MoveTo(cell2, careful: false, overridesCombat: false, 0, wandering: false, global: false, juggernaut: false, 3));
		}
		return true;
	}
}
