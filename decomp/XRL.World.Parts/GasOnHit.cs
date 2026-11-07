using System;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: gas density produced is increased by a percentage
/// equal to ((power load - 100) / 10), i.e. 30% for the standard overload
/// power load of 400.
/// </remarks>
[Serializable]
public class GasOnHit : IPart
{
	public string Blueprint = "AcidGas";

	public string Density = "3d10";

	public bool OnWielderHit;

	public GasOnHit()
	{
	}

	public GasOnHit(string Blueprint, string Density)
		: this()
	{
		this.Blueprint = Blueprint;
		this.Density = Density;
	}

	public override bool SameAs(IPart p)
	{
		GasOnHit gasOnHit = p as GasOnHit;
		if (gasOnHit.Blueprint != Blueprint)
		{
			return false;
		}
		if (gasOnHit.Density != Density)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		if (!OnWielderHit)
		{
			Registrar.Register("WeaponDealDamage");
		}
		Registrar.Register("ProjectileHit");
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "WeaponDealDamage" || E.ID == "ProjectileHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter?.CurrentCell != null)
			{
				GameObject gameObject = GameObject.Create(Blueprint);
				Gas part = gameObject.GetPart<Gas>();
				part.Density = Density.RollCached() * (100 + MyPowerLoadBonus(int.MinValue, 100, 10)) / 100;
				part.Creator = E.GetGameObjectParameter("Attacker");
				gameObjectParameter.CurrentCell.AddObject(gameObject);
			}
		}
		return base.FireEvent(E);
	}
}
