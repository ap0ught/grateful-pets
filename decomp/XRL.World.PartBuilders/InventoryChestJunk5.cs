using XRL.World.Parts;

namespace XRL.World.PartBuilders;

public class InventoryChestJunk5 : IPartBuilder
{
	public void BuildPart(IPart iPart, string Context = null)
	{
		ChestBuilders.BuildCommonChestInventory(iPart as Inventory, 5, Context);
	}
}
