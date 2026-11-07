using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class Plastronoid : IPart
{
	public string FlocksWith = "Plastronoid";

	public int MinDistance = 1;

	public int MaxDistance = 1;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AIBored");
		Registrar.Register("TakingAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIBored")
		{
			PlastronoidFlock();
		}
		else if (E.ID == "TakingAction")
		{
			PlastronoidFlock(50);
		}
		return base.FireEvent(E);
	}

	public void PlastronoidFlock(int Chance = 100)
	{
		if (ParentObject.IsPlayer() || ParentObject.IsNowhere() || ParentObject.OnWorldMap() || !Chance.in100())
		{
			return;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return;
		}
		Zone parentZone = cell.ParentZone;
		if (parentZone == null)
		{
			return;
		}
		Brain brain = ParentObject.Brain;
		if (brain == null)
		{
			return;
		}
		if (cell.X % 2 == 1)
		{
			Cell cellFromDirection = cell.GetCellFromDirection("W");
			if (cellFromDirection != null && cellFromDirection.IsEmptyOfSolidFor(ParentObject))
			{
				brain.PushGoal(new Step("W"));
				return;
			}
			Cell cellFromDirection2 = cell.GetCellFromDirection("E");
			if (cellFromDirection2 != null && cellFromDirection2.IsEmptyOfSolidFor(ParentObject))
			{
				brain.PushGoal(new Step("E"));
				return;
			}
			brain.PushGoal(new Step(Directions.GetRandomDirection()));
		}
		else if (cell.Y % 2 == 1)
		{
			Cell cellFromDirection3 = cell.GetCellFromDirection("N");
			if (cellFromDirection3 != null && cellFromDirection3.IsEmptyOfSolidFor(ParentObject))
			{
				brain.PushGoal(new Step("N"));
				return;
			}
			Cell cellFromDirection4 = cell.GetCellFromDirection("S");
			if (cellFromDirection4 != null && cellFromDirection4.IsEmptyOfSolidFor(ParentObject))
			{
				brain.PushGoal(new Step("S"));
				return;
			}
			brain.PushGoal(new Step(Directions.GetRandomDirection()));
		}
		List<GameObject> list = Event.NewGameObjectList();
		foreach (GameObject item in parentZone.FastCombatSquareVisibility(cell.X, cell.Y, 8, ParentObject, null, VisibleToPlayerOnly: false, IncludeWalls: true))
		{
			if (item != ParentObject && item.Blueprint == FlocksWith && item.CurrentCell != null)
			{
				if (item.CurrentCell.PathDistanceTo(cell) <= MinDistance)
				{
					brain.Think("I'm too close to " + FlocksWith + ".");
					brain.PushGoal(new Step(Directions.GetRandomDirection()));
					brain.PushGoal(new Step(Directions.GetRandomDirection()));
					return;
				}
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			int num = 0;
			int num2 = 0;
			foreach (GameObject item2 in list)
			{
				num += item2.CurrentCell.X;
				num2 += item2.CurrentCell.Y;
			}
			num /= list.Count;
			num2 /= list.Count;
			Cell cell2 = parentZone.GetCell(num, num2);
			if (brain.Target != null && brain.Target.CurrentCell != null)
			{
				cell2 = brain.Target.CurrentCell;
			}
			if (cell2.PathDistanceTo(cell) > MaxDistance)
			{
				brain.PushGoal(new MoveTo(cell2, careful: false, overridesCombat: false, 0, wandering: false, global: false, juggernaut: false, 3));
			}
		}
		else
		{
			brain.Think("I can't find a creature to flock with.");
		}
	}
}
