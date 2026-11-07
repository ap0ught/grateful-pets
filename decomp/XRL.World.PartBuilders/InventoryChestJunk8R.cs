using XRL.World.Parts;

namespace XRL.World.PartBuilders;

public class InventoryChestJunk8R : IPartBuilder
{
	public void BuildPart(IPart iPart, string Context = null)
	{
		ChestBuilders.BuildRareChestInventory(iPart as Inventory, 8, Context);
	}
}
