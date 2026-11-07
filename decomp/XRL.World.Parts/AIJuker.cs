using System;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AIJuker : AIBehaviorPart
{
	public float Trigger = 0.35f;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AITakingAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AITakingAction" && 25.in100())
		{
			ParentObject.Brain.PushGoal(new Step(Directions.GetRandomDirection()));
			if (ParentObject.Brain.Goals.Count > 0)
			{
				ParentObject.Brain.Goals.Peek().TakeAction();
			}
		}
		return base.FireEvent(E);
	}
}
