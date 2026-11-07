using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tactics_Juke : BaseSkill
{
	public static readonly string COMMAND_NAME = "CommandTacticsJuke";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != SingletonEvent<BeforeAbilityManagerOpenEvent>.ID && ID != PooledEvent<CommandEvent>.ID)
		{
			return ID == GetMovementCapabilitiesEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMovementCapabilitiesEvent E)
	{
		E.Add("Juke", COMMAND_NAME, 7500, MyActivatedAbility(ActivatedAbilityID));
		return base.HandleEvent(E);
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown());
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			if (ParentObject.OnWorldMap())
			{
				return ParentObject.Fail("You cannot juke on the world map.");
			}
			if (!ParentObject.CheckFrozen() || !ParentObject.CanChangeBodyPosition("Juking", ShowMessage: true) || !ParentObject.CanChangeMovementMode("Juking", ShowMessage: true))
			{
				return false;
			}
			string text = PickDirectionS("Juke where?");
			if (text.IsNullOrEmpty())
			{
				return false;
			}
			Cell cell = ParentObject.CurrentCell?.GetCellFromDirection(text, BuiltOnly: false);
			if (cell == null || cell == ParentObject.CurrentCell)
			{
				return false;
			}
			foreach (GameObject item in cell.LoopObjectsWithPart("Physics"))
			{
				if (item.ConsiderSolidFor(ParentObject))
				{
					return ParentObject.Fail("You cannot juke into " + item.t() + ".");
				}
			}
			Cell cell2 = ParentObject.CurrentCell;
			if (cell2 == null)
			{
				return false;
			}
			GameObject Object = null;
			int i = 0;
			for (int count = cell.Objects.Count; i < count; i++)
			{
				GameObject gameObject = cell.Objects[i];
				if (gameObject.IsCombatObject() && ParentObject.PhaseAndFlightMatches(gameObject) && (!gameObject.HasPart<FungalVision>() || FungalVisionary.VisionLevel > 0))
				{
					if (gameObject.GetMatterPhase() >= 3 || IsRootedInPlaceEvent.Check(gameObject))
					{
						return ParentObject.Fail("You cannot juke " + gameObject.t() + " out of your way.");
					}
					if (Object != null)
					{
						return ParentObject.Fail("You cannot juke both " + gameObject.t() + " and " + Object.t() + " out of your way.");
					}
					Object = gameObject;
				}
			}
			if (!ParentObject.Move(text, Forced: false, System: false, IgnoreGravity: true, NoStack: false, AllowDashing: false, DoConfirmations: false, null, null, NearestAvailable: false, ForceSwap: Object, EnergyCost: 0, Type: null, MoveSpeed: null, Peaceful: false, IgnoreMobility: true))
			{
				return false;
			}
			if (GameObject.Validate(ref Object) && cell2.Objects.Contains(Object))
			{
				DidXToY("juke", Directions.GetDirectionDescription(text) + ", moving", Object, "out of " + ParentObject.its + " way", null, null, null, ParentObject);
				if (ParentObject.HasSkill("ShortBlades_PointedCircle") && Object.IsHostileTowards(ParentObject) && ParentObject.PhaseAndFlightMatches(Object))
				{
					GameObject primaryWeaponOfType = ParentObject.GetPrimaryWeaponOfType("ShortBlades", AcceptFirstHandForNonHandPrimary: true);
					if (primaryWeaponOfType != null)
					{
						Combat.MeleeAttackWithWeapon(ParentObject, Object, primaryWeaponOfType, ParentObject.Body.FindDefaultOrEquippedItem(primaryWeaponOfType), "Juking", 0, 0, 0, 0, 0, Primary: true);
					}
				}
				ParentObject?.FireEvent(Event.New("JukedTarget", "Defender", Object));
				Object?.FireEvent(Event.New("WasJuked", "Attacker", ParentObject));
			}
			else
			{
				DidX("juke", Directions.GetDirectionDescription(text), null, null, null, ParentObject);
			}
			ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_juke");
			int turns = (ParentObject.HasPart<Acrobatics_Tumble>() ? 20 : 40);
			CooldownMyActivatedAbility(ActivatedAbilityID, turns, null, "Agility");
			ParentObject.FireEvent(Event.New("Juked", "FromCell", cell2, "ToCell", cell));
			Object?.Gravitate();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance == 1 && GameObject.Validate(E.Target) && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.Actor.PhaseAndFlightMatches(E.Target) && E.Target.GetMatterPhase() < 3 && !IsRootedInPlaceEvent.Check(E.Target) && !E.Target.CurrentCell.IsSolidFor(E.Actor))
		{
			int num = 10;
			if (E.Actor.HasSkill("ShortBlades_PointedCircle") && E.Actor.GetPrimaryWeaponOfType("ShortBlades", AcceptFirstHandForNonHandPrimary: true) != null)
			{
				num += 40;
			}
			if (num.in100())
			{
				E.Add(COMMAND_NAME);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public int GetCooldown()
	{
		if (!ParentObject.HasPart<Acrobatics_Tumble>())
		{
			return 40;
		}
		return 20;
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Juke", COMMAND_NAME, "Skills", null, "\u0012");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
