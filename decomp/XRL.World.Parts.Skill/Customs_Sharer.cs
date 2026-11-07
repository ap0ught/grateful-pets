using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Customs_Sharer : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetNamingChanceEvent.ID && ID != GetNamingBestowalChanceEvent.ID && ID != PooledEvent<GetRitualSifrahSetupEvent>.ID)
		{
			return ID == PooledEvent<GetWaterRitualSellSecretBehaviorEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNamingChanceEvent E)
	{
		E.PercentageBonus += 50.0;
		E.LinearBonus++;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNamingBestowalChanceEvent E)
	{
		E.PercentageBonus += 100;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetRitualSifrahSetupEvent E)
	{
		E.Turns++;
		E.Rating += 4;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetWaterRitualSellSecretBehaviorEvent E)
	{
		E.BonusReputationProvided += E.ReputationProvided / 2;
		if (E.IsGossip)
		{
			E.Message = "I sing to you of your people, =subject.waterRitualLiquid=-=pronouns.siblingTerm=.";
		}
		else
		{
			E.Message = "I sing to you of secrets, =subject.waterRitualLiquid=-=pronouns.siblingTerm=.";
		}
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		if (GO.IsPlayer())
		{
			RitualSifrah.AwardInsight();
		}
		return base.AddSkill(GO);
	}
}
