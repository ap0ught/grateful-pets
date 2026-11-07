using System;

namespace XRL.World.Parts;

[Serializable]
public class ExplodeOnHit : IPart
{
	public int Force = 10000;

	public string Damage = "0";

	public override bool SameAs(IPart p)
	{
		ExplodeOnHit explodeOnHit = p as ExplodeOnHit;
		if (explodeOnHit.Force != Force)
		{
			return false;
		}
		if (explodeOnHit.Damage != Damage)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterThrownEvent.ID)
		{
			return ID == PooledEvent<IsExplosiveEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterThrownEvent E)
	{
		Detonate(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsExplosiveEvent E)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ProjectileHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ProjectileHit")
		{
			Detonate(E.GetGameObjectParameter("Owner"));
		}
		return base.FireEvent(E);
	}

	public void Detonate(GameObject Owner = null)
	{
		DidX("explode", null, "!");
		ParentObject.Explode(Force, Owner, Damage);
	}
}
