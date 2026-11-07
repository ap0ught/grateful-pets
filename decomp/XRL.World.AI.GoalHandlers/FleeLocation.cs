using System;
using System.Collections.Generic;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class FleeLocation : IMovementGoal
{
	public Cell Target;

	public int Duration = -1;

	public bool Panicked;

	public bool LocalOnly = true;

	public List<Cell> Entered = new List<Cell>();

	public FleeLocation()
	{
	}

	public FleeLocation(Cell Target = null, int Duration = -1, bool Panicked = false, bool LocalOnly = false)
		: this()
	{
		this.Target = Target;
		this.Duration = Duration;
		this.Panicked = Panicked;
		this.LocalOnly = LocalOnly;
	}

	public override bool CanFight()
	{
		return false;
	}

	public override bool IsNonAggressive()
	{
		return true;
	}

	public override bool IsFleeing()
	{
		return true;
	}

	public override void Create()
	{
		Think("I'm trying to flee from a location!");
	}

	public override bool Finished()
	{
		return Duration == 0;
	}

	public bool TryRetreatAbilities()
	{
		return AICommandList.HandleCommandList(AIGetRetreatAbilityListEvent.GetFor(ParentObject, null, null, Target), ParentObject);
	}

	public override void TakeAction()
	{
		if (Duration > 0)
		{
			Duration--;
		}
		else if (Duration == 0)
		{
			return;
		}
		Cell currentCell = ParentObject.CurrentCell;
		if (currentCell == null)
		{
			Think("I don't have a location!");
			FailToParent();
		}
		else
		{
			if (!Panicked && TryRetreatAbilities())
			{
				return;
			}
			string directionFromCell = Target.GetDirectionFromCell(currentCell);
			Cell cell = null;
			int Nav = 268435456;
			int num;
			if (Panicked)
			{
				num = 50;
				cell = ((!(directionFromCell == ".")) ? currentCell.GetCellFromDirectionGlobal(directionFromCell) : (currentCell.GetRandomLocalAdjacentCell() ?? currentCell));
			}
			else
			{
				num = 10;
				List<Cell> directionAndAdjacentCells = currentCell.GetDirectionAndAdjacentCells(directionFromCell, LocalOnly);
				if (directionAndAdjacentCells.Count > 3)
				{
					directionAndAdjacentCells.ShuffleInPlace();
				}
				int num2 = int.MaxValue;
				foreach (Cell item in directionAndAdjacentCells)
				{
					if (item == currentCell || Entered.Contains(item) || item.NavigationWeight(ParentObject, ref Nav) >= num)
					{
						continue;
					}
					int totalAdjacentHostileDifficultyLevel = item.GetTotalAdjacentHostileDifficultyLevel(ParentObject);
					if (totalAdjacentHostileDifficultyLevel < num2)
					{
						cell = item;
						num2 = totalAdjacentHostileDifficultyLevel;
						if (num2 <= 0)
						{
							break;
						}
					}
				}
			}
			if (cell != null && !Entered.Contains(cell) && cell.NavigationWeight(ParentObject, ref Nav) < num)
			{
				ProcessDestination(cell, currentCell);
				return;
			}
			int num3 = Target.PathDistanceTo(currentCell);
			foreach (Cell item2 in currentCell.GetLocalAdjacentCells(1).ShuffleInPlace())
			{
				if (!Entered.Contains(item2) && Target.PathDistanceTo(item2) >= num3 && item2.NavigationWeight(ParentObject, ref Nav) < num)
				{
					ProcessDestination(item2, currentCell);
					return;
				}
			}
			Think("I can't find anyplace to flee!");
			FailToParent();
		}
	}

	private void ProcessDestination(Cell C, Cell From)
	{
		PushChildGoal(new Step(From.GetDirectionFromCell(C), careful: false, Panicked));
		Entered.Add(C);
		if (ParentBrain.StartingCell != null && ParentBrain.StartingCell.Equals(Target))
		{
			ParentBrain.StartingCell = new GlobalLocation(C);
		}
	}
}
