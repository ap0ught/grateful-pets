using System;

namespace XRL.World.Parts;

[Serializable]
public class HEGrenade : IGrenade
{
	public int Force = 10000;

	public string Damage = "2d6";

	[NonSerialized]
	private bool detonating;

	public override bool SameAs(IPart p)
	{
		HEGrenade hEGrenade = p as HEGrenade;
		if (hEGrenade.Force != Force)
		{
			return false;
		}
		if (hEGrenade.Damage != Damage)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetComponentNavigationWeightEvent.ID && ID != GetComponentAdjacentNavigationWeightEvent.ID)
		{
			return ID == PooledEvent<IsExplosiveEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetComponentNavigationWeightEvent E)
	{
		E.MinWeight(10);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetComponentAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(10);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsExplosiveEvent E)
	{
		return false;
	}

	protected override bool DoDetonate(Cell C, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false)
	{
		if (detonating)
		{
			return true;
		}
		detonating = true;
		try
		{
			PlayWorldSound(GetPropertyOrTag("DetonatedSound"), 1f, 0f, Combat: true);
			DidX("explode", null, "!");
			ParentObject.Explode(Force, Actor, Damage, 1f, Neutron: false, SuppressDestroy: false, Indirect);
		}
		finally
		{
			detonating = false;
		}
		return true;
	}
}
