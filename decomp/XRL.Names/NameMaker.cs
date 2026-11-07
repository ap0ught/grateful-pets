using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using XRL.World;

namespace XRL.Names;

public class NameMaker
{
	public static string MakeName(GameObject For = null, string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Region = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null, string Type = null, Dictionary<string, string> NamingContext = null, bool FailureOkay = false, bool SpecialFaildown = false, bool? HasHonorific = null, bool? HasEpithet = null)
	{
		return NameStyles.Generate(For, Genotype, Subtype, Species, Culture, Faction, Region, Gender, Mutations, Tag, Special, Type, NamingContext, FailureOkay, SpecialFaildown, null, null, null, null, HasHonorific, HasEpithet);
	}

	public static void MakeName(ref string Into, GameObject For = null, string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Region = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null, string Type = null, Dictionary<string, string> NamingContext = null, bool SpecialFaildown = false, bool? HasHonorific = null, bool? HasEpithet = null)
	{
		string text = NameStyles.Generate(For, Genotype, Subtype, Species, Culture, Faction, Region, Gender, Mutations, Tag, Special, Type, NamingContext, FailureOkay: true, SpecialFaildown, null, null, null, null, HasHonorific, HasEpithet);
		if (!text.IsNullOrEmpty())
		{
			Into = text;
		}
	}

	public static string MakeHonorific(GameObject For = null, string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Region = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null, Dictionary<string, string> NamingContext = null, bool SpecialFaildown = false, bool? HasHonorific = null, bool? HasEpithet = null)
	{
		return NameStyles.Generate(For, Genotype, Subtype, Species, Culture, Faction, Region, Gender, Mutations, Tag, Special, "Honorific", NamingContext, FailureOkay: true, SpecialFaildown, null, null, null, null, HasHonorific, HasEpithet);
	}

	public static string MakeEpithet(GameObject For = null, string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Region = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null, Dictionary<string, string> NamingContext = null, bool SpecialFaildown = false, bool? HasHonorific = null, bool? HasEpithet = null)
	{
		return NameStyles.Generate(For, Genotype, Subtype, Species, Culture, Faction, Region, Gender, Mutations, Tag, Special, "Epithet", NamingContext, FailureOkay: true, SpecialFaildown, null, null, null, null, HasHonorific, HasEpithet);
	}

	public static string MakeTitle(GameObject For = null, string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Region = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null, Dictionary<string, string> NamingContext = null, bool SpecialFaildown = false, bool? HasHonorific = null, bool? HasEpithet = null)
	{
		return NameStyles.Generate(For, Genotype, Subtype, Species, Culture, Faction, Region, Gender, Mutations, Tag, Special, "Title", NamingContext, FailureOkay: true, SpecialFaildown, null, null, null, null, HasHonorific, HasEpithet);
	}

	public static string MakeExtraTitle(GameObject For = null, string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Region = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null, Dictionary<string, string> NamingContext = null, bool SpecialFaildown = false, bool? HasHonorific = null, bool? HasEpithet = null)
	{
		return NameStyles.Generate(For, Genotype, Subtype, Species, Culture, Faction, Region, Gender, Mutations, Tag, Special, "ExtraTitle", NamingContext, FailureOkay: true, SpecialFaildown, null, null, null, null, HasHonorific, HasEpithet);
	}

	public static string Eater()
	{
		if (If.Chance(50))
		{
			return MakeName(null, null, null, null, "Eater");
		}
		return MakeName(EncountersAPI.GetASampleCreature());
	}

	public static string YdFreeholder()
	{
		return MakeName(EncountersAPI.GetASampleCreature());
	}
}
