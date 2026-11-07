using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.ObjectBuilders;

public class ObjectRetinueEntry
{
	public string Amount;

	public string Object;

	public ObjectRetinueEntry(string Amount, string Object)
	{
		this.Amount = Amount;
		this.Object = Object;
	}

	public List<GameObject> Generate()
	{
		List<GameObject> list = new List<GameObject>();
		int num = Stat.Roll(Amount);
		for (int i = 0; i < num; i++)
		{
			list.Add(GameObject.Create(Object));
		}
		return list;
	}
}
