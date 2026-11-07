using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tactics_Throwing : BaseSkill
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public override bool AddSkill(GameObject GO)
	{
		GO.ModIntProperty("CloseThrowRangeAccuracySkillBonus", 50);
		GO.ModIntProperty("ThrowRangeSkillBonus", 3);
		GO.ModIntProperty("ThrowToHitSkillBonus", 2);
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		GO.ModIntProperty("CloseThrowRangeAccuracySkillBonus", -50, RemoveIfZero: true);
		GO.ModIntProperty("ThrowRangeSkillBonus", -3, RemoveIfZero: true);
		GO.ModIntProperty("ThrowToHitSkillBonus", -2, RemoveIfZero: true);
		return base.AddSkill(GO);
	}
}
