using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AmbientPowerReceiver : IPoweredPart
{
	public int MaxChargeRate;

	public string Grid = "*";

	[NonSerialized]
	public int ChargeRate = -1;

	public AmbientPowerReceiver()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		if (p is AmbientPowerReceiver ambientPowerReceiver && ambientPowerReceiver.MaxChargeRate == MaxChargeRate && ambientPowerReceiver.Grid == Grid)
		{
			return base.SameAs(p);
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteringZoneEvent.ID)
		{
			return ID == PrimePowerSystemsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PrimePowerSystemsEvent E)
	{
		if (ParentObject.HasPropertyOrTag("Furniture"))
		{
			ReceiveCharge();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteringZoneEvent E)
	{
		if (!E.Cell.ParentZone.IsWorldMap() && IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (CheckAmbientPowerDistributionOf(E.Cell.ParentZone, out var ChargeRate))
			{
				if (this.ChargeRate == 0 && ParentObject.IsPlayerControlled())
				{
					DidX("are", "back on the ambient broadcast power grid", null, null, null, ParentObject);
				}
			}
			else if (this.ChargeRate > 0)
			{
				if (ParentObject.IsPlayer() && !E.System)
				{
					if (Popup.ShowYesNo("You are leaving the ambient broadcast power grid and transitioning to backup power. Are you sure?", "Sounds/UI/ui_notification", AllowEscape: true, DialogResult.No) != DialogResult.Yes)
					{
						return false;
					}
				}
				else
				{
					DidX("are", "leaving the ambient broadcast power grid and transitioning to backup power", null, null, null, null, ParentObject);
				}
			}
			this.ChargeRate = ChargeRate;
		}
		return base.HandleEvent(E);
	}

	public bool CheckAmbientPowerDistributionOf(Zone Z, out int ChargeRate)
	{
		ChargeRate = 0;
		if (Z == null)
		{
			return false;
		}
		if (!The.ZoneManager.TryGetZoneProperty<string>(Z.ZoneID, "AmbientPowerGrid", out var Value))
		{
			return false;
		}
		if (Grid != "*" && !Grid.HasDelimitedSubstring(',', Value))
		{
			return false;
		}
		ChargeRate = (The.ZoneManager.TryGetZoneProperty<int>(Z.ZoneID, "AmbientPowerChargeRate", out var Value2) ? Math.Min(Value2, MaxChargeRate) : MaxChargeRate);
		return ChargeRate > 0;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ReceiveCharge(Amount);
	}

	public void ReceiveCharge(int Turns = 1)
	{
		if (ChargeRate == -1)
		{
			CheckAmbientPowerDistributionOf(GetAnyBasisZone(), out ChargeRate);
		}
		if (ChargeRate > 0 && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, Turns, null, UseChargeIfUnpowered: false, 0L))
		{
			ParentObject.ChargeAvailable(ChargeRate, 0L, Turns);
		}
	}
}
