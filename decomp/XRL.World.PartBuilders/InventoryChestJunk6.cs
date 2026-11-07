using XRL.World.Parts;

namespace XRL.World.PartBuilders;

public class InventoryChestJunk6 : IPartBuilder
{
	public void BuildPart(IPart iPart, string Context = null)
	{
		ChestBuilders.BuildCommonChestInventory(iPart as Inventory, 6, Context);
	}
}
