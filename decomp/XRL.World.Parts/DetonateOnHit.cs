using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class DetonateOnHit : IPart
{
	public string Damage = "2d6";

	public int Force = 10000;

	public int Chance;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerHit");
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID.EndsWith("Hit"))
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject Object = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Weapon");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
			if (GameObject.Validate(ref Object))
			{
				GameObject subject = Object;
				GameObject projectile = gameObjectParameter3;
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObjectParameter2, "Part DetonateOnHit Activation", Chance, subject, projectile).in100())
				{
					Object.Explode(Force, gameObjectParameter, Damage, 1f, Neutron: false, SuppressDestroy: true);
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(AppendEffect);
		return base.HandleEvent(E);
	}

	public void AppendEffect(StringBuilder SB)
	{
		if (Chance > 0)
		{
			if (Chance < 100)
			{
				SB.Append(Chance).Append("% chance ");
			}
			SB.AppendCase("melee attacks cause a small explosion");
		}
	}
}
