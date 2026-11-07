using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public abstract class BaseTerrainSurvivalSkill : BaseSkill
{
	public abstract string GetTerrainName();

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EncounterChanceEvent.ID && ID != GetLostChanceEvent.ID && ID != TravelSpeedEvent.ID)
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

	public override bool HandleEvent(EncounterChanceEvent E)
	{
		if (E.TravelClass == Name)
		{
			E.PercentageBonus += 100;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetLostChanceEvent E)
	{
		if (E.TravelClass == Name)
		{
			E.PercentageBonus += 95;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TravelSpeedEvent E)
	{
		if (E.TravelClass == Name)
		{
			E.PercentageBonus += 100;
		}
		return base.HandleEvent(E);
	}
}
