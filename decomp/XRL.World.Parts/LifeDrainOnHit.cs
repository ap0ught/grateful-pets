using System;
using XRL.World.AI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class LifeDrainOnHit : IPart
{
	public string Damage = "15-20";

	public int Chance = 100;

	public bool RealityDistortionBased = true;

	public LifeDrainOnHit()
	{
	}

	public LifeDrainOnHit(string Damage, int Chance)
		: this()
	{
		this.Damage = Damage;
		this.Chance = Chance;
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
		if (E.ID == "WeaponHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject Object = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Weapon");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
			if (GameObject.Validate(ref Object))
			{
				GameObject subject = Object;
				GameObject projectile = gameObjectParameter3;
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObjectParameter2, "Part LifeDrainOnHit Activation", Chance, subject, projectile).in100())
				{
					Object.ApplyEffect(new LifeDrain(2, 10, Damage, gameObjectParameter, RealityDistortionBased));
					if (!Object.IsHostileTowards(gameObjectParameter))
					{
						Object.AddOpinion<OpinionAttack>(gameObjectParameter, gameObjectParameter2);
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
