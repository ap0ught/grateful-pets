using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Rifle_FlatteningFire : BaseSkill
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
		int num = 0;
		List<Cell> list = new List<Cell>();
		GO.Physics.CurrentCell.GetAdjacentCells(1, list, LocalOnly: false);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].IsSolid())
			{
				num++;
			}
		}
		if (num >= 6)
		{
			return true;
		}
		return false;
	}
}
