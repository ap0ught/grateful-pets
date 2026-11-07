using System;
using XRL.Language;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel_Bludgeon : BaseSkill
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerAfterAttack");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerAfterAttack")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject Object = E.GetGameObjectParameter("Defender");
			GameObject Object2 = E.GetGameObjectParameter("Weapon");
			if (GameObject.Validate(ref Object) && GameObject.Validate(ref Object2))
			{
				MeleeWeapon part = Object2.GetPart<MeleeWeapon>();
				if (part != null && part.Skill == "Cudgel")
				{
					string stringParameter = E.GetStringParameter("Properties");
					bool flag = stringParameter != null && stringParameter.HasDelimitedSubstring(',', "Conking");
					bool flag2 = ParentObject.HasSkill("Cudgel_Conk");
					if ((flag & flag2) && Object.HasEffect<Stun>())
					{
						Object.ApplyEffect(new Asleep(Stat.Random(30, 40)));
					}
					else
					{
						int num = 50;
						if (flag & flag2)
						{
							num = 100;
						}
						else if (stringParameter != null && stringParameter.HasDelimitedSubstring(',', "Charging") && ParentObject.HasSkill("Cudgel_ChargingStrike"))
						{
							num = 100;
						}
						else if (ParentObject.HasEffect<Cudgel_SmashingUp>())
						{
							num = 100;
						}
						else if (ParentObject.HasIntProperty("ImprovedBludgeon"))
						{
							num += num * ParentObject.GetIntProperty("ImprovedBludgeon");
						}
						num = GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, ParentObject, "Skill Bludgeon", num);
						if (num.in100() && Object.ApplyEffect(new Dazed(Stat.Random(3, 4), DontStunIfPlayer: true)) && Object.IsCombat)
						{
							IComponent<GameObject>.XDidY(Object, "reel", "from the force of " + (ParentObject.IsPlayer() ? "your" : Grammar.MakePossessive(ParentObject.the + ParentObject.ShortDisplayName)) + " bludgeoning", null, null, null, null, Object);
						}
					}
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
