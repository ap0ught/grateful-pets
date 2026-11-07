using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genkit;
using HistoryKit;
using Qud.API;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.Capabilities;
using XRL.World.Conversations;
using XRL.World.Parts;

namespace XRL.World.Quests;

[Serializable]
[HasWishCommand]
public class LandingPadsSystem : IQuestSystem
{
	private enum STAGE
	{
		unsettled = 0,
		decided = 50,
		settled = 100
	}

	public static readonly string QUEST_NAME = "Landing Pads";

	public static readonly int REQUIRED_CANDIDATES = 3;

	public int stage;

	public List<string> candidateFactions = new List<string>();

	public List<string> candidateFactionZones = new List<string>();

	public string hydroponZone;

	public string settledZone;

	public string settledFaction;

	public int SlynthLeaveDays = -1;

	public int SlynthArriveDays = -1;

	public int SlynthSettleDays = -1;

	public int CandidateCount = -1;

	public bool visited;

	public override bool RemoveWithQuest => false;

	public void updateQuestStatus()
	{
		if (stage != 0)
		{
			return;
		}
		Quest quest = getQuest();
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("The following factions have agreed to provide sanctuary for the slynth.\n");
		if (candidateFactions.Count > 0)
		{
			stringBuilder.Append('\n');
		}
		foreach (string candidateFaction in candidateFactions)
		{
			if (The.Game.PlayerReputation.GetLevel(candidateFaction) >= 2)
			{
				stringBuilder.Compound("{{green|û}} {{white|" + Faction.GetFormattedName(candidateFaction) + "}}", '\n');
			}
			else
			{
				stringBuilder.Compound("{{red|X}} {{K|" + Faction.GetFormattedName(candidateFaction) + " [reputation too low]}}", '\n');
			}
		}
		quest.StepsByID["Sanctuary Candidates"].Text = stringBuilder.ToString();
		if (candidateFactionsCount() >= REQUIRED_CANDIDATES)
		{
			The.Game.FinishQuestStep(QUEST_NAME, "Consult Settlements");
			return;
		}
		QuestStep questStep = The.Game.Quests[QUEST_NAME].StepsByID["Consult Settlements"];
		if (questStep.Finished)
		{
			questStep.XP = 0;
			questStep.Finished = false;
		}
	}

	public Quest getQuest()
	{
		if (!The.Game.Quests.ContainsKey(QUEST_NAME))
		{
			return null;
		}
		return The.Game.Quests[QUEST_NAME];
	}

