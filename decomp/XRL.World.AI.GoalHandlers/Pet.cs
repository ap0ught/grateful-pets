using System;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class Pet : GoalHandler
{
	public GameObject Target;

	public Cell Destination;

	public bool Done;

	public int GoToTries = 5;

	public int MaxDistance = -1;

	public Pet()
	{
	}

	public Pet(GameObject Target, int GoToTries = 5, int MaxDistance = -1)
		: this()
	{
		this.Target = Target;
		this.GoToTries = GoToTries;
		this.MaxDistance = MaxDistance;
	}

	public override bool Finished()
	{
		return Done;
	}

	public override void TakeAction()
	{
		if (!GameObject.Validate(ref Target) || !ParentObject.InSameZone(Target))
		{
			FailToParent();
			return;
		}
		if (MaxDistance > -1 && ParentObject.DistanceTo(Target) > MaxDistance)
		{
			FailToParent();
			return;
		}
		if (ParentObject.InSameOrAdjacentCellTo(Target))
		{
			InventoryActionEvent.Check(Target, ParentBrain.ParentObject, Target, "Pet");
			Done = true;
			return;
		}
		Cell currentCell = Target.CurrentCell;
		if (currentCell == null || currentCell.ParentZone != ParentObject.CurrentZone)
		{
			FailToParent();
		}
		else if (--GoToTries <= 0)
		{
			FailToParent();
		}
		else
		{
			PushChildGoal(new MoveTo(Target, careful: false, overridesCombat: false, 1, wandering: false, global: false, juggernaut: false, -1, MaxDistance));
		}
	}
}
