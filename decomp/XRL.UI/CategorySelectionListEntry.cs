using XRL.World;

namespace XRL.UI;

public class CategorySelectionListEntry
{
	public InventoryCategory Category;

	public GameObject Object;

	public CategorySelectionListEntry(InventoryCategory Cat)
	{
		Category = Cat;
	}

	public CategorySelectionListEntry(GameObject GO)
	{
		Object = GO;
	}
}
