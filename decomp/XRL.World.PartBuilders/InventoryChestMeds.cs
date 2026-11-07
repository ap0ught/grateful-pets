using XRL.World.Parts;

namespace XRL.World.PartBuilders;

public class InventoryChestMeds : IPartBuilder
{
	public void BuildPart(IPart iPart, string Context = null)
	{
		ChestBuilders.BuildTableChestInventory(iPart as Inventory, "Meds ", "1d6+1", 3, Context);
	}
}
