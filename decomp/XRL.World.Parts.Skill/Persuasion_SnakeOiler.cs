using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Persuasion_SnakeOiler : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetTradePerformanceEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("jewels", 3);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTradePerformanceEvent E)
	{
		E.LinearAdjustment += 2.0;
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		if (GO.IsPlayer())
		{
			SocialSifrah.AwardInsight();
		}
		return base.AddSkill(GO);
	}
}
