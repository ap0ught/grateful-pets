using System;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AISelfPreservation : AIBehaviorPart
{
	public int Threshold = 35;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AITakingAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AITakingAction" && ParentObject.HasStat("Hitpoints"))
		{
			Statistic stat = ParentObject.GetStat("Hitpoints");
			if (stat.Penalty >= stat.BaseValue * (100 - Threshold) / 100 && !ParentObject.Brain.HasGoal("Retreat"))
			{
				ParentObject.Brain.Goals.Clear();
				ParentObject.Brain.PushGoal(new Retreat(Stat.Random(30, 50)));
				if (ParentObject.Brain.Goals.Count > 0)
				{
					ParentObject.Brain.Goals.Peek().TakeAction();
					return false;
				}
			}
		}
		return base.FireEvent(E);
	}
}
