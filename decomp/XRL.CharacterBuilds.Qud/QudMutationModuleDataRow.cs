using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.CharacterBuilds.Qud;

[Serializable]
public class QudMutationModuleDataRow
{
	public string Mutation;

	public string Variant;

	public int Count;

	[JsonIgnore]
	public string DisplayName
	{
		get
		{
			MutationEntry entry = Entry;
			BaseMutation genericMutation = Mutations.GetGenericMutation(entry?.Class, Variant);
			if (genericMutation == null)
			{
				return entry?.Name ?? Mutation;
			}
			string text = genericMutation.GetDisplayName();
			if (Variant.IsNullOrEmpty() || text.Contains('('))
			{
				return text;
			}
			string variantName = genericMutation.GetVariantName(Variant);
			if (!variantName.IsNullOrEmpty() && !text.Contains(variantName))
			{
				text = text + " (" + variantName + ")";
			}
			return text;
		}
	}

	[JsonIgnore]
	public MutationEntry Entry => QudMutationsModuleData.getMutationEntryByName(Mutation);

	internal void Upgrade(Version Version)
	{
		if (!Variant.IsNullOrEmpty() && char.IsDigit(Variant[0]) && int.TryParse(Variant, out var result))
		{
			List<string> list = Entry.Mutation?.GetVariants();
			if (list.IsNullOrEmpty())
			{
				Variant = null;
			}
			else if (result < 0 || result >= list.Count)
			{
				Variant = list.GetRandomElement(new Random());
			}
			else
			{
				Variant = list[result];
			}
		}
	}
}
