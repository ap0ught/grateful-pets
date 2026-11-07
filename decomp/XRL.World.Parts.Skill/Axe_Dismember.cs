using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World.AI;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Axe_Dismember : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public bool bDismembering;

	public static readonly int Cooldown = 30;

	public static readonly string BleedDamage = "1d2";

	public static readonly int BleedSave = 35;

	[NonSerialized]
	private static List<BodyPart> DismemberableBodyParts = new List<BodyPart>(8);

	public Axe_Dismember()
	{
	}

	public Axe_Dismember(GameObject ParentObject)
		: this()
	{
		this.ParentObject = ParentObject;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("BleedDamage", BleedDamage + " per round");
		int bleedSave = BleedSave;
		stats.Set("BleedSave", "Toughness " + bleedSave);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), Cooldown);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerAfterDamage");
		Registrar.Register("CommandDismember");
		base.Register(Object, Registrar);
	}

	public static bool Dismember(GameObject Attacker, GameObject Defender, Cell Where = null, BodyPart LostPart = null, GameObject Weapon = null, GameObject Projectile = null, string Sound = "sfx_characterTrigger_dismember", bool assumeDecapitate = false, bool suppressDecapitate = false, bool weaponActing = false, bool UsePopups = false)
	{
		if (LostPart == null)
		{
			LostPart = GetDismemberableBodyPart(Defender, Attacker, Weapon, assumeDecapitate, suppressDecapitate);
			if (LostPart == null)
			{
				return false;
			}
		}
		if (Where != null && (!GameObject.Validate(ref Defender) || Defender.CurrentCell != Where) && Where.IsOccluding())
		{
			Where = Where.GetFirstNonOccludingAdjacentCell() ?? Where;
		}
		if (Attacker != null && Attacker.IsPlayer() && Options.DismemberAsPopup)
		{
			UsePopups = true;
		}
		if (LostPart.SeverRequiresDecapitate())
		{
			return Axe_Decapitate.Decapitate(Attacker, Defender, Where, LostPart, Weapon, Projectile, weaponActing, UsePopups);
		}
		string ordinalName = LostPart.GetOrdinalName();
		if (Defender.Body.Dismember(LostPart, Attacker, Where, Obliterate: false, UsePopups) == null)
		{
			return false;
		}
		if (weaponActing && Weapon != null && Attacker != null)
		{
			IComponent<GameObject>.XDidYToZ(Weapon, "chop", "off", Defender, ordinalName, "!", null, null, null, Defender, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true, Attacker, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopups);
		}
		else
		{
			IComponent<GameObject>.XDidYToZ(Attacker, "chop", "off", Defender, ordinalName, "!", null, null, null, Defender, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopups);
		}
		Defender.PlayWorldSound(Sound);
		Defender.ParticleText("*dismembered!*", IComponent<GameObject>.ConsequentialColorChar(null, Defender));
		Defender.ApplyEffect(new Bleeding(BleedDamage, BleedSave, Attacker, Stack: true, Internal: false, UsePopups));
		Defender.RemoveAllEffects<Hooked>();
		if (((Attacker != null && Attacker.IsPlayer()) || (Defender != null && Defender.IsPlayer())) && CombatJuice.enabled)
		{
			CombatJuice.cameraShake(0.25f);
		}
		return true;
	}

	public GameObject GetPrimaryAxe()
	{
		return ParentObject.GetPrimaryWeaponOfType("Axe");
	}

	public bool IsPrimaryAxeEquipped()
	{
		return ParentObject.HasPrimaryWeaponOfType("Axe");
	}

	public static bool BodyPartIsDismemberable(BodyPart Part, GameObject Actor = null, bool assumeDecapitate = false, bool suppressDecapitate = false)
	{
		if (!Part.IsSeverable())
		{
			return false;
		}
		if (Part.SeverRequiresDecapitate())
		{
			if (suppressDecapitate)
			{
				return false;
			}
			if (!assumeDecapitate && !Axe_Decapitate.ShouldDecapitate(Actor))
			{
				return false;
			}
		}
		return true;
	}

	public static BodyPart GetDismemberableBodyPart(GameObject obj, GameObject Actor = null, GameObject Weapon = null, bool assumeDecapitate = false, bool suppressDecapitate = false)
	{
		Body body = obj.Body;
		if (body == null)
		{
			return null;
		}
		if (!obj.CanBeDismembered(Weapon))
		{
			return null;
		}
		DismemberableBodyParts.Clear();
		foreach (BodyPart part in body.GetParts())
		{
			if (BodyPartIsDismemberable(part, Actor, assumeDecapitate, suppressDecapitate))
			{
				DismemberableBodyParts.Add(part);
			}
		}
		return DismemberableBodyParts.GetRandomElement();
	}

	public static bool HasAnyDismemberableBodyPart(GameObject obj, GameObject Actor = null, GameObject Weapon = null, bool assumeDecapitate = false, bool suppressDecapitate = false)
	{
		Body body = obj.Body;
		if (body == null)
		{
			return false;
		}
		if (!obj.CanBeDismembered(Weapon))
		{
			return false;
		}
		foreach (BodyPart part in body.GetParts())
		{
			if (BodyPartIsDismemberable(part, Actor, assumeDecapitate, suppressDecapitate))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CastForceSuccess(GameObject attacker, Axe_Dismember skill = null, GameObject weapon = null)
	{
		if (skill == null)
		{
			skill = new Axe_Dismember(attacker);
		}
		if (weapon == null)
		{
			weapon = skill.GetPrimaryAxe();
		}
		Cell cell = skill.PickDirection("Dismember");
		if (cell != null)
		{
			GameObject combatTarget = cell.GetCombatTarget(attacker);
			if (combatTarget == attacker && attacker.IsPlayer() && Popup.ShowYesNo("Are you sure you want to dismember " + attacker.itself + "?") != DialogResult.Yes)
			{
				return true;
			}
			if (combatTarget != null)
			{
				Dismember(attacker, combatTarget, null, null, weapon, null, "sfx_ability_dismember");
			}
		}
		return true;
	}

	public static bool Cast(GameObject attacker, Axe_Dismember skill = null, GameObject weapon = null)
	{
		bool flag = false;
		if (skill == null)
		{
			skill = new Axe_Dismember(attacker);
			attacker.RegisterPartEvent(skill, "AttackerAfterDamage");
			flag = true;
		}
		if (weapon == null)
		{
			weapon = skill.GetPrimaryAxe();
		}
		if (attacker.CanMoveExtremities("Dismember", ShowMessage: true) && attacker.CanChangeBodyPosition("Dismember", ShowMessage: true))
		{
			Cell cell = skill.PickDirection("Dismember");
			if (cell != null)
			{
				GameObject combatTarget = cell.GetCombatTarget(attacker);
				if (combatTarget == null)
				{
					if (attacker.IsPlayer())
					{
						if (cell.HasObjectWithPart("Combat"))
						{
							Popup.Show("There's nothing there you can dismember.");
						}
						else
						{
							Popup.Show("There's nothing there to dismember.");
						}
					}
				}
				else
				{
					try
					{
						skill.bDismembering = true;
						if (combatTarget == attacker && attacker.IsPlayer() && Popup.ShowYesNo("Are you sure you want to dismember " + attacker.itself + "?") != DialogResult.Yes)
						{
							return true;
						}
						Combat.MeleeAttackWithWeapon(attacker, combatTarget, weapon, attacker.Body.FindDefaultOrEquippedItem(weapon), "Dismembering", 0, 0, 0, 0, 0, weapon.IsEquippedOrDefaultOfPrimary(attacker));
						attacker.UseEnergy(1000, "Skill Axe Dismember");
						if (!combatTarget.IsHostileTowards(attacker))
						{
							combatTarget.AddOpinion<OpinionAttack>(attacker, weapon);
						}
						skill.CooldownMyActivatedAbility(skill.ActivatedAbilityID, Cooldown);
					}
					catch (Exception ex)
					{
						XRLCore.LogError("Dismember", ex);
					}
					finally
					{
						skill.bDismembering = false;
					}
				}
			}
		}
		if (flag)
		{
			attacker.UnregisterPartEvent(skill, "AttackerAfterDamage");
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerAfterDamage")
		{
			if (E.GetIntParameter("Penetrations") > 0)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				GameObject Object = E.GetGameObjectParameter("Weapon");
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
				if (GameObject.Validate(ref Object))
				{
					MeleeWeapon part = Object.GetPart<MeleeWeapon>();
					if (part != null && part.Skill == "Axe")
					{
						Cell cell = E.GetParameter("Cell") as Cell;
						if (bDismembering || gameObjectParameter.HasEffect<Berserk>())
						{
							return Dismember(gameObjectParameter, gameObjectParameter2, cell, null, Object, null, "sfx_ability_dismember");
						}
						int num = 3;
						string stringParameter = E.GetStringParameter("Properties");
						if (stringParameter != null && stringParameter.HasDelimitedSubstring(',', "Charging") && gameObjectParameter.HasSkill("Cudgel_ChargingStrike"))
						{
							num *= 2;
						}
						if (Object.UsesTwoSlots)
						{
							num *= 2;
						}
						GameObject gameObject = Object;
						GameObject subject = gameObjectParameter2;
						num = GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObject, "Skill Dismember", num, subject);
						if (num.in100())
						{
							return Dismember(gameObjectParameter, gameObjectParameter2, null, null, null, null, "sfx_ability_dismember");
						}
					}
				}
			}
		}
		else if (E.ID == "CommandDismember")
		{
			GameObject primaryAxe = GetPrimaryAxe();
			if (primaryAxe == null)
			{
				return ParentObject.Fail("You must have an axe equipped in your primary hand to dismember.");
			}
			if (!ParentObject.CheckFrozen())
			{
				return false;
			}
			if (ParentObject.OnWorldMap())
			{
				return ParentObject.Fail("You cannot do that on the world map.");
			}
			return Cast(ParentObject, this, primaryAxe);
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Dismember", "CommandDismember", "Skills", "You make an attack with your axe at an adjacent opponent. If you hit and penetrate at least once, you dismember one of their limbs and they start bleeding (1-2 damage per turn. toughness save; difficulty 35). Additionally, your axe attacks that penetrate have a percentage chance to dismember: 3% for one-handed axes and 6% for two-handed axes.", "-");
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}
