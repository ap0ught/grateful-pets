using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class StunningForceOnJump : IPart
{
	public int Level = 1;

	public int Distance = 1;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<JumpedEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(JumpedEvent E)
	{
		StunningForce.Concussion(E.TargetCell, E.Actor, Level, Distance, E.Actor.GetPhase());
		return base.HandleEvent(E);
	}
}
