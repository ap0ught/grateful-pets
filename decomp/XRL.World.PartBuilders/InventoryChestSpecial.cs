using XRL.World.Parts;

namespace XRL.World.PartBuilders;

public class InventoryChestSpecial
{
	public static void BuildPart(IPart iPart, int Level, string Context = null)
	{
		ChestBuilders.BuildSpecialChestInventory(iPart as Inventory, Level, Context);
	}
}
