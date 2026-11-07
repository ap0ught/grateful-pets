using System;
using System.Text;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ProneOnHit : IPart
{
	public int Chance = 100;

	public int SaveTarget = 25;

	public string SaveVs = "Knockdown";

	public string SaveStat = "Strength";

	public string AttackerStat;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerHit");
		Registrar.Register("ProjectileHit");
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerHit" || E.ID == "WeaponHit" || E.ID == "ProjectileHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject Object = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Weapon");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
			if (GameObject.Validate(ref Object))
			{
				GameObject subject = Object;
				GameObject projectile = gameObjectParameter3;
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObjectParameter2, "Part ProneOnHit Activation", Chance, subject, projectile).in100() && !Object.MakeSave(SaveStat, SaveTarget, Vs: SaveVs, Attacker: gameObjectParameter, AttackerStat: AttackerStat, IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, Source: (E.ID == "WeaponHit") ? gameObjectParameter2 : null))
				{
					Object.ApplyEffect(new Prone());
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
			if (ParentObject != null && ParentObject.IsProjectile)
			{
				SB.Append("Causes enemies to be knocked prone");
			}
			else
			{
				SB.Append("Melee attacks cause enemies to be knocked prone");
			}
		}
	}
}
