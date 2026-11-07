using System;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class DisposeOfCorpse : GoalHandler
{
	public GameObject Corpse;

	public GameObject Container;

	public Cell Destination;

	public bool Done;

	public int GoToContainerTries;

	public int GoToCorpseTries;

	public DisposeOfCorpse()
	{
	}

	public DisposeOfCorpse(GameObject Corpse, GameObject Container)
		: this()
	{
		this.Corpse = Corpse;
		this.Container = Container;
	}

	public override bool CanFight()
	{
		return true;
	}

	public override bool Finished()
	{
		return Done;
	}

	public override void TakeAction()
	{
		if (!GameObject.Validate(ref Corpse) || !GameObject.Validate(ref Container) || !ParentObject.InSameZone(Container))
		{
			FailToParent();
			return;
		}
		if (Corpse.InInventory == ParentObject)
		{
			if (ParentObject.InSameOrAdjacentCellTo(Container))
			{
				if (!Container.FireEvent(Event.New("PerformTake", "Object", Corpse, "PutBy", ParentObject, "Context", "DisposeOfCorpse")))
				{
					ParentObject.FireEvent(Event.New("PerformDrop", "Object", Corpse));
				}
				Done = true;
			}
			else if (++GoToContainerTries <= 10)
			{
				PushChildGoal(new MoveTo(Container, careful: false, overridesCombat: false, 1));
			}
			else
			{
				ParentObject.FireEvent(Event.New("PerformDrop", "Object", Corpse));
				Done = true;
			}
			return;
		}
		Cell currentCell = Corpse.CurrentCell;
		if (currentCell == null || currentCell.ParentZone != ParentObject.CurrentZone)
		{
			FailToParent();
		}
		else if (ParentObject.InSameOrAdjacentCellTo(Corpse))
		{
			if (!ParentObject.ReceiveObject(Corpse, NoStack: false, "DisposeOfCorpse"))
			{
				FailToParent();
			}
		}
		else if (++GoToCorpseTries <= 10)
		{
			PushChildGoal(new MoveTo(Corpse, careful: false, overridesCombat: false, 1));
		}
		else
		{
			Done = true;
		}
	}
}
