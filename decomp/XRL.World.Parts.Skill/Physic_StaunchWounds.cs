using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Physic_StaunchWounds : BaseSkill
{
	public static readonly int DEFAULT_BANDAGE_PERFORMANCE_BONUS = 1;

	public static readonly int DEFAULT_BANDAGE_PERFORMANCE_FACTOR = 2;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetBandagePerformanceEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("scholarship", 3);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetBandagePerformanceEvent E)
	{
		if (E.Pass == 1 && E.Actor == ParentObject && E.Checking == "Actor")
		{
			E.Performance += GlobalConfig.GetIntSetting("StaunchWoundsBandagePerformanceBonus", DEFAULT_BANDAGE_PERFORMANCE_BONUS);
		}
		else if (E.Pass == 2 && E.Actor == ParentObject && E.Checking == "Actor")
		{
			E.Performance *= GlobalConfig.GetIntSetting("StaunchWoundsBandagePerformanceFactor", DEFAULT_BANDAGE_PERFORMANCE_FACTOR);
		}
		return base.HandleEvent(E);
	}
}
