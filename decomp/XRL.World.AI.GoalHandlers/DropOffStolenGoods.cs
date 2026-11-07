using System;
using System.Collections.Generic;
using System.Linq;
using Wintellect.PowerCollections;
using XRL.Rules;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class DropOffStolenGoods : GoalHandler
{
	public string TargetObject = "OpenShaft";

	public override bool Finished()
	{
		return ParentBrain.ParentObject.Inventory.Objects.Count == 0;
	}

	public override void Create()
	{
	}

	public void MoveToDropoff()
	{
		List<GameObject> list = CurrentZone.FindObjects(TargetObject);
		list.Sort((GameObject a, GameObject b) => a.DistanceTo(ParentObject).CompareTo(b.DistanceTo(ParentObject)));
		GameObject gameObject = list.FirstOrDefault();
		if (gameObject != null)
		{
			if (gameObject.DistanceTo(ParentBrain.ParentObject) <= 1 && ParentBrain.ParentObject.IsVisible())
			{
				GameObject randomElement = ParentBrain.ParentObject.Inventory.Objects.GetRandomElement();
				if (randomElement != null)
				{
					ParentObject.Inventory.RemoveObject(randomElement);
					gameObject.CurrentCell.AddObject(randomElement);
					GoalHandler.AddPlayerMessage(ParentObject.Does("drop") + " " + randomElement.an() + " down " + gameObject.t() + ".", 'W');
				}
			}
			List<Cell> localAdjacentCells = gameObject.CurrentCell.GetLocalAdjacentCells(1);
			Algorithms.RandomShuffleInPlace(localAdjacentCells, Stat.Rand);
			for (int num = 0; num < localAdjacentCells.Count; num++)
			{
				if (localAdjacentCells[num].IsEmpty())
				{
					ParentBrain.PushGoal(new MoveTo(CurrentZone.ZoneID, localAdjacentCells[num].X, localAdjacentCells[num].Y));
					return;
				}
			}
			ParentBrain.PushGoal(new WanderRandomly(6));
		}
		else
		{
			FailToParent();
		}
	}

	public override void TakeAction()
	{
		MoveToDropoff();
	}
}
