using System;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AITryKeepSteadyDistance : AIBehaviorPart
{
	public int Distance = 5;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			GameObject target = ParentObject.Target;
			if (target != null)
			{
				int num = target.DistanceTo(ParentObject);
				if (num < Distance)
				{
					ParentObject.Brain.PushGoal(new Flee(target, 2));
				}
				else if (num > Distance)
				{
					ParentObject.Brain.StepTowards(target.CurrentCell);
				}
			}
		}
		return base.FireEvent(E);
	}
}
