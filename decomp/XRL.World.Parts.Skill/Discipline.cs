using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Discipline : BaseSkill
{
	public override int Priority => int.MinValue;
}
