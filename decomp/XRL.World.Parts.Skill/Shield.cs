using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Shield : BaseSkill
{
	public override int Priority => int.MinValue;
}
