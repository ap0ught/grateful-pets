using System;

namespace XRL.World.Parts;

[Serializable]
public class ClearInventoryOnZoneThaw : IActivePart
{
	public string Blueprint;

	public bool RequireOwnerIfOwned = true;

	public ClearInventoryOnZoneThaw()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart Part)
	{
		ClearInventoryOnZoneThaw clearInventoryOnZoneThaw = Part as ClearInventoryOnZoneThaw;
		if (clearInventoryOnZoneThaw.Blueprint != Blueprint)
		{
			return false;
		}
		if (clearInventoryOnZoneThaw.RequireOwnerIfOwned != RequireOwnerIfOwned)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (!HasNearbyOwnerIfNeeded())
		{
			return true;
		}
		return base.GetActivePartLocallyDefinedFailure();
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		if (!HasNearbyOwnerIfNeeded())
		{
			return "NoOwner";
		}
		return base.GetActivePartLocallyDefinedFailureDescription();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneThawedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (ActivePartHasMultipleSubjects())
			{
				ForeachActivePartSubject(ClearInventory, MayMoveAddOrDestroy: true);
			}
			else
			{
				ClearInventory(GetActivePartFirstSubject());
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void ClearInventory(GameObject Object)
	{
		foreach (GameObject item in Object.GetWholeInventoryReadonly())
		{
			if (Blueprint == null || item.Blueprint == Blueprint)
			{
				item.Obliterate();
			}
		}
	}

	public bool HasNearbyOwnerIfNeeded()
	{
		if (!RequireOwnerIfOwned)
		{
			return true;
		}
		string owners = ParentObject.Owner;
		if (owners.IsNullOrEmpty())
		{
			return true;
		}
		return ParentObject.CurrentZone?.HasObject((GameObject o) => o.IsFactionMember(owners)) ?? false;
	}
}
