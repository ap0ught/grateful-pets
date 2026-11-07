using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Rifle_DisorientingFire : BaseSkill
{
	public static bool MeetsCriteria(GameObject GO)
	{
		if (!GameObject.Validate(ref GO))
		{
			return false;
		}
		if (!GO.IsCombat)
		{
			return false;
		}
		if (GO.Brain != null)
		{
			Cell cell = GO.CurrentCell;
			List<GameObject> list = cell.ParentZone.FastFloodVisibility(cell.X, cell.Y, 80, "Brain", GO);
			for (int i = 0; i < list.Count; i++)
			{
				GameObject gameObject = list[i];
				if (gameObject == GO)
				{
					continue;
				}
				Brain brain = gameObject.Brain;
				foreach (KeyValuePair<string, int> item in GO.Brain.Allegiance)
				{
					item.Deconstruct(out var key, out var value);
					string text = key;
					int num = value;
					foreach (KeyValuePair<string, int> item2 in brain.Allegiance)
					{
						item2.Deconstruct(out key, out value);
						string text2 = key;
						int num2 = value;
						if (text == text2 && num > 0 && num2 > 0)
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}
}
