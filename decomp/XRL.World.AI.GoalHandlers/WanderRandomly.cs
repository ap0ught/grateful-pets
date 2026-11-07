using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class WanderRandomly : IMovementGoal
{
	public int TurnsLeft;

	[NonSerialized]
	private static Dictionary<string, int> dirs = new Dictionary<string, int>(8);

	public WanderRandomly()
	{
	}

	public WanderRandomly(int Turns)
		: this()
	{
		TurnsLeft = Turns;
	}

	public override bool IsBusy()
	{
		return false;
	}

	public override bool Finished()
	{
		return TurnsLeft <= 0;
	}

	public override void Create()
	{
	}

	public override void TakeAction()
	{
		TurnsLeft--;
		if (TurnsLeft < 0)
		{
			Pop();
			return;
		}
		Cell currentCell = ParentObject.CurrentCell;
		if (currentCell.HasObjectWithPart("StairsUp") && ParentObject.HasTagOrProperty("WanderUpStairs") && 35.in100())
		{
			PushChildGoal(new Step("U", careful: false, overridesCombat: false, wandering: true), ParentHandler);
			return;
		}
		if (currentCell.HasObjectWithPart("StairsDown") && ParentObject.HasTagOrProperty("WanderDownStairs") && 35.in100())
		{
			PushChildGoal(new Step("D", careful: false, overridesCombat: false, wandering: true), ParentHandler);
			return;
		}
		dirs.Clear();
		foreach (Cell localAdjacentCell in currentCell.GetLocalAdjacentCells())
		{
			if (ParentBrain.LimitToAquatic() && !localAdjacentCell.HasAquaticSupportFor(ParentObject))
			{
				continue;
			}
			if (localAdjacentCell.HasObjectWithTagOrProperty("WanderStopper"))
			{
				return;
			}
			if (localAdjacentCell.IsSolidFor(ParentObject))
			{
				continue;
			}
			int num = 10 - localAdjacentCell.GetNavigationWeightFor(ParentObject);
			if (num > 0)
			{
				string directionFromCell = currentCell.GetDirectionFromCell(localAdjacentCell);
				if (Directions.IsActualDirection(directionFromCell))
				{
					dirs[directionFromCell] = num;
				}
			}
		}
		string randomElement = dirs.GetRandomElement();
		if (!string.IsNullOrEmpty(randomElement))
		{
			PushChildGoal(new Step(randomElement, careful: false, overridesCombat: false, wandering: true), ParentHandler);
		}
	}
}
