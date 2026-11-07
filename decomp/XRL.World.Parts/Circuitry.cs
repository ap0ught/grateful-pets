using System;
using System.Text;
using XRL.Core;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Circuitry : IPoweredPart
{
	public int MaxCharge;

	public string ChargeDisplayStyle;

	public string AltChargeDisplayStyle;

	public string AltChargeDisplayProperty = Scanning.GetScanPropertyName(Scanning.Scan.Tech);

	public string Description = "circuitry";

	public bool ConsiderLive = true;

	public long ActiveTurn;

	public int Charge;

	public int IncomingCharge;

	public int StartCharge
	{
		set
		{
			Charge = value;
			IncomingCharge = value;
			ActiveTurn = XRLCore.CurrentTurn;
		}
	}

	public Circuitry()
	{
		ChargeUse = 0;
		IsBootSensitive = false;
		WorksOnSelf = true;
	}

	public bool HasCharge(int Amount)
	{
		return Charge + IncomingCharge >= Amount;
	}

	public int GetCharge()
	{
		return Charge + IncomingCharge;
	}

	public void UseCharge(int Amount)
	{
		Charge -= Amount;
		if (Charge < 0)
		{
			IncomingCharge += Charge;
			Charge = 0;
			if (IncomingCharge < 0)
			{
				IncomingCharge = 0;
			}
		}
		else if (MaxCharge > 0 && Charge + IncomingCharge > MaxCharge)
		{
			Charge = MaxCharge - IncomingCharge;
			if (Charge < 0)
			{
				Charge = 0;
			}
			if (IncomingCharge > MaxCharge)
			{
				IncomingCharge = MaxCharge - Charge;
			}
		}
	}

	public void AddCharge(int Amount)
	{
		IncomingCharge += Amount;
		if (MaxCharge > 0 && Charge + IncomingCharge > MaxCharge)
		{
			Charge = MaxCharge - IncomingCharge;
			if (Charge < 0)
			{
				Charge = 0;
			}
			if (IncomingCharge > MaxCharge)
			{
				IncomingCharge = MaxCharge - Charge;
			}
		}
		else if (IncomingCharge < 0)
		{
			IncomingCharge = 0;
		}
	}

	public int GetChargeLevel()
	{
		return EnergyStorage.GetChargeLevel(Charge + IncomingCharge, MaxCharge);
	}

	public bool UseAltChargeDisplayStyle()
	{
		if (IComponent<GameObject>.ThePlayer == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(AltChargeDisplayProperty))
		{
			return false;
		}
		if (IComponent<GameObject>.ThePlayer.GetIntProperty(AltChargeDisplayProperty) <= 0)
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
		return EnergyStorage.GetChargeStatus(Charge + IncomingCharge, MaxCharge, text);
	}

	public void CheckTurn(int Multiple = 1)
	{
		long num = XRLCore.CurrentTurn + (Multiple - 1);
		if (ActiveTurn < num)
		{
			if (ActiveTurn < num - 1)
			{
				Charge = 0;
			}
			else
			{
				Charge = IncomingCharge;
			}
			IncomingCharge = 0;
			ActiveTurn = num;
		}
	}

	public override bool SameAs(IPart p)
	{
		Circuitry circuitry = p as Circuitry;
		if (circuitry.MaxCharge != MaxCharge)
		{
			return false;
		}
		if (circuitry.ChargeDisplayStyle != ChargeDisplayStyle)
		{
			return false;
		}
		if (circuitry.AltChargeDisplayStyle != AltChargeDisplayStyle)
		{
			return false;
		}
		if (circuitry.AltChargeDisplayProperty != AltChargeDisplayProperty)
		{
			return false;
		}
		if (circuitry.Description != Description)
		{
			return false;
		}
		if (circuitry.ConsiderLive != ConsiderLive)
		{
			return false;
		}
		if (circuitry.ActiveTurn != ActiveTurn)
		{
			return false;
		}
		if (circuitry.Charge != Charge)
		{
			return false;
		}
		if (circuitry.IncomingCharge != IncomingCharge)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EffectAppliedEvent.ID && ID != FinishChargeAvailableEvent.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && (ID != PooledEvent<GetElectricalConductivityEvent>.ID || !ConsiderLive) && ID != QueryChargeEvent.ID && ID != QueryChargeStorageEvent.ID && ID != TestChargeEvent.ID && ID != UseChargeEvent.ID && ID != ZoneActivatedEvent.ID && ID != ZoneDeactivatedEvent.ID)
		{
			return ID == ZoneThawedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Charge", Charge);
		E.AddEntry(this, "IncomingCharge", IncomingCharge);
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

	public override bool HandleEvent(QueryChargeEvent E)
	{
		if ((!E.LiveOnly || ConsiderLive) && E.IncludeTransient && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			CheckTurn(E.Multiple);
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
		if ((!E.LiveOnly || ConsiderLive) && E.IncludeTransient && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			CheckTurn(E.Multiple);
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
		if (E.Pass == 1 && (!E.LiveOnly || ConsiderLive) && E.IncludeTransient && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			CheckTurn(E.Multiple);
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

	public override bool HandleEvent(FinishChargeAvailableEvent E)
	{
		if ((!E.LiveOnly || ConsiderLive) && E.IncludeTransient && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			CheckTurn(E.Multiple);
			if (E.Amount > 0 && (MaxCharge <= 0 || Charge + IncomingCharge < MaxCharge))
			{
				int num = E.Amount;
				if (MaxCharge > 0 && num > MaxCharge - Charge - IncomingCharge)
				{
					num = MaxCharge - Charge - IncomingCharge;
				}
				if (num > 0)
				{
					AddCharge(num);
					E.Amount -= num;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeStorageEvent E)
	{
		if ((!E.LiveOnly || ConsiderLive) && E.IncludeTransient && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (MaxCharge > 0)
			{
				CheckTurn(E.Multiple);
				int num = MaxCharge - GetCharge();
				if (num > 0)
				{
					E.Amount += num;
					E.Transient += num;
				}
			}
			else
			{
				E.UnlimitedTransient = true;
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
				stringBuilder.Append("{{y|(").Append(Description).Append(": ")
					.Append(text)
					.Append(")}}");
				E.AddTag(stringBuilder.ToString());
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Charge = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		ActiveTurn = XRLCore.Core.Game.Turns + 1;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneDeactivatedEvent E)
	{
		ActiveTurn = XRLCore.Core.Game.Turns + 1;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		ActiveTurn = XRLCore.Core.Game.Turns + 1;
		return base.HandleEvent(E);
	}
}
