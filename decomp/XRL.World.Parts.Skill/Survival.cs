using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Survival : BaseSkill
{
	public override int Priority => int.MinValue;
}
