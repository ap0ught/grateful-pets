using System;
using XRL.Core;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, teleport distance is increased by
/// a percentage equal to ((power load - 100) / 10), i.e. 30% for
/// the standard overload power load of 400.
/// </remarks>
[Serializable]
public class AccelerativeTeleporter : IPoweredPart
{
	public int TriggerDistance = 3;

	public int TeleportDistance = 9;

	public int MaximumShuntDistance = 3;

	public bool AccelerateTravel;

	public bool WorksOnWorldMap;

	public bool GroundContactRequired = true;

	public bool DoSpatialDistortionBlip = true;

	public bool DoTeleportVisualEffects = true;

	public bool DoTeleportReducedVisualEffects = true;

	public string CurrentDirection;

	public int CurrentDistance;

	[NonSerialized]
	private bool AcceleratedTravel;

	public AccelerativeTeleporter()
	{
		ChargeUse = 250;
		IsPowerLoadSensitive = true;
		IsPowerSwitchSensitive = true;
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		AccelerativeTeleporter accelerativeTeleporter = p as AccelerativeTeleporter;
		if (accelerativeTeleporter.TriggerDistance != TriggerDistance)
		{
			return false;
		}
		if (accelerativeTeleporter.TeleportDistance != TeleportDistance)
		{
			return false;
		}
		if (accelerativeTeleporter.MaximumShuntDistance != MaximumShuntDistance)
		{
			return false;
		}
		if (accelerativeTeleporter.AccelerateTravel != AccelerateTravel)
		{
			return false;
		}
		if (accelerativeTeleporter.WorksOnWorldMap != WorksOnWorldMap)
		{
			return false;
		}
		if (accelerativeTeleporter.GroundContactRequired != GroundContactRequired)
		{
			return false;
		}
		if (accelerativeTeleporter.DoSpatialDistortionBlip != DoSpatialDistortionBlip)
		{
			return false;
		}
		if (accelerativeTeleporter.DoTeleportVisualEffects != DoTeleportVisualEffects)
		{
			return false;
		}
		if (accelerativeTeleporter.DoTeleportReducedVisualEffects != DoTeleportReducedVisualEffects)
		{
			return false;
		}
		return base.SameAs(p);
	}

	private bool IsOutOfGroundContact(GameObject obj)
	{
		if (!obj.IsFlying)
		{
			return !obj.PhaseMatches(1);
		}
		return true;
	}

