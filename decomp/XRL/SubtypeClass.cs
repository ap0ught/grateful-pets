using System.Collections.Generic;

namespace XRL;

public class SubtypeClass
{
	public string ID;

	public string ChargenTitle;

	public string SingluarTitle;

	public string StatBoxDisplay = "false";

	public List<SubtypeCategory> Categories = new List<SubtypeCategory>();

	public List<SubtypeEntry> GetAllSubtypes()
	{
		List<SubtypeEntry> list = new List<SubtypeEntry>();
		for (int i = 0; i < Categories.Count; i++)
		{
			list.AddRange(Categories[i].Subtypes);
		}
		return list;
	}
}
