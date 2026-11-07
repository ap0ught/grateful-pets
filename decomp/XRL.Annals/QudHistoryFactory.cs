using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using XRL.Core;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.World;

namespace XRL.Annals;

public static class QudHistoryFactory
{
	public const string PLAYER_ENTITY_ID = "Player";

	public const int numSultans = 5;

	public const int avgYearsInSultanate = 6000;

	public const float avgNumVillages = 28f;

	public const float percentOfWorldmap_Saltdunes = 12f;

	public const float percentOfWorldmap_Saltmarsh = 2f;

	public const float percentOfWorldmap_DesertCanyon = 7f;

	public const float percentOfWorldmap_Jungle = 24f;

	public const float percentOfWorldmap_DeepJungle = 10f;

	public const float percentOfWorldmap_Hills = 9f;

	public const float percentOfWorldmap_Water = 8f;

	public const float percentOfWorldmap_BananaGrove = 1f;

	public const float percentOfWorldmap_Fungal = 2f;

	public const float percentOfWorldmap_LakeHinnom = 3f;

	public const float percentOfWorldmap_PalladiumReef = 2f;

	public const float percentOfWorldmap_Mountains = 10f;

	public const float percentOfWorldmap_Flowerfields = 3f;

	public const float percentOfWorldmap_Ruins = 3f;

	public const float percentOfWorldmap_BaroqueRuins = 2f;

	public const float percentOfWorldmap_MoonStair = 4f;

	public const float villageModifier_Saltdunes = 0.8f;

	public const float villageModifier_Saltmarsh = 1.4f;

	public const float villageModifier_DesertCanyon = 1.2f;

	public const float villageModifier_Jungle = 1f;

	public const float villageModifier_DeepJungle = 1f;

	public const float villageModifier_Hills = 1f;

	public const float villageModifier_Water = 0.8f;

	public const float villageModifier_BananaGrove = 1f;

	public const float villageModifier_Fungal = 1.2f;

	public const float villageModifier_LakeHinnom = 1.2f;

	public const float villageModifier_PalladiumReef = 1.2f;

	public const float villageModifier_Mountains = 0.8f;

	public const float villageModifier_Flowerfields = 1.2f;

	public const float villageModifier_Ruins = 1f;

	public const float villageModifier_BaroqueRuins = 1f;

	public const float villageModifier_MoonStair = 1f;

	public const int numVillageEvents = 2;

	public const int ruinedVillagesOneIn = 20;

	private static HistoricEntitySnapshot SnapshotInstance = new HistoricEntitySnapshot();

	public static void AddSultanCultNames(History history)
	{
		history.GetEntitiesWherePropertyEquals("type", "sultan").ForEach((HistoricEntity sultan) =>
		{
			HistoricEntitySnapshot currentSnapshot = sultan.GetCurrentSnapshot();
			XRLCore.Core.Game.SetStringGameState("CultDisplayName_SultanCult" + currentSnapshot.GetProperty("period"), currentSnapshot.properties["cultName"]);
			string item = "include:SultanCult" + currentSnapshot.GetProperty("period");
			foreach (JournalSultanNote sultanNote in JournalAPI.GetSultanNotes((JournalSultanNote note) => note.SultanID == sultan.id))
			{
				sultanNote.Attributes.Add(item);
			}
		});
	}

	public static History GenerateNewSultanHistory()
	{
		History history = new History(1L);
		InitializeHistory(history);
		List<int> spreadOfSultanYears = QudHistoryHelpers.GetSpreadOfSultanYears(6000, 5);
		for (int i = 1; i <= 5; i++)
		{
			GenerateNewRegions(history, Stat.Random(2, 3), i);
			GenerateNewSultan(history, i);
			history.currentYear += spreadOfSultanYears[i - 1];
		}
		AddSultanCultNames(history);
		AddResheph(history);
		return history;
	}

