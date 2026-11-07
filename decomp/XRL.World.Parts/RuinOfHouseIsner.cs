using System;

namespace XRL.World.Parts;

[Serializable]
public class RuinOfHouseIsner : IPart
{
	public int EgoBonus;

	public int ShotCount;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (EgoBonus == 0 && E.Actor.HasStat("Ego") && ParentObject.IsEquippedProperly())
		{
			E.Actor.GetStat("Ego").Bonus++;
			EgoBonus = 1;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		if (EgoBonus != 0 && E.Actor.HasStat("Ego"))
		{
			E.Actor.GetStat("Ego").Bonus -= EgoBonus;
			EgoBonus = 0;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("QueryMissileFireSound");
		Registrar.Register("MagazineAmmoLoaderReloaded");
		Registrar.Register("WeaponMissileWeaponHit");
		Registrar.Register("WeaponMissileWeaponShot");
		Registrar.Register("WeaponMissleWeaponFiring");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponMissileWeaponShot")
		{
			if (ShotCount == 7)
			{
				E.SetParameter("AimVariance", 0);
				E.SetParameter("FlatVariance", 0);
				E.SetParameter("WeaponAccuracy", 0);
			}
		}
		else if (E.ID == "MagazineAmmoLoaderReloaded")
		{
			ShotCount = 0;
		}
		else if (E.ID == "QueryMissileFireSound")
		{
			if (ShotCount == 7)
			{
				E.SetParameter("Sound", "Sounds/Missile/Fires/Pistols/sfx_missile_ruinOfHouseIsner_finalShot");
			}
		}
		else if (E.ID == "WeaponMissleWeaponFiring")
		{
			ShotCount++;
		}
		else if (E.ID == "WeaponMissileWeaponHit")
		{
			if (ShotCount == 7)
			{
				E.SetFlag("Critical", State: true);
			}
			if (E.HasFlag("Critical"))
			{
				E.SetParameter("Penetrations", E.GetIntParameter("Penetrations") + 4);
				E.SetParameter("PenetrationCap", E.GetIntParameter("PenetrationCap") + 4);
			}
		}
		return base.FireEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
