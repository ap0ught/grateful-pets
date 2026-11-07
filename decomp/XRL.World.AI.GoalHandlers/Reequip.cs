using System;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class Reequip : GoalHandler
{
	public override bool Finished()
	{
		return false;
	}

	public override void TakeAction()
	{
		ParentBrain.DoReequip = true;
		ParentBrain.DoPrimaryChoiceOnReequip = false;
		Pop();
	}
}
