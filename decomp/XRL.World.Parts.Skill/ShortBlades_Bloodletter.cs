using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class ShortBlades_Bloodletter : BaseSkill
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerAfterDamage");
		base.Register(Object, Registrar);
	}

	public static void Bloodlet(GameObject Defender, GameObject Attacker)
	{
		if (Defender.GetEffectCount(typeof(Bleeding)) < 1 + Attacker.StatMod("Agility"))
		{
			Defender.ApplyEffect(new Bleeding("1d2-1", 20 + Attacker.StatMod("Agility"), Attacker, Stack: false));
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerAfterDamage")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Weapon");
			if (gameObjectParameter != null && gameObjectParameter.HasPart<MeleeWeapon>() && gameObjectParameter.GetPart<MeleeWeapon>().Skill == "ShortBlades")
			{
				int num = 75;
				if (E.GetStringParameter("Properties", "").Contains("Juking"))
				{
					num = 100;
				}
				if (Stat.Random(1, 100) <= num)
				{
					Bloodlet(E.GetGameObjectParameter("Defender"), ParentObject);
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		return true;
	}
}
