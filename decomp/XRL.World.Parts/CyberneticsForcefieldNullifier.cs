using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsForcefieldNullifier : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanMissilePassForcefieldEvent>.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		E.Implantee.ModIntProperty("ForcefieldNullifier", 1, RemoveIfZero: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.ModIntProperty("ForcefieldNullifier", -1, RemoveIfZero: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanMissilePassForcefieldEvent E)
	{
		GameObject projectile = E.Projectile;
		if (projectile != null && projectile.HasTagOrProperty("ForcefieldNullifierCarryover") && E.Actor == ParentObject.Implantee)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
