using System;
using System.Collections.Generic;

namespace XRL;

[Serializable]
public class MutationCategory
{
	private string _CategoryModifierName;

	public string Name = "";

	public string DisplayName = "";

	public string Help = "";

	public string Stat = "";

	public string Property = "";

	public string ForceProperty = "";

	public string Foreground = "w";

	public string Detail = "W";

	[Obsolete("Read MutationEntry.ExcludeFromPool on individual entries.")]
	public bool IncludeInMutatePool;

	public List<MutationEntry> Entries = new List<MutationEntry>(128);

	public string CategoryModifierName
	{
		get
		{
			if (_CategoryModifierName == null)
			{
				_CategoryModifierName = ((Name == null) ? "UnknownMutationLevelModifier" : (Name + "MutationLevelModifier"));
			}
			return _CategoryModifierName;
		}
	}

	public MutationCategory()
	{
	}

	public MutationCategory(string _Name)
	{
		Name = _Name;
	}

	public void Add(MutationEntry Entry)
	{
		Entry.Category = this;
		Entries.Add(Entry);
	}
}
