using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Pistol_EmptyTheClips : BaseSkill
{
	public static readonly string COMMAND_NAME = "CommandPistolEmptyTheClips";

	public static readonly int DURATION = 20;

	public static readonly int COOLDOWN = 200;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != PooledEvent<CommandEvent>.ID)
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

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("Duration", DURATION);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		ActivatedAbilityEntry activatedAbilityEntry = MyActivatedAbility(ActivatedAbilityID);
		if (activatedAbilityEntry != null)
		{
			activatedAbilityEntry.Enabled = !ParentObject.HasEffect<EmptyTheClips>();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME && E.Actor == ParentObject && ParentObject.CanMoveExtremities(null, ShowMessage: true))
		{
			ParentObject.ApplyEffect(new EmptyTheClips(DURATION + 1));
			CooldownMyActivatedAbility(ActivatedAbilityID, COOLDOWN, null, "Agility");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.Actor.HasMissileWeapon(null, IsPistol) && E.Actor.CanMoveExtremities() && 50.in100())
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		string cOMMAND_NAME = COMMAND_NAME;
		string[] array = new string[5] { "Cooldown ", null, null, null, null };
		int cOOLDOWN = COOLDOWN;
		array[1] = cOOLDOWN.ToString();
		array[2] = ". For ";
		array[3] = DURATION.Things("round");
		array[4] = ", the action cost of firing pistols is reduced from 1000 to 500.";
		ActivatedAbilityID = AddMyActivatedAbility("Empty the Clips", cOMMAND_NAME, "Skills", string.Concat(array), "®");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}

	public static bool IsPistol(MissileWeapon MW)
	{
		return MW?.Skill == "Pistol";
	}

	public static bool IsPistol(GameObject Object)
	{
		return IsPistol(Object.GetPart<MissileWeapon>());
	}
}
