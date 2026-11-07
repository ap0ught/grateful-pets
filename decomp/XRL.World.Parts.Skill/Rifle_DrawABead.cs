using System;
using XRL.Messages;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Rifle_DrawABead : BaseSkill
{
	public static readonly string COMMAND_NAME = "CommandMarkTarget";

	public GameObject Mark;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != SingletonEvent<EarlyBeforeBeginTakeActionEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != SuspendingEvent.ID)
		{
			return ID == ApplyEffectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance >= 3 && GameObject.Validate(E.Target) && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && !ParentObject.IsConfused && ParentObject.CanMoveExtremities() && !E.Target.HasEffect((RifleMark fx) => fx.Marker == ParentObject) && E.Distance <= ParentObject.GetVisibilityRadius() && ParentObject.HasMissileWeapon(null, (MissileWeapon mw) => IsCompatibleMissileWeapon(mw) && mw.ReadyToFire()) && ParentObject.HasLOSTo(E.Target, IncludeSolid: true, BlackoutStops: false, UseTargetability: true))
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EarlyBeforeBeginTakeActionEvent E)
	{
		ValidateMark();
		return true;
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			if (ParentObject.IsConfused)
			{
				return ParentObject.Fail("You cannot mark a target while you are confused.");
			}
			if (!ParentObject.CanMoveExtremities(null, ShowMessage: true))
			{
				return false;
			}
			Cell cell = PickDestinationCell(80, AllowVis.OnlyVisible, Locked: true, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Mark Target", Snap: true);
			if (cell == null)
			{
				return true;
			}
			GameObject gameObject = cell.GetCombatTarget(ParentObject, IgnoreFlight: true) ?? cell.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
			if (gameObject != null)
			{
				SetMark(gameObject);
			}
			ParentObject.UseEnergy(1000, "Physical Skill");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(SuspendingEvent E)
	{
		ClearMark();
		return true;
	}

	public override bool HandleEvent(ApplyEffectEvent E)
	{
		if (E.Name == "Confusion")
		{
			ClearMark();
		}
		return base.HandleEvent(E);
	}

	public bool SetMark(GameObject Target)
	{
		if (!Target.ApplyEffect(new RifleMark(ParentObject)))
		{
			return false;
		}
		if (ParentObject.IsPlayer())
		{
			IComponent<GameObject>.PlayUISound("Sounds/Abilities/sfx_ability_rifle_markTarget");
		}
		else
		{
			Target.PlayWorldSound("Sounds/Abilities/sfx_ability_rifle_markTarget");
		}
		ClearMark();
		Mark = Target;
		if (ParentObject.IsPlayer())
		{
			Sidebar.CurrentTarget = Target;
		}
		if (Visible())
		{
			DidXToY("draw", "a bead on", Target, ((Target.IsPlayer() || ParentObject.IsPlayer()) ? "" : The.Player.DescribeDirectionToward(Target)) + ". " + Target.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false) + " marked", null, null, null, ParentObject);
		}
		return true;
	}

	public void ClearMark()
	{
		if (GameObject.Validate(ref Mark))
		{
			RifleMark effect = Mark.GetEffect((RifleMark fx) => fx.Marker == ParentObject);
			if (effect != null)
			{
				Mark.RemoveEffect(effect);
			}
			Mark = null;
		}
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Mark Target", COMMAND_NAME, "Skills", null, "Î");
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}

	public void ValidateMark()
	{
		if (!GameObject.Validate(ref Mark))
		{
			return;
		}
		if (Mark.IsNowhere())
		{
			ClearMark();
		}
		else if (ParentObject.IsPlayer() && !Mark.IsVisible())
		{
			MessageQueue.AddPlayerMessage("You lose sight of your mark.", 'R');
			ClearMark();
		}
		else if (!ParentObject.IsPlayer() && (!ParentObject.HasLOSTo(Mark, IncludeSolid: true, BlackoutStops: false, UseTargetability: true) || ParentObject.DistanceTo(Mark) > ParentObject.GetVisibilityRadius()))
		{
			ClearMark();
		}
		else if (!Mark.HasEffect((RifleMark fx) => fx.Marker == ParentObject))
		{
			if (ParentObject.IsPlayer())
			{
				MessageQueue.AddPlayerMessage("Your tracking of your mark has been disrupted.");
			}
			ClearMark();
		}
	}

	public static bool IsCompatibleMissileWeapon(string Skill)
	{
		if (!(Skill == "Rifle"))
		{
			return Skill == "Bow";
		}
		return true;
	}

	public static bool IsCompatibleMissileWeapon(MissileWeapon MW)
	{
		return IsCompatibleMissileWeapon(MW?.Skill);
	}

	public static bool IsCompatibleMissileWeapon(GameObject Object)
	{
		return IsCompatibleMissileWeapon(Object?.GetPart<MissileWeapon>());
	}
}
