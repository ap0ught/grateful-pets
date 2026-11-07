using XRL.World.Parts;

namespace XRL.World.PartBuilders;

public class InventoryChestJunk4 : IPartBuilder
{
	public void BuildPart(IPart iPart, string Context = null)
	{
		ChestBuilders.BuildCommonChestInventory(iPart as Inventory, 4, Context);
	}
}
