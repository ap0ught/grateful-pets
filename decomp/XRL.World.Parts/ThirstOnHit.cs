using System;

namespace XRL.World.Parts;

[Serializable]
public class ThirstOnHit : IPart
{
	public int Chance = 100;

	public string Amount = "60000";

	public bool RequiresOrganic = true;

	public override bool SameAs(IPart p)
	{
		ThirstOnHit thirstOnHit = p as ThirstOnHit;
		if (thirstOnHit.Chance != Chance)
		{
			return false;
		}
		if (thirstOnHit.Amount != Amount)
		{
			return false;
		}
		if (thirstOnHit.RequiresOrganic != RequiresOrganic)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ProjectileHit");
		Registrar.Register("WeaponHit");
		Registrar.Register("WeaponThrowHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "WeaponThrowHit" || E.ID == "ProjectileHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject Object = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Weapon");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
			if (GameObject.Validate(ref Object))
			{
				GameObject subject = Object;
				GameObject projectile = gameObjectParameter3;
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObjectParameter2, "Part ThirstOnHit Activation", Chance, subject, projectile).in100() && (!RequiresOrganic || Object.IsOrganic) && !Object.FireEvent(new Event("AddWater", "Amount", -Amount.RollCached(), "Forced", 1, "External", 1)))
				{
					return false;
				}
			}
		}
		return base.FireEvent(E);
	}
}
