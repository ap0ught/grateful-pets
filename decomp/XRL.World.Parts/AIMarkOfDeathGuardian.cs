using System;

namespace XRL.World.Parts;

[Serializable]
public class AIMarkOfDeathGuardian : AIBehaviorPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register(PooledEvent<GetFeelingEvent>.ID);
	}

	public override bool HandleEvent(GetFeelingEvent E)
	{
		if (!E.Faction)
		{
			return base.HandleEvent(E);
		}
		if (E.Target.HasPart(typeof(AIMarkOfDeathGuardian)))
		{
			E.Feeling = 50;
			return false;
		}
		E.Feeling = ((!E.Target.HasMarkOfDeath()) ? (-100) : 0);
		return false;
	}
}
