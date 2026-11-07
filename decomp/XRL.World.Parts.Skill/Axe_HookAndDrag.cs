using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Axe_HookAndDrag : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public GameObject HookedObject;

	public GlobalLocation LeftCell = new GlobalLocation();

	public GameObject HookingWeapon;

	public static readonly int COOLDOWN = 50;

	public static readonly int RESISTSAVE = 20;

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("DurationSpecial", "9 rounds or until opponent is dismembered");
		int rESISTSAVE = RESISTSAVE;
		stats.Set("ResistSave", "Strength " + rESISTSAVE);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != BeforeDeathRemovalEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != EnteredCellEvent.ID && ID != LeftCellEvent.ID)
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

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance == 1 && !E.Actor.IsFrozen() && ParentObject.PhaseAndFlightMatches(E.Actor))
		{
			if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.Add("CommandAxeHookAndDrag");
			}
			if (GameObject.Validate(ref HookedObject) && !HookedObject.IsNowhere() && HookedObject.GetEffect(typeof(Hooked), WeaponMatch) is Hooked { Duration: >0 })
			{
				E.Add("MetaCommandMoveAway");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		Validate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (GameObject.Validate(ref HookedObject) && HookedObject.GetEffect(typeof(Hooked), WeaponMatch) is Hooked e)
		{
			HookedObject.RemoveEffect(e);
		}
		HookingWeapon = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (HookedObject != null && HookedObject.IsInvalid())
		{
			HookedObject = null;
			HookingWeapon = null;
		}
		if (HookedObject != null && LeftCell.IsCell())
		{
			if (HookedObject.GetEffect(typeof(Hooked), WeaponMatch) is Hooked hooked)
			{
				if (!HookedObject.PhaseAndFlightMatches(ParentObject))
				{
					HookedObject.RemoveEffect(hooked);
					HookedObject = null;
					HookingWeapon = null;
				}
				else if (hooked.Duration > 0)
				{
					int num = ParentObject.DistanceTo(HookedObject);
					if (num == 2)
					{
						string directionFromCell = HookedObject.CurrentCell.GetDirectionFromCell(LeftCell.ResolveCell());
						Combat.MeleeAttackWithWeapon(ParentObject, HookedObject, HookingWeapon, ParentObject.Body.FindDefaultOrEquippedItem(HookingWeapon), "Autohit", 0, 0, 0, 0, 0, HookingWeapon?.IsEquippedOrDefaultOfPrimary(ParentObject) ?? false);
						if (HookedObject.IsValid())
						{
							HookedObject.Move(directionFromCell, Forced: false, System: false, IgnoreGravity: false, NoStack: false, AllowDashing: true, DoConfirmations: true, HookingWeapon);
							HookedObject.PlayWorldSound("Sounds/Abilities/sfx_ability_drag");
						}
					}
					else if (num > 2)
					{
						HookedObject.RemoveEffect(hooked);
						HookedObject = null;
						HookingWeapon = null;
					}
				}
			}
			else
			{
				HookedObject = null;
				HookingWeapon = null;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		Validate(E.Cell);
		if (HookedObject != null)
		{
			LeftCell.SetCell(E.Cell);
		}
		else
		{
			LeftCell.Clear();
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginMove");
		Registrar.Register("CommandAxeHookAndDrag");
		Registrar.Register("StopFighting");
		base.Register(Object, Registrar);
	}

	private bool WeaponMatch(Effect FX)
	{
		if (FX is Hooked hooked)
		{
			return hooked.HookingWeapon == HookingWeapon;
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginMove")
		{
			if (ParentObject != null && GameObject.Validate(ref HookedObject) && HookedObject.DistanceTo(ParentObject) <= 1 && HookedObject.PhaseAndFlightMatches(ParentObject))
			{
				if (HookedObject.GetEffect(typeof(Hooked), WeaponMatch) is Hooked hooked)
				{
					if (E.GetParameter("DestinationCell") is Cell c && HookedObject.DistanceTo(c) == 2 && hooked.Duration > 0 && (!HookedObject.FireEvent("BeforeGrabbed") || HookedObject.MakeSave("Strength", RESISTSAVE, ParentObject, null, "HookAndDrag Move Grab Restraint", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, hooked.HookingWeapon)))
					{
						IComponent<GameObject>.XDidY(HookedObject, "stand", HookedObject.its + " ground", "!", null, null, null, ParentObject);
						ParentObject.UseEnergy(1000, "Movement Failure");
						return false;
					}
				}
				else
				{
					HookedObject = null;
					HookingWeapon = null;
				}
			}
			else
			{
				if (HookedObject != null)
				{
					if (HookedObject.IsValid() && HookedObject.GetEffect(typeof(Hooked), WeaponMatch) is Hooked e)
					{
						HookedObject.RemoveEffect(e);
					}
					HookedObject = null;
				}
				HookingWeapon = null;
			}
		}
		else if (E.ID == "CommandAxeHookAndDrag")
		{
			GameObject primaryWeaponOfType = ParentObject.GetPrimaryWeaponOfType("Axe", AcceptFirstHandForNonHandPrimary: true);
			if (primaryWeaponOfType == null)
			{
				ParentObject.ShowFailure("You must have an axe equipped in your primary hand to use Hook and Drag.");
				return false;
			}
			if (!ParentObject.CheckFrozen())
			{
				return false;
			}
			Cell cell = PickDirection("Hook and Drag");
			if (cell == null)
			{
				return false;
			}
			GameObject combatTarget = cell.GetCombatTarget(ParentObject);
			if (combatTarget == null || !combatTarget.PhaseAndFlightMatches(ParentObject))
			{
				ParentObject.ShowFailure("There's nothing there you can hook.");
				return false;
			}
			if (!combatTarget.FireEvent("BeforeGrabbed"))
			{
				return false;
			}
			if (!combatTarget.ApplyEffect(new Hooked(primaryWeaponOfType, 20, 9)))
			{
				return false;
			}
			ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_drag");
			DidXToYWithZ("hook", combatTarget, "with", primaryWeaponOfType, null, "!", null, null, null, combatTarget, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: false, IndefiniteIndirectObject: false, IndefiniteDirectObjectForOthers: false, IndefiniteIndirectObjectForOthers: false, PossessiveDirectObject: false, PossessiveIndirectObject: false, null, null, ParentObject);
			HookedObject = combatTarget;
			HookingWeapon = primaryWeaponOfType;
			combatTarget.Bloodsplatter();
			CooldownMyActivatedAbility(ActivatedAbilityID, COOLDOWN);
		}
		else if (E.ID == "StopFighting")
		{
			if (HookedObject != null && !HookedObject.IsValid())
			{
				HookedObject = null;
				HookingWeapon = null;
			}
			if (HookedObject != null)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
				if (gameObjectParameter == null || gameObjectParameter == HookedObject)
				{
					if (HookedObject.GetEffect(typeof(Hooked), WeaponMatch) is Hooked e2)
					{
						HookedObject.RemoveEffect(e2);
					}
					HookedObject = null;
					HookingWeapon = null;
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Hook and Drag", "CommandAxeHookAndDrag", "Skills", "You grab an opponent's limb with the heel of your axe and pull them toward you. If successful, you pull your opponent with you as you move and make a free attack with your axe. Your opponent is forced to move with you but can attack you while moving. Your opponent gets a chance to resist the move (strength save; difficulty 20 + your strength modifier) and a chance to break free at the start of their turn (same save).\n\nThis effect lasts for 9 rounds or until you dismember the opponent.", "ô");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.AddSkill(GO);
	}

	public bool Validate(Cell C = null)
	{
		Hooked hooked = null;
		if (GameObject.Validate(ref HookedObject))
		{
			hooked = HookedObject.GetEffect(typeof(Hooked), WeaponMatch) as Hooked;
			if (hooked != null)
			{
				if (C != null)
				{
					if (C.DistanceTo(HookedObject) <= 1)
					{
						goto IL_0061;
					}
				}
				else if (ParentObject.DistanceTo(HookedObject) <= 1)
				{
					goto IL_0061;
				}
			}
		}
		if (hooked != null)
		{
			HookedObject?.RemoveEffect(hooked);
		}
		HookedObject = null;
		HookingWeapon = null;
		return false;
		IL_0061:
		return true;
	}
}
