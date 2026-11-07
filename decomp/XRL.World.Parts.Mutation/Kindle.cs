using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Kindle : BaseMutation
{
	public Kindle()
	{
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AIGetOffensiveAbilityListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 12 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandKindle");
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandKindle");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return string.Concat(string.Concat("" + "You ignite a small fire with your mind.\n\n", "Range: 12\n"), "Cooldown: 50");
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandKindle")
		{
			Cell cell = PickDestinationCell(12, AllowVis.Any, Locked: false, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Kindle");
			if (cell == null)
			{
				return false;
			}
			if (ParentObject.DistanceTo(cell) > 12)
			{
				return ParentObject.ShowFailure("That is out of range (12 squares)");
			}
			GameObject gameObject = GameObject.Create("Kindleflame");
			gameObject.RequirePart<TorchProperties>().LastThrower = ParentObject;
			gameObject.RequirePart<Temporary>().Duration = Stat.Random(50, 75);
			cell.AddObject(gameObject);
			UseEnergy(1000, "Mental Mutation Kindle");
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
			IComponent<GameObject>.XDidY(gameObject, "appear", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: true, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
			ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_pyrokinesis_active");
		}
		return base.FireEvent(E);
	}

	public int GetCooldown(int Level)
	{
		return 50;
	}

	public int GetRange(int Level)
	{
		return 12;
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Range", GetRange(Level));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Kindle", "CommandKindle", "Mental Mutations", null, "\u00a8");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
