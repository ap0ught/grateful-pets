using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.World;
using XRL.World.Skills;

namespace XRL.Annals;

[Serializable]
public class BecomesKnownFor : HistoricEvent
{
	public static readonly string[] Temperaments = new string[1] { "Angry" };

	public static readonly string[] ItemCategories = new string[10] { "MeleeWeapon", "MissileWeapon", "Armor", "Shield", "Grenade", "Preservable", "Book", "LightSource", "WaterContainer", "Furniture" };

	public bool bVillageZero;

	public BecomesKnownFor()
	{
		bVillageZero = false;
	}

	public BecomesKnownFor(bool VillageZero)
	{
		bVillageZero = VillageZero;
	}

	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapVillage = entity.GetSnapshotAtYear(entity.lastYear);
		while (true)
		{
			int num = Random(0, 40);
			if (num < 20)
			{
				if (!snapVillage.hasProperty("signatureItem") && !snapVillage.hasProperty("signatureHistoricObjectType"))
				{
					GameObject anItem = EncountersAPI.GetAnItem((GameObjectBlueprint ob) => ob.Tier <= snapVillage.Tier && ob.HasPart("Physics") && ob.GetPartParameter("Physics", "Takeable", Default: true) && ob.GetPartParameter("Physics", "Weight", 0) <= 100 && !ob.HasTag("Corpse") && !ob.HasTag("ExcludeFromQuests"));
					string blueprint = anItem.Blueprint;
					SetEntityProperty("signatureItem", blueprint);
					AddEntityListItem("sacredThings", anItem.GetReferenceDisplayName(int.MaxValue, null, "BecomesKnownForSacred", NoColor: false, Stripped: true, ColorOnly: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true));
					AddEntityListItem("profaneThings", ExpandString("those who <spice.commonPhrases.profane.!random> ") + anItem.GetPluralName(AsIfKnown: false, NoConfusion: false, Stripped: true, BaseOnly: true));
					string text = MutantNameMaker.MakeMutantName();
					string value = Grammar.InitCap(string.Format("{0}, on the {1}, {2} founded {3} with {4} in each hand. {5}, the villagers of {3} were known as the people of {7}.|{8}", ExpandString("<spice.commonPhrases.yearsAgo.!random>"), ExpandString("<spice.myth.mythicDays.!random>"), text, snapVillage.GetProperty("name"), anItem.GetReferenceDisplayName(int.MaxValue, null, "BecomesKnownForAppear", NoColor: false, Stripped: true, ColorOnly: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true), Grammar.InitCap(ExpandString("<spice.commonPhrases.fromThenOn.!random>").Replace("<entity.name>", Grammar.MakePossessive(text))), ExpandString("<spice.commonPhrases.cult.!random>"), anItem.GetReferenceDisplayName(int.MaxValue, null, "BecomesKnownForPeople", NoColor: false, Stripped: true, ColorOnly: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: true), id));
					AddEntityListItem("Gospels", value);
					return;
				}
				continue;
			}
			if (num < 40)
			{
				List<PowerEntry> list = new List<PowerEntry>(200);
				foreach (SkillEntry value5 in SkillFactory.Factory.SkillList.Values)
				{
					foreach (PowerEntry value6 in value5.Powers.Values)
					{
						if (!string.IsNullOrEmpty(value6.Class) && value6.Cost > 0)
						{
							list.Add(value6);
						}
					}
				}
				PowerEntry powerEntry = null;
				SkillEntry skillEntry = null;
				string text2 = "";
				if (bVillageZero)
				{
					switch (snapVillage.GetProperty("region"))
					{
					case "Saltmarsh":
						powerEntry = list.Find((PowerEntry p) => p.Class == "CookingAndGathering_Harvestry");
						break;
					case "Saltdunes":
						powerEntry = list.Find((PowerEntry p) => p.Class == "Discipline_FastingWay");
						break;
					case "DesertCanyon":
						skillEntry = SkillFactory.Factory.SkillByClass["Survival"];
						break;
					default:
						powerEntry = list.Find((PowerEntry p) => p.Class == "CookingAndGathering_Butchery");
						break;
					}
				}
				else if (Stat.Random(1, list.Count + SkillFactory.Factory.SkillList.Count) <= list.Count)
				{
					powerEntry = list.GetRandomElement();
					if (powerEntry.IsSkillInitiatory)
					{
						skillEntry = powerEntry.ParentSkill;
						powerEntry = null;
					}
				}
				else
				{
					skillEntry = SkillFactory.Factory.SkillList.GetRandomElement().Value;
				}
				if (powerEntry != null)
				{
					SetEntityProperty("signatureSkill", powerEntry.Class);
					AddEntityListItem("sacredThings", "the " + ExpandString("<spice.commonPhrases.practice.!random>") + " of " + powerEntry.Snippet);
					AddEntityListItem("profaneThings", "the " + ExpandString("<spice.commonPhrases.misuse.!random>") + " of " + powerEntry.Snippet);
					AddEntityListItem("itemAdjectiveRoots", powerEntry.Name);
					text2 = powerEntry.Snippet;
				}
				else if (skillEntry != null)
				{
					SetEntityProperty("signatureSkill", skillEntry.Class);
					AddEntityListItem("sacredThings", "the " + ExpandString("<spice.commonPhrases.practice.!random>") + " of " + skillEntry.Snippet);
					AddEntityListItem("profaneThings", "the " + ExpandString("<spice.commonPhrases.misuse.!random>") + " of " + skillEntry.Snippet);
					AddEntityListItem("itemAdjectiveRoots", skillEntry.Name);
					text2 = skillEntry.Snippet;
				}
				string text3 = Grammar.RandomShePronoun();
				string value2 = Grammar.InitCap(string.Format("{0}, {1} from {2} {3} through land {4}. There {5} {6} {7} and {8} {9} who taught {10} the way of {11}. {14} returned home and taught what {5} had learned to the {12} of {13}.|{15}", ExpandString("<spice.instancesOf.openingTime.!random>").Replace("*year*", "%" + Random(1, (int)year) + "%"), Grammar.A(ExpandString("<spice.personNouns.!random>")), snapVillage.GetProperty("name"), ExpandString("<spice.commonPhrases.trekked.!random>"), ExpandString("<spice.history.regions.terrain." + snapVillage.GetProperty("region") + ".over.!random>"), text3, "visited", Grammar.A(ExpandString("<spice.professions.!random.guildhall>")), "met", Grammar.A(ExpandString("<spice.personNouns.!random>")), Grammar.ObjectPronoun(text3), text2, ExpandString("<spice.commonPhrases.cult.!random>"), snapVillage.GetProperty("name"), Grammar.InitCap(text3), id));
				AddEntityListItem("Gospels", value2);
				return;
			}
			if (num < 60)
			{
				string randomElement = Temperaments.GetRandomElement();
				SetEntityProperty("signatureTemperament", randomElement);
				string randomProperty = QudHistoryHelpers.GetRandomProperty(snapVillage, snapVillage.GetProperty("defaultSacredThing"), "sacredThings");
				string value3 = Grammar.InitCap(string.Format("{0}, {1} from a nearby {2} visited {3} and insulted {4}. The villagers of {3} responded with great {5} and had {6} {7}.|{8}", ExpandString("<spice.instancesOf.openingTime.!random>").Replace("*year*", "%" + Random(1, (int)year) + "%"), Grammar.A(ExpandString("<spice.personNouns.!random>")), ExpandString("<spice.placeNouns.!random>"), snapVillage.GetProperty("name"), randomProperty, ExpandString("<spice.temperaments." + randomElement + ".synonym.!random>"), Grammar.ObjectPronoun(Grammar.RandomShePronoun()), ExpandString("<spice.temperaments." + randomElement + ".action.!random>"), id));
				AddEntityListItem("Gospels", value3);
				return;
			}
			if (!snapVillage.hasProperty("signatureItem") && !snapVillage.hasProperty("signatureHistoricObjectType"))
			{
				break;
			}
		}
		string randomElement2 = ItemCategories.GetRandomElement();
		int num2 = 0;
		List<GameObjectBlueprint> blueprintsWithTag;
		do
		{
			blueprintsWithTag = GameObjectFactory.Factory.GetBlueprintsWithTag(randomElement2);
			if (num2++ > 0)
			{
				randomElement2 = ItemCategories.GetRandomElement();
			}
		}
		while (blueprintsWithTag.Count == 0 && num2 < 50);
		num2 = 0;
		GameObjectBlueprint randomElement3;
		do
		{
			randomElement3 = blueprintsWithTag.GetRandomElement();
			num2++;
		}
		while ((randomElement3 == null || randomElement3.HasTag("ExcludeFromQuests")) && num2 < 50);
		string name = randomElement3.Name;
		GameObject gameObject = GameObject.Create(name);
		string text4 = ExpandString("<spice.villages.SignatureHistoricObject.!random>").Replace("*var*", gameObject.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: true, ColorOnly: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: true));
		SetEntityProperty("signatureHistoricObjectType", name);
		SetEntityProperty("signatureHistoricObjectName", text4);
		AddEntityListItem("sacredThings", text4);
		AddEntityListItem("profaneThings", ExpandString("those who <spice.commonPhrases.profane.!random> the ") + text4);
		string value4 = Grammar.InitCap(string.Format("{0}, {1} gathered around {2} in {3} {4}. Thus the village of {5} was founded.|{6}", ExpandString("<spice.commonPhrases.yearsAgo.!random>"), ExpandString("<spice.commonPhrases.people.!random>"), text4, ExpandString("<spice.commonPhrases.grave.!random>"), ExpandString("<spice.commonPhrases.reverence.!random>"), snapVillage.GetProperty("name"), id));
		AddEntityListItem("Gospels", value4);
	}
}
