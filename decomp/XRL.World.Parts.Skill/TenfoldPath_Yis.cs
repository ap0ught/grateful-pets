using System;
using XRL.UI;

namespace XRL.World.Parts.Skill;

/// This part is not used in the base game.
[Serializable]
public class TenfoldPath_Yis : BaseInitiatorySkill
{
	public static readonly int AMOUNT_PER_LEVEL = 30;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetLevelUpSkillPointsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetLevelUpSkillPointsEvent E)
	{
		if (E.Actor == ParentObject)
		{
			E.Amount += AMOUNT_PER_LEVEL;
		}
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject Object)
	{
		int num = AMOUNT_PER_LEVEL * (Object.Stat("Level") - 1);
		if (num > 0)
		{
			Object.GetStat("SP").BaseValue += num;
			if (Object.IsPlayer())
			{
				Popup.Show("You gain " + num + " skill " + ((num == 1) ? "point" : "points") + ".");
			}
		}
		return base.AddSkill(Object);
	}
}
