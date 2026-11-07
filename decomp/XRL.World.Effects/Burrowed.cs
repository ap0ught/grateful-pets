using System;
using ConsoleLib.Console;
using UnityEngine;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Burrowed : Effect
{
	public Guid EndAbilityID = Guid.Empty;

	public int MovePenalty;

	[NonSerialized]
	private long BoredUnburrowCheck = -1L;

	public Burrowed()
	{
		Duration = 1;
		DisplayName = "{{w|burrowed}}";
	}

	public Burrowed(int MSPenalty)
		: this()
	{
		MovePenalty = MSPenalty;
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
		return "Traveling underground.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Burrowed>())
		{
			return false;
		}
		Object.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_burrowingClaws_burrow");
		DidX("burrow", "into the ground");
		EndAbilityID = AddMyActivatedAbility("Stop Burrowing", "CommandEndBurrowing", "Physical Mutations", null, "\u0018", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: false, -1, null, null, Renderable.UITile("Abilities/abil_stop_burrow.bmp", 'w', 'W'));
		Object.DustPuff();
		ApplyChanges();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		RemoveMyActivatedAbility(ref EndAbilityID);
		Object.DustPuff();
		UnapplyChanges();
	}

	private void ApplyChanges()
	{
		Object.Render.Visible = false;
		StatShifter.SetStatShift(Object, "MoveSpeed", MovePenalty);
	}

	private void UnapplyChanges()
	{
		Object.Render.Visible = true;
		StatShifter.RemoveStatShifts(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanChangeMovementModeEvent>.ID && ID != PooledEvent<CanTravelEvent>.ID)
		{
			return ID == PooledEvent<AIBoredEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanChangeMovementModeEvent E)
	{
		if (Duration > 0 && !E.Involuntary && E.Object == Object)
		{
			if (E.ShowMessage)
			{
				E.Object.Fail("You cannot do that while burrowed.");
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanTravelEvent E)
	{
		if (E.Object == Object)
		{
			return E.Object.Fail("You cannot travel long distances while burrowed.");
		}
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
		Registrar.Register("CommandEndBurrowing");
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
					Popup.ShowFail("You cannot do that while burrowed.");
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
					DidX("are", "forced to the surface");
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
				if (E.HasFlag("ShowMessage") && Object.IsPlayer())
				{
					Popup.ShowFail("You cannot do that while burrowed.");
				}
				return false;
			}
		}
		else if (E.ID == "CommandEndBurrowing" || E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			Emerge();
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

	public void Emerge()
	{
		Object.PlayWorldOrUISound("Sounds/Abilities/sfx_ability_skill_generic");
		DidX("emerge", "from the ground");
		Object.RemoveEffect(this);
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		long turns = The.Game.Turns;
		if (turns > BoredUnburrowCheck && Object.Target == null)
		{
			if (BoredUnburrowCheck != -1 && ((int)(50f * Mathf.InverseLerp(0f, 100f, turns - BoredUnburrowCheck))).in100())
			{
				Emerge();
			}
			BoredUnburrowCheck = turns;
		}
		return base.HandleEvent(E);
	}
}
