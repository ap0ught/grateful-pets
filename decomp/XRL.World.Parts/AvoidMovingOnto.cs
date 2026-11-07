using System;

namespace XRL.World.Parts;

[Serializable]
public class AvoidMovingOnto : IPart
{
	public int Weight = 1;

	public bool RespectPhase = true;

	public override bool SameAs(IPart p)
	{
		AvoidMovingOnto avoidMovingOnto = p as AvoidMovingOnto;
		if (avoidMovingOnto.Weight != Weight)
		{
			return false;
		}
		if (avoidMovingOnto.RespectPhase != RespectPhase)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (!RespectPhase || E.PhaseMatches(ParentObject))
		{
			E.MinWeight(Weight);
		}
		return base.HandleEvent(E);
	}
}
