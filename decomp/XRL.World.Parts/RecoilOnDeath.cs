using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class RecoilOnDeath : IPoweredPart
{
	public string DestinationZone = "";

	public int DestinationX = 40;

	public int DestinationY = 13;

	public RecoilOnDeath()
	{
		ChargeUse = 0;
		WorksOnEquipper = true;
		IsEMPSensitive = false;
		NameForStatus = "EmergencyMedEvac";
	}

	public override bool SameAs(IPart p)
	{
		RecoilOnDeath recoilOnDeath = p as RecoilOnDeath;
		if (recoilOnDeath.DestinationZone != DestinationZone)
		{
			return false;
		}
		if (recoilOnDeath.DestinationX != DestinationX)
		{
			return false;
		}
		if (recoilOnDeath.DestinationY != DestinationY)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeforeDismemberEvent>.ID && ID != EquippedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		if (ParentObject.Equipped.IsValid())
		{
			Registrar.Register(ParentObject.Equipped, BeforeDieEvent.ID);
		}
	}

	public override bool HandleEvent(BeforeDismemberEvent E)
	{
		if (E.IsObject && E.Part.ObjectEquippedOnThisOrAnyParent(ParentObject) && ParentObject.IsWorn())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterEvent(this, BeforeDieEvent.ID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterEvent(this, BeforeDieEvent.ID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDieEvent E)
	{
		GameObject dying = E.Dying;
		if (!dying.IsValid() || ParentObject.Equipped != dying || !ParentObject.IsWorn() || DestinationZone.IsNullOrEmpty())
		{
			return true;
		}
		if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return true;
		}
		Zone zone = The.ZoneManager.GetZone(DestinationZone);
		WorldBlueprint worldBlueprint = GetAnyBasisZone()?.ResolveWorldBlueprint();
		WorldBlueprint worldBlueprint2 = zone?.ResolveWorldBlueprint();
		if (zone == null || worldBlueprint != worldBlueprint2)
		{
			return true;
		}
		Cell targetCell = zone.GetCell(40, 20);
		dying.RestorePristineHealth();
		if (DestinationX == -1 || DestinationY == -1)
		{
			try
			{
				List<Cell> emptyReachableCells = zone.GetEmptyReachableCells();
				if (emptyReachableCells.Count > 0)
				{
					targetCell = emptyReachableCells.GetRandomElement();
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		else
		{
			targetCell = zone.GetCell(DestinationX, DestinationY);
		}
		if (dying.IsPlayer())
		{
			Popup.Show("Just before your demise, you are transported to safety! " + ParentObject.Does("disintegrate") + ".");
		}
		IComponent<GameObject>.XDidY(dying, "dematerialize", "out of the local region of spacetime", null, null, null, dying);
		dying.TeleportTo(targetCell, 1000, ignoreCombat: true, ignoreGravity: false, forced: false, UsePopups: false, null);
		E.RequestInterfaceExit();
		if (dying.IsPlayer())
		{
			ParentObject.Destroy();
		}
		return false;
	}
}
