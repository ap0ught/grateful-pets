using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class BroadcastPowerReceiver : IPoweredPart
{
	public int ChargeRate = 10;

	public bool CanReceiveSatellitePower = true;

	public int MaxSatellitePowerDepth = 12;

	public string SatelliteWorld = "JoppaWorld";

	public bool IgnoresSatellitePowerOcclusion;

	public bool Obvious;

	public bool SatellitePowerOcclusionReadout;

	public List<string> SatelliteWorlds;

	public static long SatellitePowerOcclusionCheckTurn
	{
		get
		{
			return The.Game?.GetInt64GameState("SatellitePowerOcclusionCheckTurn", 0L) ?? 0;
		}
		set
		{
			The.Game?.SetInt64GameState("SatellitePowerOcclusionCheckTurn", value);
		}
	}

	public static bool SatellitePowerOccluded
	{
		get
		{
			return The.Game?.GetBooleanGameState("SatellitePowerOccluded") ?? false;
		}
		set
		{
			The.Game?.SetBooleanGameState("SatellitePowerOccluded", value);
		}
	}

	public static string SatellitePowerOcclusionReason
	{
		get
		{
			return The.Game?.GetStringGameState("SatellitePowerOcclusionReason");
		}
		set
		{
			The.Game?.SetStringGameState("SatellitePowerOcclusionReason", value);
		}
	}

	public static void CheckSatellitePowerOcclusion(int Turns = 1)
	{
		if (SatellitePowerOcclusionCheckTurn >= The.CurrentTurn)
		{
			return;
		}
		int intSetting = GlobalConfig.GetIntSetting("SatellitePowerOcclusionChance", 1);
		int intSetting2 = GlobalConfig.GetIntSetting("SatellitePowerDeocclusionChance", 5);
		for (int i = 0; i < Turns; i++)
		{
			if (SatellitePowerOccluded)
			{
				if (intSetting2.in1000())
				{
					SatellitePowerOccluded = false;
					SatellitePowerOcclusionReason = null;
				}
			}
			else if (intSetting.in1000())
			{
				SatellitePowerOccluded = true;
				SatellitePowerOcclusionReason = null;
			}
		}
		SatellitePowerOcclusionCheckTurn = The.CurrentTurn;
	}

	public BroadcastPowerReceiver()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		BroadcastPowerReceiver broadcastPowerReceiver = p as BroadcastPowerReceiver;
		if (broadcastPowerReceiver.ChargeRate != ChargeRate)
		{
			return false;
		}
		if (broadcastPowerReceiver.CanReceiveSatellitePower != CanReceiveSatellitePower)
		{
			return false;
		}
		if (broadcastPowerReceiver.MaxSatellitePowerDepth != MaxSatellitePowerDepth)
		{
			return false;
		}
		if (broadcastPowerReceiver.SatelliteWorld != SatelliteWorld)
		{
			return false;
		}
		if (broadcastPowerReceiver.IgnoresSatellitePowerOcclusion != IgnoresSatellitePowerOcclusion)
		{
			return false;
		}
		if (broadcastPowerReceiver.Obvious != Obvious)
		{
			return false;
		}
		if (broadcastPowerReceiver.SatellitePowerOcclusionReadout != SatellitePowerOcclusionReadout)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID && ID != PooledEvent<QueryBroadcastDrawEvent>.ID)
		{
			return ID == PrimePowerSystemsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Obvious || IComponent<GameObject>.ThePlayer.GetIntProperty("TechScannerEquipped") > 0)
		{
			E.Postfix.Append("\n{{rules|This object has a broadcast power receiver that can pick up electrical charge").Append(CanReceiveSatellitePower ? " either from satellites if not too far underground or" : "").Append(" from a nearby broadcast power transmitter.");
			AddStatusSummary(E.Postfix);
			E.Postfix.Append("}}");
		}
		if (!IgnoresSatellitePowerOcclusion && SatellitePowerOccluded && (SatellitePowerOcclusionReadout || Scanning.HasScanningFor(IComponent<GameObject>.ThePlayer, Scanning.Scan.Tech)) && CouldReceivePowerFromSatellite())
		{
			if (SatellitePowerOcclusionReason == null)
			{
				SatellitePowerOcclusionReason = HistoricStringExpander.ExpandString("<spice.satellitePower.occlusionReasons.!random>", null, null);
			}
			E.Postfix.AppendRules("\n{{rules|Satellite broadcast power is currently occluded by {{R|" + SatellitePowerOcclusionReason + "}}.}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PrimePowerSystemsEvent E)
	{
		if (ParentObject.HasPropertyOrTag("Furniture"))
		{
			ReceiveCharge();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryBroadcastDrawEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Draw += ChargeRate;
		}
		return base.HandleEvent(E);
	}

	public bool CouldReceivePowerFromSatellite(Cell C = null)
	{
		if (C == null)
		{
			C = GetAnyBasisCell();
			if (C == null)
			{
				return false;
			}
		}
		Zone zone = C.ParentZone;
		if (zone is InteriorZone interiorZone)
		{
			zone = interiorZone.ResolveBasisZone() ?? zone;
		}
		if (string.IsNullOrEmpty(SatelliteWorld))
		{
			if (zone == null)
			{
				return false;
			}
			SatelliteWorld = zone.ResolveZoneWorld();
			SatelliteWorlds = new List<string>(1) { SatelliteWorld };
		}
		else if (SatelliteWorld != "*")
		{
			if (SatelliteWorlds == null)
			{
				SatelliteWorlds = SatelliteWorld.CachedCommaExpansion();
			}
			if (zone == null || !SatelliteWorlds.Contains(zone.ResolveZoneWorld()))
			{
				return false;
			}
		}
		if (!zone.IsWorldMap() && zone.Z > MaxSatellitePowerDepth && !C.ConsideredOutside())
		{
			return false;
		}
		return true;
	}

	public bool IsReceivingPowerFromSatellite(Cell C = null)
	{
		if (!CanReceiveSatellitePower)
		{
			return false;
		}
		if (!IgnoresSatellitePowerOcclusion && SatellitePowerOccluded)
		{
			return false;
		}
		return CouldReceivePowerFromSatellite(C);
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
		Cell anyBasisCell = GetAnyBasisCell();
		if (anyBasisCell == null || (anyBasisCell.ParentZone != null && !anyBasisCell.ParentZone.IsActive()))
		{
			return;
		}
		CheckSatellitePowerOcclusion(Turns);
		if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, Turns, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		if (IsReceivingPowerFromSatellite(anyBasisCell))
		{
			ParentObject.ChargeAvailable(ChargeRate, 0L, Turns);
		}
		else if (!anyBasisCell.OnWorldMap())
		{
			int num = CollectBroadcastChargeEvent.GetFor(ParentObject, anyBasisCell.ParentZone, anyBasisCell, ChargeRate, Turns);
			if (num < ChargeRate)
			{
				int charge = ChargeRate - num;
				ParentObject.ChargeAvailable(charge, 0L, Turns);
			}
		}
	}
}