	private bool TriggerSpatialDistortionBlip(GameObject obj)
	{
		obj.SpatialDistortionBlip();
		return true;
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (GroundContactRequired)
		{
			return GetActivePartFirstSubject(IsOutOfGroundContact) != null;
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "NoGroundContact";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != EquippedEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != UnequippedEvent.ID)
		{
			return ID == PooledEvent<AfterTravelEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (CurrentDirection != null && !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ResetAcceleration();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		if (CurrentDirection != null && !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ResetAcceleration();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "AfterDirectMove");
		E.Actor.RegisterPartEvent(this, "AfterMoved");
		E.Actor.RegisterPartEvent(this, "AfterTeleport");
		E.Actor.RegisterPartEvent(this, "TravelSpeed");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "AfterDirectMove");
		E.Actor.UnregisterPartEvent(this, "AfterMoved");
		E.Actor.UnregisterPartEvent(this, "AfterTeleport");
		E.Actor.UnregisterPartEvent(this, "TravelSpeed");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("travel", 5);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CellChanged");
		base.Register(Object, Registrar);
	}

	public void ResetAcceleration()
	{
		CurrentDirection = null;
		CurrentDistance = 0;
	}

	public static Cell FindTargetCell(GameObject who, string dir, int dist)
	{
		Cell cellFromDirection = who.CurrentCell;
		Cell cell = null;
		for (int i = 0; i < dist; i++)
		{
			if (cellFromDirection == null)
			{
				break;
			}
			cellFromDirection = cellFromDirection.GetCellFromDirection(dir, BuiltOnly: false);
			if (cellFromDirection != null)
			{
				cell = cellFromDirection;
			}
		}
		return cellFromDirection ?? cell;
	}

	public static Cell FindDestinationCell(GameObject who, string dir, int dist, int maxShunt)
	{
		Cell cell = FindTargetCell(who, dir, dist);
		if (cell == null || cell == who.CurrentCell)
		{
			return null;
		}
		if (cell.IsSolidFor(who))
		{
			Cell cell2 = cell;
			cell = cell2.getClosestReachableCellFor(who);
			if (cell != null && cell.PathDistanceTo(cell2) > maxShunt)
			{
				cell = null;
			}
		}
		return cell;
	}

	public bool PerformAccelerativeTeleport(GameObject who, string dir, Event E, int? PowerLoad = null)
	{
		Cell cell = FindDestinationCell(who, dir, GetEffectiveTeleportDistance(PowerLoad ?? MyPowerLoadLevel()), MaximumShuntDistance);
		if (cell != null && (!who.IsPlayer() || !AutoAct.IsExploration() || who.InSameZone(cell)) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			who.CellTeleport(cell, E, ParentObject, who, null, null, VisualEffects: DoTeleportVisualEffects, ReducedVisualEffects: DoTeleportReducedVisualEffects, EnergyCost: 0);
		}
		return true;
	}

	private bool ProcessMoveInner(Cell oldCell, Cell newCell, Event E, bool forced)
	{
		if (forced)
		{
			return true;
		}
		if (oldCell == null)
		{
			return false;
		}
		if (newCell == null)
		{
			return false;
		}
		if (oldCell.OnWorldMap() != newCell.OnWorldMap())
		{
			return false;
		}
		if (!WorksOnWorldMap && newCell.OnWorldMap())
		{
			return false;
		}
		if (!oldCell.IsAdjacentTo(newCell))
		{
			return false;
		}
		if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: true, 0L))
		{
			return false;
		}
		string dir = oldCell.GetDirectionFromCell(newCell);
		if (dir == CurrentDirection)
		{
			CurrentDistance++;
			if (CurrentDistance >= TriggerDistance)
			{
				if (DoTeleportVisualEffects && newCell.OnWorldMap())
				{
					XRLCore.Core.AllowWorldMapParticles = true;
				}
				ForeachActivePartSubjectWhile((GameObject o) => PerformAccelerativeTeleport(o, dir, E), MayMoveAddOrDestroy: true);
				return false;
			}
			if (DoSpatialDistortionBlip)
			{
				if (newCell.OnWorldMap())
				{
					XRLCore.Core.AllowWorldMapParticles = true;
				}
				ForeachActivePartSubjectWhile(TriggerSpatialDistortionBlip);
				return true;
			}
		}
		else if (!string.IsNullOrEmpty(dir) && dir != ".")
		{
			CurrentDirection = dir;
			CurrentDistance = 1;
			return true;
		}
		return false;
	}

	public void ProcessMove(Cell oldCell, Cell newCell, Event E, bool forced = false)
	{
		if (!ProcessMoveInner(oldCell, newCell, E, forced))
		{
			ResetAcceleration();
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterMoved")
		{
			Cell oldCell = E.GetParameter("FromCell") as Cell;
			Cell newCell = ParentObject.GetCurrentCell();
			ProcessMove(oldCell, newCell, E, E.HasParameter("Forced"));
		}
		else if (E.ID == "TravelSpeed")
		{
			if (AccelerateTravel)
			{
				int num = MyPowerLoadLevel();
				int? powerLoadLevel = num;
				if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
				{
					int num2 = 0;
					int effectiveTeleportDistance = GetEffectiveTeleportDistance(num);
					if (ChargeUse <= 0)
					{
						num2 = effectiveTeleportDistance * 100 / TriggerDistance;
					}
					else
					{
						int num3 = ParentObject.QueryCharge(LiveOnly: false, 0L);
						if (num3 >= ChargeUse * 20)
						{
							num2 = effectiveTeleportDistance * 100 / TriggerDistance;
						}
						else if (num3 >= ChargeUse * 10)
						{
							num2 = effectiveTeleportDistance * 50 / TriggerDistance;
						}
						else
						{
							num2 = ((num3 < ChargeUse * 5) ? (effectiveTeleportDistance / TriggerDistance) : (effectiveTeleportDistance * 25 / TriggerDistance));
						}
					}
					if (num2 > 0)
					{
						if (DoSpatialDistortionBlip)
						{
							XRLCore.Core.AllowWorldMapParticles = true;
							ForeachActivePartSubjectWhile(TriggerSpatialDistortionBlip);
						}
						E.SetParameter("PercentageBonus", E.GetIntParameter("PercentageBonus") + num2);
						AcceleratedTravel = true;
					}
				}
			}
		}
		else if (E.ID == "AfterTeleport")
		{
			ResetAcceleration();
		}
		else if (E.ID == "CellChanged" && CurrentDirection != null && !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ResetAcceleration();
		}
		return base.FireEvent(E);
	}

	public override bool HandleEvent(AfterTravelEvent E)
	{
		if (AcceleratedTravel && ChargeUse > 0 && IsObjectActivePartSubject(E.Actor) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = E.Segments / 10;
			ParentObject.UseCharge(ChargeUse * num, LiveOnly: false, 0L);
			AcceleratedTravel = false;
		}
		return base.HandleEvent(E);
	}

	public int GetEffectiveTeleportDistance()
	{
		int num = TeleportDistance;
		int num2 = MyPowerLoadBonus(int.MinValue, 100, 10);
		if (num2 != 0)
		{
			num = num * (100 + num2) / 100;
		}
		return num;
	}

	public int GetEffectiveTeleportDistance(int PowerLoad)
	{
		int num = TeleportDistance;
		int num2 = MyPowerLoadBonus(PowerLoad, 100, 10);
		if (num2 != 0)
		{
			num = num * (100 + num2) / 100;
		}
		return num;
	}
}
