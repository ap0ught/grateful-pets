using System;
using System.Text;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class TwoHearted : BaseMutation
{
	public override void Initialize()
	{
		base.Initialize();
		Run.SyncAbility(ParentObject);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetSprintDurationEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetSprintDurationEvent E)
	{
		E.PercentageIncrease += GetSprintBonus();
		E.Stats?.AddPercentageBonusModifier("Duration", GetSprintBonus(), GetDisplayName() + " " + GetMutationTerm());
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "You have two hearts.";
	}

	public override string GetLevelText(int Level)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("+{{rules|").Append(2 + (Level - 1) / 2).Append("}} Toughness\n")
			.Append("You can sprint for {{rules|")
			.Append(GetSprintBonus(Level))
			.Append("%}} longer.");
		return stringBuilder.ToString();
	}

	public static int GetSprintBonus(int Level)
	{
		return 20 + Level * 10;
	}

	public int GetSprintBonus()
	{
		return GetSprintBonus(Level);
	}

	public static int GetToughnessBonus(int Level)
	{
		return 2 + (Level - 1) / 2;
	}

	public int GetToughnessBonus()
	{
		return GetToughnessBonus(Level);
	}

	public int GetToughnessBonusAttributableToTemporaryLevels()
	{
		return GetToughnessBonus() - GetToughnessBonus(Level - GetTemporaryLevels());
	}

	public override bool ChangeLevel(int NewLevel)
	{
		int toughnessBonusAttributableToTemporaryLevels = GetToughnessBonusAttributableToTemporaryLevels();
		int amount = GetToughnessBonus(Level) - toughnessBonusAttributableToTemporaryLevels;
		StatShifter.SetStatShift(ParentObject, "Toughness", amount, baseValue: true);
		StatShifter.SetStatShift(ParentObject, "Toughness", toughnessBonusAttributableToTemporaryLevels);
		Run.SyncAbility(ParentObject);
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		StatShifter.RemoveStatShifts(GO);
		return base.Unmutate(GO);
	}
}
