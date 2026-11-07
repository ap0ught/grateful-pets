using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class PoisonOnHit : IPart
{
	public int Chance = 100;

	public string Strength = "15";

	public string DamageIncrement = "3d3";

	public string Duration = "6-9";

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		if (Object.IsCreature)
		{
			Registrar.Register("AttackerHit");
			return;
		}
		if (Object.IsProjectile)
		{
			Registrar.Register("ProjectileHit");
			return;
		}
		Registrar.Register("WeaponHit");
		Registrar.Register("WeaponThrowHit");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID.EndsWith("Hit"))
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject Object = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Projectile");
			if (GameObject.Validate(ref Object))
			{
				GameObject parentObject = ParentObject;
				GameObject subject = Object;
				GameObject projectile = gameObjectParameter2;
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part PoisonOnHit Activation", Chance, subject, projectile).in100())
				{
					int num = Strength.RollCached();
					if (!Object.MakeSave("Toughness", num, null, null, "Injected Damaging Poison", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
					{
						Object.ApplyEffect(new Poisoned(Duration.RollCached(), DamageIncrement, num, ParentObject.Equipped));
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
