using System;
using System.Text;
using UnityEngine;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class IrisdualCallow : Effect
{
	public int AVShift;

	public int DVShift;

	public int MAShift;

	public int SpeedShift;

	public int HeatResistanceShift;

	public int ColdResistanceShift;

	public int ElectricResistanceShift;

	public int AcidResistanceShift;

	[NonSerialized]
	private string Details;

	public IrisdualCallow()
	{
		DisplayName = "{{r|callow}}";
	}

	public IrisdualCallow(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 33554560;
	}

	public override string GetDetails()
	{
		if (Details.IsNullOrEmpty())
		{
			StringBuilder sB = Event.NewStringBuilder();
			if (AVShift != 0)
			{
				Statistic.CompoundStatAdjustDescription(sB, "AV", AVShift, '\n');
			}
			if (DVShift != 0)
			{
				Statistic.CompoundStatAdjustDescription(sB, "DV", DVShift, '\n');
			}
			if (MAShift != 0)
			{
				Statistic.CompoundStatAdjustDescription(sB, "MA", MAShift, '\n');
			}
			if (SpeedShift != 0)
			{
				Statistic.CompoundStatAdjustDescription(sB, "Speed", SpeedShift, '\n');
			}
			if (HeatResistanceShift != 0)
			{
				Statistic.CompoundStatAdjustDescription(sB, "HeatResistance", HeatResistanceShift, '\n');
			}
			if (ColdResistanceShift != 0)
			{
				Statistic.CompoundStatAdjustDescription(sB, "ColdResistance", ColdResistanceShift, '\n');
			}
			if (ElectricResistanceShift != 0)
			{
				Statistic.CompoundStatAdjustDescription(sB, "ElectricResistance", ElectricResistanceShift, '\n');
			}
			if (AcidResistanceShift != 0)
			{
				Statistic.CompoundStatAdjustDescription(sB, "AcidResistance", AcidResistanceShift, '\n');
			}
			Details = Event.FinalizeString(sB);
		}
		return Details;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<IrisdualCallow>())
		{
			return false;
		}
		int num = Stats.GetCombatAV(Object) / 2;
		if (num > 0)
		{
			StatShifter.SetStatShift("AV", AVShift = -num);
		}
		num = Stats.GetCombatDV(Object) / 2;
		if (num > 0)
		{
			StatShifter.SetStatShift("DV", DVShift = -num);
		}
		num = Stats.GetCombatMA(Object) / 2;
		if (num > 0)
		{
			StatShifter.SetStatShift("MA", MAShift = -num);
		}
		num = Mathf.RoundToInt((float)Object.GetStatValue("Speed") * 0.25f);
		if (num > 0)
		{
			StatShifter.SetStatShift("Speed", SpeedShift = -num);
		}
		num = Object.GetStatValue("HeatResistance") / 2;
		if (num > 0)
		{
			StatShifter.SetStatShift("HeatResistance", HeatResistanceShift = -num);
		}
		num = Object.GetStatValue("ColdResistance") / 2;
		if (num > 0)
		{
			StatShifter.SetStatShift("ColdResistance", ColdResistanceShift = -num);
		}
		num = Object.GetStatValue("ElectricResistance") / 2;
		if (num > 0)
		{
			StatShifter.SetStatShift("ElectricResistance", ElectricResistanceShift = -num);
		}
		num = Object.GetStatValue("AcidResistance") / 2;
		if (num > 0)
		{
			StatShifter.SetStatShift("AcidResistance", AcidResistanceShift = -num);
		}
		Object.EmitMessage(Object.Poss("rind") + " softens while " + Object.it + Object.GetVerb("recrystallize", PrependSpace: true, PronounAntecedent: true) + "!", null, IComponent<GameObject>.ConsequentialColor(null, Object));
		return true;
	}

	public override void Remove(GameObject Object)
	{
		StatShifter.RemoveStatShifts();
		Object.EmitMessage(Object.Poss("rind") + " recrystallizes and hardens once more.", null, IComponent<GameObject>.ConsequentialColor(Object));
	}
}
