using System;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class GoFetch : GoalHandler
{
	private Cell targetCell;

	public GoFetch(Cell c)
	{
		targetCell = c;
	}

	public override bool Finished()
	{
		return false;
	}

	public override void Create()
	{
	}

	public override void TakeAction()
	{
		Pop();
		ParentBrain.PushGoal(new GoFetchGet());
		ParentBrain.PushGoal(new MoveTo(targetCell));
	}
}
