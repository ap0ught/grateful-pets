using System;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class NoFightGoal : GoalHandler
{
	public override bool CanFight()
	{
		return false;
	}
}
