using System;

namespace XRL.World.Parts;

[Serializable]
public class MaxPenetration : IPart
{
	public int Max = 1;

	public override bool SameAs(IPart p)
	{
		if ((p as MaxPenetration).Max != Max)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNamePenetrationColorEvent>.ID)
		{
			return ID == PooledEvent<MissilePenetrateEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNamePenetrationColorEvent E)
	{
		if (Max <= 1)
		{
			E.Color = "K";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(MissilePenetrateEvent E)
	{
		if (E.Penetrations > Max)
		{
			E.Penetrations = Max;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" && E.GetIntParameter("Penetrations") > Max)
		{
			E.SetParameter("Penetrations", Max);
		}
		return base.FireEvent(E);
	}
}
