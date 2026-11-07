using XRL.World.Parts;

namespace XRL.World.PartBuilders;

public class InventoryChestJunk2 : IPartBuilder
{
	public void BuildPart(IPart iPart, string Context = null)
	{
		ChestBuilders.BuildCommonChestInventory(iPart as Inventory, 2, Context);
	}
}
