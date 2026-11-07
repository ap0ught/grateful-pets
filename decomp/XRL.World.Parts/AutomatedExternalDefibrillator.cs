using System;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Skills;

namespace XRL.World.Parts;

/// <remarks>
/// This part is not used in the base game.
///
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, chance to activate is increased by a
/// percentage equal to ((power load - 100) / 10), i.e. 30% for
/// the standard overload power load of 400, and damage is increased
/// by the standard power load bonus, i.e. 2 for the standard overload
/// power load of 400.
/// </remarks>
[Serializable]
public class AutomatedExternalDefibrillator : IPoweredPart
{
	public static readonly string COMMAND_NAME = "Defibrillate";

	public int Chance = 50;

	public string Damage = "1d4";

	public string DamageAttributes = "Electric";

	public string RequireSkill = "Physic";

	public AutomatedExternalDefibrillator()
	{
		ChargeUse = 500;
		IsPowerLoadSensitive = true;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		AutomatedExternalDefibrillator automatedExternalDefibrillator = p as AutomatedExternalDefibrillator;
		if (automatedExternalDefibrillator.Chance != Chance)
		{
			return false;
		}
		if (automatedExternalDefibrillator.Damage != Damage)
		{
			return false;
		}
		if (automatedExternalDefibrillator.DamageAttributes != DamageAttributes)
		{
			return false;
		}
		if (automatedExternalDefibrillator.RequireSkill != RequireSkill)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ExamineCriticalFailureEvent.ID && ID != ExamineFailureEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Defibrillate", "activate", COMMAND_NAME, null, 'a');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			AttemptDefibrillate(E.Actor, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!RequireSkill.IsNullOrEmpty())
		{
			int value = MyPowerLoadLevel();
			int effectiveChance = GetEffectiveChance(value);
			if (effectiveChance >= 100)
			{
				E.Postfix.AppendRules("Stops cardiac arrest.");
			}
			else if (effectiveChance > 0)
			{
				E.Postfix.AppendRules(effectiveChance + "% chance to stop cardiac arrest.");
			}
			if (!Damage.IsNullOrEmpty())
			{
				E.Postfix.AppendRules("Does " + GetEffectiveDamage(value) + ((!DamageAttributes.IsNullOrEmpty()) ? (" " + DamageAttributes.ToLower()) : "") + " damage per application.");
			}
			if (!RequireSkill.IsNullOrEmpty())
			{
				E.Postfix.AppendRules("Requires training in " + SkillFactory.GetSkillOrPowerName(RequireSkill) + " to use.");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineFailureEvent E)
	{
		if (ExamineFailure(E, 25))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineCriticalFailureEvent E)
	{
		if (ExamineFailure(E, 50))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool AttemptDefibrillate(GameObject Actor, IEvent FromEvent = null)
	{
		if (!Actor.CanMoveExtremities("Defibrillate", ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
		{
			return false;
		}
		if (!RequireSkill.IsNullOrEmpty() && !Actor.HasSkill(RequireSkill))
		{
			return Actor.Fail("You don't know how to use " + ParentObject.t() + ".");
		}
		int value = MyPowerLoadLevel();
		int? powerLoadLevel = value;
		if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			return Actor.Fail(ParentObject.Does("are") + " " + GetStatusPhrase() + ".");
		}
		Cell cell = Actor.PickDirection();
		if (cell == null)
		{
			return false;
		}
		GameObject Object = FindOptimalTarget(cell, Actor);
		if (Object == null)
		{
			Object = FindAlternateTarget(cell, Actor);
			if (Object == null)
			{
				if (Actor.IsPlayer())
				{
					if (cell.GetCombatTarget(Actor, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5) == null)
					{
						Actor.Fail("There is no one there to use " + ParentObject.t() + " on.");
					}
					else
					{
						Actor.Fail("There is no one there you can use " + ParentObject.t() + " on.");
					}
				}
				return false;
			}
			string message = ((Object != Actor) ? (Object.T() + Object.Is + " not in cardiac arrest. Do you want to use " + ParentObject.t() + " on " + Object.them + " anyway?") : ("You are not in cardiac arrest. Do you want to use " + ParentObject.t() + " on " + Actor.itself + " anyway?"));
			if (Popup.ShowYesNo(message) != DialogResult.Yes)
			{
				return false;
			}
		}
		powerLoadLevel = value;
		ConsumeCharge(null, powerLoadLevel);
		FromEvent?.RequestInterfaceExit();
		Actor.UseEnergy(1000, "Item Medical Defibrillator");
		if (Object != Actor && Object.IsHostileTowards(Actor) && Object.IsMobile())
		{
			int combatDV = Stats.GetCombatDV(Object);
			if (Stat.Random(1, 20) + Actor.StatMod("Agility") < combatDV)
			{
				IComponent<GameObject>.WDidXToYWithZ(Actor, "try", "to use", ParentObject, "on", Object, ", but " + Object.it + Object.GetVerb("dodge", PrependSpace: true, PronounAntecedent: true), "!", null, null, null, Actor, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: false, IndefiniteIndirectObject: false, IndefiniteDirectObjectForOthers: false, IndefiniteIndirectObjectForOthers: false, PossessiveDirectObject: false, PossessiveIndirectObject: false, null, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
				return false;
			}
		}
		IComponent<GameObject>.WDidXToYWithZ(Actor, "use", ParentObject, "on", Object, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: false, IndefiniteIndirectObject: false, IndefiniteDirectObjectForOthers: false, IndefiniteIndirectObjectForOthers: false, PossessiveDirectObject: false, PossessiveIndirectObject: false, null, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		if (!Damage.IsNullOrEmpty())
		{
			GameObject gameObject = Object;
			int amount = GetEffectiveDamage(value).RollCached();
			string damageAttributes = DamageAttributes;
			bool accidental = Object.HasEffect<CardiacArrest>();
			gameObject.TakeDamage(amount, "from the device.", damageAttributes, null, null, null, Actor, null, null, null, accidental);
		}
		if (GameObject.Validate(ref Object) && Object.HasEffect<CardiacArrest>() && GetEffectiveChance(value).in100())
		{
			Object.RemoveEffect<CardiacArrest>();
		}
		return true;
	}

	private bool ExamineFailure(IExamineEvent E, int Chance)
	{
		if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && Chance.in100())
		{
			int num = MyPowerLoadLevel();
			int? powerLoadLevel = num;
			if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
			{
				int low = 1 + IComponent<GameObject>.PowerLoadBonus(num);
				int high = (3 + IComponent<GameObject>.PowerLoadBonus(num)) * 3 / 2;
				int voltage = Stat.Random(low, high);
				string damageRange = low + "-" + high;
				if (ParentObject.Discharge(null, Target: E.Actor, Voltage: voltage, Damage: 0, DamageRange: damageRange, DamageRoll: null, Owner: E.Actor, Source: ParentObject, DescribeAsFrom: ParentObject, Skip: null, SkipList: null, StartCell: null, Phase: 0, Accidental: false, Environmental: false, UsePopups: E.Actor.IsPlayer()) > 0)
				{
					E.Identify = true;
					return true;
				}
			}
		}
		return false;
	}

	private static GameObject FindOptimalTarget(Cell C, GameObject Subject)
	{
		foreach (GameObject @object in C.Objects)
		{
			if (@object.HasEffect<CardiacArrest>() && Subject.PhaseAndFlightMatches(@object))
			{
				return @object;
			}
		}
		return null;
	}

	private static GameObject FindAlternateTarget(Cell C, GameObject Subject)
	{
		return C.GetCombatTarget(Subject);
	}

	public int GetEffectiveChance(int? PowerLoadLevel = null)
	{
		int num = Chance;
		int num2 = IComponent<GameObject>.PowerLoadBonus(PowerLoadLevel ?? MyPowerLoadLevel(), 100, 10);
		if (num2 != 0)
		{
			num = num * (100 + num2) / 100;
		}
		return num;
	}

	public string GetEffectiveDamage(int? PowerLoadLevel = null)
	{
		string text = Damage;
		int num = IComponent<GameObject>.PowerLoadBonus(PowerLoadLevel ?? MyPowerLoadLevel());
		if (num != 0)
		{
			text += num.Signed();
		}
		return text;
	}
}
