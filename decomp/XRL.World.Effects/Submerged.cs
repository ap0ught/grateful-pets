using System;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Submerged : Effect
{
	public Guid EndAbilityID = Guid.Empty;

	public int MovePenalty;

	public int HiddenDifficulty = 20;

	public Submerged()
	{
		Duration = 1;
		DisplayName = "{{B|submerged}}";
	}

	public Submerged(int MovePenalty = 0, int HiddenDifficulty = 20)
		: this()
	{
		this.MovePenalty = MovePenalty;
		this.HiddenDifficulty = HiddenDifficulty;
	}

	public override int GetEffectType()
	{
		return 128;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Staying underwater.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Submerged>())
		{
			return false;
		}
		GameObject gameObject = Object.CurrentCell?.GetAquaticSupportFor(Object);
		if (gameObject == null)
		{
			return false;
		}
		Object?.PlayWorldSound("Sounds/Abilities/sfx_ability_water_submerge");
		DidX("submerge", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
		EndAbilityID = AddMyActivatedAbility("Surface", "CommandEndSubmerged", "Maneuvers", null, "\u0018");
		Object.LiquidSplash(gameObject.LiquidVolume?.GetPrimaryLiquid());
		ApplyChanges();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		try
		{
			Hidden part = Object.GetPart<Hidden>();
			GameObject gameObject = Object.CurrentCell?.GetAquaticSupportFor(Object);
			if (gameObject != null)
			{
				part?.Reveal(Silent: true);
				Object?.PlayWorldSound("Sounds/Abilities/sfx_ability_water_emerge");
				DidXToY("emerge", "from", gameObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: true, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
				Object.LiquidSplash(gameObject.LiquidVolume?.GetPrimaryLiquid());
			}
			else if (part != null)
			{
				part.Silent = false;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Submerged::Remove", x);
		}
		RemoveMyActivatedAbility(ref EndAbilityID);
		UnapplyChanges();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanChangeMovementModeEvent>.ID && ID != SingletonEvent<EarlyBeforeBeginTakeActionEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == LeavingCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanChangeMovementModeEvent E)
	{
		if (Duration > 0 && !E.Involuntary && E.Object == Object && E.To != "Swimming" && E.To != "Wading")
		{
			if (E.ShowMessage)
			{
				E.Object.Fail("You cannot do that while submerged.");
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EarlyBeforeBeginTakeActionEvent E)
	{
		Validate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Validate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeavingCellEvent E)
	{
		Surface();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("AIAttackMelee");
		Registrar.Register("AIAttackRange");
		Registrar.Register("AICanAttackMelee");
		Registrar.Register("AICanAttackRange");
		Registrar.Register("AILookForTarget");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("BeforeDirectMove");
		Registrar.Register("BeforeTeleport");
		Registrar.Register("BodyPositionChanged");
		Registrar.Register("CanChangeBodyPosition");
		Registrar.Register("CanMoveExtremities");
		Registrar.Register("CommandEndSubmerged");
		Registrar.Register("FiringMissile");
		Registrar.Register("LateBeforeApplyDamage");
		Registrar.Register("MovementModeChanged");
		Registrar.Register("BeginAttack");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AILookForTarget" || E.ID == "AIAttackMelee" || E.ID == "AIAttackRange" || E.ID == "AICanAttackMelee" || E.ID == "AICanAttackRange")
		{
			if (Duration > 0)
			{
				return false;
			}
		}
		else if (E.ID == "BeginAttack" || E.ID == "CanFireMissileWeapon")
		{
			if (Duration > 0)
			{
				if (Object.IsPlayer())
				{
					Popup.ShowFail("You cannot do that while submerged.");
				}
				return false;
			}
		}
		else if (E.ID == "LateBeforeApplyDamage")
		{
			if (Duration > 0)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Owner");
				Damage damage = E.GetParameter("Damage") as Damage;
				if (gameObjectParameter != null && gameObjectParameter != Object && damage != null && !damage.HasAttribute("Bleeding"))
				{
					DidX("are", "forced to the surface", null, null, null, null, Object);
					Object.RemoveEffect(this);
					return false;
				}
			}
		}
		else if (E.ID == "BeforeDirectMove" || E.ID == "BeforeTeleport" || E.ID == "BeginAttack" || E.ID == "FiringMissile")
		{
			if (Duration > 0)
			{
				Object.RemoveEffect(this);
			}
		}
		else if (E.ID == "CanChangeBodyPosition" || E.ID == "CanMoveExtremities")
		{
			if (Duration > 0 && !E.HasFlag("Involuntary"))
			{
				string stringParameter = E.GetStringParameter("To");
				if (stringParameter != "Swimming" && stringParameter != "Wading")
				{
					if (E.HasFlag("ShowMessage") && Object.IsPlayer())
					{
						Popup.ShowFail("You cannot do that while submerged.");
					}
					return false;
				}
			}
		}
		else if (E.ID == "CommandEndSubmerged")
		{
			Surface();
		}
		else if (E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			string stringParameter2 = E.GetStringParameter("To");
			if (stringParameter2 != "Swimming" && stringParameter2 != "Wading")
			{
				Surface();
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyChanges();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyChanges();
		}
		return base.FireEvent(E);
	}

	private void ApplyChanges()
	{
		StatShifter.SetStatShift(Object, "MoveSpeed", MovePenalty);
		Hidden part = Object.GetPart<Hidden>();
		if (part == null)
		{
			part = new Hidden(HiddenDifficulty, Silent: true);
			Object.AddPart(part);
		}
		else
		{
			part.Difficulty = HiddenDifficulty;
			part.Silent = true;
			part.Found = false;
		}
	}

	private void UnapplyChanges()
	{
		StatShifter.RemoveStatShifts(Object);
		Object.GetPart<Hidden>()?.Reveal();
	}

	private bool Validate()
	{
		if (CanSubmerge())
		{
			return true;
		}
		Object.RemoveEffect(this);
		return false;
	}

	public void Surface()
	{
		Object.PlayWorldOrUISound("Sounds/Abilities/sfx_ability_skill_generic");
		Object.RemoveEffect(this);
	}

	public bool CanSubmerge()
	{
		return CanSubmerge(Object);
	}

	public static bool CanSubmerge(GameObject Object)
	{
		return CanSubmergeIn(Object, Object?.CurrentCell);
	}

	public static bool CanSubmergeIn(GameObject Object, Cell C)
	{
		if (C != null && GameObject.Validate(ref Object))
		{
			return C.HasAquaticSupportFor(Object);
		}
		return false;
	}
}
