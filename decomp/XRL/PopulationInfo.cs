using System.Collections.Generic;

namespace XRL;

public class PopulationInfo : PopulationList
{
	public Dictionary<string, PopulationGroup> GroupLookup = new Dictionary<string, PopulationGroup>();

	public PopulationInfo()
	{
	}

	public PopulationInfo(string Name)
		: this()
	{
		base.Name = Name;
	}

	public string[] GetEachUniqueObjectRoot()
	{
		List<string> list = new List<string>();
		foreach (PopulationItem item in Items)
		{
			item.GetEachUniqueObject(list);
		}
		return list.ToArray();
	}

	public override void GetEachUniqueObject(List<string> List)
	{
		foreach (PopulationItem item in Items)
		{
			item.GetEachUniqueObject(List);
		}
	}
}
