using System;
using System.Collections.Generic;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Axe_Decapitate : BaseSkill
{
	public Guid ActivatedAbilityID;

	public override void Attach()
	{
		if (ActivatedAbilityID == Guid.Empty)
		{
			AddAbility();
		}
		base.Attach();
	}

	public static bool ShouldDecapitate(GameObject who)
	{
		return (who?.GetPart<Axe_Decapitate>())?.ShouldDecapitate() ?? false;
	}

	public bool ShouldDecapitate()
	{
		if (!IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			return false;
		}
		return true;
	}

	public static bool Decapitate(GameObject Attacker, GameObject Defender, Cell Where = null, BodyPart LostPart = null, GameObject Weapon = null, GameObject Projectile = null, bool weaponActing = false, bool UsePopups = false)
	{
		Body body = Defender.Body;
		if (body == null)
		{
			return false;
		}
		if (!Defender.CanBeDismembered(Weapon))
		{
			return false;
		}
		if (!Defender.FireEvent("BeforeDecapitate"))
		{
			return false;
		}
		if (LostPart == null)
		{
			List<BodyPart> list = new List<BodyPart>();
			foreach (BodyPart part in body.GetParts())
			{
				if (part.IsSeverable() && part.SeverRequiresDecapitate())
				{
					list.Add(part);
				}
			}
			LostPart = list.GetRandomElement();
			if (LostPart == null)
			{
				return false;
			}
		}
		Defender.PlayWorldSound("sfx_characterTrigger_decapitate");
		if (weaponActing && Weapon != null && Attacker != null)
		{
			IComponent<GameObject>.XDidYToZ(Weapon, "lop", "off", Defender, LostPart.GetOrdinalName(), "!", null, null, null, Defender, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true, Attacker, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopups);
		}
		else
		{
			IComponent<GameObject>.XDidYToZ(Attacker, "lop", "off", Defender, LostPart.GetOrdinalName(), "!", null, null, null, Defender, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopups);
		}
		if (LostPart.Type == "Head")
		{
			Defender.ParticleText("*decapitated!*", IComponent<GameObject>.ConsequentialColorChar(null, Defender));
			if (Defender.IsPlayer())
			{
				Achievement.GET_DECAPITATED.Unlock();
			}
		}
		else
		{
			Defender.ParticleText("*dismembered!*", IComponent<GameObject>.ConsequentialColorChar(null, Defender));
		}
		body.Dismember(LostPart, Attacker, Where, Obliterate: false, UsePopups);
		Defender.ApplyEffect(new Bleeding("1d2+1", 35, Attacker, Stack: true, Internal: false, UsePopups));
		Defender.RemoveAllEffects<Hooked>();
		if (((Attacker != null && Attacker.IsPlayer()) || (Defender != null && Defender.IsPlayer())) && CombatJuice.enabled)
		{
			CombatJuice.cameraShake(0.5f);
		}
		if (body.AnyDismemberedMortalParts() && !body.AnyMortalParts())
		{
			Defender.Die(Attacker, null, "You were " + ((LostPart.Type == "Head") ? "decapitated" : "relieved of your vital anatomy") + " by " + Attacker.an() + ".", Defender.Does("were", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " @@" + ((LostPart.Type == "Head") ? "decapitated" : ("relieved of " + Defender.its + " vital anatomy")) + " by " + Attacker.an() + ".", Accidental: false, Weapon, Projectile);
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandToggleDecapitate")
		{
			ToggleMyActivatedAbility(ActivatedAbilityID);
		}
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject Object)
	{
		AddAbility();
		return base.AddSkill(Object);
	}

	public override bool RemoveSkill(GameObject Object)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(Object);
	}

	public void AddAbility()
	{
		ActivatedAbilityID = AddMyActivatedAbility("Decapitate", "CommandToggleDecapitate", "Skills", "Toggles whether you will attempt to decapitate opponents when dismembering them.", "ö", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: true, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
	}
}
