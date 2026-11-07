using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Physic : BaseSkill
{
	public override int Priority => int.MinValue;
}
