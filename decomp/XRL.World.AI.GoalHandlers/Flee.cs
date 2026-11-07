using System;
using System.Collections.Generic;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class Flee : IMovementGoal
{
	public GameObject Target;

	public int Duration = -1;

	public bool Panicked;

	public bool LocalOnly = true;

	private List<Cell> Entered = new List<Cell>();

	public Flee()
	{
	}

	public Flee(GameObject Target = null, int Duration = -1, bool Panicked = false, bool LocalOnly = true)
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
		Think("I'm trying to flee from " + (Target?.an() ?? "nothing") + "!");
	}

	public override bool Finished()
	{
		return Duration == 0;
	}

	public bool TryRetreatAbilities()
	{
		return AICommandList.HandleCommandList(AIGetRetreatAbilityListEvent.GetFor(ParentObject, Target), ParentObject, Target);
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
		if (!GameObject.Validate(ref Target))
		{
			Think("I don't have a target any more!");
			Target = null;
			FailToParent();
			return;
		}
		if (ParentObject.IsPlayer())
		{
			GoalHandler.AddPlayerMessage("You are fleeing from " + Target.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: true) + "!", 'r');
		}
		Cell currentCell = ParentObject.CurrentCell;
		if (currentCell == null)
		{
			Think("I don't have a location!");
			Target = null;
			FailToParent();
			return;
		}
		Cell currentCell2 = Target.GetCurrentCell();
		if (currentCell2 == null || currentCell2.ParentZone?.ZoneID == null)
		{
			Think("My target has no location!");
			Target = null;
			FailToParent();
		}
		else if (LocalOnly && currentCell.ParentZone != currentCell2.ParentZone)
		{
			Think("My target is in another zone!");
			Target = null;
			FailToParent();
		}
		else
		{
			if (!Panicked && TryRetreatAbilities())
			{
				return;
			}
			string directionFromCell = currentCell2.GetDirectionFromCell(currentCell);
			Cell cell = null;
			int Nav = 268435456;
			int num;
			if (Panicked)
			{
				num = 50;
				cell = ((!(directionFromCell == ".")) ? currentCell.GetCellFromDirectionGlobal(directionFromCell, LocalOnly) : currentCell.GetRandomLocalAdjacentCell());
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
			int num3 = currentCell2.PathDistanceTo(currentCell);
			foreach (Cell item2 in currentCell.GetLocalAdjacentCells(1).ShuffleInPlace())
			{
				if (!Entered.Contains(item2) && currentCell2.PathDistanceTo(item2) >= num3 && item2.NavigationWeight(ParentObject, ref Nav) < num)
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
	}
}
