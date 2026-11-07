using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using UnityEngine;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;
using XRL.World.Encounters;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.ZoneParts;

namespace XRL;

[Serializable]
[HasWishCommand]
public class PsychicHunterSystem : IGameSystem
{
	[NonSerialized]
	public Dictionary<string, bool> Visited = new Dictionary<string, bool>();

	public override void Read(SerializationReader Reader)
	{
		Visited = Reader.ReadDictionary<string, bool>();
	}

	public override void Write(SerializationWriter Writer)
	{
		Writer.Write(Visited);
	}

	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(ZoneActivatedEvent.ID);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		CheckPsychicHunters(E.Zone);
		return base.HandleEvent(E);
	}

	public void CreatePsychicHunter(int numHunters, Zone Z)
	{
		if (The.Player.GetPsychicGlimmer() < DimensionManager.GLIMMER_EXTRADIMENSIONAL_FLOOR)
		{
			CreateSeekerHunters(numHunters, Z);
		}
		else if (numHunters > 1)
		{
			if (50.in100())
			{
				CreateSeekerHunters(numHunters, Z);
			}
			else
			{
				CreateExtradimensionalCultHunters(Z, numHunters);
			}
		}
		else if (numHunters > 0)
		{
			int num = Stat.Random(1, 100);
			if (num <= 30)
			{
				CreateSeekerHunters(numHunters, Z);
			}
			else if (num <= 70)
			{
				CreateExtradimensionalSoloHunters(Z, numHunters);
			}
			else
			{
				CreateExtradimensionalCultHunters(Z, numHunters);
			}
		}
	}

	public static void CreateSeekerHunters(int numHunters, Zone Z)
	{
		bool flag = false;
		if (The.Game.PlayerReputation.Get("Seekers") >= RuleSettings.REPUTATION_LIKED)
		{
			return;
		}
		for (int i = 1; i <= numHunters; i++)
		{
			XRL.World.GameObject gameObject = XRL.World.GameObject.Create("PsychicSeekerHunter");
			gameObject.Render.SetForegroundColor("M");
			gameObject.Brain.Allegiance.Hostile = true;
			gameObject.Brain.Hibernating = false;
			gameObject.Brain.AddOpinion<OpinionPsychicHunt>(The.Player);
			gameObject.AwardXP(The.Player.Stat("XP"), -1, 0, int.MaxValue, null, The.Player);
			gameObject.Statistics["Ego"].BaseValue = The.Player.Statistics["Ego"].Value;
			if (gameObject.Statistics.ContainsKey("MP"))
			{
				gameObject.Statistics["MP"].BaseValue = 0;
			}
			Mutations part = The.Player.GetPart<Mutations>();
			Mutations part2 = gameObject.GetPart<Mutations>();
			foreach (BaseMutation mutation in part.MutationList)
			{
				MutationEntry mutationEntry = mutation.GetMutationEntry();
				if (mutationEntry?.Category == null || !(mutationEntry.Category.Name == "Mental") || mutationEntry.Cost <= 1)
				{
					continue;
				}
				List<MutationEntry> list = new List<MutationEntry>(gameObject.GetPart<Mutations>().GetMutatePool());
				list.ShuffleInPlace();
				MutationEntry mutationEntry2 = null;
				foreach (MutationEntry item in list)
				{
					if (item.Category != null && item.Category.Name == "Mental" && !gameObject.HasPart(item.Class) && item.Cost > 1)
					{
						mutationEntry2 = item;
						break;
					}
				}
				if (mutationEntry2 != null)
				{
					part2.AddMutation(mutationEntry2.Class, mutation.BaseLevel);
				}
			}
			string newValue;
			if (The.Player.GetPsychicGlimmer() < DimensionManager.GLIMMER_FLOOR + 15)
			{
				newValue = "Osprey";
			}
			else if (The.Player.GetPsychicGlimmer() < DimensionManager.GLIMMER_FLOOR + 30)
			{
				newValue = "Harrier";
			}
			else if (The.Player.GetPsychicGlimmer() < DimensionManager.GLIMMER_FLOOR + 45)
			{
				newValue = "Owl";
			}
			else if (The.Player.GetPsychicGlimmer() < DimensionManager.GLIMMER_FLOOR + 60)
			{
				newValue = "Condor";
			}
			else if (The.Player.GetPsychicGlimmer() < DimensionManager.GLIMMER_FLOOR + 75)
			{
				newValue = "Strix";
			}
			else
			{
				newValue = ((The.Player.GetPsychicGlimmer() >= DimensionManager.GLIMMER_FLOOR + 90) ? "Rukh" : "Eagle");
			}
			string value = "Ptoh's " + Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<spice.seekers.title.!random>").Replace("*rank*", newValue));
			Dictionary<string, string> dictionary = new Dictionary<string, string> { { "*Position*", value } };
			string text = NameMaker.MakeTitle(gameObject, null, null, null, null, null, null, null, null, null, "PsychicHunter", dictionary, SpecialFaildown: true);
			Dictionary<string, string> namingContext = dictionary;
			gameObject.GiveProperName(null, Force: false, "PsychicHunter", SpecialFaildown: true, null, null, namingContext);
			gameObject.RequirePart<DisplayNameColor>().SetColorByPriority("M", 30);
			if (!text.IsNullOrEmpty())
			{
				gameObject.RequirePart<Titles>().AddTitle(text, -40);
			}
			gameObject.RequirePart<AbsorbablePsyche>();
			gameObject.AddPart(new HasThralls(GetThrallRoll(The.Player.GetPsychicGlimmer()), StripGear: true));
			RemoveParts(gameObject);
			flag |= PlaceHunter(Z, gameObject);
		}
		if (flag)
		{
			if (numHunters > 1)
			{
				Popup.Show("{{c|You sense the animus of a vast mind. They are near.}}");
			}
			else
			{
				Popup.Show("{{c|You sense the animus of a vast mind. Someone is near.}}");
			}
		}
	}

	public static void CreateExtradimensionalSoloHunters(Zone Z, int Number = 1, List<XRL.World.GameObject> ObjectList = null, bool Place = true, bool TeleportSwirl = false, bool UseMessage = true, bool UsePopup = true)
	{
		ExtraDimension randomElement = (The.Game.GetObjectGameState("DimensionManager") as DimensionManager).ExtraDimensions.GetRandomElement();
		int num = 0;
		for (int i = 0; i < Number; i++)
		{
			XRL.World.GameObject nonLegendaryCreatureAroundPlayerLevel = EncountersAPI.GetNonLegendaryCreatureAroundPlayerLevel();
			nonLegendaryCreatureAroundPlayerLevel.Render.SetForegroundColor(randomElement.mainColor);
			nonLegendaryCreatureAroundPlayerLevel.Render.DetailColor = "O";
			if (!nonLegendaryCreatureAroundPlayerLevel.HasProperty("PsychicHunter"))
			{
				nonLegendaryCreatureAroundPlayerLevel.SetStringProperty("PsychicHunter", "true");
			}
			nonLegendaryCreatureAroundPlayerLevel.Brain.Allegiance.Clear();
			nonLegendaryCreatureAroundPlayerLevel.Brain.Allegiance.Add("Playerhater", 100);
			nonLegendaryCreatureAroundPlayerLevel.Brain.Allegiance.Hostile = true;
			nonLegendaryCreatureAroundPlayerLevel.Brain.Allegiance.Calm = false;
			nonLegendaryCreatureAroundPlayerLevel.Brain.Hibernating = false;
			nonLegendaryCreatureAroundPlayerLevel.Brain.Aquatic = false;
			nonLegendaryCreatureAroundPlayerLevel.Brain.Mobile = true;
			nonLegendaryCreatureAroundPlayerLevel.Brain.AddOpinion<OpinionPsychicHunt>(The.Player);
			nonLegendaryCreatureAroundPlayerLevel.RequirePart<Combat>();
			ConversationScript part = nonLegendaryCreatureAroundPlayerLevel.GetPart<ConversationScript>();
			if (part != null)
			{
				part.Filter = "Weird";
				part.FilterExtras = randomElement.Name;
			}
			nonLegendaryCreatureAroundPlayerLevel.AwardXP(The.Player.Stat("XP"), -1, 0, int.MaxValue, null, The.Player);
			nonLegendaryCreatureAroundPlayerLevel.GetStat("Ego").BaseValue = The.Player.Stat("Ego");
			if (nonLegendaryCreatureAroundPlayerLevel.HasStat("MP"))
			{
				nonLegendaryCreatureAroundPlayerLevel.GetStat("MP").BaseValue = 0;
			}
			Mutations part2 = The.Player.GetPart<Mutations>();
			Mutations part3 = nonLegendaryCreatureAroundPlayerLevel.GetPart<Mutations>();
			foreach (BaseMutation mutation in part2.MutationList)
			{
				MutationEntry mutationEntry = mutation.GetMutationEntry();
				if (mutationEntry?.Category == null || !(mutationEntry.Category.Name == "Mental") || mutationEntry.Cost <= 1)
				{
					continue;
				}
				List<MutationEntry> list = new List<MutationEntry>(nonLegendaryCreatureAroundPlayerLevel.GetPart<Mutations>().GetMutatePool());
				list.ShuffleInPlace();
				MutationEntry mutationEntry2 = null;
				foreach (MutationEntry item in list)
				{
					if (item.Category != null && item.Category.Name == "Mental" && !nonLegendaryCreatureAroundPlayerLevel.HasPart(item.Class) && item.Cost > 1)
					{
						mutationEntry2 = item;
						break;
					}
				}
				if (mutationEntry2 != null)
				{
					part3.AddMutation(mutationEntry2.Class, mutation.BaseLevel);
				}
			}
			string text = NameMaker.MakeTitle(nonLegendaryCreatureAroundPlayerLevel, null, null, null, null, null, null, null, null, null, "PsychicHunter", null, SpecialFaildown: true);
			Func<string, string> process = randomElement.Weirdify;
			nonLegendaryCreatureAroundPlayerLevel.GiveProperName(null, Force: false, "PsychicHunter", SpecialFaildown: true, null, null, null, process);
			Titles titles = nonLegendaryCreatureAroundPlayerLevel.RequirePart<Titles>();
			if (!text.IsNullOrEmpty())
			{
				titles.AddTitle(text, -40);
			}
			titles.AddTitle("extradimensional " + nonLegendaryCreatureAroundPlayerLevel.GetCreatureType(), -20);
			titles.AddTitle("esper " + HistoricStringExpander.ExpandString("<spice.commonPhrases.hunter.!random>"), -5);
			nonLegendaryCreatureAroundPlayerLevel.RequirePart<DisplayNameColor>().SetColorAndPriority("O", 30);
			XRL.World.Parts.Temporary.AddHierarchically(nonLegendaryCreatureAroundPlayerLevel);
			nonLegendaryCreatureAroundPlayerLevel.RequirePart<AbsorbablePsyche>();
			nonLegendaryCreatureAroundPlayerLevel.AddPart(new Extradimensional("{{O|" + randomElement.Name.Replace("*dimensionSymbol*", ((char)randomElement.Symbol).ToString()) + "}}", randomElement.WeaponIndex, randomElement.MissileWeaponIndex, randomElement.ArmorIndex, randomElement.ShieldIndex, randomElement.MiscIndex, randomElement.Training, randomElement.SecretID));
			nonLegendaryCreatureAroundPlayerLevel.RequirePart<ExtradimensionalLoot>();
			nonLegendaryCreatureAroundPlayerLevel.AddPart(new RevealObservationOnLook(randomElement.SecretID));
			RemoveParts(nonLegendaryCreatureAroundPlayerLevel);
			ObjectList?.Add(nonLegendaryCreatureAroundPlayerLevel);
			if (Place && PlaceHunter(Z, nonLegendaryCreatureAroundPlayerLevel, null, TeleportSwirl))
			{
				num++;
			}
		}
		if (UseMessage && num > 0)
		{
			PsychicPresenceMessage(num, UsePopup);
		}
	}

	public static void CreateExtradimensionalSoloDeviant(Zone Z)
	{
		ExtraDimension randomElement = (The.Game.GetObjectGameState("DimensionManager") as DimensionManager).ExtraDimensions.GetRandomElement();
		XRL.World.GameObject nonLegendaryCreatureAroundPlayerLevel = EncountersAPI.GetNonLegendaryCreatureAroundPlayerLevel();
		nonLegendaryCreatureAroundPlayerLevel.Render.SetForegroundColor(randomElement.mainColor);
		nonLegendaryCreatureAroundPlayerLevel.Render.DetailColor = "O";
		if (!nonLegendaryCreatureAroundPlayerLevel.HasProperty("PsychicHunter"))
		{
			nonLegendaryCreatureAroundPlayerLevel.SetStringProperty("PsychicHunter", "true");
		}
		nonLegendaryCreatureAroundPlayerLevel.Brain.Hibernating = false;
		nonLegendaryCreatureAroundPlayerLevel.Brain.Aquatic = false;
		nonLegendaryCreatureAroundPlayerLevel.Brain.Mobile = true;
		nonLegendaryCreatureAroundPlayerLevel.Brain.Allegiance.Clear();
		nonLegendaryCreatureAroundPlayerLevel.Brain.Allegiance.Add("Entropic", 100);
		nonLegendaryCreatureAroundPlayerLevel.RequirePart<Combat>();
		ConversationScript part = nonLegendaryCreatureAroundPlayerLevel.GetPart<ConversationScript>();
		if (part != null)
		{
			part.Filter = "Weird";
			part.FilterExtras = randomElement.Name;
		}
		nonLegendaryCreatureAroundPlayerLevel.AwardXP(The.Player.Stat("XP"), -1, 0, int.MaxValue, null, The.Player);
		nonLegendaryCreatureAroundPlayerLevel.GetStat("Ego").BaseValue = The.Player.Stat("Ego");
		if (nonLegendaryCreatureAroundPlayerLevel.HasStat("MP"))
		{
			nonLegendaryCreatureAroundPlayerLevel.GetStat("MP").BaseValue = 0;
		}
		Mutations part2 = The.Player.GetPart<Mutations>();
		Mutations part3 = nonLegendaryCreatureAroundPlayerLevel.GetPart<Mutations>();
		foreach (BaseMutation mutation in part2.MutationList)
		{
			MutationEntry mutationEntry = mutation.GetMutationEntry();
			if (mutationEntry?.Category == null || !(mutationEntry.Category.Name == "Mental") || mutationEntry.Cost <= 1)
			{
				continue;
			}
			List<MutationEntry> list = new List<MutationEntry>(nonLegendaryCreatureAroundPlayerLevel.GetPart<Mutations>().GetMutatePool());
			list.ShuffleInPlace();
			MutationEntry mutationEntry2 = null;
			foreach (MutationEntry item in list)
			{
				if (item.Category != null && item.Category.Name == "Mental" && !nonLegendaryCreatureAroundPlayerLevel.HasPart(item.Class) && item.Cost > 1)
				{
					mutationEntry2 = item;
					break;
				}
			}
			if (mutationEntry2 != null)
			{
				part3.AddMutation(mutationEntry2.Class, mutation.BaseLevel);
			}
		}
		string text = NameMaker.MakeTitle(nonLegendaryCreatureAroundPlayerLevel, null, null, null, null, null, null, null, null, null, "PsychicHunter", null, SpecialFaildown: true);
		Func<string, string> process = randomElement.Weirdify;
		nonLegendaryCreatureAroundPlayerLevel.GiveProperName(null, Force: false, "PsychicHunter", SpecialFaildown: true, null, null, null, process);
		Titles titles = nonLegendaryCreatureAroundPlayerLevel.RequirePart<Titles>();
		if (!text.IsNullOrEmpty())
		{
			titles.AddTitle(text, -40);
		}
		titles.AddTitle(nonLegendaryCreatureAroundPlayerLevel.GetCreatureType(), -20);
		titles.AddTitle("transdimensional " + HistoricStringExpander.ExpandString("<spice.commonPhrases.entropist.!random>"), -5);
		nonLegendaryCreatureAroundPlayerLevel.RequirePart<DisplayNameColor>().SetColorAndPriority("O", 30);
		XRL.World.Parts.Temporary.AddHierarchically(nonLegendaryCreatureAroundPlayerLevel);
		nonLegendaryCreatureAroundPlayerLevel.RequirePart<AbsorbablePsyche>();
		nonLegendaryCreatureAroundPlayerLevel.AddPart(new Extradimensional("{{O|" + randomElement.Name.Replace("*dimensionSymbol*", ((char)randomElement.Symbol).ToString()) + "}}", randomElement.WeaponIndex, randomElement.MissileWeaponIndex, randomElement.ArmorIndex, randomElement.ShieldIndex, randomElement.MiscIndex, randomElement.Training, randomElement.SecretID));
		nonLegendaryCreatureAroundPlayerLevel.RequirePart<ExtradimensionalLoot>();
		nonLegendaryCreatureAroundPlayerLevel.AddPart(new RevealObservationOnLook(randomElement.SecretID));
		RemoveParts(nonLegendaryCreatureAroundPlayerLevel);
		Debug.Log(nonLegendaryCreatureAroundPlayerLevel.DisplayName);
		Debug.Log(nonLegendaryCreatureAroundPlayerLevel.GetPart<Description>().Short);
		PlaceHunter(Z, nonLegendaryCreatureAroundPlayerLevel);
	}

	public static void CreateExtradimensionalCultHunters(Zone Z, int Number = 1, List<XRL.World.GameObject> ObjectList = null, bool Place = true, bool TeleportSwirl = false, bool UseMessage = true, bool UsePopup = true)
	{
		PsychicFaction randomElement = (The.Game.GetObjectGameState("DimensionManager") as DimensionManager).PsychicFactions.GetRandomElement();
		string factionName = randomElement.factionName;
		int num = 0;
		for (int i = 0; i < Number; i++)
		{
			List<GameObjectBlueprint> factionMembers = GameObjectFactory.Factory.GetFactionMembers(factionName);
			factionMembers.ShuffleInPlace();
			GameObjectBlueprint gameObjectBlueprint = null;
			GameObjectBlueprint gameObjectBlueprint2 = null;
			int num2 = int.MaxValue;
			int num3 = The.Player.Stat("Level");
			int num4 = 0;
			foreach (GameObjectBlueprint item in factionMembers)
			{
				if (item.HasStat("Level") && EncountersAPI.IsEligibleForDynamicEncounters(item) && EncountersAPI.IsLegendaryEligible(item))
				{
					int value = item.GetStat("Level").Value;
					if (gameObjectBlueprint == null && value < num3)
					{
						gameObjectBlueprint = item;
						num4 = value;
					}
					else if (value < num3 && value > num4)
					{
						gameObjectBlueprint = item;
						num4 = value;
					}
					else if (value < num3 && value == num4 && 50.in100())
					{
						gameObjectBlueprint = item;
						num4 = value;
					}
					if (gameObjectBlueprint2 == null)
					{
						gameObjectBlueprint2 = item;
						num2 = value;
					}
					else if (value < num2)
					{
						gameObjectBlueprint2 = item;
						num2 = value;
					}
					else if (value == num2 && 50.in100())
					{
						gameObjectBlueprint2 = item;
						num2 = value;
					}
				}
			}
			GameObjectBlueprint gameObjectBlueprint3 = gameObjectBlueprint ?? gameObjectBlueprint2;
			if (gameObjectBlueprint3 == null)
			{
				Debug.Log("No member found from faction:" + factionName);
				return;
			}
			string name = gameObjectBlueprint3.Name;
			string preferredMutation = randomElement.preferredMutation;
			XRL.World.GameObject gameObject = XRL.World.GameObject.Create(name);
			if (gameObject.Stat("Level") - The.Player.Stat("Level") >= 10)
			{
				continue;
			}
			gameObject.Render.SetForegroundColor(randomElement.mainColor);
			gameObject.Render.DetailColor = "O";
			if (!gameObject.HasProperty("PsychicHunter"))
			{
				gameObject.SetStringProperty("PsychicHunter", "true");
			}
			gameObject.Brain.Allegiance.Clear();
			gameObject.Brain.Allegiance.Add("Playerhater", 100);
			gameObject.Brain.Allegiance.Hostile = true;
			gameObject.Brain.Allegiance.Calm = false;
			gameObject.Brain.Hibernating = false;
			gameObject.Brain.Aquatic = false;
			gameObject.Brain.Mobile = true;
			gameObject.Brain.AddOpinion<OpinionPsychicHunt>(The.Player);
			gameObject.RequirePart<Combat>();
			ConversationScript part = gameObject.GetPart<ConversationScript>();
			if (part != null)
			{
				part.Filter = "Weird";
				part.FilterExtras = randomElement.factionName;
			}
			gameObject.AwardXP(The.Player.Stat("XP"), -1, 0, int.MaxValue, null, The.Player);
			gameObject.GetStat("Ego").BaseValue = The.Player.Stat("Ego");
			if (gameObject.HasStat("MP"))
			{
				gameObject.GetStat("MP").BaseValue = 0;
			}
			Mutations part2 = The.Player.GetPart<Mutations>();
			Mutations part3 = gameObject.GetPart<Mutations>();
			bool flag = false;
			if (preferredMutation != "none")
			{
				part3.AddMutation(preferredMutation, gameObject.Stat("Level") / 4);
			}
			else
			{
				flag = true;
			}
			foreach (BaseMutation mutation in part2.MutationList)
			{
				if (!flag)
				{
					flag = true;
					continue;
				}
				MutationEntry mutationEntry = mutation.GetMutationEntry();
				if (mutationEntry?.Category == null || !(mutationEntry.Category.Name == "Mental") || mutationEntry.Cost <= 1)
				{
					continue;
				}
				List<MutationEntry> list = new List<MutationEntry>(gameObject.GetPart<Mutations>().GetMutatePool());
				list.ShuffleInPlace();
				MutationEntry mutationEntry2 = null;
				foreach (MutationEntry item2 in list)
				{
					if (item2.Category != null && item2.Category.Name == "Mental" && !gameObject.HasPart(item2.Class) && item2.Cost > 1)
					{
						mutationEntry2 = item2;
						break;
					}
				}
				if (mutationEntry2 != null)
				{
					part3.AddMutation(mutationEntry2.Class, mutation.BaseLevel);
				}
			}
			string text = NameMaker.MakeTitle(gameObject, null, null, null, null, null, null, null, null, null, "PsychicHunter", null, SpecialFaildown: true);
			Func<string, string> process = randomElement.Weirdify;
			gameObject.GiveProperName(null, Force: false, "PsychicHunter", SpecialFaildown: true, null, null, null, process);
			Titles titles = gameObject.RequirePart<Titles>();
			if (!text.IsNullOrEmpty())
			{
				titles.AddTitle(text, -40);
			}
			titles.AddTitle("extradimensional " + gameObject.GetCreatureType(), -20);
			titles.AddTitle("esper from the " + randomElement.cultForm.Replace("*CultSymbol*", ((char)randomElement.cultSymbol).ToString()), -5);
			gameObject.RequirePart<DisplayNameColor>().SetColorAndPriority("O", 30);
			XRL.World.Parts.Temporary.AddHierarchically(gameObject);
			gameObject.RequirePart<AbsorbablePsyche>();
			gameObject.AddPart(new Extradimensional("{{O|" + randomElement.dimensionName.Replace("*DimensionSymbol*", ((char)randomElement.dimensionSymbol).ToString()) + "}}", randomElement.dimensionalWeaponIndex, randomElement.dimensionalMissileWeaponIndex, randomElement.dimensionalArmorIndex, randomElement.dimensionalShieldIndex, randomElement.dimensionalMiscIndex, randomElement.dimensionalTraining, randomElement.dimensionSecretID));
			gameObject.RequirePart<ExtradimensionalLoot>();
			gameObject.AddPart(new RevealObservationOnLook(randomElement.dimensionSecretID));
			RemoveParts(gameObject);
			ObjectList?.Add(gameObject);
			if (Place && PlaceHunter(Z, gameObject, null, TeleportSwirl))
			{
				num++;
			}
		}
		if (UseMessage && num > 0)
		{
			PsychicPresenceMessage(num, UsePopup);
		}
	}

	public static bool PlaceHunter(Zone Zone, XRL.World.GameObject Hunter, Cell Cell = null, bool TeleportSwirl = false, string TeleportColor = "&C")
	{
		for (int i = 0; i < 100; i++)
		{
			if (Cell != null)
			{
				break;
			}
			int x = Stat.Random(0, Zone.Width - 1);
			int y = Stat.Random(0, Zone.Height - 1);
			Cell cell = Zone.GetCell(x, y);
			if (cell.IsSpawnable() && cell.GetNavigationWeightFor(Hunter) < 30)
			{
				Cell = cell;
			}
		}
		if (Cell == null)
		{
			List<Cell> passableCells = Zone.GetPassableCells(Hunter);
			if (passableCells.IsNullOrEmpty())
			{
				return false;
			}
			Cell = passableCells.GetRandomElement();
		}
		Cell.AddObject(Hunter);
		Hunter.MakeActive();
		Hunter.Brain.PushGoal(new Kill(The.Player));
		if (TeleportSwirl)
		{
			Hunter.SmallTeleportSwirl(Cell, TeleportColor, Voluntary: true);
		}
		return true;
	}

	public static void PsychicPresenceMessage(int Number = 1, bool UsePopup = true)
	{
		string text = ((Number > 1) ? "psychic presences" : "a psychic presence");
		Messaging.XDidY(The.Player, "sense", text + " foreign to this place and time", null, null, "c", null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup);
	}

	public static int GetNumPsychicHunters(int glimmer)
	{
		int num = Stat.Random(1, 1000);
		int num2 = 0;
		if (glimmer < 20)
		{
			return 0;
		}
		if (glimmer >= 20 && glimmer <= 49)
		{
			if (num >= (glimmer - 20 + 15) * 2)
			{
				return 0;
			}
			num2++;
		}
		else
		{
			if (!((double)num < (double)(glimmer - 40 + 105) * 0.666))
			{
				return 0;
			}
			num2++;
		}
		if (glimmer >= 35 && num2 > 0)
		{
			if (Stat.Random(1, 1000) >= (glimmer - 20) * 5)
			{
				return num2;
			}
			num2++;
		}
		if (glimmer >= 50)
		{
			if (Stat.Random(1, 1000) >= (glimmer - 20) * 5)
			{
				return num2;
			}
			num2++;
			if (Stat.Random(1, 1000) >= (glimmer - 20) * 5)
			{
				return num2;
			}
			num2++;
			if (glimmer >= 80)
			{
				if (Stat.Random(1, 1000) >= (glimmer - 50) * 5)
				{
					return num2;
				}
				num2++;
			}
		}
		return num2;
	}

	public static string GetThrallRoll(int glimmer)
	{
		string text = "0";
		if (glimmer <= DimensionManager.GLIMMER_FLOOR + 15)
		{
			return "0-1";
		}
		if (glimmer <= DimensionManager.GLIMMER_FLOOR + 30)
		{
			return "0-2";
		}
		if (glimmer <= DimensionManager.GLIMMER_FLOOR + 45)
		{
			return "1-2";
		}
		if (glimmer <= DimensionManager.GLIMMER_FLOOR + 60)
		{
			return "1-3";
		}
		if (glimmer <= DimensionManager.GLIMMER_FLOOR + 75)
		{
			return "2-3";
		}
		if (glimmer <= DimensionManager.GLIMMER_FLOOR + 90)
		{
			return "2-4";
		}
		return "3-4";
	}

	public static string GetPsychicGlimmerDescription(int glimmer)
	{
		string text = "";
		string text2;
		if (glimmer <= DimensionManager.GLIMMER_FLOOR + 15)
		{
			text2 = "Currently, you are being watched and pursued by ospreys, Ptoh's servants and birds of psychic prey who pluck larval espers from their egg sacs.";
		}
		else if (glimmer <= DimensionManager.GLIMMER_FLOOR + 30)
		{
			text2 = "Currently, you are being watched and pursued by harriers, Ptoh's servants and birds of psychic prey who pluck fledgling espers from the shallows.";
		}
		else if (glimmer <= DimensionManager.GLIMMER_FLOOR + 45)
		{
			text2 = "Currently, you are being watched and pursued by owls, Ptoh's servants and birds of psychic prey who snatch espers from the nighted weald.";
		}
		else if (glimmer <= DimensionManager.GLIMMER_FLOOR + 60)
		{
			text2 = "Currently, you are being watched and pursued by condors, Ptoh's servants and birds of psychic prey who snatch thriving espers from the vast wood.";
		}
		else if (glimmer <= DimensionManager.GLIMMER_FLOOR + 75)
		{
			text2 = "Currently, you are being watched and pursued by strixes, Ptoh's servants and birds of psychic prey who drink the blood of mature espers.";
		}
		else
		{
			text2 = ((glimmer > DimensionManager.GLIMMER_FLOOR + 90) ? "Currently, you are being watched and pursued by rukhs, Ptoh's most powerful servants and birds of psychic prey who seize masterful espers from their belfries." : "Currently, you are being watched and pursued by eagles, Ptoh's servants and birds of psychic prey who seize powerful espers from their roosts.");
		}
		if (glimmer >= DimensionManager.GLIMMER_EXTRADIMENSIONAL_FLOOR)
		{
			text = "\n\nYou are also visible to psychic beings from other dimensions.";
		}
		return "Your psychic glimmer represents how noticeable you are in the vast psychic aether. As your mental mutations increase in level, so does your psychic glimmer and the frequency, strength, and number of those who desire to absorb your mind." + "\n\n" + text2 + text;
	}

	public void CheckPsychicHunters(Zone Z)
	{
		if (Z.IsWorldMap() || Visited.ContainsKey(Z.ZoneID))
		{
			return;
		}
		Visited.Add(Z.ZoneID, value: true);
		if (!The.ZoneManager.TryGetZoneProperty<int>(Z.ZoneID, "AmbushChance", out var Value))
		{
			Value = Z.ResolveWorldBlueprint()?.PsychicHunterChance ?? 100;
		}
		if (!Value.in100())
		{
			return;
		}
		AmbientStabilization part = Z.GetPart<AmbientStabilization>();
		int numPsychicHunters = GetNumPsychicHunters(The.Player.GetPsychicGlimmer());
		if (numPsychicHunters > 0)
		{
			if (part != null && (part.Strength + 50).in100())
			{
				Messaging.EmitMessage(The.Player, "Some dimensional interlopers attempt to enter this region of spacetime, but the ambient normality field keeps them at bay.");
			}
			else
			{
				CreatePsychicHunter(numPsychicHunters, Z);
			}
		}
	}

	public static void RemoveParts(XRL.World.GameObject Object)
	{
		Object.RemovePart(typeof(MentalShield));
		Object.RemovePart(typeof(DroidScramblerWeakness));
		Object.RemovePart(typeof(DromadCaravan));
		Object.RemovePart(typeof(SecretObject));
	}

	[WishCommand("seekerhunter", null)]
	public static void SeekerWish()
	{
		CreateSeekerHunters(1, The.ActiveZone);
	}

	[WishCommand("seekerhunter", null)]
	public static void SeekerWish(string Number)
	{
		if (int.TryParse(Number, out var result))
		{
			CreateSeekerHunters(result, The.ActiveZone);
		}
	}

	[WishCommand("extraculthunter", null)]
	public static void CultWish()
	{
		CreateExtradimensionalCultHunters(The.ActiveZone, 1, null, Place: true, TeleportSwirl: true);
	}

	[WishCommand("extraculthunter", null)]
	public static void CultWish(string Number)
	{
		if (int.TryParse(Number, out var result))
		{
			CreateExtradimensionalCultHunters(The.ActiveZone, result, null, Place: true, TeleportSwirl: true);
		}
	}

	[WishCommand("extrasolohunter", null)]
	public static void SoloWish()
	{
		CreateExtradimensionalSoloHunters(The.ActiveZone, 1, null, Place: true, TeleportSwirl: true);
	}

	[WishCommand("extrasolohunter", null)]
	public static void SoloWish(string Number)
	{
		if (int.TryParse(Number, out var result))
		{
			CreateExtradimensionalSoloHunters(The.ActiveZone, result, null, Place: true, TeleportSwirl: true);
		}
	}
}
