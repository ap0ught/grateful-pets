using System;
using System.Text;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class EnergyCell : IEnergyCell
{
	public int Charge = 100;

	public int MaxCharge = 100;

	public int ChargeRate = 10;

	public int RechargeValue = 3000;

	public char RechargeBit = 'R';

	public string StartCharge;

	public string ChargeDisplayStyle = "electrical";

	public string AltChargeDisplayStyle = "percentage";

	public string AltChargeDisplayProperty = Scanning.GetScanPropertyName(Scanning.Scan.Tech);

	public bool ConsiderLive = true;

	public bool IsRechargeable = true;

	public override bool HasAnyCharge()
	{
		return Charge > 0;
	}

	public override bool HasCharge(int Amount)
	{
		return Charge >= Amount;
	}

	public override int GetCharge()
	{
		return Charge;
	}

	public override int GetChargePercentage()
	{
		if (MaxCharge == 0)
		{
			return 0;
		}
		return Charge * 100 / MaxCharge;
	}

	public override void UseCharge(int Amount)
	{
		bool flag = Charge > 0;
		Charge -= Amount;
		if (Charge < 0)
		{
			Charge = 0;
		}
		else if (Charge > MaxCharge)
		{
			Charge = MaxCharge;
		}
		if (flag && Charge == 0)
		{
			CellDepletedEvent.Send(ParentObject, this);
		}
	}

	public override void AddCharge(int Amount)
	{
		Charge += Amount;
		if (Charge > MaxCharge)
		{
			Charge = MaxCharge;
		}
		else if (Charge < 0)
		{
			Charge = 0;
		}
		if (ParentObject.Physics.InInventory != null || ParentObject.Physics.CurrentCell != null)
		{
			ParentObject.CheckStack();
		}
	}

	public override void TinkerInitialize()
	{
		Charge = 0;
	}

	public override void SetChargePercentage(int Percentage)
	{
		Charge = Math.Max(0, Math.Min(MaxCharge * Percentage / 100, MaxCharge));
	}

	public override void RandomizeCharge()
	{
		Charge = Stat.Random(1, MaxCharge);
	}

	public override void MaximizeCharge()
	{
		Charge = MaxCharge;
	}

	public int GetChargeLevel()
	{
		return EnergyStorage.GetChargeLevel(Charge, MaxCharge);
	}

	public bool UseAltChargeDisplayStyle()
	{
		if (The.Player == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(AltChargeDisplayProperty))
		{
			return false;
		}
		if (The.Player.GetIntProperty(AltChargeDisplayProperty) <= 0)
		{
			return false;
		}
		return true;
	}

	public override string ChargeStatus()
	{
		string text = (UseAltChargeDisplayStyle() ? AltChargeDisplayStyle : ChargeDisplayStyle);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return null;
		}
		return EnergyStorage.GetChargeStatus(Charge, MaxCharge, text);
	}

	public override bool CanBeRecharged()
	{
		if (IsRechargeable)
		{
			return IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: true, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: true, IgnoreSubject: true, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		}
		return false;
	}

	public override int GetRechargeAmount()
	{
		return MaxCharge - Charge;
	}

	public override int GetRechargeValue()
	{
		return RechargeValue;
	}

	public override char GetRechargeBit()
	{
		return RechargeBit;
	}

	public override bool SameAs(IPart p)
	{
		EnergyCell energyCell = p as EnergyCell;
		if (energyCell.Charge != Charge)
		{
			return false;
		}
		if (energyCell.MaxCharge != MaxCharge)
		{
			return false;
		}
		if (energyCell.ChargeRate != ChargeRate)
		{
			return false;
		}
		if (energyCell.RechargeValue != RechargeValue)
		{
			return false;
		}
		if (energyCell.RechargeBit != RechargeBit)
		{
			return false;
		}
		if (energyCell.StartCharge != StartCharge)
		{
			return false;
		}
		if (energyCell.ChargeDisplayStyle != ChargeDisplayStyle)
		{
			return false;
		}
		if (energyCell.AltChargeDisplayStyle != AltChargeDisplayStyle)
		{
			return false;
		}
		if (energyCell.AltChargeDisplayProperty != AltChargeDisplayProperty)
		{
			return false;
		}
		if (energyCell.ConsiderLive != ConsiderLive)
		{
			return false;
		}
		if (energyCell.IsRechargeable != IsRechargeable)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != AfterObjectCreatedEvent.ID || StartCharge.IsNullOrEmpty()) && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && (ID != PooledEvent<GetElectricalConductivityEvent>.ID || !ConsiderLive) && ID != GetInventoryActionsEvent.ID && ID != GetSlottedInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != QueryChargeEvent.ID && ID != QueryRechargeStorageEvent.ID && ID != RechargeAvailableEvent.ID && ID != TestChargeEvent.ID && ID != UseChargeEvent.ID)
		{
			if (Options.AutogetAmmo)
			{
				return ID == AutoexploreObjectEvent.ID;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (!StartCharge.IsNullOrEmpty())
		{
			Charge = Math.Min(Math.Max(StartCharge.RollCached(), 0), MaxCharge);
		}
		return base.HandleEvent(E);
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
		E.AddEntry(this, "Charge", Charge);
		E.AddEntry(this, "MaxCharge", MaxCharge);
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
			int num2 = Math.Min(E.Amount, GetCharge() - num);
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

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Context != "Tinkering" && E.Understood())
		{
			string text = ChargeStatus();
			if (text != null)
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("{{y|(").Append(text).Append(")}}");
				E.AddTag(stringBuilder.ToString());
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (CanBeRecharged() && The.Player.HasSkill("Tinkering_Tinker1") && Charge < MaxCharge && The.Player.CanMoveExtremities("Recharge"))
		{
			E.AddAction("Recharge", "recharge", "RechargeEnergyCell", null, 'R', FireOnActor: false, 100 - Charge * 100 / MaxCharge);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetSlottedInventoryActionsEvent E)
	{
		if (CanBeRecharged() && The.Player.HasSkill("Tinkering_Tinker1") && Charge < MaxCharge && ParentObject.Understood() && The.Player.CanMoveExtremities("Recharge"))
		{
			E.AddAction("RechargeSlotted", "recharge " + ParentObject.BaseDisplayNameStripped, "RechargeEnergyCell", null, 'R', FireOnActor: false, 90 - Charge * 90 / MaxCharge, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: true, ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "RechargeEnergyCell" && CanBeRecharged())
		{
			E.Actor.GetPart<Tinkering_Tinker1>().Recharge(ParentObject, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RechargeAvailableEvent E)
	{
		if (E.Amount > 0 && Charge < MaxCharge && IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = E.Amount;
			if (!E.Forced && ChargeRate >= 0)
			{
				int num2 = ChargeRate * E.Multiple;
				if (num > num2)
				{
					num = num2;
				}
			}
			if (num > MaxCharge - Charge)
			{
				num = MaxCharge - Charge;
			}
			if (num > 0)
			{
				AddCharge(num);
				E.Amount -= num;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryRechargeStorageEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = MaxCharge - GetCharge();
			if (num > 0)
			{
				E.Amount += num;
			}
		}
		return base.HandleEvent(E);
	}
}
