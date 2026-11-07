using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsTibularHydrojets : IPart
{
	public int Bonus = 200;

	public override bool SameAs(IPart p)
	{
		return false;
	}

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
		E.MoveSpeedPenalty -= Bonus;
		return base.HandleEvent(E);
	}
}
