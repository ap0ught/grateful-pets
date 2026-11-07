using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tactics : BaseSkill
{
	public override int Priority => int.MinValue;
}