	public static History GenerateVillageEraHistory(History history)
	{
		int num = int.Parse(history.GetEntitiesWithProperty("Resheph").GetRandomElement().GetCurrentSnapshot()
			.GetProperty("flipYear"));
		for (int i = 0; i < history.events.Count; i++)
		{
			if (history.events[i].HasEventProperty("gospel"))
			{
				string sentence = QudHistoryHelpers.ConvertGospelToSultanateCalendarEra(history.events[i].GetEventProperty("gospel"), num);
				history.events[i].SetEventProperty("gospel", Grammar.ConvertAtoAn(sentence));
			}
			if (history.events[i].HasEventProperty("tombInscription"))
			{
				history.events[i].SetEventProperty("tombInscription", Grammar.ConvertAtoAn(history.events[i].GetEventProperty("tombInscription")));
			}
		}
		GenerateNewVillage(history, num, "DesertCanyon", null, 400, 900, 2, VillageZero: true);
		GenerateNewVillage(history, num, "Saltdunes", null, 400, 900, 2, VillageZero: true);
		GenerateNewVillage(history, num, "Saltmarsh", null, 400, 900, 2, VillageZero: true);
		GenerateNewVillage(history, num, "Hills", null, 400, 900, 2, VillageZero: true);
		int num2 = (int)Math.Round(1.9600000000000002 * (double)Stat.Random(80, 120) / 100.0 * 1.2000000476837158);
		int num3 = (int)Math.Round(3.36 * (double)Stat.Random(80, 120) / 100.0 * 0.800000011920929);
		int num4 = (int)Math.Round(0.56 * (double)Stat.Random(80, 120) / 100.0 * 1.399999976158142);
		int num5 = (int)Math.Round(2.52 * (double)Stat.Random(80, 120) / 100.0 * 1.0);
		int num6 = (int)Math.Round(2.24 * (double)Stat.Random(80, 120) / 100.0 * 0.800000011920929);
		int num7 = (int)Math.Round(0.28 * (double)Stat.Random(80, 120) / 100.0 * 1.0);
		int num8 = (int)Math.Round(0.56 * (double)Stat.Random(80, 120) / 100.0 * 1.2000000476837158);
		int num9 = (int)Math.Round(0.84 * (double)Stat.Random(80, 120) / 100.0 * 1.2000000476837158);
		int num10 = (int)Math.Round(0.56 * (double)Stat.Random(80, 120) / 100.0 * 1.2000000476837158);
		num7 += Stat.Random(-2, 2);
		int num11 = (int)Math.Round(2.8000000000000003 * (double)Stat.Random(80, 120) / 100.0 * 0.800000011920929);
		int num12 = (int)Math.Round(0.84 * (double)Stat.Random(80, 120) / 100.0 * 1.2000000476837158);
		int num13 = (int)Math.Round(6.72 * (double)Stat.Random(80, 120) / 100.0 * 1.0);
		int num14 = (int)Math.Round(2.8000000000000003 * (double)Stat.Random(80, 120) / 100.0 * 1.0);
		int num15 = (int)Math.Round(0.84 * (double)Stat.Random(80, 120) / 100.0 * 1.0);
		int num16 = (int)Math.Round(0.84 * (double)Stat.Random(80, 120) / 100.0 * 1.0);
		int num17 = (int)Math.Round(1.12 * (double)Stat.Random(80, 120) / 100.0 * 1.0);
		for (int j = 1; j <= num2; j++)
		{
			GenerateNewVillage(history, num, "DesertCanyon");
		}
		for (int k = 1; k <= num3; k++)
		{
			GenerateNewVillage(history, num, "Saltdunes");
		}
		for (int l = 1; l <= num4; l++)
		{
			GenerateNewVillage(history, num, "Saltmarsh");
		}
		for (int m = 1; m <= num5; m++)
		{
			GenerateNewVillage(history, num, "Hills");
		}
		for (int n = 1; n <= num6; n++)
		{
			GenerateNewVillage(history, num, "Water");
		}
		for (int num18 = 1; num18 <= num7; num18++)
		{
			GenerateNewVillage(history, num, "BananaGrove");
		}
		for (int num19 = 1; num19 <= num8; num19++)
		{
			GenerateNewVillage(history, num, "Fungal");
		}
		for (int num20 = 1; num20 <= num9; num20++)
		{
			GenerateNewVillage(history, num, "LakeHinnom");
		}
		for (int num21 = 1; num21 <= num10; num21++)
		{
			GenerateNewVillage(history, num, "PalladiumReef");
		}
		for (int num22 = 1; num22 <= num11; num22++)
		{
			GenerateNewVillage(history, num, "Mountains");
		}
		for (int num23 = 1; num23 <= num12; num23++)
		{
			GenerateNewVillage(history, num, "Flowerfields");
		}
		for (int num24 = 1; num24 <= num13; num24++)
		{
			GenerateNewVillage(history, num, "Jungle");
		}
		for (int num25 = 1; num25 <= num14; num25++)
		{
			GenerateNewVillage(history, num, "DeepJungle");
		}
		for (int num26 = 1; num26 <= num15; num26++)
		{
			GenerateNewVillage(history, num, "Ruins");
		}
		for (int num27 = 1; num27 <= num16; num27++)
		{
			GenerateNewVillage(history, num, "BaroqueRuins");
		}
		for (int num28 = 1; num28 <= num17; num28++)
		{
			GenerateNewVillage(history, num, "MoonStair");
		}
		history.currentYear = num + 1000;
		return history;
	}

