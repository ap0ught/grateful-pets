using System.Collections.Generic;

namespace XRL.World.ZoneBuilders;

public class InteractWithAnObjectDynamicQuestTemplate_FabricateQuestItem : ZoneBuilderSandbox
{
	public string deliveryItemID;

	public InteractWithAnObjectDynamicQuestTemplate_FabricateQuestItem()
	{
	}

	public InteractWithAnObjectDynamicQuestTemplate_FabricateQuestItem(string deliveryItemID)
	{
		this.deliveryItemID = deliveryItemID;
	}

	public bool BuildZone(Zone zone)
	{
		GameObject cachedObjects = The.ZoneManager.GetCachedObjects(deliveryItemID);
		cachedObjects.SetIntProperty("norestock", 1);
		List<GameObject> objectsWithTagOrProperty = zone.GetObjectsWithTagOrProperty("LairOwner");
		if (objectsWithTagOrProperty.Count > 0 && cachedObjects.Physics.Owner == null)
		{
			GameObject randomElement = objectsWithTagOrProperty.GetRandomElement();
			if (randomElement.Brain != null)
			{
				cachedObjects.Physics.Owner = randomElement.Brain.GetPrimaryFaction();
			}
		}
		if (cachedObjects.Physics.Owner == null)
		{
			List<GameObject> objectsWithTagOrProperty2 = zone.GetObjectsWithTagOrProperty("Villager");
			if (objectsWithTagOrProperty2.Count > 0)
			{
				GameObject randomElement2 = objectsWithTagOrProperty2.GetRandomElement();
				if (randomElement2.Brain != null)
				{
					cachedObjects.Physics.Owner = randomElement2.Brain.GetPrimaryFaction();
				}
			}
		}
		if (cachedObjects.Physics.Owner == null)
		{
			List<GameObject> objectsWithPart = zone.GetObjectsWithPart("Brain");
			if (objectsWithPart.Count > 0)
			{
				GameObject randomElement3 = objectsWithPart.GetRandomElement();
				if (randomElement3.Brain != null)
				{
					cachedObjects.Physics.Owner = randomElement3.Brain.GetPrimaryFaction();
				}
			}
		}
		List<Cell> list = zone.GetEmptyCellsWithNoFurniture();
		if (list.Count == 0)
		{
			list = zone.GetEmptyCells();
		}
		if (list.Count == 0)
		{
			list = zone.GetEmptyReachableCells();
		}
		if (list.Count == 0)
		{
			list = zone.GetReachableCells();
		}
		if (list.Count == 0)
		{
			list = zone.GetCells();
		}
		if (list.Count > 0)
		{
			Cell cell = list.ShuffleInPlace()[0];
			cell.AddObject(cachedObjects);
			EnsureCellReachable(zone, cell);
		}
		return true;
	}
}
