using System;
using XRL.Liquids;

namespace XRL.World.Parts;

[Serializable]
public class GlitchOnHit : IPart
{
	public int ChancePerThousand = 1;

	public override bool SameAs(IPart p)
	{
		if ((p as GlitchOnHit).ChancePerThousand != ChancePerThousand)
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
		Registrar.Register("WeaponHit");
		Registrar.Register("AttackerAfterDamage");
		Registrar.Register("DealingMissileDamage");
		Registrar.Register("WeaponMissileWeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "AttackerAfterDamage" || E.ID == "DealingMissileDamage" || E.ID == "WeaponMissileWeaponHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			Cell cell = gameObjectParameter2?.GetCurrentCell();
			if (gameObjectParameter != null && cell != null)
			{
				GameObject parentObject = ParentObject;
				GameObject subject = gameObjectParameter2;
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part GlitchOnHit Activation", ChancePerThousand, subject, null, ConstrainToPercentage: false, ConstrainToPermillage: true).in1000())
				{
					LiquidWarmStatic.GlitchObject(gameObjectParameter2);
				}
			}
		}
		return base.FireEvent(E);
	}
}
