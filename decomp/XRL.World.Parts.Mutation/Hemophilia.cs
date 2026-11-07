using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

/// This class is not used in the base game.
[Serializable]
public class Hemophilia : BaseMutation
{
	public static readonly int DEFAULT_BANDAGE_PERFORMANCE_DIVISOR = 3;

	public override bool CanLevel()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetBandagePerformanceEvent>.ID)
		{
			return ID == ModifyDefendingSaveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetBandagePerformanceEvent E)
	{
		if (E.Pass == GetBandagePerformanceEvent.PASSES && E.Subject == ParentObject && E.Checking == "Subject")
		{
			int performance = E.Performance;
			E.Performance = performance / GlobalConfig.GetIntSetting("HemophiliaBandagePerformanceDivisor", DEFAULT_BANDAGE_PERFORMANCE_DIVISOR);
			if (performance > 0 && E.Performance == 0)
			{
				E.Performance = 1;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (SavingThrows.Applicable("Bleeding", E.Vs))
		{
			E.Roll -= E.NaturalRoll / 2;
			E.IgnoreNatural20 = true;
		}
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "Your blood does not clot easily.\n\nIt takes much longer than usual for you to stop bleeding.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		return base.Unmutate(GO);
	}
}
