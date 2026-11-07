using System;

namespace XRL.World.Parts;

[Serializable]
public class InterceptImmunity : IPart
{
	public string Except;

	public override bool SameAs(IPart p)
	{
		if ((p as InterceptImmunity).Except == Except)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<PointDefenseInterceptEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(PointDefenseInterceptEvent E)
	{
		if (E.TargetProjectile == ParentObject && ImmuneTo(E.InterceptProjectile))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool ImmuneTo(GameObject Projectile)
	{
		if (Except.IsNullOrEmpty())
		{
			return true;
		}
		Projectile projectile = Projectile?.GetPart<Projectile>();
		if (projectile == null || projectile.Attributes == null)
		{
			return true;
		}
		if (projectile.Attributes.HasDelimitedSubstring(' ', Except))
		{
			return false;
		}
		return true;
	}
}
