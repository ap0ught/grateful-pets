using System;

namespace XRL.World.Parts;

[Serializable]
public class AmmoDart : IAmmo
{
	public override bool SameAs(IPart p)
	{
		return true;
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
		if (E.SlotType.Contains("AmmoDart") && !E.List.Contains(ParentObject))
		{
			E.List.Add(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetProjectileObjectEvent E)
	{
		E.Projectile = ParentObject;
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
