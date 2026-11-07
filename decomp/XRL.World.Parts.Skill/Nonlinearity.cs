using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Nonlinearity : BaseSkill
{
	public override int Priority => int.MinValue;
}
