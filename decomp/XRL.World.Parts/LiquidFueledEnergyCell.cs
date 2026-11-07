using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class LiquidFueledEnergyCell : IEnergyCell
{
	public string Liquid = "water";

	public int ChargePerDram = 10000;

	public int ChargeCounter;

	public bool ConsiderLive;

	public LiquidVolume EnergyLiquid()
	{
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume == null)
		{
			return null;
		}
		if (liquidVolume.Volume <= 0)
		{
			return null;
		}
		if (!liquidVolume.IsPureLiquid(Liquid))
		{
			return null;
		}
		return liquidVolume;
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (EnergyLiquid() == null)
		{
			return true;
		}
		return base.GetActivePartLocallyDefinedFailure();
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		if (EnergyLiquid() == null)
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume == null || liquidVolume.Volume <= 0)
			{
				return "ProcessInputMissing";
			}
			if (liquidVolume.ContainsLiquid(Liquid) && !liquidVolume.IsPureLiquid())
			{
				return "ProcessInputInvalid";
			}
			return "ProcessInputMissing";
		}
		return base.GetActivePartLocallyDefinedFailureDescription();
	}

	public override bool HasAnyCharge()
	{
		return EnergyLiquid() != null;
	}

	public override bool HasCharge(int Amount)
	{
		return GetCharge() >= Amount;
	}

	public override int GetCharge()
	{
		LiquidVolume liquidVolume = EnergyLiquid();
		if (liquidVolume == null)
		{
			return 0;
		}
		return liquidVolume.Volume * ChargePerDram - ChargeCounter;
	}

	public override int GetChargePercentage()
	{
		LiquidVolume liquidVolume = EnergyLiquid();
		if (liquidVolume == null)
		{
			return 0;
		}
		return (liquidVolume.Volume * ChargePerDram - ChargeCounter) * 100 / (liquidVolume.MaxVolume * ChargePerDram);
	}

	public override void UseCharge(int Amount)
	{
		LiquidVolume liquidVolume = EnergyLiquid();
		if (liquidVolume == null)
		{
			return;
		}
		bool flag = liquidVolume.Volume > 0;
		ChargeCounter += Amount;
		while (ChargeCounter >= ChargePerDram)
		{
			liquidVolume.UseDram();
			if (liquidVolume.Volume == 0)
			{
				ChargeCounter = 0;
				break;
			}
			ChargeCounter -= ChargePerDram;
		}
		if (flag && liquidVolume.Volume == 0)
		{
			CellDepletedEvent.Send(ParentObject, this);
		}
	}

	public override void AddCharge(int Amount)
	{
		if (Amount < 0)
		{
			UseCharge(-Amount);
		}
	}

	public override string ChargeStatus()
	{
		LiquidVolume liquidVolume = EnergyLiquid();
		if (liquidVolume == null)
		{
			return "&K-";
		}
		string primaryLiquidColor = liquidVolume.GetPrimaryLiquidColor();
		if (primaryLiquidColor != null)
		{
			return "&" + primaryLiquidColor + liquidVolume.Volume + "&y";
		}
		return liquidVolume.Volume.ToString();
	}

	public override void TinkerInitialize()
	{
	}

	public override void SetChargePercentage(int Percentage)
	{
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		liquidVolume.Empty();
		if (liquidVolume.MaxVolume > 0)
		{
			int num = Math.Max(0, Math.Min(liquidVolume.MaxVolume * Percentage / 100, liquidVolume.MaxVolume));
			if (num > 0)
			{
				liquidVolume.Fill(Liquid, num);
			}
		}
	}

	public override void RandomizeCharge()
	{
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		liquidVolume.Empty();
		if (liquidVolume.MaxVolume > 0)
		{
			int num = Stat.Random(0, liquidVolume.MaxVolume);
			if (num > 0)
			{
				liquidVolume.Fill(Liquid, num);
			}
		}
	}

	public override void MaximizeCharge()
	{
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		liquidVolume.Empty();
		if (liquidVolume.MaxVolume > 0)
		{
			liquidVolume.Fill(Liquid, liquidVolume.MaxVolume);
		}
	}

	public override bool CanBeRecharged()
	{
		return false;
	}

	public override int GetRechargeAmount()
	{
		return 0;
	}

	public override bool SameAs(IPart p)
	{
		LiquidFueledEnergyCell liquidFueledEnergyCell = p as LiquidFueledEnergyCell;
		if (liquidFueledEnergyCell.Liquid != Liquid)
		{
			return false;
		}
		if (liquidFueledEnergyCell.ChargePerDram != ChargePerDram)
		{
			return false;
		}
		if (liquidFueledEnergyCell.ChargeCounter != ChargeCounter)
		{
			return false;
		}
		if (liquidFueledEnergyCell.ConsiderLive != ConsiderLive)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AllowLiquidCollectionEvent.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && (ID != PooledEvent<GetElectricalConductivityEvent>.ID || !ConsiderLive) && ID != GetPreferredLiquidEvent.ID && ID != QueryChargeEvent.ID && ID != RemoveFromContextEvent.ID && ID != TestChargeEvent.ID && ID != TryRemoveFromContextEvent.ID && ID != UseChargeEvent.ID && ID != WantsLiquidCollectionEvent.ID)
		{
			if (Options.AutogetAmmo)
			{
				return ID == AutoexploreObjectEvent.ID;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetElectricalConductivityEvent E)
	{
		if (E.Pass == 1 && E.Object == ParentObject && ConsiderLive)
		{
			E.MinValue(95);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (E.Command == null && Options.AutogetAmmo)
		{
			E.Command = "Autoget";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "ChargePerDram", ChargePerDram);
		E.AddEntry(this, "ChargeCounter", ChargeCounter);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AllowLiquidCollectionEvent E)
	{
		if (!IsLiquidCollectionCompatible(E.Liquid))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPreferredLiquidEvent E)
	{
		if (E.Liquid == null)
		{
			E.Liquid = Liquid;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(WantsLiquidCollectionEvent E)
	{
		if (IsLiquidCollectionCompatible(E.Liquid))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeEvent E)
	{
		if ((!E.LiveOnly || ConsiderLive) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = ChargeUse * E.Multiple;
			int num2 = GetCharge() - num;
			if (num2 > 0)
			{
				E.Amount += num2;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TestChargeEvent E)
	{
		if ((!E.LiveOnly || ConsiderLive) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = ChargeUse * E.Multiple;
			int num2 = GetCharge() - num;
			if (num2 > 0)
			{
				E.Amount -= num2;
				if (E.Amount <= 0)
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UseChargeEvent E)
	{
		if (E.Pass == 2 && (!E.LiveOnly || ConsiderLive) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = ChargeUse * E.Multiple;
			int num2 = Math.Min(E.Amount, GetCharge() - num);
			if (num2 > 0)
			{
				UseCharge(num2 + num);
				E.Amount -= num2;
				if (E.Amount <= 0)
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public bool IsLiquidCollectionCompatible(string LiquidType)
	{
		return Liquid == LiquidType;
	}
}
