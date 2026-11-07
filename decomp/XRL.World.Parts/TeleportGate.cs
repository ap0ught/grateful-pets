using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class TeleportGate : IPoweredPart
{
	public bool RingBasedName = true;

	public GlobalLocation Target;

	public TeleportGate()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		TeleportGate teleportGate = p as TeleportGate;
		if (teleportGate.RingBasedName != RingBasedName)
		{
			return false;
		}
		if (teleportGate.Target != Target)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckPossibleSubjects();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != GetAdjacentNavigationWeightEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetNavigationWeightEvent.ID && ID != PooledEvent<GetPointsOfInterestEvent>.ID && ID != ObjectEnteredCellEvent.ID)
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (E.StandardChecks(this, E.Actor))
		{
			E.Add(ParentObject, ParentObject.GetReferenceDisplayName(), null, null, null, null, null, 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		GameObject parentObject = ParentObject;
		if (parentObject != null && parentObject.CurrentZone?.Built == true)
		{
			CheckIncomingTarget();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood())
		{
			Zone zone = ParentObject?.CurrentZone;
			if (zone != null)
			{
				string text = The.ZoneManager.GetZoneProperty(zone.ZoneID, "TeleportGateName") as string;
				if (!text.IsNullOrEmpty())
				{
					E.ReplacePrimaryBase(text);
				}
				else
				{
					string desc = "secant";
					if (The.ZoneManager.HasZoneProperty(zone.ZoneID, "TeleportGateRingSize"))
					{
						int num = (int)The.ZoneManager.GetZoneProperty(zone.ZoneID, "TeleportGateRingSize");
						if (num > 0)
						{
							desc = num + "-ring";
						}
					}
					E.AddBase(desc, -5);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		CheckIncomingTarget();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		E.MinWeight(60);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(2);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		GameObject parentObject = ParentObject;
		if (parentObject != null && parentObject.CurrentZone?.Built == true)
		{
			CheckPossibleSubject(E.Object, E);
		}
		return base.HandleEvent(E);
	}

	public int CheckPossibleSubjects()
	{
		int num = 0;
		Cell cell = ParentObject.CurrentCell;
		if (cell != null && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int i = 0;
			for (int count = cell.Objects.Count; i < count; i++)
			{
				if (CheckPossibleSubject(cell.Objects[i], null, ReadyKnown: true))
				{
					num++;
					i = -1;
					count = cell.Objects.Count;
				}
			}
		}
		return num;
	}

	public bool CheckPossibleSubject(GameObject Object, IEvent FromEvent = null, bool ReadyKnown = false)
	{
		if (Object != ParentObject && Object.IsReal && !Object.IsScenery && (Object.IsCreature || Object.IsTakeable()) && Object.PhaseAndFlightMatches(ParentObject) && (ReadyKnown || IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L)))
		{
			GlobalLocation target = GetTarget();
			if (target != null)
			{
				if (target.ResolveCell() != null && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					Zone zone = The.ZoneManager.GetZone(target.ZoneID);
					Cell value = Object.CurrentCell;
					bool flag = zone.LastPlayerPresence != -1;
					bool flag2 = Object.ZoneTeleport(zone.ZoneID, target.CellX, target.CellY, FromEvent, ParentObject, Object.IsCreature ? Object : null);
					if (flag2 && !flag && Object.IsPlayer() && Object.ApplyEffect(new Lost(9999, zone.ZoneID)))
					{
						Popup.ShowSpace("You're lost! Regain your bearings by exploring your surroundings.");
						Object.FireEvent(Event.New("AfterLost", "FromCell", value));
					}
					return flag2;
				}
			}
			else
			{
				Object.RandomTeleport(Swirl: true, null, null, null, null, 0, 10);
			}
		}
		return false;
	}

	public void CheckIncomingTarget()
	{
		Cell cell = ParentObject?.CurrentCell;
		if (cell == null)
		{
			return;
		}
		Zone parentZone = cell.ParentZone;
		if (parentZone == null)
		{
			return;
		}
		string zoneID = parentZone.ZoneID;
		if (!The.ZoneManager.HasZoneProperty(zoneID, "TeleportGateIncomingX") || !The.ZoneManager.HasZoneProperty(zoneID, "TeleportGateIncomingY"))
		{
			Cell cell2 = cell.GetEmptyConnectedAdjacentCells(1).GetRandomElement() ?? cell.GetEmptyConnectedAdjacentCells(2).GetRandomElement() ?? parentZone.GetEmptyCells().GetRandomElement();
			if (cell2 != null)
			{
				The.ZoneManager.SetZoneProperty(zoneID, "TeleportGateIncomingX", cell2.X);
				The.ZoneManager.SetZoneProperty(zoneID, "TeleportGateIncomingY", cell2.Y);
			}
		}
	}

	private string GetRandomDestinationZoneID(string World)
	{
		if (World != "JoppaWorld")
		{
			return null;
		}
		int parasangX = Stat.Random(0, 79);
		int parasangY = Stat.Random(0, 24);
		int zoneX = Stat.Random(0, 2);
		int zoneY = Stat.Random(0, 2);
		int zoneZ = (50.in100() ? Stat.Random(10, 40) : 10);
		return ZoneID.Assemble(World, parasangX, parasangY, zoneX, zoneY, zoneZ);
	}

	private string GetRandomTeleportGateZoneID(string World)
	{
		if (The.Game.GetObjectGameState((World ?? "Default") + "TeleportGateZones") is List<string> { Count: >0 } list)
		{
			return list.GetRandomElement();
		}
		return null;
	}

	private GlobalLocation GetRandomTarget()
	{
		string world = ParentObject.CurrentZone?.GetZoneWorld();
		string text = GetRandomTeleportGateZoneID(world);
		if (text == null)
		{
			text = GetRandomDestinationZoneID(world);
			if (text == null)
			{
				return null;
			}
		}
		return GetTargetFromZone(text);
	}

	private GlobalLocation GetPreferredTarget()
	{
		string text = ParentObject?.CurrentZone?.ZoneID;
		if (!text.IsNullOrEmpty())
		{
			string text2 = The.ZoneManager.GetZoneProperty(text, "TeleportGateDestinationZone") as string;
			if (!text2.IsNullOrEmpty())
			{
				return GetTargetFromZone(text2);
			}
		}
		return null;
	}

	private GlobalLocation GetTarget()
	{
		GlobalLocation globalLocation = Target;
		if (globalLocation == null)
		{
			GlobalLocation globalLocation2 = GetPreferredTarget() ?? GetRandomTarget();
			GlobalLocation globalLocation3 = globalLocation2;
			Target = globalLocation2;
			globalLocation = globalLocation3;
		}
		return globalLocation;
	}

	private GlobalLocation GetTargetFromZone(string zoneID)
	{
		if (The.ZoneManager.HasZoneProperty(zoneID, "TeleportGateIncomingX") && The.ZoneManager.HasZoneProperty(zoneID, "TeleportGateIncomingY"))
		{
			return GlobalLocation.FromZoneId(zoneID, (int)The.ZoneManager.GetZoneProperty(zoneID, "TeleportGateIncomingX"), (int)The.ZoneManager.GetZoneProperty(zoneID, "TeleportGateIncomingY"));
		}
		Zone zone = The.ZoneManager.GetZone(zoneID);
		if (The.ZoneManager.HasZoneProperty(zoneID, "TeleportGateIncomingX") && The.ZoneManager.HasZoneProperty(zoneID, "TeleportGateIncomingY"))
		{
			return GlobalLocation.FromZoneId(zoneID, (int)The.ZoneManager.GetZoneProperty(zoneID, "TeleportGateIncomingX"), (int)The.ZoneManager.GetZoneProperty(zoneID, "TeleportGateIncomingY"));
		}
		Cell cell = zone.GetEmptyCells().GetRandomElement() ?? zone.GetCells().GetRandomElement();
		if (cell != null)
		{
			return GlobalLocation.FromZoneId(zoneID, cell.X, cell.Y);
		}
		return GlobalLocation.FromZoneId(zoneID, zone.Width / 2, zone.Height / 2);
	}
}
