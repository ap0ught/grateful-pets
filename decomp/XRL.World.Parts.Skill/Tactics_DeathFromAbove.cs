using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

/// This part is not used in the base game.
[Serializable]
public class Tactics_DeathFromAbove : BaseSkill
{
	public static readonly string COMMAND_NAME = "DeathFromAbove";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<CommandEvent>.ID && ID != GetAttackerMeleePenetrationEvent.ID)
		{
			return ID == GetMovementCapabilitiesEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMovementCapabilitiesEvent E)
	{
		E.Add("Death From Above", COMMAND_NAME, 8500, MyActivatedAbility(ActivatedAbilityID), IsAttack: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (GameObject.Validate(E.Target) && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.Actor.CanChangeBodyPosition() && E.Actor.CanChangeMovementMode() && !E.Actor.IsOverburdened())
		{
			bool flag = false;
			if (E.Actor.IsFlying)
			{
				if (E.Distance <= 1)
				{
					flag = true;
				}
			}
			else
			{
				GetJumpingBehaviorEvent.Retrieve(E.Actor, out var RangeModifier, out var MinimumRange, out var _, out var _, out var _, out var CanJumpOverCreatures);
				int num = Acrobatics_Jump.GetBaseRange(E.Actor) + RangeModifier;
				if (E.Distance >= MinimumRange && E.Distance <= num && Acrobatics_Jump.CheckPath(E.Actor, E.Target.CurrentCell, CanJumpOverCreatures, CanLandOnCreature: true, Silent: true))
				{
					flag = true;
				}
			}
			if (flag)
			{
				E.Add(COMMAND_NAME, 1, null, Inv: false, Self: false, E.Target);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAttackerMeleePenetrationEvent E)
	{
		string properties = E.Properties;
		if (properties != null && properties.HasDelimitedSubstring(',', "DeathFromAbove"))
		{
			E.PenetrationBonus += 2;
			E.MaxPenetrationBonus += 2;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMeleeAttackChanceEvent E)
	{
		if (E.Properties.HasDelimitedSubstring(',', "DeathFromAbove"))
		{
			E.SetFinalizedChance((E.Intrinsic && !E.Consecutive) ? 100 : 0);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			if (!PerformDeathFromAbove(ParentObject, E.Target))
			{
				return false;
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, 40);
		}
		return base.HandleEvent(E);
	}

	private static bool ValidDeathFromAboveTarget(GameObject obj)
	{
		return obj?.IsCombat ?? false;
	}

	public int GetMinimumRange()
	{
		return 2;
	}

	public int GetMaximumRange()
	{
		return 3 + ParentObject.GetIntProperty("ChargeRangeModifier");
	}

	public static bool PerformDeathFromAbove(GameObject Actor, GameObject Target = null, string SourceKey = null)
	{
		if (Actor.OnWorldMap())
		{
			Actor.Fail("You cannot perform Death From Above on the world map.");
			return false;
		}
		if (Actor.IsOverburdened())
		{
			Actor.Fail("You cannot perform Death From Above while overburdened.");
			return false;
		}
		if (!Actor.CanChangeBodyPosition("Charging", ShowMessage: true))
		{
			return false;
		}
		if (!Actor.CanChangeMovementMode("Charging", ShowMessage: true))
		{
			return false;
		}
		bool isFlying = Actor.IsFlying;
		int MinimumRange = 0;
		int num = 1;
		bool CanJumpOverCreatures = false;
		string AbilityName = null;
		string Verb = null;
		string ProviderKey = null;
		if (!isFlying)
		{
			GetJumpingBehaviorEvent.Retrieve(Actor, out var RangeModifier, out MinimumRange, out AbilityName, out Verb, out ProviderKey, out CanJumpOverCreatures);
			num = Acrobatics_Jump.GetBaseRange(Actor) + RangeModifier;
		}
		Cell cell = null;
		if (Target == null)
		{
			if (Actor.IsPlayer())
			{
				cell = ((!isFlying) ? Actor.Physics.PickLine(num + 1, AllowVis.OnlyVisible, ValidDeathFromAboveTarget, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, BlackoutStops: false, null, null, "Death From Above", Snap: true)?.Last() : Actor.Physics.PickDirection(ForAttack: true));
				if (cell == null)
				{
					return false;
				}
				Target = cell.GetCombatTarget(Actor, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: true, 0, null, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: true);
				if (Target == null)
				{
					Actor.Fail("There is nobody there to perform Death From Above on.");
					return false;
				}
			}
			else
			{
				Target = Actor.Target;
			}
			if (Target == null)
			{
				return false;
			}
		}
		cell = Target.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		int num2 = Actor.DistanceTo(Target);
		List<Point> Path = null;
		if (isFlying)
		{
			if (num2 > 1)
			{
				Actor.Fail("While flying, you can only perform Death From Above on nearby targets.");
				return false;
			}
		}
		else
		{
			if (num2 < MinimumRange)
			{
				Actor.Fail("To perform Death From Above from the ground, you must select a target at least " + MinimumRange.Things("square") + " away.");
				return false;
			}
			if (num2 > num)
			{
				Actor.Fail("To perform Death From Above from the ground, you must select a target no more than " + num.Things("square") + " away.");
				return false;
			}
			if (!Acrobatics_Jump.CheckPath(Actor, cell, out Path, false, CanJumpOverCreatures, true, "jump"))
			{
				return false;
			}
		}
		if (Target == Actor)
		{
			Actor.Fail("You cannot perform Death From Above on " + Actor.itself + ".");
			return false;
		}
		if (!Target.IsCombatObject())
		{
			if (Target.IsWall())
			{
				Actor.Fail("You cannot perform Death From Above on a wall.");
				return false;
			}
			if (Target.IsDoor())
			{
				Actor.Fail("You cannot perform Death From Above on a door.");
				return false;
			}
		}
		Cell originCell = Actor.CurrentCell;
		IComponent<GameObject>.XDidYToZ(Actor, isFlying ? "dive" : (Verb ?? "leap"), "at", Target, null, "!", null, null, null, Target);
		Acrobatics_Jump.PlayAnimation(Actor, cell);
		bool flag = false;
		if (!isFlying)
		{
			Cell cell2 = null;
			Point point = null;
			if (Path != null && Path.Count >= 2)
			{
				point = Path[Path.Count - 2];
			}
			if (point != null)
			{
				cell2 = cell.ParentZone.GetCell(point.X, point.Y);
			}
			if (cell2 == null)
			{
				cell2 = cell;
			}
			bool terse = The.Game.Player.Messages.Terse;
			try
			{
				if (!terse)
				{
					The.Game.Player.Messages.Terse = true;
				}
				flag = Actor.DirectMoveTo(cell2, 0, Forced: false, IgnoreCombat: true, IgnoreGravity: true);
			}
			catch (Exception x)
			{
				MetricsManager.LogError("DeathFromAbove tempCell move", x);
			}
			finally
			{
				if (!terse)
				{
					The.Game.Player.Messages.Terse = false;
				}
			}
		}
		bool isFlying2 = Target.IsFlying;
		if (Actor.InSameOrAdjacentCellTo(Target))
		{
			Combat.PerformMeleeAttack(Actor, Target, 0, 0, 0, 0, "Charging,DeathFromAbove", IgnoreFlight: true);
			Actor.FireEvent(Event.New("ChargedTarget", "Defender", Target).SetFlag("DeathFromAbove", State: true));
			Target.FireEvent(Event.New("WasCharged", "Attacker", Actor).SetFlag("DeathFromAbove", State: true));
		}
		Cell targetCell = ((GameObject.Validate(Target) && !Target.IsNowhere()) ? null : cell) ?? cell.GetRandomLocalAdjacentCell((Cell c) => c.IsEmptyOfSolidFor(Actor)) ?? cell.GetRandomLocalAdjacentCell(2, (Cell c) => c.IsEmptyOfSolidFor(Actor)) ?? cell.GetRandomLocalAdjacentCell() ?? cell;
		Actor.DirectMoveTo(targetCell, 0, Forced: true, IgnoreCombat: true);
		if (isFlying)
		{
			Flight.Land(Actor, Silent: true);
		}
		if (flag)
		{
			JumpedEvent.Send(Actor, originCell, cell, Path, num, AbilityName, ProviderKey, SourceKey);
		}
		Actor.PlayWorldSound("Sounds/Foley/fly_generic_fall");
		if (!Actor.HasEffect<Prone>() && !Actor.MakeSave("Agility", 20, null, null, "DeathFromAbove Knockdown", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, Actor))
		{
			Actor.ApplyEffect(new Prone());
		}
		if (GameObject.Validate(Target) && !Target.IsNowhere())
		{
			if (isFlying2 && Actor.PhaseMatches(Target))
			{
				Target.ApplyEffect(new Grounded(Stat.Random(3, 5)));
			}
			if (!Target.HasEffect<Prone>() && !Target.MakeSave("Agility", 20, null, null, "DeathFromAbove Knockdown", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, Actor))
			{
				Target.ApplyEffect(new Prone());
			}
		}
		Actor.UseEnergy(1000, "DeathFromAbove");
		return true;
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Death From Above", COMMAND_NAME, "Skills", null, "\u0019", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}