	public static void InitializeHistory(History history)
	{
		history.CreateEntity(history.currentYear).ApplyEvent(new Regionalize());
	}

	public static HistoricEvent GenerateVillageEvent(bool VillageZero = false, bool AllowAbandoned = true)
	{
		if ((!VillageZero & AllowAbandoned) && If.OneIn(20))
		{
			return new Abandoned(VillageZero);
		}
		return Stat.Random(0, 6) switch
		{
			0 => (HistoricEvent)new BecomesKnownFor(VillageZero), 
			1 => new PopulationInflux(VillageZero), 
			2 => new Worships(VillageZero), 
			3 => new Despises(VillageZero), 
			4 => new SharedMutation(VillageZero), 
			5 => new NewGovernment(VillageZero), 
			_ => new ImportedFoodorDrink(VillageZero), 
		};
	}

	public static HistoricEntity GenerateNewVillage(History History, int Year, string Region, string BaseFaction = null, int YearOffsetLow = 400, int YearOffsetHigh = 900, int EventAmount = 2, bool VillageZero = false, bool AllowAbandoned = true)
	{
		long currentYear = History.currentYear;
		History.currentYear = Year + Stat.Random(YearOffsetLow, YearOffsetHigh);
		HistoricEntity historicEntity = History.CreateEntity(History.currentYear);
		bool villageZero = VillageZero;
		historicEntity.ApplyEvent(new InitializeVillage(Region, BaseFaction, null, null, null, villageZero));
		for (int i = 0; i < EventAmount; i++)
		{
			HistoricEvent newEvent = GenerateVillageEvent(VillageZero, AllowAbandoned);
			historicEntity.ApplyEvent(newEvent, historicEntity.lastYear + Stat.Random(10, 20));
		}
		historicEntity.ApplyEvent(new VillageProverb(), historicEntity.lastYear);
		historicEntity.MutateListPropertyAtCurrentYear("Gospels", (string s) => QudHistoryHelpers.ConvertGospelToSultanateCalendarEra(s, Year));
		History.currentYear = currentYear;
		return historicEntity;
	}

