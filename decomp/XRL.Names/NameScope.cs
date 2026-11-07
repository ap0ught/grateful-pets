using System.Collections.Generic;
using System.Text;
using XRL.World;

namespace XRL.Names;

public class NameScope : NameElement
{
	public string Genotype;

	public string Subtype;

	public string Species;

	public string Culture;

	public string Faction;

	public string Region;

	public string Gender;

	public string Mutation;

	public string Tag;

	public string Special;

	public string Type;

	public int Priority;

	public int Chance = 100;

	public bool Combine;

	public bool SkipIfHasHonorific;

	public bool SkipIfHasEpithet;

	public bool ApplyTo(string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Region = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null, string Type = null, bool HasHonorific = false, bool HasEpithet = false)
	{
		if (Type != this.Type)
		{
			return false;
		}
		if (SkipIfHasHonorific & HasHonorific)
		{
			return false;
		}
		if (SkipIfHasEpithet & HasEpithet)
		{
			return false;
		}
		if ((!this.Special.IsNullOrEmpty() || !Special.IsNullOrEmpty()) && Special != this.Special)
		{
			return false;
		}
		if (!this.Tag.IsNullOrEmpty() && Tag != this.Tag)
		{
			return false;
		}
		if (!this.Gender.IsNullOrEmpty() && Gender != this.Gender)
		{
			return false;
		}
		if (!Mutation.IsNullOrEmpty() && (Mutations == null || !Mutations.Contains(Mutation)))
		{
			return false;
		}
		if (!this.Genotype.IsNullOrEmpty() && Genotype != this.Genotype)
		{
			return false;
		}
		if (!this.Subtype.IsNullOrEmpty() && Subtype != this.Subtype)
		{
			return false;
		}
		if (!this.Species.IsNullOrEmpty() && Species != this.Species)
		{
			return false;
		}
		if (!this.Culture.IsNullOrEmpty() && Culture != this.Culture)
		{
			return false;
		}
		if (!this.Faction.IsNullOrEmpty() && Faction != this.Faction)
		{
			return false;
		}
		if (!this.Region.IsNullOrEmpty() && Region != this.Region)
		{
			return false;
		}
		return Chance.in100();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (!Type.IsNullOrEmpty())
		{
			stringBuilder.Compound("Type:", ',').Append(Type);
		}
		if (!Special.IsNullOrEmpty())
		{
			stringBuilder.Compound("Special:", ',').Append(Special);
		}
		if (!Genotype.IsNullOrEmpty())
		{
			stringBuilder.Compound("Genotype:", ',').Append(Genotype);
		}
		if (!Subtype.IsNullOrEmpty())
		{
			stringBuilder.Compound("Subtype:", ',').Append(Subtype);
		}
		if (!Species.IsNullOrEmpty())
		{
			stringBuilder.Compound("Species:", ',').Append(Species);
		}
		if (!Culture.IsNullOrEmpty())
		{
			stringBuilder.Compound("Culture:", ',').Append(Culture);
		}
		if (!Faction.IsNullOrEmpty())
		{
			stringBuilder.Compound("Faction:", ',').Append(Faction);
		}
		if (!Region.IsNullOrEmpty())
		{
			stringBuilder.Compound("Region:", ',').Append(Region);
		}
		if (!Gender.IsNullOrEmpty())
		{
			stringBuilder.Compound("Gender:", ',').Append(Gender);
		}
		if (!Mutation.IsNullOrEmpty())
		{
			stringBuilder.Compound("Mutation:", ',').Append(Mutation);
		}
		if (!Tag.IsNullOrEmpty())
		{
			stringBuilder.Compound("Tag:", ',').Append(Tag);
		}
		stringBuilder.Compound("Priority:", '/').Append(Priority);
		if (Chance != 100)
		{
			stringBuilder.Compound("Chance:", '/').Append(Chance);
		}
		stringBuilder.Compound("Combine:", '/').Append(Combine);
		if (SkipIfHasHonorific)
		{
			stringBuilder.Compound("SkipIfHasHonorific", '/');
		}
		if (SkipIfHasEpithet)
		{
			stringBuilder.Compound("SkipIfHasEpithet", '/');
		}
		return stringBuilder.ToString();
	}

	public string Summarize(string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Region = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null, string Type = null, bool HasHonorific = false, bool HasEpithet = false)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (!Type.IsNullOrEmpty())
		{
			stringBuilder.Compound("Type:", ',').Append(Type);
		}
		if (!Special.IsNullOrEmpty())
		{
			stringBuilder.Compound("Special:", ',').Append(Special);
		}
		if (!Genotype.IsNullOrEmpty())
		{
			stringBuilder.Compound("Genotype:", ',').Append(Genotype);
		}
		if (!Subtype.IsNullOrEmpty())
		{
			stringBuilder.Compound("Subtype:", ',').Append(Subtype);
		}
		if (!Species.IsNullOrEmpty())
		{
			stringBuilder.Compound("Species:", ',').Append(Species);
		}
		if (!Culture.IsNullOrEmpty())
		{
			stringBuilder.Compound("Culture:", ',').Append(Culture);
		}
		if (!Faction.IsNullOrEmpty())
		{
			stringBuilder.Compound("Faction:", ',').Append(Faction);
		}
		if (!Region.IsNullOrEmpty())
		{
			stringBuilder.Compound("Region:", ',').Append(Region);
		}
		if (!Gender.IsNullOrEmpty())
		{
			stringBuilder.Compound("Gender:", ',').Append(Gender);
		}
		if (!Mutations.IsNullOrEmpty())
		{
			stringBuilder.Compound("Mutations:", ',').Append(string.Join("+", Mutations.ToArray()));
		}
		if (!Tag.IsNullOrEmpty())
		{
			stringBuilder.Compound("Tag:", ',').Append(Tag);
		}
		if (HasHonorific)
		{
			stringBuilder.Compound("HasHonorific", '/');
		}
		if (HasEpithet)
		{
			stringBuilder.Compound("HasEpithet", '/');
		}
		return stringBuilder.ToString();
	}
}
