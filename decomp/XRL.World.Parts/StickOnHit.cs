using System;
using System.Text;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class StickOnHit : IPart
{
	public string Duration = "5";

	public int Chance;

	public int SaveTarget = 15;

	public string SaveVs = "Stuck Restraint";

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
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObjectParameter2, "Part StickOnHit Activation", Chance, subject, projectile).in100())
				{
					Object.ApplyEffect(new Stuck(Duration.RollCached(), SaveTarget, SaveVs));
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
			SB.Append("Melee attacks cause enemies to get stuck for ").Append(Duration).Append(" turns");
		}
	}
}
