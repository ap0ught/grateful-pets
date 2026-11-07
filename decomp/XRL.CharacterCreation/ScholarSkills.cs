using System.Collections.Generic;
using XRL.World;

namespace XRL.CharacterCreation;

public class ScholarSkills : CustomChargenSandbox, ICustomChargenClass
{
	public void BuildCharacterBody(GameObject body)
	{
		List<string> list = new List<string>(2);
		for (int i = 0; i < 2; i++)
		{
			bool flag = false;
			string text = "";
			while (!flag)
			{
				text = new List<string>(7) { "Survival_PlainsSurvival", "Survival_SaltmarshSurvival", "Survival_MountainsSurvival", "Survival_DesertCanyonSurvival", "Survival_JungleSurvival", "Survival_SaltDesertSurvival", "Survival_RuinsSurvival" }.GetRandomElement();
				if (!list.CleanContains(text))
				{
					flag = true;
				}
			}
			list.Add(text);
			AddSkill(body, text);
		}
	}

	public IEnumerable<string> GetChargenInfo()
	{
		yield return "Wilderness Lore: Random";
		yield return "Wilderness Lore: Random";
	}
}
