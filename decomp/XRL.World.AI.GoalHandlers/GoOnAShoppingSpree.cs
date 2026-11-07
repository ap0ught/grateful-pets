using System;
using System.Collections.Generic;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class GoOnAShoppingSpree : GoalHandler
{
	public static List<string> TargetZones = new List<string> { "JoppaWorld.5.2.0.0.10", "JoppaWorld.5.2.0.1.10", "JoppaWorld.5.2.0.2.10", "JoppaWorld.5.2.1.0.10", "JoppaWorld.5.2.1.2.10", "JoppaWorld.5.2.2.0.10", "JoppaWorld.5.2.2.1.10", "JoppaWorld.5.2.2.2.10" };

	public override bool Finished()
	{
		return false;
	}

	public void MoveToNewZone()
	{
		ParentBrain.PushGoal(new MoveToGlobal(TargetZones.GetRandomElement(), 38, 10));
	}

	public void LookForMerchant()
	{
		if (!ParentObject.InACell())
		{
			return;
		}
		GameObject gameObject = CurrentZone.FindClosestObjectWithPart(ParentObject, "ConversationScript");
		if (gameObject == null || !gameObject.InACell())
		{
			Pop();
			return;
		}
		List<Cell> list = gameObject.CurrentCell.GetLocalAdjacentCells().ShuffleInPlace();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].IsEmpty())
			{
				ParentBrain.PushGoal(new MoveTo(CurrentZone.ZoneID, list[i].X, list[i].Y, careful: true));
				return;
			}
		}
		ParentBrain.PushGoal(new WanderRandomly(6));
	}

	public override void TakeAction()
	{
		if (25.in100())
		{
			MoveToNewZone();
		}
		else
		{
			LookForMerchant();
		}
	}
}
