using XRL.World.Parts;

namespace XRL.World.PartBuilders;

public class InventoryChestJunk5R : IPartBuilder
{
	public void BuildPart(IPart iPart, string Context = null)
	{
		ChestBuilders.BuildRareChestInventory(iPart as Inventory, 5, Context);
	}
}