	public override void OnAdded()
	{
		hydroponZone = The.Game.GetStringGameState("HydroponZoneID", null);
	}

	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(PooledEvent<AfterReputationChangeEvent>.ID);
		Registrar.Register(ZoneActivatedEvent.ID);
		Registrar.Register(PooledEvent<GenericCommandEvent>.ID);
	}

	public override bool HandleEvent(AfterReputationChangeEvent E)
	{
		if (stage == 0)
		{
			updateQuestStatus();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (SlynthLeaveDays >= 0 && E.Zone.ZoneID == hydroponZone)
		{
			HydroponActivated(E.Zone);
		}
		if (SlynthSettleDays >= 0 && E.Zone.ZoneID == settledZone)
		{
			SettlementActivated(E.Zone);
		}
		if (SlynthLeaveDays == -1 && SlynthArriveDays == -1 && SlynthSettleDays == -1 && The.Game.HasFinishedQuest(QUEST_NAME))
		{
			The.Game.FlagSystemForRemoval(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GenericCommandEvent E)
	{
		if (E.Command == "AfterChavvahLocationSet")
		{
			string[] array = (string[])E.Object;
			if (settledFaction.IsNullOrEmpty())
			{
				int num = candidateFactions.IndexOf("Chavvah");
				if (num >= 0)
				{
					candidateFactionZones[num] = array[1];
				}
			}
			else if (settledFaction == "Chavvah")
			{
				The.Game.SetStringGameState("SlynthSettlementZone", settledZone = array[1]);
			}
		}
		return base.HandleEvent(E);
	}

	public int candidateFactionsCount()
	{
		return Enumerable.Count(candidateFactions, (string f) => The.Game.PlayerReputation.Get(f) >= RuleSettings.REPUTATION_LOVED);
	}

	public override void Finish()
	{
		RollResult();
		stage = 100;
		SlynthLeaveDays = 1;
		SlynthArriveDays = Stat.Random(5, 7);
		SlynthSettleDays = SlynthArriveDays + 7;
		CandidateCount = candidateFactionsCount();
		Quest.SetProperty("Faction", settledFaction);
		Quest.SetProperty("Zone", settledZone);
		Quest.SetProperty("Count", CandidateCount);
		string zoneDisplayName = The.ZoneManager.GetZoneDisplayName(settledZone, WithIndefiniteArticle: false, WithDefiniteArticle: true, WithStratum: false, Mutate: false);
		JournalAPI.AddAccomplishment("You found a new home for a young people.", "Inwith Maqqom Yd, =name= created a new people from lilypads and coral-stuff, and sent them to live throughout wider Qud.", "Acting against labor laws restricting the rights of newly sentient beings, =name= trekked to the reef-skirted pad of the Hydropon and led the plant people there to their new home, " + zoneDisplayName + ".", null, "general", MuralCategory.BecomesLoved, MuralWeight.Medium, null, -1L);
		The.Game.PlayerReputation.Modify(settledFaction, -600, "Quest");
	}

	public void RollResult()
	{
		if (stage < 50)
		{
			stage = 50;
		}
		if (settledFaction == null)
		{
			int index;
			do
			{
				index = Stat.Rand.Next(0, candidateFactions.Count);
			}
			while (The.Game.PlayerReputation.Get(candidateFactions[index]) < RuleSettings.REPUTATION_LOVED);
			The.Game.SetStringGameState("SlynthSettlementFaction", settledFaction = candidateFactions[index]);
			The.Game.SetStringGameState("SlynthSettlementZone", settledZone = candidateFactionZones[index]);
		}
	}

	public long GetQuestFinishDays()
	{
		long questFinishTime = The.Game.GetQuestFinishTime(QUEST_NAME);
		if (questFinishTime < 0)
		{
			return -1L;
		}
		return (The.Game.TimeTicks - questFinishTime) / 1200;
	}

	public void HydroponActivated(Zone Z)
	{
		if (SlynthLeaveDays < 0 || GetQuestFinishDays() < SlynthLeaveDays || The.Game.HasIntGameState("LandingPadsSlynthLeft"))
		{
			return;
		}
		SlynthLeaveDays = -1;
		The.Game.SetIntGameState("LandingPadsSlynthLeft", 1);
		List<GameObject> objects = Z.GetObjects("BaseSlynth");
		int num = Stat.Random(8, 10);
		using List<GameObject>.Enumerator enumerator = objects.GetEnumerator();
		while (enumerator.MoveNext() && (!enumerator.Current.Destroy(null, Silent: true) || --num > 0))
		{
		}
	}

	public void GetSlynthCells(Zone Z, out List<Cell> Cells, out int Amount, out bool Wanders)
	{
		if (settledFaction == "Pariahs")
		{
			Cells = Z.GetEmptyReachableCells(new Rect2D(43, 18, 46, 22));
			Amount = 2;
			Wanders = false;
		}
		else if (settledFaction == "Mechanimists")
		{
			Cells = new List<Cell>(Stat.Random(5, 9));
			Wanders = true;
			for (int i = 0; i < Cells.Capacity; i++)
			{
				int num = Stat.Random(0, Directions.DirectionList.Length);
				string direction = ".";
				if (num < Directions.DirectionList.Length)
				{
					direction = Directions.DirectionList[num];
				}
				string zoneFromIDAndDirection = The.ZoneManager.GetZoneFromIDAndDirection(Z.ZoneID, direction);
				Zone zone = The.ZoneManager.GetZone(zoneFromIDAndDirection);
				for (int j = 0; j < 100; j++)
				{
					Cell randomCell = zone.GetRandomCell(1);
					if (randomCell.IsReachable() && randomCell.IsPassable() && !Cells.Contains(randomCell))
					{
						Cells.Add(randomCell);
						break;
					}
				}
			}
			Amount = Cells.Count;
		}
		else if (settledFaction == "YdFreehold")
		{
			int z = ((Z.Z == 10) ? 11 : 10);
			Cells = The.ZoneManager.GetZone(Z.ZoneWorld, Z.wX, Z.wY, Z.X, Z.Y, z).GetEmptyReachableCells();
			Cells.AddRange(Z.GetEmptyReachableCells());
			Amount = Math.Min(Stat.Random(5, 9), Cells.Count);
			Wanders = true;
		}
		else if (settledFaction == "Mopango")
		{
			Cells = Z.GetCells((Cell C) => C.IsEmpty() && C.IsReachable() && C.HasObject("MopangoHideoutTile"));
			Amount = Math.Min(Stat.Random(5, 9), Cells.Count);
			Wanders = true;
			if (Cells.Count < Amount)
			{
				Cells.AddRange(Z.GetEmptyReachableCells());
			}
		}
		else if (settledFaction == "Chavvah")
		{
			Cells = new List<Cell>(Stat.Random(5, 9));
			Wanders = true;
			string[] array = new string[3];
			string zoneID = The.Game.GetSystem<ChavvahSystem>()?.TrunkID ?? Z.ZoneID;
			for (int num2 = 0; num2 < 3; num2++)
			{
				zoneID = (array[num2] = The.ZoneManager.GetZoneFromIDAndDirection(zoneID, "U"));
			}
			for (int num3 = 0; num3 < Cells.Capacity; num3++)
			{
				string randomElement = array.GetRandomElement();
				Zone zone2 = The.ZoneManager.GetZone(randomElement);
				for (int num4 = 0; num4 < 100; num4++)
				{
					Cell randomCell2 = zone2.GetRandomCell(1);
					if (randomCell2.IsReachable() && randomCell2.IsPassable() && !Cells.Contains(randomCell2))
					{
						Cells.Add(randomCell2);
						break;
					}
				}
			}
			Amount = Cells.Count;
		}
		else
		{
			Cells = Z.GetEmptyReachableCells();
			Amount = Math.Min(Stat.Random(5, 9), Cells.Count);
			Wanders = true;
		}
	}

	public void SettlementActivated(Zone Z)
	{
		if (SlynthArriveDays >= 0 && GetQuestFinishDays() >= SlynthArriveDays && !The.Game.HasIntGameState("LandingPadsSlynthArrived"))
		{
			SlynthArriveDays = -1;
			The.Game.SetIntGameState("LandingPadsSlynthArrived", 1);
			GetSlynthCells(Z, out var Cells, out var Amount, out var Wanders);
			string factions = settledFaction + "-100";
			for (int i = 0; i < Amount; i++)
			{
				Cell randomElement = Cells.GetRandomElement();
				GameObject gameObject = randomElement?.AddObject("BaseSlynth");
				if (gameObject != null)
				{
					gameObject.RequirePart<ConversationScript>().ConversationID = "SlynthSettler";
					gameObject.Brain.Wanders = Wanders;
					gameObject.Brain.Factions = factions;
					gameObject.Brain.Allegiance.Calm = true;
					gameObject.MakeActive();
					Cells.Remove(randomElement);
				}
			}
		}
		if (SlynthSettleDays >= 0 && GetQuestFinishDays() >= SlynthSettleDays && !The.Game.HasIntGameState("LandingPadsSlynthSettled"))
		{
			SlynthSettleDays = -1;
			The.Game.SetIntGameState("LandingPadsSlynthSettled", 1);
		}
	}

	public static void AddDynamicVillageConversation(GameObject Speaker)
	{
		if (Speaker == null)
		{
			return;
		}
		ConversationXMLBlueprint conversationXMLBlueprint = Speaker.GetPart<ConversationScript>()?.Blueprint;
		if (conversationXMLBlueprint == null)
		{
			MetricsManager.LogWarning(Speaker.DebugName + " has no existing conversation blueprint.");
			return;
		}
		if (!Conversation.Blueprints.TryGetValue("DynamicVillageMayor", out var value))
		{
			MetricsManager.LogWarning("DynamicVillageMayor blueprint cannot be found.");
			return;
		}
		foreach (ConversationXMLBlueprint child in value.Children)
		{
			if (child.ID.StartsWith("Slynth") || child.Name == "Part")
			{
				conversationXMLBlueprint.AddChild(child);
			}
		}
		foreach (ConversationXMLBlueprint child2 in value.GetChild("Welcome").Children)
		{
			if (child2.ID.StartsWith("Slynth"))
			{
				ConversationsAPI.DistributeChoice(conversationXMLBlueprint, "Start", child2);
			}
		}
	}

	public static void RevealHydropon()
	{
		JournalAPI.RevealMapNote(JournalAPI.GetMapNote("$hydropon"));
	}

	public static void SlynthQuestWish(Faction Faction = null, bool Complete = false)
	{
		The.Game.StartQuest(QUEST_NAME);
		LandingPadsSystem system = The.Game.GetSystem<LandingPadsSystem>();
		if (system.candidateFactions.Count < REQUIRED_CANDIDATES && The.Game.HasUnfinishedQuest(QUEST_NAME))
		{
			SlynthQuestCandidates();
		}
		if (Faction != null)
		{
			int num = system.candidateFactions.IndexOf(Faction.Name);
			The.Game.SetStringGameState("SlynthSettlementFaction", system.settledFaction = ((num >= 0) ? system.candidateFactions[num] : null));
			The.Game.SetStringGameState("SlynthSettlementZone", system.settledZone = ((num >= 0) ? system.candidateFactionZones[num] : null));
		}
		if (Complete || Faction != null)
		{
			The.Game.CompleteQuest(QUEST_NAME);
		}
	}

	public static void SlynthQuestCandidates()
	{
		LandingPadsSystem system = The.Game.GetSystem<LandingPadsSystem>();
		system.candidateFactions.Clear();
		system.candidateFactionZones.Clear();
		system.candidateFactions.Add("Joppa");
		system.candidateFactionZones.Add("JoppaWorld.11.22.1.1.10");
		system.candidateFactions.Add("Mechanimists");
		system.candidateFactionZones.Add("JoppaWorld.5.2.1.1.10");
		system.candidateFactions.Add("Barathrumites");
		system.candidateFactionZones.Add("JoppaWorld.22.14.1.0.13");
		system.candidateFactions.Add("Kyakukya");
		system.candidateFactionZones.Add("JoppaWorld.27.20.1.1.10");
		system.candidateFactions.Add("Ezra");
		system.candidateFactionZones.Add("JoppaWorld.53.4.0.0.10");
		system.candidateFactions.Add("Mopango");
		system.candidateFactionZones.Add("JoppaWorld.53.3.0.0.11");
		system.candidateFactions.Add("Pariahs");
		system.candidateFactionZones.Add("JoppaWorld.5.2.1.2.10");
		system.candidateFactions.Add("YdFreehold");
		system.candidateFactionZones.Add("JoppaWorld.67.17.1.1.10");
		system.candidateFactions.Add("Hindren");
		system.candidateFactionZones.Add(The.Game.GetStringGameState("BeyLahZoneID"));
		system.candidateFactions.Add("Chavvah");
		system.candidateFactionZones.Add(The.Game.GetSystem<ChavvahSystem>().TrunkID);
		Zone zone = The.ZoneManager.GetZone("JoppaWorld");
		List<VillageTerrain> list = new List<VillageTerrain>();
		for (int i = 0; i < zone.Height; i++)
		{
			for (int j = 0; j < zone.Width; j++)
			{
				GameObject firstObjectWithPart = zone.GetCell(j, i).GetFirstObjectWithPart("VillageTerrain");
				if (firstObjectWithPart != null)
				{
					list.Add(firstObjectWithPart.GetPart<VillageTerrain>());
				}
			}
		}
		list.ShuffleInPlace();
		for (int k = 0; k < 3; k++)
		{
			VillageTerrain villageTerrain = list[k];
			Cell currentCell = villageTerrain.ParentObject.CurrentCell;
			HistoricEntitySnapshot currentSnapshot = villageTerrain.Village.GetCurrentSnapshot();
			villageTerrain.FireEvent(Event.New("VillageReveal"));
			system.candidateFactions.Add("villagers of " + currentSnapshot.Name);
			system.candidateFactionZones.Add(ZoneID.Assemble("JoppaWorld", currentCell.X, currentCell.Y, 1, 1, 10));
		}
		int num = -The.Player.GetIntProperty("AllVisibleRepModifier");
		foreach (string candidateFaction in system.candidateFactions)
		{
			The.Game.PlayerReputation.Set(candidateFaction, RuleSettings.REPUTATION_LOVED + num);
		}
		system.updateQuestStatus();
	}

	[WishCommand("slynthquest", null)]
	public static void SlynthQuestWish(string Value)
	{
		Popup.Suppress = true;
		ItemNaming.Suppress = true;
		SlynthQuestWish();
		LandingPadsSystem system = The.Game.GetSystem<LandingPadsSystem>();
		if (!Value.Contains("start"))
		{
			if (Value.Contains("complete"))
			{
				SlynthQuestWish(null, Complete: true);
			}
			else if (Value.Contains("reset"))
			{
				ResetSlynthQuest(system);
			}
			else if (Value.Contains("leave"))
			{
				SlynthQuestWish(null, Complete: true);
				system.SlynthLeaveDays = 0;
				if (The.Player.InZone(The.Game.GetStringGameState("HydroponZoneID")))
				{
					system.HydroponActivated(The.ActiveZone);
				}
				else
				{
					The.Player.ZoneTeleport(The.Game.GetStringGameState("HydroponZoneID"));
				}
			}
			else if (Value.Contains("arrive"))
			{
				SlynthQuestWish(null, Complete: true);
				system.SlynthLeaveDays = 0;
				system.SlynthArriveDays = 0;
				if (The.Player.InZone(system.settledZone))
				{
					system.SettlementActivated(The.ActiveZone);
				}
				else
				{
					The.Player.ZoneTeleport(system.settledZone);
				}
			}
			else if (Value.Contains("settle"))
			{
				SlynthQuestWish(null, Complete: true);
				system.SlynthLeaveDays = 0;
				system.SlynthArriveDays = 0;
				system.SlynthSettleDays = 0;
				if (The.Player.InZone(system.settledZone))
				{
					system.SettlementActivated(The.ActiveZone);
				}
				else
				{
					The.Player.ZoneTeleport(system.settledZone);
				}
			}
			else
			{
				Faction faction = Factions.Loop().FirstOrDefault((Faction f) => f.Name.EqualsNoCase(Value));
				if (faction == null)
				{
					MessageQueue.AddPlayerMessage("No faction found by that name.");
					return;
				}
				SlynthQuestWish(faction);
			}
		}
		Popup.Suppress = false;
		ItemNaming.Suppress = false;
	}

	public static void ResetSlynthQuest(LandingPadsSystem System)
	{
		The.Game.Quests.Remove(QUEST_NAME);
		The.Game.FinishedQuests.Remove(QUEST_NAME);
		The.Game.RemoveInt64GameState("QuestFinishedTime_" + QUEST_NAME);
		System.candidateFactions.Clear();
		System.candidateFactionZones.Clear();
		System.stage = 0;
		System.settledZone = null;
		System.settledFaction = null;
		System.SlynthLeaveDays = -1;
		System.SlynthArriveDays = -1;
		System.SlynthSettleDays = -1;
		if (!The.Player.InZone(System.hydroponZone))
		{
			The.Player.ZoneTeleport(System.hydroponZone);
		}
		Zone activeZone = The.ActiveZone;
		List<Cell> reachableCells = activeZone.GetReachableCells();
		reachableCells.ShuffleInPlace();
		int i = activeZone.CountObjects("BaseSlynth");
		int num = 0;
		for (; i <= 14; i++)
		{
			reachableCells[num++].AddObject("BaseSlynth").MakeActive();
		}
	}
}
