using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.Rules;
using XRL.World.Conversations;
using XRL.World.Parts;
using XRL.World.WorldBuilders;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class FindASpecificSiteDynamicQuestTemplate_FabricateQuestGiver : ZoneBuilderSandbox
{
	private string typeOfDirections;

	private JournalMapNote targetLocation;

	private JournalMapNote landmarkLocation;

	private string direction = "";

	private string path = "";

	private int min = 12;

	private int max = 18;

	public string deliveryItemID;

	public Func<GameObject, bool> questGiverFilter;

	public VillageDynamicQuestContext questContext;

	public string sanctityOfSacredThing;

	public QuestStoryType_FindASite QST;

	public void addQuestConversationToGiver(GameObject go, Quest quest)
	{
		go.SetStringProperty("GivesDynamicQuest", quest.ID);
		go.RequirePart<Interesting>();
		ConversationXMLBlueprint conversationXMLBlueprint = go.RequirePart<ConversationScript>().Blueprint;
		if (conversationXMLBlueprint == null)
		{
			conversationXMLBlueprint = ConversationsAPI.AddConversation(go, null, null, null, ClearLost: true);
		}
		go.SetIntProperty("QuestGiver", 1);
		if (go.Brain != null)
		{
			go.Brain.Wanders = false;
			go.Brain.WandersRandomly = false;
		}
		conversationXMLBlueprint.ID = Guid.NewGuid().ToString();
		string text = ColorUtility.StripFormatting(targetLocation.Text);
		if (text.StartsWith("the snapjaw who wields") || text.StartsWith("a secluded merchant"))
		{
			text = "the location of " + text;
		}
		string text2 = "";
		string text3 = HistoricStringExpander.ExpandString("<spice.quests.FindaSite.travelers.cameToOurVillage.!random.capitalize>");
		string text4 = HistoricStringExpander.ExpandString("<spice.quests.FindaSite.records.intro.!random.capitalize>").Replace("*siteInitLower*", Grammar.InitLowerIfArticle(text));
		Regex regex = new Regex("\\*.*\\*");
		Match match = Regex.Match(text3, "(?<=\\*)(.*?)(?=\\*)");
		if (match.Success)
		{
			text3 = regex.Replace(text3, Grammar.Pluralize(match.Value));
		}
		match = Regex.Match(text4, "(?<=\\*)(.*?)(?=\\*)");
		if (match.Success)
		{
			text4 = regex.Replace(text4, Grammar.Pluralize(match.Value));
		}
		text2 = Grammar.ConvertAtoAn((QST switch
		{
			QuestStoryType_FindASite.Travelers => string.Format("{0} {1}. {2}. But they wouldn't reveal the location. {3} {4}. {5}.\n\n{6}.", HistoricStringExpander.ExpandString("<spice.quests.FindaSite.travelers.intro.!random.capitalize>"), text3, Grammar.ConvertAtoAn(HistoricStringExpander.ExpandString("<spice.quests.FindaSite.travelers.spokeOfPlace.!random.capitalize>")).Replace("*GuestActivity*", go.GetxTag_CommaDelimited("TextFragments", "GuestActivity", "breaking bread")), HistoricStringExpander.ExpandString("<spice.quests.FindaSite.travelers.wouldYouFindIt.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.FindaSite.travelers.greatBoon.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.FindaSite.travelers.rewardYou.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.FindaSite.typeOfDirections." + typeOfDirections + ".intro.!random.capitalize>")), 
			QuestStoryType_FindASite.Records => string.Format("{0}. {1}? {2}? {3}? {4}? {5}. {6}. {7} {8}.", text4, HistoricStringExpander.ExpandString("<spice.quests.FindaSite.records.whatTreasures.!random.capitalize>"), Grammar.InitCap(go.GetxTag_CommaDelimited("TextFragments", "ValuedOre", "precious metals")), Grammar.InitialCap(go.GetxTag_CommaDelimited("TextFragments", "ArableLand", "arable land")), HistoricStringExpander.ExpandString("A <spice.commonPhrases.shrine.!random> to ") + questContext.getSacredThings().GetRandomElement(), "We must know", HistoricStringExpander.ExpandString("<spice.quests.FindaSite.records.ifYouFindIt.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.willYouDoIt.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.FindaSite.typeOfDirections." + typeOfDirections + ".intro.!random.capitalize>")), 
			_ => "ERROR: Failed to generate quest text from quest story type.", 
		}).Replace("*site*", text).Replace("*landmark*", Grammar.InitLowerIfArticle(landmarkLocation.Text)).Replace("*direction*", direction)
			.Replace("*min*", Math.Max(1, min / 3).ToString())
			.Replace("*max*", (max / 3).ToString())
			.Replace("*path*", path));
		ConversationXMLBlueprint questIntroNode = ConversationsAPI.AddNode(conversationXMLBlueprint, null, text2);
		ConversationXMLBlueprint conversationXMLBlueprint2 = DynamicQuestConversationHelper.fabricateIntroAcceptChoice("Yes. I will locate " + targetLocation.Text + "&G as you ask.", questIntroNode, quest);
		conversationXMLBlueprint2["IfNotHaveMapNote"] = targetLocation.ID;
		conversationXMLBlueprint2["RevealMapNote"] = landmarkLocation.ID;
		DynamicQuestConversationHelper.fabricateIntroRejectChoice("No, I will not.", questIntroNode);
		ConversationXMLBlueprint conversationXMLBlueprint3 = DynamicQuestConversationHelper.fabricateIntroAdditionalChoice("I already know where " + targetLocation.Text + " is.", questIntroNode);
		conversationXMLBlueprint3["StartQuest"] = quest.ID;
		conversationXMLBlueprint3["IfNotHaveQuest"] = quest.ID;
		conversationXMLBlueprint3["IfHaveMapNote"] = targetLocation.ID;
		conversationXMLBlueprint3["RevealMapNote"] = landmarkLocation.ID;
		ConversationXMLBlueprint conversationXMLBlueprint4 = ConversationsAPI.AddPart(conversationXMLBlueprint3, "QuestHandler");
		conversationXMLBlueprint4["QuestID"] = quest.ID;
		conversationXMLBlueprint4["StepID"] = "a_locate~b_return";
		conversationXMLBlueprint4["Action"] = "Step";
		DynamicQuestConversationHelper.appendQuestCompletionSequence(conversationXMLBlueprint, quest, questIntroNode, "I've located " + targetLocation.Text + ".", "I haven't located " + targetLocation.Text + " yet.", (ConversationXMLBlueprint questIntroChoiceFinalizer) =>
		{
		}, (ConversationXMLBlueprint gotoAcceptNodeFinalizer) =>
		{
			gotoAcceptNodeFinalizer["IfNotFinishedQuest"] = quest.ID;
			gotoAcceptNodeFinalizer["IfFinishedQuestStep"] = quest.ID + "~a_locate";
		}, (ConversationXMLBlueprint incompleteNodeFinalizer) =>
		{
			incompleteNodeFinalizer["IfNotFinishedQuest"] = quest.ID;
			incompleteNodeFinalizer["IfNotFinishedQuestStep"] = quest.ID + "~a_locate";
		}, (ConversationXMLBlueprint completeNodeFinalizer) =>
		{
			completeNodeFinalizer["CompleteQuestStep"] = quest.ID + "~b_return";
		}, (ConversationXMLBlueprint startNodeFinalizer) =>
		{
		});
	}

	public Quest fabricateFindASpecificSiteQuest(GameObject giver)
	{
		typeOfDirections = "path";
		Quest quest = QuestsAPI.fabricateEmptyQuest();
		int num = Stat.Random(0, 12);
		min = 12;
		max = 18;
		targetLocation = null;
		int broaden = 0;
		while (targetLocation == null && broaden <= 240)
		{
			targetLocation = JournalAPI.GetUnrevealedMapNotesWithinZoneRadiusN(zone, min - broaden, max + broaden, questContext.IsValidQuestDestination).GetRandomElement();
			broaden++;
		}
		if (targetLocation == null)
		{
			targetLocation = JournalAPI.GetMapNotesWithinRadiusN(zone, min + broaden).GetRandomElement();
		}
		while (true)
		{
			broaden = 0;
			if (typeOfDirections == "radius")
			{
				min = 0;
				max = 1;
				landmarkLocation = null;
				landmarkLocation = JournalAPI.GetUnrevealedMapNotesWithinZoneRadiusN(targetLocation.ZoneID, min - broaden, max + broaden, questContext.IsValidQuestDestination).GetRandomElement();
				if (landmarkLocation == null)
				{
					typeOfDirections = "direction";
					continue;
				}
			}
			if (typeOfDirections == "radius_Failsafe")
			{
				min = 0;
				max = 2;
				landmarkLocation = null;
				broaden = 0;
				while (landmarkLocation == null && broaden <= 240)
				{
					landmarkLocation = JournalAPI.GetUnrevealedMapNotesWithinZoneRadiusN(targetLocation.ZoneID, min - broaden, max + broaden, questContext.IsValidQuestDestination).GetRandomElement();
					broaden++;
				}
				break;
			}
			if (typeOfDirections == "direction")
			{
				List<JournalMapNote> mapNotesInCardinalDirections = JournalAPI.GetMapNotesInCardinalDirections(targetLocation.ZoneID);
				min = 12;
				max = 18;
				for (broaden = 0; broaden <= 240; broaden += 3)
				{
					List<JournalMapNote> list = mapNotesInCardinalDirections.FindAll((JournalMapNote l) => l.ResolvedLocation.Distance(targetLocation.ResolvedLocation) >= min - broaden && l.ResolvedLocation.Distance(targetLocation.ResolvedLocation) <= max + broaden && questContext.IsValidQuestDestination(targetLocation.ResolvedLocation) && Math.Abs(l.ZoneZ - targetLocation.ZoneZ) == 0);
					if (list.Count > 0)
					{
						landmarkLocation = list.GetRandomElement();
						break;
					}
				}
				if (landmarkLocation != null)
				{
					min = Math.Max(1, landmarkLocation.ResolvedLocation.Distance(targetLocation.ResolvedLocation) - num);
					max = min + 12;
					if (landmarkLocation.ResolvedX > targetLocation.ResolvedX)
					{
						direction = "west";
					}
					if (landmarkLocation.ResolvedX < targetLocation.ResolvedX)
					{
						direction = "east";
					}
					if (landmarkLocation.ResolvedY < targetLocation.ResolvedY)
					{
						direction = "south";
					}
					if (landmarkLocation.ResolvedY > targetLocation.ResolvedY)
					{
						direction = "north";
					}
					break;
				}
				typeOfDirections = "radius_Failsafe";
			}
			else
			{
				if (!(typeOfDirections == "path"))
				{
					break;
				}
				if (questContext == null)
				{
					throw new Exception("questContext missimg");
				}
				if (questContext.worldInfo == null)
				{
					throw new Exception("worldInfo missing");
				}
				if (targetLocation == null)
				{
					throw new Exception("targetLocation missimg");
				}
				GeneratedLocationInfo generatedLocationInfo = questContext.worldInfo.FindLocationAlongPathFromLandmark(targetLocation.ResolvedLocation, out path, out var _, out var directionFromLandmark, questContext.IsValidQuestDestination);
				if (generatedLocationInfo != null)
				{
					direction = directionFromLandmark;
					landmarkLocation = JournalAPI.GetMapNote(generatedLocationInfo.secretID);
					break;
				}
				typeOfDirections = new List<string> { "radius", "direction" }.GetRandomElement();
			}
		}
		if (landmarkLocation == null)
		{
			MetricsManager.LogError("fabricateFindASpecificSiteQuest", "Couldn't find a site!");
			return quest;
		}
		quest.Name = Grammar.MakeTitleCase(ColorUtility.StripFormatting(targetLocation.Text));
		quest.Manager = new FindASiteDynamicQuestManager(targetLocation.ZoneID, targetLocation.ID, quest.ID, "a_locate");
		quest.StepsByID = new Dictionary<string, QuestStep>();
		QuestStep questStep = new QuestStep();
		questStep.ID = "a_locate";
		questStep.Name = Grammar.MakeTitleCase("Find " + ColorUtility.StripFormatting(targetLocation.Text));
		string text = Grammar.InitLowerIfArticle(landmarkLocation.Text);
		if (typeOfDirections == "radius_Failsafe")
		{
			questStep.Text = "Locate " + targetLocation.Text + ", located within " + max + " parasangs of " + text + ".";
		}
		if (typeOfDirections == "radius")
		{
			questStep.Text = "Locate " + targetLocation.Text + ", located next to " + text + ".";
		}
		if (typeOfDirections == "direction")
		{
			questStep.Text = "Locate " + targetLocation.Text + ", located " + Math.Max(min / 3, 1) + "-" + max / 3 + " parasangs " + direction + " of " + text + ".";
		}
		if (typeOfDirections == "path")
		{
			questStep.Text = "Locate " + targetLocation.Text + ", located " + direction + " along the " + path + " that runs through " + text + ".";
		}
		questStep.XP = 100;
		questStep.Finished = false;
		quest.StepsByID.Add(questStep.ID, questStep);
		QuestStep questStep2 = new QuestStep();
		questStep2.ID = "b_return";
		questStep2.Name = Grammar.MakeTitleCase("Return to " + questContext.getQuestOriginZone());
		questStep2.Text = "Return to " + questContext.getQuestOriginZone() + " and speak to " + giver.DisplayNameOnlyDirectAndStripped + ".";
		questStep2.XP = 100;
		questStep2.Finished = false;
		quest.StepsByID.Add(questStep2.ID, questStep2);
		quest.dynamicReward = questContext.getQuestReward();
		DynamicQuestsGameState.Add(quest);
		addQuestConversationToGiver(giver, quest);
		return quest;
	}

	public FindASpecificSiteDynamicQuestTemplate_FabricateQuestGiver()
	{
	}

	public FindASpecificSiteDynamicQuestTemplate_FabricateQuestGiver(Func<GameObject, bool> questGiverFilter)
	{
		this.questGiverFilter = questGiverFilter;
	}

	public bool BuildZone(Zone zone)
	{
		base.zone = zone;
		questGiverFilter = questContext.getQuestGiverFilter();
		foreach (GameObject item in zone.GetObjects().ShuffleInPlace())
		{
			if (questGiverFilter(item))
			{
				fabricateFindASpecificSiteQuest(item);
				return true;
			}
		}
		return true;
	}
}
