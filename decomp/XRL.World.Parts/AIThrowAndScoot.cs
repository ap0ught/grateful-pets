using System;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AIThrowAndScoot : AIBehaviorPart
{
	public string Duration = "1d3";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<AIAfterThrowEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIAfterThrowEvent E)
	{
		FleeFrom(E.Target);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private void FleeFrom(GameObject Target)
	{
		if (GameObject.Validate(ref Target))
		{
			ParentObject.Brain.PushGoal(new Flee(Target, Duration.RollCached()));
		}
	}
}
