using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Survival_Trailblazer : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetLostChanceEvent.ID && ID != TravelSpeedEvent.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("travel", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetLostChanceEvent E)
	{
		E.PercentageBonus += 17;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TravelSpeedEvent E)
	{
		E.PercentageBonus += 100;
		return base.HandleEvent(E);
	}
}
