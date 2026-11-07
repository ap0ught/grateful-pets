using System;
using System.Text;
using XRL.World.Capabilities;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class Capacitor : IRechargeable
{
	public int Charge;

	public int ChargeRate = 5;

	public int MaxCharge = 10000;

	public int MinimumChargeToExplode = 1000;

	public string Description = "capacitor";

	public string ChargeDisplayStyle = "electrical";

	public string AltChargeDisplayStyle = "percentage";

	public string AltChargeDisplayProperty = Scanning.GetScanPropertyName(Scanning.Scan.Tech);

	public string StartCharge;

	public bool CatastrophicDisable;

	public bool ChargeLossDisable = true;

	public bool IsRechargeable = true;

	public bool ConsiderLive = true;

	public Capacitor()
	{
		ChargeUse = 0;
		IsBootSensitive = false;
	}

	public bool HasCharge(int Amount)
	{
		return Charge >= Amount;
	}

	public int GetCharge()
	{
		return Charge;
	}

	public void UseCharge(int Amount)
	{
		Charge -= Amount;
		if (Charge < 0)
		{
			Charge = 0;
		}
		else if (Charge > MaxCharge)
		{
			Charge = MaxCharge;
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
	}

	public override bool CanBeRecharged()
	{
		if (IsRechargeable)
		{
			return IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: true, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: true, IgnoreSubject: true, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		}
		return false;
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

	public string ChargeStatus()
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

	public override int GetRechargeAmount()
	{
		return MaxCharge - Charge;
	}

	public override bool SameAs(IPart p)
	{
		Capacitor capacitor = p as Capacitor;
		if (capacitor.Charge != Charge)
		{
			return false;
		}
		if (capacitor.ChargeRate != ChargeRate)
		{
			return false;
		}
		if (capacitor.MaxCharge != MaxCharge)
		{
			return false;
		}
		if (capacitor.MinimumChargeToExplode != MinimumChargeToExplode)
		{
			return false;
		}
		if (capacitor.Description != Description)
		{
			return false;
		}
		if (capacitor.ChargeDisplayStyle != ChargeDisplayStyle)
		{
			return false;
		}
		if (capacitor.AltChargeDisplayStyle != AltChargeDisplayStyle)
		{
			return false;
		}
		if (capacitor.AltChargeDisplayProperty != AltChargeDisplayProperty)
		{
			return false;
		}
		if (capacitor.StartCharge != StartCharge)
		{
			return false;
		}
		if (capacitor.CatastrophicDisable != CatastrophicDisable)
		{
			return false;
		}
		if (capacitor.ChargeLossDisable != ChargeLossDisable)
		{
			return false;
		}
		if (capacitor.IsRechargeable != IsRechargeable)
		{
			return false;
		}
		if (capacitor.ConsiderLive != ConsiderLive)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDeathRemovalEvent.ID && ID != ChargeAvailableEvent.ID && ID != EffectAppliedEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && (ID != PooledEvent<GetElectricalConductivityEvent>.ID || !ConsiderLive) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && (ID != ObjectCreatedEvent.ID || string.IsNullOrEmpty(StartCharge)) && ID != QueryChargeEvent.ID && ID != QueryChargeStorageEvent.ID && ID != QueryRechargeStorageEvent.ID && ID != RechargeAvailableEvent.ID && ID != TestChargeEvent.ID)
		{
			return ID == UseChargeEvent.ID;
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

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if ((CatastrophicDisable || ChargeLossDisable) && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (CatastrophicDisable && MinimumChargeToExplode > 0 && Charge >= MinimumChargeToExplode)
			{
				ParentObject.Die(null, "catastrophic " + Description + " failure", "Your " + Description + " failed catastrophically.", ParentObject.Its + " " + Description + " failed catastrophically.", Accidental: true);
			}
			if (ChargeLossDisable)
			{
				Charge = 0;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Reference && E.Understood())
		{
			string text = ChargeStatus();
			if (text != null)
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("{{y|(").Append(Description).Append(": ")
					.Append(text)
					.Append(")}}");
				E.AddTag(stringBuilder.ToString());
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (CanBeRecharged() && IComponent<GameObject>.ThePlayer.HasSkill("Tinkering_Tinker1") && Charge < MaxCharge && ParentObject.Understood() && The.Player.CanMoveExtremities("Recharge"))
		{
			E.AddAction("Recharge", "recharge", "RechargeCapacitor", null, 'R', FireOnActor: false, 100 - Charge * 100 / MaxCharge);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "RechargeCapacitor" && CanBeRecharged())
		{
			E.Actor.GetPart<Tinkering_Tinker1>().Recharge(ParentObject, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (!StartCharge.IsNullOrEmpty())
		{
			Charge = StartCharge.RollCached();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ChargeAvailableEvent E)
	{
		Process(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RechargeAvailableEvent E)
	{
		Process(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeEvent E)
	{
		if (InteractWithChargeEvent(E))
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
		if (InteractWithChargeEvent(E))
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
		if (E.Pass == 2 && InteractWithChargeEvent(E))
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

	public override bool HandleEvent(QueryChargeStorageEvent E)
	{
		Process(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryRechargeStorageEvent E)
	{
		Process(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (MinimumChargeToExplode > 0 && Charge >= MinimumChargeToExplode && (CatastrophicDisable || IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L)))
		{
			DidX("explode", null, "!", null, null, null, ParentObject);
			ParentObject.Explode(Charge, ParentObject.IsCombat ? ParentObject : E.Killer, null, 1f, Neutron: false, SuppressDestroy: true);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public virtual bool InteractWithChargeEvent(IChargeEvent E)
	{
		if (!E.LiveOnly || ConsiderLive)
		{
			return IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		}
		return false;
	}

	private void Process(IInitialChargeProductionEvent E)
	{
		if (Charge >= MaxCharge || !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) || E.Amount <= 0)
		{
			return;
		}
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

	private void Process(IChargeStorageEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = MaxCharge - GetCharge();
			if (num > 0)
			{
				E.Amount += num;
			}
		}
	}
}
