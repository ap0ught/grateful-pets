using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Physic_AmputateLimb : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("scholarship", 3);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandAmputateLimb");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		GameObject parentObject;
		GameObject gameObject;
		string ordinalName;
		if (E.ID == "CommandAmputateLimb")
		{
			parentObject = ParentObject;
			if (parentObject.AreHostilesNearby())
			{
				parentObject.ShowFailure("You can't perform field amputations with hostiles nearby!");
				return false;
			}
			if (!parentObject.CanMoveExtremities(null, ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			GameObject weapon = parentObject.GetWeapon(IsUsableWeapon);
			if (weapon == null)
			{
				parentObject.ShowFailure("You must have an axe or a weapon capable of dismemberment equipped in order to perform a field amputation.");
				return false;
			}
			Cell cell = PickDirection("Amputate whose limb?");
			if (cell == null)
			{
				return false;
			}
			gameObject = ((cell == parentObject.CurrentCell) ? parentObject : cell.GetCombatTarget(parentObject, IgnoreFlight: false, IgnoreAttackable: true, IgnorePhase: true, 0, null, null, null, null, null, AllowInanimate: false));
			if (gameObject == null)
			{
				gameObject = cell.GetCombatTarget(parentObject, IgnoreFlight: true, IgnoreAttackable: true, IgnorePhase: true, 0, null, null, null, null, null, AllowInanimate: false);
				if (gameObject != null)
				{
					parentObject.ShowFailure("You cannot reach " + gameObject.t() + " to amputate " + gameObject.its + " limb.");
					return false;
				}
				parentObject.ShowFailure("There is no one there for you to amputate their limb.");
				return false;
			}
			if (gameObject != parentObject && !gameObject.IsLedBy(parentObject))
			{
				if (gameObject.CanMoveExtremities())
				{
					parentObject.ShowFailure(gameObject.T() + " won't let you do that.");
				}
				else
				{
					parentObject.ShowFailure("You cannot amputate " + gameObject.poss("limbs") + ".");
				}
				return false;
			}
			Body body = gameObject.Body;
			if (body == null)
			{
				parentObject.ShowFailure(gameObject.Does("have") + " no limbs.");
				return false;
			}
			List<BodyPart> list = new List<BodyPart>();
			foreach (BodyPart item in body.LoopParts())
			{
				if (item.IsSeverable())
				{
					list.Add(item);
				}
			}
			if (list.Count <= 0)
			{
				parentObject.ShowFailure(gameObject.Does("have") + " no limbs that can be amputated.");
				return false;
			}
			string[] array = new string[list.Count];
			char[] array2 = new char[list.Count];
			char c = 'a';
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				BodyPart bodyPart = list[i];
				string text = bodyPart.GetOrdinalName();
				if (bodyPart.Equipped != null)
				{
					text = text + " (" + bodyPart.Equipped.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true) + ")";
				}
				array[i] = text;
				array2[i] = ((c <= 'z') ? c++ : ' ');
			}
			int num = Popup.PickOption("Amputate which limb?", null, "", "Sounds/UI/ui_notification", array, array2, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
			if (num < 0)
			{
				return false;
			}
			BodyPart targetPart = list[num];
			ordinalName = targetPart.GetOrdinalName();
			if (gameObject == parentObject)
			{
				if (targetPart.Mortal && body.GetMortalPartCount() < 2)
				{
					parentObject.ShowFailure("You cannot bring yourself to amputate your " + ordinalName + ".");
					return false;
				}
				BodyPart bodyPart2 = weapon.EquippedOn();
				if (bodyPart2 != null && (bodyPart2 == targetPart || targetPart.IsParentPartOf(bodyPart2)))
				{
					GameObject weapon2 = parentObject.GetWeapon((GameObject obj) =>
					{
						if (obj == weapon)
						{
							return false;
						}
						if (!IsUsableWeapon(obj))
						{
							return false;
						}
						BodyPart bodyPart3 = obj.EquippedOn();
						return (bodyPart3 == null || (bodyPart3 != targetPart && !targetPart.IsParentPartOf(bodyPart3))) ? true : false;
					});
					if (weapon2 == null)
					{
						parentObject.ShowFailure("You cannot amputate the " + targetPart.TypeModel().Name + " holding " + parentObject.poss(weapon) + ".");
						return false;
					}
					weapon = weapon2;
				}
			}
			else
			{
				GameObject equipped = targetPart.Equipped;
				if (equipped == null || !equipped.IsAffliction())
				{
					GameObject cybernetics = targetPart.Cybernetics;
					if (cybernetics == null || !cybernetics.IsAffliction())
					{
						GameObject defaultBehavior = targetPart.DefaultBehavior;
						if (defaultBehavior == null || !defaultBehavior.IsAffliction())
						{
							goto IL_04e5;
						}
					}
				}
				if (targetPart.Mortal && body.GetMortalPartCount() < 2)
				{
					goto IL_04e5;
				}
			}
			if (!parentObject.PhaseMatches(gameObject))
			{
				IComponent<GameObject>.WDidXToYWithZ(parentObject, "try", "to amputate", gameObject, ordinalName + " with", weapon, ", but " + weapon.does("pass", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " through " + gameObject.them, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: false, IndefiniteIndirectObject: false, IndefiniteDirectObjectForOthers: false, IndefiniteIndirectObjectForOthers: false, PossessiveDirectObject: true);
				parentObject.UseEnergy(1000, "Physical Skill Physic AmputateLimb Failure PhaseFailure");
			}
			else if (gameObject.IsInStasis())
			{
				IComponent<GameObject>.WDidXToYWithZ(parentObject, "try", "to amputate", gameObject, ordinalName + " with", weapon, ", but " + weapon.does("have", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " no effect", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: false, IndefiniteIndirectObject: false, IndefiniteDirectObjectForOthers: false, IndefiniteIndirectObjectForOthers: false, PossessiveDirectObject: true);
				parentObject.UseEnergy(1000, "Physical Skill Physic AmputateLimb Failure StasisFailure");
			}
			else
			{
				ParentObject.PlayWorldOrUISound("Sounds/Abilities/sfx_ability_skill_attack_generic_activate");
				body.Dismember(targetPart, null, null, Obliterate: false, Silent: true);
				if (CombatJuice.enabled && (parentObject.IsPlayer() || gameObject.IsPlayer()))
				{
					CombatJuice.cameraShake(0.25f);
				}
				IComponent<GameObject>.WDidXToYWithZ(parentObject, "amputate", gameObject, ordinalName + " with", weapon, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: false, IndefiniteIndirectObject: false, IndefiniteDirectObjectForOthers: false, IndefiniteIndirectObjectForOthers: false, PossessiveDirectObject: true, PossessiveIndirectObject: false, null, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
				gameObject.ApplyEffect(new Bleeding("1d2", 35, parentObject));
				parentObject.UseEnergy(3000, "Physical Skill Physic AmputateLimb");
			}
		}
		return base.FireEvent(E);
		IL_04e5:
		parentObject.ShowFailure(gameObject.Does("see") + " no reason for you to amputate " + gameObject.its + " " + ordinalName + ".");
		return false;
	}

	public static bool IsUsableWeapon(GameObject Object)
	{
		if (Object == null)
		{
			return false;
		}
		MeleeWeapon part = Object.GetPart<MeleeWeapon>();
		if (part == null)
		{
			return false;
		}
		if (part.Skill == "Axe")
		{
			return true;
		}
		if (Object.HasPart<ModSerrated>())
		{
			return true;
		}
		if (Object.HasPart<ModGlazed>())
		{
			return true;
		}
		return false;
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Amputate Limb", "CommandAmputateLimb", "Skills", null, "ö");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.AddSkill(GO);
	}
}
