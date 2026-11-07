using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Endurance_Swimming : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetSwimmingPerformanceEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetSwimmingPerformanceEvent E)
	{
		E.MoveSpeedPenalty -= 25;
		return base.HandleEvent(E);
	}
}
