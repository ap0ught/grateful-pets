using System;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Skill;

namespace XRL.CharacterCreation;

public class CustomChargenSandbox
{
	public void AddSkill(GameObject body, string Class)
	{
		BaseSkill newSkill = Activator.CreateInstance(ModManager.ResolveType("XRL.World.Parts.Skill." + Class)) as BaseSkill;
		body.GetPart<Skills>().AddSkill(newSkill);
	}
}
