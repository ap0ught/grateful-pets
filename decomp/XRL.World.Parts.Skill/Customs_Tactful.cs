using System;
using UnityEngine;
using XRL.Rules;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Customs_Tactful : BaseSkill
{
	public static int GetBonusFor(GameObject Object)
	{
		if (Object != null && Object.TryGetPart<GivesRep>(out var Part))
		{
			return Mathf.RoundToInt((float)Part.repValue / 20f) * 5;
		}
		return RuleSettings.REPUTATION_BASE_UNIT / 2;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetRitualSifrahSetupEvent>.ID && ID != PooledEvent<GetSocialSifrahSetupEvent>.ID && ID != PooledEvent<GetWaterRitualReputationAmountEvent>.ID && ID != PooledEvent<WaterRitualStartEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("scholarship", 3);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(WaterRitualStartEvent E)
	{
		if (E.Actor == ParentObject && !E.Record.Has("usedTactful") && E.Actor.IsPlayer())
		{
			E.Record.attributes.Add("usedTactful");
			The.Game.PlayerReputation.Modify(E.Record.faction, GetBonusFor(E.SpeakingWith), "WaterRitualTactfulAward");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetWaterRitualReputationAmountEvent E)
	{
		if (E.Actor == ParentObject && !E.Record.Has("usedTactful") && E.Actor.IsPlayer() && E.Faction == E.Record.faction)
		{
			E.Record.attributes.Add("usedTactful");
			E.Amount += GetBonusFor(E.SpeakingWith);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetRitualSifrahSetupEvent E)
	{
		if (E.Type == "FormalWaterRitual")
		{
			E.Rating++;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetSocialSifrahSetupEvent E)
	{
		E.Rating++;
		return base.HandleEvent(E);
	}
}
