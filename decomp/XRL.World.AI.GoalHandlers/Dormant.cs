using System;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class Dormant : GoalHandler
{
	public int Duration = -1;

	public Dormant(int Duration)
	{
		this.Duration = Duration;
	}

	public override void Create()
	{
		Think("I'm going dormant for " + Duration.Things("turn") + ".");
	}

	public override bool Finished()
	{
		if (Duration == 0)
		{
			return true;
		}
		return false;
	}

	public override void TakeAction()
	{
		if (Duration > 0)
		{
			Duration--;
		}
		ParentBrain.ParentObject.UseEnergy(1000);
	}

	public override void PushChildGoal(GoalHandler Child)
	{
	}

	public override void PushGoal(GoalHandler Goal)
	{
	}
}
