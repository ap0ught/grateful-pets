using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class SunderGrenade : IGrenade
{
	public int Radius = 1;

	public int Level = 1;

	public override bool SameAs(IPart p)
	{
		SunderGrenade sunderGrenade = p as SunderGrenade;
		if (sunderGrenade.Radius != Radius)
		{
			return false;
		}
		if (sunderGrenade.Level != Level)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetComponentNavigationWeightEvent.ID)
		{
			return ID == GetComponentAdjacentNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetComponentNavigationWeightEvent E)
	{
		E.MinWeight(8);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetComponentAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(8);
		return base.HandleEvent(E);
	}

	protected override bool DoDetonate(Cell C, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false)
	{
		DidX("detonate", null, "!");
		Disintegration.Disintegrate(C, Radius, Level, ParentObject, Actor, ParentObject, lowPrecision: true, Indirect);
		ParentObject.Destroy(null, Silent: true);
		return true;
	}
}
