using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class BlastOnHit : IPart
{
	public int Force = 2;

	public override bool SameAs(IPart p)
	{
		if ((p as ExplodeOnHit).Force != Force)
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
		try
		{
			Detonate();
		}
		finally
		{
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsExplosiveEvent E)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
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
			Detonate(E.GetGameObjectParameter("ApparentTarget"));
		}
		return base.FireEvent(E);
	}

	public void Detonate(GameObject ApparentTarget = null)
	{
		DidX("explode", null, "!");
		Cell cell = ParentObject.GetCurrentCell();
		if (cell != null)
		{
			try
			{
				StunningForce.Concussion(cell, ParentObject, 3, Force, ParentObject.GetPhase(), ApparentTarget);
			}
			catch (Exception message)
			{
				MetricsManager.LogError(message);
			}
		}
	}
}