	public static void GenerateNewSultan(History history, int period)
	{
		HistoricEntity historicEntity = history.CreateEntity(history.currentYear);
		historicEntity.ApplyEvent(new InitializeSultan(period));
		historicEntity.ApplyEvent(new SetEntityProperty("isCandidate", "true"));
		if (Stat.Random(0, 4) == 0)
		{
			historicEntity.ApplyEvent(new BornAsHeir(), historicEntity.lastYear + Stat.Random(6, 8));
		}
		else
		{
			historicEntity.ApplyEvent(new FoundAsBabe(), historicEntity.lastYear + Stat.Random(6, 8));
		}
		for (int i = 0; i < 8; i++)
		{
			int num = Stat.Random(0, 16);
			if (num == 0)
			{
				historicEntity.ApplyEvent(new CorruptAdministrator(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 1)
			{
				historicEntity.ApplyEvent(new CapturedByBandits(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 2)
			{
				historicEntity.ApplyEvent(new InspiringExperience(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 3)
			{
				historicEntity.ApplyEvent(new MeetFaction(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 4)
			{
				historicEntity.ApplyEvent(new SecretRitual(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 5)
			{
				historicEntity.ApplyEvent(new ChallengeSultan(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 6)
			{
				historicEntity.ApplyEvent(new ForgeItem(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 7)
			{
				historicEntity.ApplyEvent(new UnderWeirdSky(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 8)
			{
				historicEntity.ApplyEvent(new LiberateCity(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 9)
			{
				historicEntity.ApplyEvent(new RampageRegion(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 10)
			{
				historicEntity.ApplyEvent(new FoundGuild(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 11)
			{
				historicEntity.ApplyEvent(new BattleItem(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 12)
			{
				historicEntity.ApplyEvent(new LoseItemAtTavern(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 13)
			{
				historicEntity.ApplyEvent(new BloodyBattle(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 14)
			{
				historicEntity.ApplyEvent(new ChariotDrivesOffCliff(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 15)
			{
				historicEntity.ApplyEvent(new Abdicate(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num == 16)
			{
				historicEntity.ApplyEvent(new Marry(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (historicEntity.GetCurrentSnapshot().GetProperty("isAlive") != "true")
			{
				historicEntity.ApplyEvent(new FakedDeath(), historicEntity.lastYear + Stat.Random(1, 16));
			}
		}
		if (!historicEntity.GetCurrentSnapshot().GetProperty("isSultan").EqualsNoCase("true"))
		{
			int num2 = Stat.Random(0, 1);
			if (num2 == 0)
			{
				historicEntity.ApplyEvent(new ChallengeSultan(), historicEntity.lastYear + Stat.Random(1, 16));
			}
			if (num2 == 1)
			{
				historicEntity.ApplyEvent(new Abdicate(), historicEntity.lastYear + Stat.Random(1, 16));
			}
		}
		HistoricEntityList entitiesByDelegate = history.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).GetProperty("type").Equals("region") && int.Parse(entity.GetSnapshotAtYear(entity.lastYear).GetProperty("period")) == period);
		for (int num3 = 0; num3 < entitiesByDelegate.Count; num3++)
		{
			string property = entitiesByDelegate.entities[num3].GetCurrentSnapshot().GetProperty("newName");
			bool flag = false;
			for (int num4 = 0; num4 < historicEntity.events.Count; num4++)
			{
				if (historicEntity.events[num4].HasEventProperty("revealsRegion") && historicEntity.events[num4].GetEventProperty("revealsRegion") == property)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				string property2 = entitiesByDelegate.entities[num3].GetCurrentSnapshot().GetProperty("name");
				int num5 = Stat.Random(0, 5);
				if (num5 == 0)
				{
					historicEntity.ApplyEvent(new CorruptAdministrator(property2), historicEntity.lastYear + Stat.Random(1, 16));
				}
				if (num5 == 1)
				{
					historicEntity.ApplyEvent(new BloodyBattle(property2), historicEntity.lastYear + Stat.Random(1, 16));
				}
				if (num5 == 2)
				{
					historicEntity.ApplyEvent(new LoseItemAtTavern(property2), historicEntity.lastYear + Stat.Random(1, 16));
				}
				if (num5 == 3)
				{
					historicEntity.ApplyEvent(new MeetFaction(property2), historicEntity.lastYear + Stat.Random(1, 16));
				}
				if (num5 == 4)
				{
					historicEntity.ApplyEvent(new RampageRegion(property2), historicEntity.lastYear + Stat.Random(1, 16));
				}
				if (num5 == 5)
				{
					historicEntity.ApplyEvent(new SecretRitual(property2), historicEntity.lastYear + Stat.Random(1, 16));
				}
			}
		}
		for (int num6 = 0; num6 < 3; num6++)
		{
			if (historicEntity.GetCurrentSnapshot().GetProperty("isAlive").EqualsNoCase("true"))
			{
				break;
			}
		}
		if (historicEntity.GetCurrentSnapshot().GetProperty("isAlive").EqualsNoCase("true"))
		{
			historicEntity.ApplyEvent(new GenericDeath());
		}
		FillOutLikedFactions(historicEntity);
		GenerateCultName(historicEntity, history);
	}

	public static void AddResheph(History history)
	{
		HistoricEntity historicEntity = history.CreateEntity(history.currentYear);
		historicEntity.ApplyEvent(new InitializeResheph(6), historicEntity.lastYear);
		historicEntity.ApplyEvent(new SetEntityProperty("isCandidate", "true"));
		historicEntity.ApplyEvent(new ReshephIsBorn(), historicEntity.lastYear);
		historicEntity.ApplyEvent(new ReshephHasStarExperience(), historicEntity.lastYear + 33);
		historicEntity.ApplyEvent(new ReshephMeetsRebekah(), historicEntity.lastYear + 10);
		historicEntity.ApplyEvent(new ReshephBecomesSultan(), historicEntity.lastYear + 29);
		historicEntity.ApplyEvent(new ReshephAppointsRebekah(), historicEntity.lastYear + 41);
		historicEntity.ApplyEvent(new ReshephFoundsHarborage(), historicEntity.lastYear + 91);
		historicEntity.ApplyEvent(new ReshephHealsGyre1(), historicEntity.lastYear + 5);
		historicEntity.ApplyEvent(new ReshephBetrayed(), historicEntity.lastYear);
		historicEntity.ApplyEvent(new ReshephHealsGyre2(), historicEntity.lastYear + 1);
		historicEntity.ApplyEvent(new ReshephLearnsCurse(), historicEntity.lastYear + 1);
		historicEntity.ApplyEvent(new ReshephRebuffsCurse(), historicEntity.lastYear);
		historicEntity.ApplyEvent(new ReshephClosesTomb(), historicEntity.lastYear + 1);
		historicEntity.ApplyEvent(new ReshephAbsolvesRebekah(), historicEntity.lastYear + 1);
		historicEntity.ApplyEvent(new ReshephCleansesGyre(), historicEntity.lastYear + 1);
		historicEntity.ApplyEvent(new ReshephWeirdSky(), historicEntity.lastYear + 3);
		historicEntity.ApplyEvent(new ReshephDies(), historicEntity.lastYear);
	}

	public static void GenerateNewRegions(History history, int numRegions, int period)
	{
		for (int i = 0; i < numRegions; i++)
		{
			history.CreateEntity(history.currentYear).ApplyEvent(new InitializeRegion(period));
		}
	}

	public static string NameRuinsSite(History history, out bool Proper, out string nameRoot)
	{
		HistoricEntitySnapshot currentSnapshot = history.GetEntitiesWherePropertyEquals("name", "regionalizationParameters").GetRandomElement().GetCurrentSnapshot();
		string text = HistoricStringExpander.ExpandString("<spice." + currentSnapshot.GetProperty("siteTopology1") + ".!random>", null, history);
		string text2 = HistoricStringExpander.ExpandString("<spice." + currentSnapshot.GetProperty("siteTopology2") + ".!random>", null, history);
		nameRoot = NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, null, null, "Site");
		string text3 = (int.Parse(currentSnapshot.GetProperty("siteTopologyTheChance")).in100() ? "the " : "");
		int num = Stat.Random(0, 80);
		if (num < 15)
		{
			Proper = true;
			return nameRoot;
		}
		if (80.in100())
		{
			nameRoot = currentSnapshot.GetProperty("siteName" + Stat.Random(1, 3));
		}
		if (1.in100())
		{
			nameRoot = "";
		}
		string text4;
		if (num < 30)
		{
			text4 = text3 + nameRoot;
			if (text4 != "")
			{
				text4 += " ";
			}
			text4 += text;
			Proper = true;
		}
		else if (num < 45)
		{
			text4 = text3 + text2 + " " + nameRoot;
			Proper = true;
		}
		else
		{
			if (num >= 60)
			{
				_ = 80;
				Proper = false;
				return "some forgotten ruins";
			}
			text4 = text2 + " " + text + " " + nameRoot;
			Proper = true;
		}
		return Grammar.MakeTitleCase(text4.Replace("  ", " "));
	}

	public static string NameRuinsSite(History history)
	{
		bool Proper;
		string nameRoot;
		return NameRuinsSite(history, out Proper, out nameRoot);
	}

	public static void GenerateCultName(HistoricEntity sultan, History history)
	{
		int num = Stat.Random(0, 100);
		HistoricEntitySnapshot currentSnapshot = sultan.GetCurrentSnapshot();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (currentSnapshot.GetList("cognomen").Count == 0)
		{
			num = 100;
		}
		string text = HistoricStringExpander.ExpandString("<spice.commonPhrases.cult.!random.capitalize>", null, history);
		if (num < 70)
		{
			string randomElement = currentSnapshot.GetList("cognomen").GetRandomElement();
			if (Stat.Random(0, 1) == 0)
			{
				dictionary.Add("cultName", text + " of the " + Grammar.TrimLeadingThe(randomElement));
			}
			else
			{
				dictionary.Add("cultName", Grammar.TrimLeadingThe(randomElement + " " + text));
			}
		}
		else if (Stat.Random(0, 1) == 0)
		{
			dictionary.Add("cultName", "Cult of " + currentSnapshot.GetProperty("name"));
		}
		else
		{
			int num2 = int.Parse(currentSnapshot.GetProperty("suffix"));
			if (num2 > 0)
			{
				dictionary.Add("cultName", Grammar.MakeTitleCase(Grammar.Ordinal(num2) + " " + Grammar.GetWordRoot(currentSnapshot.GetProperty("nameRoot")) + "ian " + text));
			}
			else
			{
				dictionary.Add("cultName", Grammar.GetWordRoot(currentSnapshot.GetProperty("nameRoot")) + "ian " + text);
			}
		}
		sultan.ApplyEvent(new SetEntityProperties(dictionary, null));
	}

	public static void FillOutLikedFactions(HistoricEntity sultan)
	{
		HistoricEntitySnapshot currentSnapshot = sultan.GetCurrentSnapshot();
		int num = int.Parse(currentSnapshot.GetProperty("period"));
		if (num == 5 || num == 4)
		{
			for (int i = currentSnapshot.GetList("likedFactions").Count; i < 3; i++)
			{
				HistoricEntitySnapshot currentSnapshot2 = sultan.GetCurrentSnapshot();
				string blueprint;
				do
				{
					blueprint = PopulationManager.RollOneFrom("RandomFaction_Period" + num).Blueprint;
				}
				while (currentSnapshot2.GetList("likedFactions").Contains(blueprint));
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("likedFactions", blueprint);
				sultan.ApplyEvent(new SetEntityProperties(null, dictionary));
			}
			return;
		}
		for (int j = currentSnapshot.GetList("likedFactions").Count; j < 3; j++)
		{
			HistoricEntitySnapshot currentSnapshot3 = sultan.GetCurrentSnapshot();
			string name;
			do
			{
				name = Factions.GetRandomFactionWithAtLeastOneMember().Name;
			}
			while (currentSnapshot3.GetList("likedFactions").Contains(name));
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			dictionary2.Add("likedFactions", name);
			sultan.ApplyEvent(new SetEntityProperties(null, dictionary2));
		}
	}

	public static HistoricEntity RequirePlayerEntity()
	{
		XRLGame game = The.Game;
		History history = game?.sultanHistory;
		if (history == null)
		{
			MetricsManager.LogError("Cannot get player entity, sultan history not yet generated.");
			return null;
		}
		GameObject body = game.Player.Body;
		HistoricEntity historicEntity = history.GetEntity("Player") ?? history.CreateEntity("Player", 1001L);
		HistoricEvent historicEvent2;
		HistoricEvent historicEvent = (historicEvent2 = historicEntity.events[0]);
		Dictionary<string, string> dictionary = historicEvent2.entityProperties ?? (historicEvent2.entityProperties = new Dictionary<string, string>());
		IPronounProvider pronounProvider = body.GetPronounProvider();
		string value = (dictionary["nameRoot"] = body.Render.DisplayName.Strip());
		dictionary["name"] = value;
		dictionary["subjectPronoun"] = pronounProvider.Subjective;
		dictionary["possessivePronoun"] = pronounProvider.PossessiveAdjective;
		dictionary["objectPronoun"] = pronounProvider.Objective;
		historicEvent2 = historicEvent;
		Dictionary<string, List<string>> dictionary2 = historicEvent2.addedListProperties ?? (historicEvent2.addedListProperties = new Dictionary<string, List<string>>());
		if (!dictionary2.TryGetValue("elements", out var value2))
		{
			value2 = (dictionary2["elements"] = new List<string>());
		}
		value2.Clear();
		GetItemElementsEvent E = GetItemElementsEvent.GetMythicFor(body);
		if (E.Bag.IsNullOrEmpty())
		{
			value2.Add("might");
		}
		else
		{
			value2.AddRange(E.Bag.Items);
		}
		PooledEvent<GetItemElementsEvent>.ResetTo(ref E);
		return historicEntity;
	}

	public static HistoricEntitySnapshot RequirePlayerEntitySnapshot()
	{
		HistoricEntity historicEntity = RequirePlayerEntity();
		HistoricEntitySnapshot snapshotInstance = SnapshotInstance;
		snapshotInstance.entity = historicEntity;
		snapshotInstance.properties = historicEntity.events[0].entityProperties ?? snapshotInstance.properties;
		snapshotInstance.listProperties = historicEntity.events[0].addedListProperties ?? snapshotInstance.listProperties;
		return snapshotInstance;
	}
}
