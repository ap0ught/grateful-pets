using System;
using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Skill;

[Serializable]
public class ShortBlades_Rejoinder : BaseSkill
{
	public Guid ActivatedAbilityID;

	public bool Checked;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		Registrar.Register("CommandToggleRejoinder");
		Registrar.Register("DefenderAfterAttackMissed");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			Checked = false;
		}
		else if (E.ID == "DefenderAfterAttackMissed")
		{
			if (Checked)
			{
				return true;
			}
			Checked = true;
			if (!IsMyActivatedAbilityVoluntarilyUsable(ActivatedAbilityID) || !IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
			{
				return true;
			}
			if (!60.in100())
			{
				return true;
			}
			if (!ParentObject.CanMoveExtremities("Attack") || !ParentObject.CanChangeBodyPosition("Attack"))
			{
				return true;
			}
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObject = null;
			GameObject gameObject2 = null;
			List<GameObject> list = Event.NewGameObjectList();
			Body body = ParentObject.Body;
			foreach (BodyPart part in body.GetParts())
			{
				if (part.Primary && gameObject2 == null)
				{
					gameObject2 = part.GetFirstValidWeapon();
				}
				if (part.Type == "Thrown Weapon")
				{
					continue;
				}
				MeleeWeapon meleeWeapon = part.Equipped?.GetPart<MeleeWeapon>();
				if (meleeWeapon != null && meleeWeapon.Skill == "ShortBlades")
				{
					list.Add(part.Equipped);
					continue;
				}
				meleeWeapon = part.DefaultBehavior?.GetPart<MeleeWeapon>();
				if (meleeWeapon != null && meleeWeapon.Skill == "ShortBlades")
				{
					list.Add(part.DefaultBehavior);
				}
			}
			gameObject = list.GetRandomElement();
			if (gameObject != null)
			{
				ParentObject.ParticleText("*rejoinder*", IComponent<GameObject>.ConsequentialColorChar(ParentObject));
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You rejoinder with " + ParentObject.poss(gameObject) + ".", 'G');
				}
				else if (gameObjectParameter.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("rejoinder") + " with " + ParentObject.poss(gameObject) + ".", 'R');
				}
				Combat.MeleeAttackWithWeapon(ParentObject, gameObjectParameter, gameObject, body.FindDefaultOrEquippedItem(gameObject), null, 0, 0, 0, 0, 0, gameObject == gameObject2);
			}
		}
		else if (E.ID == "CommandToggleRejoinder")
		{
			ToggleMyActivatedAbility(ActivatedAbilityID);
		}
		return base.FireEvent(E);
	}

	private void AddAbility()
	{
		ActivatedAbilityID = AddMyActivatedAbility("Rejoinder", "CommandToggleRejoinder", "Skills", null, "\u001b", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
	}

	public override bool AddSkill(GameObject GO)
	{
		AddAbility();
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
