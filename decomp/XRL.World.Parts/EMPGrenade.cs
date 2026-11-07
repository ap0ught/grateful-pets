using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class EMPGrenade : IGrenade
{
	public int Radius = 4;

	public string Duration = "1d2+4";

	public override bool SameAs(IPart p)
	{
		EMPGrenade eMPGrenade = p as EMPGrenade;
		if (eMPGrenade.Radius != Radius)
		{
			return false;
		}
		if (eMPGrenade.Duration != Duration)
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
		E.MinWeight(3);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetComponentAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(2);
		return base.HandleEvent(E);
	}

	protected override bool DoDetonate(Cell C, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false)
	{
		DidX("detonate", null, "!");
		PlayWorldSound(GetPropertyOrTag("DetonatedSound"), 1f, 0f, Combat: true);
		ElectromagneticPulse.EMP(C, Radius, Duration.RollCached(), IncludeBaseCell: true, ParentObject.GetPhase());
		ParentObject.Destroy(null, Silent: true);
		return true;
	}
}
