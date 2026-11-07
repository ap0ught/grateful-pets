using System;
using System.Text;
using XRL.Language;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Clockwork : IPoweredPart
{
	public int Charge;

	public int ChargeRate = 1;

	public int MaxCharge = 100;

	public int MinimumChargeToExplode = 1000;

	public string Description = "clockwork";

	public string ChargeDisplayStyle = "clockwork";

	public string AltChargeDisplayStyle = "percentage";

	public string AltChargeDisplayProperty = Scanning.GetScanPropertyName(Scanning.Scan.Structure);

	public string StartCharge;

	public bool CatastrophicDisable;

	public bool ChargeLossDisable = true;

	public bool ConsiderLive;

	public Clockwork()
	{
		ChargeUse = 0;
		IsBootSensitive = false;
		IsEMPSensitive = false;
		WorksOnSelf = true;
	}

	public bool HasCharge(int Amount)
	{
		return Charge >= Amount;
	}

	public int GetCharge()
	{
		return Charge;
	}

	public Clockwork UseCharge(int Amount)
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
		return this;
	}

	public Clockwork AddCharge(int Amount)
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
		return this;
	}

	public int GetChargeLevel()
	{
		return EnergyStorage.GetChargeLevel(Charge, MaxCharge);
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
		return EnergyStorage.GetChargeStatus(Charge, MaxCharge, text);
	}

	public override bool SameAs(IPart p)
	{
		Clockwork clockwork = p as Clockwork;
		if (clockwork.Charge != Charge)
		{
			return false;
		}
		if (clockwork.ChargeRate != ChargeRate)
		{
			return false;
		}
		if (clockwork.MaxCharge != MaxCharge)
		{
			return false;
		}
		if (clockwork.MinimumChargeToExplode != MinimumChargeToExplode)
		{
			return false;
		}
		if (clockwork.Description != Description)
		{
			return false;
		}
		if (clockwork.ChargeDisplayStyle != ChargeDisplayStyle)
		{
			return false;
		}
		if (clockwork.AltChargeDisplayStyle != AltChargeDisplayStyle)
		{
			return false;
		}
		if (clockwork.AltChargeDisplayProperty != AltChargeDisplayProperty)
		{
			return false;
		}
		if (clockwork.StartCharge != StartCharge)
		{
			return false;
		}
		if (clockwork.CatastrophicDisable != CatastrophicDisable)
		{
			return false;
		}
		if (clockwork.ChargeLossDisable != ChargeLossDisable)
		{
			return false;
		}
		if (clockwork.ConsiderLive != ConsiderLive)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ChargeAvailableEvent.ID && ID != EffectAppliedEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && (ID != PooledEvent<GetElectricalConductivityEvent>.ID || !ConsiderLive) && (ID != ObjectCreatedEvent.ID || string.IsNullOrEmpty(StartCharge)) && ID != QueryChargeEvent.ID && ID != QueryChargeStorageEvent.ID && ID != TestChargeEvent.ID)
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

	public override bool HandleEvent(ChargeAvailableEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && E.Amount > 0 && Charge < MaxCharge)
		{
			int num = E.Amount;
			if (!E.Forced && ChargeRate != 0)
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

	public override bool HandleEvent(QueryChargeStorageEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = MaxCharge - GetCharge();
			if (num > 0)
			{
				E.Amount += num;
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

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (!string.IsNullOrEmpty(StartCharge))
		{
			Charge = StartCharge.RollCached();
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeDeathRemoval");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeathRemoval" && MinimumChargeToExplode > 0 && Charge >= MinimumChargeToExplode && (CatastrophicDisable || !IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L)))
		{
			if (Visible())
			{
				IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(ParentObject.The + ParentObject.ShortDisplayName) + " " + Description + " tears itself apart!", 'R');
			}
			ParentObject.Explode(Charge, ParentObject.IsCombat ? ParentObject : E.GetGameObjectParameter("Killer"), null, 1f, Neutron: false, SuppressDestroy: true);
		}
		return base.FireEvent(E);
	}
}
