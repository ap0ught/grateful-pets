using System;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class EngulfingWanders : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
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
			Engulfing part = ParentObject.GetPart<Engulfing>();
			if (part != null)
			{
				if (part.Engulfed != null)
				{
					if (!ParentObject.Brain.HasGoal("FleeLocation"))
					{
						ParentObject.Brain.Goals.Clear();
						ParentObject.Brain.PushGoal(new FleeLocation(ParentObject.CurrentCell, "2d4".RollCached()));
					}
				}
				else if (ParentObject.Brain.HasGoal("FleeLocation"))
				{
					ParentObject.Brain.Goals.Clear();
				}
			}
		}
		return base.FireEvent(E);
	}
}
