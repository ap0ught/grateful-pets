using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Acrobatics_Tumble : BaseSkill
{
	public override int Priority => int.MinValue;

	public override bool AddSkill(GameObject GO)
	{
		StatShifter.SetStatShift("DV", 1);
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		StatShifter.RemoveStatShifts();
		return base.RemoveSkill(GO);
	}
}
