using System;

namespace XRL.World.Parts;

[Serializable]
public class AmmoShotgunShell : IAmmo
{
	public string ProjectileObject;

	public override bool SameAs(IPart p)
	{
		if ((p as AmmoShotgunShell).ProjectileObject != ProjectileObject)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetProjectileObjectEvent>.ID)
		{
			return ID == QueryEquippableListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(QueryEquippableListEvent E)
	{
		if (E.SlotType.Contains("AmmoShotgunShell") && !E.List.Contains(ParentObject))
		{
			E.List.Add(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetProjectileObjectEvent E)
	{
		if (!string.IsNullOrEmpty(ProjectileObject))
		{
			E.Projectile = GameObject.Create(ProjectileObject);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
