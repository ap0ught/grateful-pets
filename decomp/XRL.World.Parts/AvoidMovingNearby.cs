using System;

namespace XRL.World.Parts;

[Serializable]
public class AvoidMovingNearby : IPart
{
	public int Weight = 2;

	public bool RespectPhase = true;

	public override bool SameAs(IPart p)
	{
		AvoidMovingNearby avoidMovingNearby = p as AvoidMovingNearby;
		if (avoidMovingNearby.Weight != Weight)
		{
			return false;
		}
		if (avoidMovingNearby.RespectPhase != RespectPhase)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetAdjacentNavigationWeightEvent.ID)
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

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		if (!RespectPhase || E.PhaseMatches(ParentObject))
		{
			E.MinWeight(Weight);
		}
		return base.HandleEvent(E);
	}
}
