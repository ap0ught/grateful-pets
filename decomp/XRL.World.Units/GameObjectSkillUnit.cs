using System;
using System.Collections.Generic;
using XRL.World.Parts;
using XRL.World.Parts.Skill;
using XRL.World.Skills;

namespace XRL.World.Units;

[Serializable]
public class GameObjectSkillUnit : GameObjectUnit
{
	public string Skill;

	public string Power;

	public override void Apply(GameObject Object)
	{
		XRL.World.Parts.Skills skills = Object.RequirePart<XRL.World.Parts.Skills>();
		SkillEntry value = null;
		if (!Skill.IsNullOrEmpty() && SkillFactory.Factory.SkillByClass.TryGetValue(Skill, out value))
		{
			skills.AddSkill(Skill);
		}
		if (Power.IsNullOrEmpty())
		{
			return;
		}
		if (value?.Powers != null && (Power == "*" || Power == "All"))
		{
			foreach (KeyValuePair<string, PowerEntry> power in value.Powers)
			{
				skills.AddSkill(power.Value.Class);
			}
			return;
		}
		if (SkillFactory.Factory.PowersByClass.ContainsKey(Power))
		{
			skills.AddSkill(Power);
		}
	}

	public override void Remove(GameObject Object)
	{
		XRL.World.Parts.Skills skills = Object.RequirePart<XRL.World.Parts.Skills>();
		SkillEntry value = null;
		if (!Skill.IsNullOrEmpty() && SkillFactory.Factory.SkillByClass.TryGetValue(Skill, out value))
		{
			skills.RemoveSkill(Object.GetPart(Skill) as BaseSkill);
		}
		if (Power.IsNullOrEmpty())
		{
			return;
		}
		if (value?.Powers != null && (Power == "*" || Power == "All"))
		{
			foreach (KeyValuePair<string, PowerEntry> power in value.Powers)
			{
				skills.RemoveSkill(Object.GetPart(power.Key) as BaseSkill);
			}
			return;
		}
		if (SkillFactory.Factory.PowersByClass.ContainsKey(Power))
		{
			skills.RemoveSkill(Object.GetPart(Power) as BaseSkill);
		}
	}

	public override void Reset()
	{
		base.Reset();
		Skill = null;
		Power = null;
	}

	public override string GetDescription(bool Inscription = false)
	{
		if (!Skill.IsNullOrEmpty() && SkillFactory.Factory.SkillByClass.TryGetValue(Skill, out var value))
		{
			if (value.Powers != null && (Power == "*" || Power == "All"))
			{
				return "Has every " + value.Name + " skill";
			}
			return "Has the " + value.Name + " skill";
		}
		if (!Power.IsNullOrEmpty() && SkillFactory.Factory.PowersByClass.TryGetValue(Power, out var value2))
		{
			return "Has the " + value2.Name + " skill";
		}
		return "Has the " + (Skill.IsNullOrEmpty() ? Power : Skill) + " skill";
	}
}
