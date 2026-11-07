using System;
using XRL.Rules;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class Confused : GoalHandler
{
	private int TurnsLeft;

	public Confused(int Turns, int Level)
	{
		TurnsLeft = Turns;
	}

	public override bool CanFight()
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
		if (TurnsLeft < 0)
		{
			Pop();
			return;
		}
		string randomDirection = Directions.GetRandomDirection();
		Cell localCellFromDirection = ParentObject.CurrentCell.GetLocalCellFromDirection(randomDirection);
		if (localCellFromDirection == null || ((!ParentBrain.LimitToAquatic() || localCellFromDirection.HasAquaticSupportFor(ParentObject)) && localCellFromDirection.IsEmpty()))
		{
			Think("I'll stumble " + randomDirection);
			PushChildGoal(new Step(randomDirection));
			TurnsLeft--;
		}
	}
}
