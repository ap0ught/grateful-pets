using XRL.World.Parts;

namespace XRL.World.PartBuilders;

public class InventoryChestJunk3or4 : IPartBuilder
{
	public void BuildPart(IPart iPart, string Context = null)
	{
		if (50.in100())
		{
			ChestBuilders.BuildCommonChestInventory(iPart as Inventory, 3, Context);
		}
		else
		{
			ChestBuilders.BuildCommonChestInventory(iPart as Inventory, 4, Context);
		}
	}
}
