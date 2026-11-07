using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Endurance_Weathered : BaseSkill
{
	public override bool AddSkill(GameObject GO)
	{
		StatShifter.SetStatShift("HeatResistance", 15);
		StatShifter.SetStatShift("ColdResistance", 15);
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		StatShifter.RemoveStatShifts();
		return base.RemoveSkill(GO);
	}
}
