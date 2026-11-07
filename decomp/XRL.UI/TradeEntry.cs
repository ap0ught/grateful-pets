using XRL.World;

namespace XRL.UI;

public class TradeEntry
{
	public GameObject GO;

	public string CategoryName = "";

	public string NameForSort
	{
		get
		{
			if (GO != null)
			{
				return GO.GetCachedDisplayNameForSort();
			}
			return CategoryName;
		}
	}

	public TradeEntry(string CategoryName)
	{
		this.CategoryName = CategoryName;
	}

	public TradeEntry(GameObject GO)
	{
		this.GO = GO;
	}
}
