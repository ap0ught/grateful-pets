using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class TeleporterPair : IPoweredPart
{
	public int Cooldown = 100;

	public string CooldownKey = "TeleporterOrbLastUsed";

	public string LocationKey = "";

	public string DestinationKey = "";

	public GameObject tracking;

	[NonSerialized]
	public Cell lastCell;

	public long lastUsed
	{
		get
		{
			if (The.Game.GetObjectGameState(CooldownKey) == null)
			{
				return -Cooldown;
			}
			return (long)The.Game.GetObjectGameState(CooldownKey);
		}
		set
		{
			The.Game.SetObjectGameState(CooldownKey, value);
		}
	}

	public TeleporterPair()
	{
		ChargeUse = 0;
		WorksOnCellContents = true;
		WorksOnAdjacentCellContents = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("travel", 5);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		SyncLocation();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		SyncLocation();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		long num = Cooldown - (The.Game.Turns - lastUsed);
		if (num <= 0)
		{
			E.AddAction("Activate", "activate", "ActivateTeleporterPair", null, 'a', FireOnActor: false, 100);
		}
		else
		{
			E.AddAction("Activate", "{{K|activate}} [{{C|" + num + "}} turn cooldown]", "ActivateTeleporterPair", null, 'a');
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateTeleporterPair" && AttemptTeleport(E.Actor, E))
		{
			E.Actor.UseEnergy(1000, "Item Teleporter Pair");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		Registrar.Register("AddedToInventory");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Equipped" || E.ID == "Unequipped" || E.ID == "EnteredCell" || E.ID == "AddedToInventory" || E.ID == "OnDestroyObject")
		{
			SyncLocation();
		}
		return base.FireEvent(E);
	}

	public void UpdateTracking()
	{
		GameObject gameObject = ParentObject.Equipped ?? ParentObject.InInventory;
		if (gameObject != tracking)
		{
			Untrack();
			tracking = gameObject;
			if (tracking != null)
			{
				tracking.RegisterPartEvent(this, "Equipped");
				tracking.RegisterPartEvent(this, "Unequipped");
				tracking.RegisterPartEvent(this, "EnteredCell");
				tracking.RegisterPartEvent(this, "AddedToInventory");
				tracking.RegisterPartEvent(this, "OnDestroyObject");
			}
		}
	}

	public void Untrack()
	{
		if (tracking != null)
		{
			tracking.UnregisterPartEvent(this, "Equipped");
			tracking.UnregisterPartEvent(this, "Unequipped");
			tracking.UnregisterPartEvent(this, "EnteredCell");
			tracking.UnregisterPartEvent(this, "AddedToInventory");
			tracking.UnregisterPartEvent(this, "OnDestroyObject");
			tracking = null;
		}
	}

	public bool AttemptTeleport(GameObject who, IEvent FromEvent = null)
	{
		if (string.IsNullOrEmpty(DestinationKey))
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("Nothing happens.");
			}
			return false;
		}
		string stringGameState = The.Game.GetStringGameState(DestinationKey);
		if (string.IsNullOrEmpty(stringGameState) || !IsObjectActivePartSubject(who))
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("Nothing happens.");
			}
			return false;
		}
		string[] array = stringGameState.Split('@');
		string text = array[0];
		if (who.AreHostilesNearby() && !who.InZone(text))
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You can't teleport with hostiles nearby!");
			}
			return false;
		}
		if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("Nothing happens.");
			}
			return false;
		}
		string[] array2 = array[1].Split(',');
		int x = Convert.ToInt32(array2[0]);
		int y = Convert.ToInt32(array2[1]);
		int num = (int)(Cooldown - (The.Game.Turns - lastUsed));
		if (num > 0)
		{
			if (who.IsPlayer())
			{
				Popup.Show("You must wait " + num.Things("turn") + " before using " + ParentObject.indicativeProximal + " again.");
			}
			return false;
		}
		bool flag = who.ZoneTeleport(text, x, y, FromEvent, ParentObject, who);
		if (flag)
		{
			lastUsed = The.Game.Turns;
		}
		return flag;
	}

	private void SyncLocation()
	{
		UpdateTracking();
		Cell cell = ParentObject.GetCurrentCell();
		if (cell != null && cell != lastCell)
		{
			The.Game.SetStringGameState(LocationKey, cell.GetAddress());
			lastCell = cell;
		}
	}
}
