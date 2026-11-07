using System;

namespace XRL.World.Parts;

[Serializable]
public class Aquatic : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
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
		E.MoveSpeedPenalty -= 25;
		return base.HandleEvent(E);
	}
}
