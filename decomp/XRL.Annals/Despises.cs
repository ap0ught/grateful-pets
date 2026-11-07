using System;
using HistoryKit;
using XRL.Language;
using XRL.World;

namespace XRL.Annals;

[Serializable]
public class Despises : HistoricEvent
{
	public bool bVillageZero;

	public Despises()
	{
		bVillageZero = false;
	}

	public Despises(bool VillageZero)
	{
		bVillageZero = VillageZero;
	}

	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		int num = Random(1, 3);
		string randomProperty = QudHistoryHelpers.GetRandomProperty(snapshotAtYear, snapshotAtYear.GetProperty("defaultProfaneThing"), "profaneThings");
		switch (num)
		{
		case 1:
		{
			HistoricEntitySnapshot currentSnapshot = Worships.GetSultanToWorshipByTier(snapshotAtYear.Tier, history).GetCurrentSnapshot();
			string text = currentSnapshot.GetProperty("name");
			string text2 = ((currentSnapshot.GetList("cognomen").Count > 0) ? currentSnapshot.GetList("cognomen").GetRandomElement() : null);
			SetEntityProperty("despises_sultan", text);
			SetEntityProperty("despises_sultan_id", currentSnapshot.entity.id);
			if (text2 != null)
			{
				text = (If.d100(30) ? (text + ", " + text2) : text);
			}
			AddEntityListItem("profaneThings", text);
			string value = string.Format("The {0} of {1} was revealed to the people of {2} through {3}.|{4}", ExpandString("<spice.instancesOf.profaneFeeling.!random>"), text, snapshotAtYear.GetProperty("name"), randomProperty, id);
			AddEntityListItem("Gospels", value);
			break;
		}
		case 2:
		{
			string name = Factions.GetRandomFaction(snapshotAtYear.GetProperty("baseFaction")).Name;
			string formattedName = Faction.GetFormattedName(name);
			AddEntityListItem("profaneThings", formattedName);
			SetEntityProperty("despises_faction", name);
			string value = string.Format("The {0} of {1} was revealed to the people of {2} through {3}.|{4}", ExpandString("<spice.instancesOf.profaneFeeling.!random>"), formattedName, snapshotAtYear.GetProperty("name"), randomProperty, id);
			AddEntityListItem("Gospels", value);
			break;
		}
		case 3:
		{
			SetEntityProperty("despises_creature", "*Despises.LegendaryCreature*");
			AddEntityListItem("profaneThings", "*Despises.LegendaryCreature.DisplayName*");
			string value = Grammar.InitCap(string.Format("{0} {1}, who {2} {3}!|{4}", ExpandString("<spice.instancesOf.aCurseUpon.!random>"), "*Despises.LegendaryCreature.DisplayName*", ExpandString("<spice.commonPhrases.supports.!random>"), randomProperty, id));
			AddEntityListItem("Gospels", value);
			break;
		}
		}
	}

	public static void PostProcessEvent(HistoricEntity village, string creatureName, string creatureId)
	{
		village.SetEntityPropertyAtCurrentYear("despises_creature", creatureName);
		village.SetEntityPropertyAtCurrentYear("despises_creature_id", creatureId);
		village.MutateListPropertyAtCurrentYear("sacredThings", (string s) => s.Replace("*Despises.LegendaryCreature.DisplayName*", creatureName));
		village.MutateListPropertyAtCurrentYear("profaneThings", (string s) => s.Replace("*Despises.LegendaryCreature.DisplayName*", creatureName));
		village.MutateListPropertyAtCurrentYear("Gospels", (string s) => s.Replace("*Despises.LegendaryCreature.DisplayName*", creatureName));
		village.SetEntityPropertyAtCurrentYear("proverb", village.GetCurrentSnapshot().GetProperty("proverb").Replace("*Despises.LegendaryCreature.DisplayName*", creatureName));
		village.SetEntityPropertyAtCurrentYear("signatureDishName", village.GetCurrentSnapshot().GetProperty("signatureDishName").Replace("*Despises.LegendaryCreature.DisplayName*", creatureName));
		village.SetEntityPropertyAtCurrentYear("newFactionName", village.GetCurrentSnapshot().GetProperty("newFactionName").Replace("*Despises.LegendaryCreature.DisplayName*", creatureName));
	}
}
