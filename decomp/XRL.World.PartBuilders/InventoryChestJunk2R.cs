using XRL.World.Parts;

namespace XRL.World.PartBuilders;

public class InventoryChestJunk2R : IPartBuilder
{
	public void BuildPart(IPart iPart, string Context = null)
	{
		ChestBuilders.BuildRareChestInventory(iPart as Inventory, 2, Context);
	}
}
