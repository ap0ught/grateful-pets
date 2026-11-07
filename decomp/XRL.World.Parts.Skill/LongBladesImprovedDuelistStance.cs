using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class LongBladesImprovedDuelistStance : LongBladesSkillBase
{
	public override bool AddSkill(GameObject GO)
	{
		GO.FireEvent("LongBlades.UpdateStance");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		GO.FireEvent("LongBlades.UpdateStance");
		return base.RemoveSkill(GO);
	}
}
