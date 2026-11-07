using System.Collections.Generic;
using QupKit;
using XRL.UI;
using XRL.World;

public class QudItemList : ObjectPool<QudItemList>
{
	public List<QudItemListElement> objects = new List<QudItemListElement>();

	public Dictionary<string, List<QudItemListElement>> categories;

	public List<string> categoryNames;

	public int eqWeight;

	public void Categorize()
	{
		for (int i = 0; i < objects.Count; i++)
		{
			objects[i].category = objects[i].go.GetInventoryCategory();
		}
		if (categories == null)
		{
			categories = new Dictionary<string, List<QudItemListElement>>();
		}
		categories.Clear();
		if (categoryNames == null)
		{
			categoryNames = new List<string>();
		}
		categoryNames.Clear();
		for (int j = 0; j < objects.Count; j++)
		{
			if (!categoryNames.Contains(objects[j].category))
			{
				categoryNames.Add(objects[j].category);
			}
			if (!categories.ContainsKey(objects[j].category))
			{
				categories.Add(objects[j].category, new List<QudItemListElement>());
			}
			categories[objects[j].category].Add(objects[j]);
		}
		categoryNames.Sort();
		for (int k = 0; k < categoryNames.Count; k++)
		{
			categories[categoryNames[k]].Sort((QudItemListElement o1, QudItemListElement o2) => o1.displayName.CompareTo(o2.displayName));
		}
	}

	public void Clear()
	{
		if (categoryNames != null)
		{
			categoryNames.Clear();
		}
		if (categories != null)
		{
			categories.Clear();
		}
		foreach (QudItemListElement @object in objects)
		{
			@object.PoolReset();
			ObjectPool<QudItemListElement>.Return(@object);
		}
		objects.Clear();
	}

	public void Add(GameObject go)
	{
		QudItemListElement qudItemListElement = ObjectPool<QudItemListElement>.Checkout();
		qudItemListElement.InitFrom(go);
		objects.Add(qudItemListElement);
	}

	public void Add(List<GameObject> gos)
	{
		foreach (GameObject go in gos)
		{
			Add(go);
		}
	}

	public void Add(IEnumerable<GameObject> gos)
	{
		foreach (GameObject go in gos)
		{
			Add(go);
		}
	}

	public void Add(List<TradeEntry> gos)
	{
		foreach (TradeEntry go in gos)
		{
			if (go.GO != null)
			{
				Add(go.GO);
			}
		}
	}
}
