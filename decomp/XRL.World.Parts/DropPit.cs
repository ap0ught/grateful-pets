using System;
using XRL.Names;

namespace XRL.World.Parts;

[Serializable]
public class DropPit : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (ParentObject.GetIntProperty("SuppressCorpseDrops") <= 0)
		{
			ProcessPitDrop(E.ThirdPersonReason);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private void ProcessPitDrop(string DeathReason = null)
	{
		IInventory dropInventory = ParentObject.GetDropInventory();
		if (dropInventory == null)
		{
			return;
		}
		Zone inventoryZone = dropInventory.GetInventoryZone();
		if (inventoryZone == null || !inventoryZone.Built)
		{
			return;
		}
		Cell inventoryCell = dropInventory.GetInventoryCell();
		GameObject gameObject = GameObject.Create("Pit");
		if (gameObject == null)
		{
			return;
		}
		if (ParentObject.HasProperName)
		{
			gameObject.SetStringProperty("CreatureName", ParentObject.BaseDisplayName);
		}
		else
		{
			string text = NameMaker.MakeName(ParentObject, null, null, null, null, null, null, null, null, null, null, null, null, FailureOkay: true);
			if (text != null)
			{
				gameObject.SetStringProperty("CreatureName", text);
			}
		}
		if (!DeathReason.IsNullOrEmpty())
		{
			gameObject.SetStringProperty("DeathReason", DeathReason);
		}
		if (ParentObject.HasProperty("StoredByPlayer") || ParentObject.HasProperty("FromStoredByPlayer"))
		{
			gameObject.SetIntProperty("FromStoredByPlayer", 1);
		}
		inventoryCell.RemoveObject(ParentObject);
		inventoryCell.AddObject(gameObject);
		inventoryCell.PaintWallsAround();
		inventoryZone.FireEvent("FirmPitEdges");
		inventoryZone.FireEvent("PaintPitEdges");
		ZoneManager.PaintWalls(inventoryZone);
	}
}
