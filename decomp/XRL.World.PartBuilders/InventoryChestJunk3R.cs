using XRL.World.Parts;

namespace XRL.World.PartBuilders;

public class InventoryChestJunk3R : IPartBuilder
{
	public void BuildPart(IPart iPart, string Context = null)
	{
		ChestBuilders.BuildRareChestInventory(iPart as Inventory, 3, Context);
	}
}
