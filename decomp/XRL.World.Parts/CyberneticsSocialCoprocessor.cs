using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsSocialCoprocessor : IPoweredPart
{
	public static readonly int BASE_WATER_RITUAL_REPUTATION_BONUS = RuleSettings.REPUTATION_BASE_UNIT / 2;

	public static readonly int BASE_REP_COST_REDUCTION_PERCENTAGE = 20;

	public static readonly int BASE_PROSELYTIZE_LIMIT_BONUS = 1;

	public CyberneticsSocialCoprocessor()
	{
		WorksOnImplantee = true;
		ChargeUse = 0;
		NameForStatus = "SocialCoprocessor";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetCompanionLimitEvent>.ID && ID != PooledEvent<GetCyberneticsBehaviorDescriptionEvent>.ID && ID != PooledEvent<GetSocialSifrahSetupEvent>.ID && ID != PooledEvent<GetWaterRitualReputationAmountEvent>.ID && ID != PooledEvent<ReputationChangeEvent>.ID)
		{
			return ID == PooledEvent<WaterRitualStartEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetCyberneticsBehaviorDescriptionEvent E)
	{
		if (ParentObject.Implantee != null)
		{
			int waterRitualReputationBonus = GetWaterRitualReputationBonus();
			int proselytizeLimitBonus = GetProselytizeLimitBonus();
			E.Description = "Whenever you perform the water ritual with a new creature, you gain an extra " + waterRitualReputationBonus + " reputation. If you install this implant after you treat with a creature for the first time, you gain " + waterRitualReputationBonus + " reputation the next time you treat with them.\nReputation costs in the water ritual are reduced by " + GetWaterRitualReputationCostReductionPercentage() + "%.\nYou may Proselytize " + proselytizeLimitBonus.Things("additional creature") + ".\nCompute power on the local lattice increases this implant's effectiveness.";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(WaterRitualStartEvent E)
	{
		if (IsObjectActivePartSubject(E.Actor) && E.Actor.IsPlayer())
		{
			int num = (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) ? GetWaterRitualReputationBonus() : 0);
			int num2 = num - E.Record.LastSocialCoprocessorBonus;
			if (num2 > 0)
			{
				E.Record.LastSocialCoprocessorBonus = num;
				The.Game.PlayerReputation.Modify(E.Record.faction, num2, "WaterRitualSocialCoprocessorAward");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetWaterRitualReputationAmountEvent E)
	{
		if (IsObjectActivePartSubject(E.Actor) && E.Actor.IsPlayer() && E.Faction == E.Record.faction)
		{
			int num = (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) ? GetWaterRitualReputationBonus() : 0);
			int num2 = num - E.Record.LastSocialCoprocessorBonus;
			if (num2 > 0)
			{
				E.Record.LastSocialCoprocessorBonus = num;
				E.Amount += num2;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReputationChangeEvent E)
	{
		if (E.BaseAmount < 0 && !E.Transient && E.Type == "WaterRitualUse" && IsReady(!E.Prospective, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Amount = E.Amount * (100 - GetWaterRitualReputationCostReductionPercentage()) / 100;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCompanionLimitEvent E)
	{
		if (E.Means == "Proselytize" && E.Actor == ParentObject.Implantee && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Limit += GetProselytizeLimitBonus();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetSocialSifrahSetupEvent E)
	{
		E.Rating += 5;
		E.Turns++;
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public int GetWaterRitualReputationBonus()
	{
		return GetAvailableComputePowerEvent.AdjustUp(ParentObject.Implantee, BASE_WATER_RITUAL_REPUTATION_BONUS);
	}

	public int GetWaterRitualReputationCostReductionPercentage()
	{
		return GetAvailableComputePowerEvent.AdjustUp(ParentObject.Implantee, BASE_REP_COST_REDUCTION_PERCENTAGE);
	}

	public int GetProselytizeLimitBonus()
	{
		return GetAvailableComputePowerEvent.AdjustUp(ParentObject.Implantee, BASE_PROSELYTIZE_LIMIT_BONUS);
	}
}
