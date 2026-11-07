using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class ThermalAmp : IPoweredPart
{
	public int HeatDamage;

	public int ColdDamage;

	public int ModifyHeat;

	public int ModifyCold;

	public ThermalAmp()
	{
		ChargeUse = 0;
		WorksOnWearer = true;
		IsPowerLoadSensitive = false;
		NameForStatus = "ThermalAmp";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID && (!WorksOnWearer || ID != EquippedEvent.ID) && (!WorksOnWearer || ID != UnequippedEvent.ID))
		{
			if (WorksOnSelf)
			{
				return ID == BeforeApplyDamageEvent.ID;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		int powerLoad = MyPowerLoadLevel();
		string statusSummary = GetStatusSummary();
		AppendRule(E.Postfix, GetPercentage(HeatDamage, powerLoad), "heat damage dealt", "R", "r", statusSummary);
		AppendRule(E.Postfix, GetPercentage(ColdDamage, powerLoad), "cold damage dealt", "C", "c", statusSummary);
		AppendRule(E.Postfix, GetPercentage(ModifyHeat, powerLoad), "to the intensity of your heating effects", "R", "r", statusSummary);
		AppendRule(E.Postfix, GetPercentage(ModifyCold, powerLoad), "to the intensity of your cooling effects", "C", "c", statusSummary);
		return base.HandleEvent(E);
	}

	public void AppendRule(StringBuilder SB, int Value, string Effect, string Positive = "rules", string Negative = "rules", string Status = null)
	{
		if (Value != 0)
		{
			SB.Compound("{{", '\n').Append((Value > 0) ? Positive : Negative).Append('|')
				.AppendSigned(Value)
				.Append('%')
				.Compound(Effect)
				.Append("}}");
			AddStatusSummary(SB, Status);
		}
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (WorksOnWearer)
		{
			E.Actor.RegisterPartEvent(this, "AttackerDealingDamage");
			E.Actor.RegisterPartEvent(this, "AttackerBeforeTemperatureChange");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		if (WorksOnWearer)
		{
			E.Actor.UnregisterPartEvent(this, "AttackerDealingDamage");
			E.Actor.UnregisterPartEvent(this, "AttackerBeforeTemperatureChange");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (WorksOnSelf)
		{
			AmplifyDamage(E.Damage);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		if (WorksOnSelf)
		{
			Registrar.Register("AttackerDealingDamage");
			Registrar.Register("AttackerBeforeTemperatureChange");
		}
	}

	public void AmplifyDamage(Damage Damage)
	{
		int num = MyPowerLoadLevel();
		int? powerLoadLevel = num;
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			if (HeatDamage != 0 && Damage.IsHeatDamage())
			{
				Damage.Amount *= 100 + GetPercentage(HeatDamage, num);
				Damage.Amount /= 100;
			}
			else if (ColdDamage != 0 && Damage.IsColdDamage())
			{
				Damage.Amount *= 100 + GetPercentage(ColdDamage, num);
				Damage.Amount /= 100;
			}
		}
	}

	public void AmplifyTemperature(ref int Amount)
	{
		int num = MyPowerLoadLevel();
		int? powerLoadLevel = num;
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			if (Amount > 0 && ModifyHeat != 0)
			{
				Amount *= 100 + GetPercentage(ModifyHeat, num);
				Amount /= 100;
			}
			else if (Amount < 0 && ModifyCold != 0)
			{
				Amount *= 100 + GetPercentage(ModifyCold, num);
				Amount /= 100;
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerDealingDamage" && E.GetParameter("Damage") is Damage damage)
		{
			AmplifyDamage(damage);
		}
		else if (E.ID == "AttackerBeforeTemperatureChange")
		{
			int Amount = E.GetIntParameter("Amount");
			AmplifyTemperature(ref Amount);
			E.SetParameter("Amount", Amount);
		}
		return base.FireEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (ChargeUse > 0 && !OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
		}
	}

	public int GetPercentage(int Value, int PowerLoad)
	{
		int num = MyPowerLoadBonus(PowerLoad, 100, 10);
		if (num == 0)
		{
			return Value;
		}
		return Value * (100 + num) / 100;
	}
}
