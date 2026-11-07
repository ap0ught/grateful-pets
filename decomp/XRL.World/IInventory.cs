namespace XRL.World;

public interface IInventory
{
	GameObject AddObjectToInventory(GameObject Object, GameObject Actor = null, bool Silent = false, bool NoStack = false, bool FlushTransient = true, string Context = null, IEvent ParentEvent = null);

	bool RemoveObjectFromInventory(GameObject Object, GameObject Actor = null, bool Silent = false, bool NoStack = false, bool FlushTransient = true, string Context = null, IEvent ParentEvent = null);

	bool InventoryContains(GameObject Object);

	Cell GetInventoryCell();

	Zone GetInventoryZone();
}
