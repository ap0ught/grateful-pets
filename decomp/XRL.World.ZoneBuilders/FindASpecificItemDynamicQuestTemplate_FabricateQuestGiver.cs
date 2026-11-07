using System;
using System.Collections.Generic;
using System.Linq;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.Rules;
using XRL.World.Conversations;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class FindASpecificItemDynamicQuestTemplate_FabricateQuestGiver : ZoneBuilderSandbox
{
	public DynamicQuestDeliveryTarget deliveryTarget;

	public string deliveryItemID;

	public Func<GameObject, bool> questGiverFilter;

	public DynamicQuestContext questContext;

	public string sanctityOfSacredThing;

	public QuestStoryType_FindASpecificItem QST;

	public void addQuestConversationToGiver(GameObject go, Quest quest, GameObject fetchItem)
	{
		go.SetStringProperty("GivesDynamicQuest", quest.ID);
		go.RequirePart<Interesting>();
		ConversationXMLBlueprint conversationXMLBlueprint = go.RequirePart<ConversationScript>().Blueprint;
		if (conversationXMLBlueprint == null)
		{
			conversationXMLBlueprint = ConversationsAPI.AddConversation(go, null, null, null, ClearLost: true);
		}
		conversationXMLBlueprint.ID = Guid.NewGuid().ToString();
		go.SetIntProperty("QuestGiver", 1);
		if (go.Brain != null)
		{
			go.Brain.Wanders = false;
		}
		go.Brain.WandersRandomly = false;
		string displayName = fetchItem.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: false, Reflexive: false, null, AsPossessed: false, null, Reference: false, IncludeImplantPrefix: false);
		List<GameObject> list = (from o in zone.GetObjects(questContext.getQuestActorFilter().Invoke)
			where o != go
			select o).ToList();
		GameObject gameObject = ((list != null) ? list.GetRandomElement() : null) ?? GameObject.Create("Mehmet");
		ConversationXMLBlueprint questIntroNode = ConversationsAPI.AddNode(conversationXMLBlueprint, null, QST switch
		{
			QuestStoryType_FindASpecificItem.SacredItem => string.Format("{0}. {1}. {2}, {3}. {4} {5}. {6}.", HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.sacred.intro.!random.capitalize>").Replace("*Activity*", go.GetxTag_CommaDelimited("TextFragments", "Activity")), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.sacred.afterLearning.!random.capitalize>").Replace("*sanctityOfSacredThing*", sanctityOfSacredThing), HistoricStringExpander.ExpandString("<spice.instancesOf.unfortunately.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.sacred.lostOur.!random>").Replace("*itemName*", displayName).Replace("*itemName.a*", fetchItem.a)
				.Replace("*itemName.an*", fetchItem.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true))
				.Replace("*itemName.are*", fetchItem.GetVerb("are", PrependSpace: false))
				.Replace("*itemName.have*", fetchItem.GetVerb("have", PrependSpace: false))
				.Replace("*name*", gameObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: true, ForSort: false, Short: false, BaseOnly: true))
				.Replace("*were*", fetchItem.GetVerb("were", PrependSpace: false)), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.sacred.willingToRecover.!random.capitalize>").Replace("*itemTheAndName*", fetchItem.the + displayName).Replace("*it*", fetchItem.them), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.sacred.takenTo.!random.capitalize>").Replace("*deliveryTarget*", Grammar.LowerArticles(Grammar.TrimTrailingPunctuation(deliveryTarget.displayName))).Replace("*it*", fetchItem.it)
				.Replace("*has*", fetchItem.GetVerb("have", PrependSpace: false)), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.sacred.rewardYou.!random.capitalize>")), 
			QuestStoryType_FindASpecificItem.PersonalFavor => string.Format("{0} {1}. {2}. {3}. {4}. {5}", HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.intro.!random.capitalize>"), Grammar.ConvertAtoAn(HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.iHaveATask.!random.capitalize>")), Grammar.ConvertAtoAn(HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.rumor.!random.capitalize>").Replace("*itemLocation*", "{{|" + deliveryTarget.displayName + "}}").Replace("*itemName*", displayName)
				.Replace("*itemName.a*", fetchItem.a)
				.Replace("*itemName.an*", fetchItem.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true))
				.Replace("*itemName.are*", fetchItem.GetVerb("are", PrependSpace: false))
				.Replace("*itemName.have*", fetchItem.GetVerb("have", PrependSpace: false))
				.Replace("*villagerName*", gameObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: true, ForSort: false, Short: false, BaseOnly: true))), Grammar.InitCap(HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.loveToHave.!random.capitalize>").Replace("*it*", fetchItem.them).Replace("*NeedsItemFor*", go.GetxTag_CommaDelimited("TextFragments", "NeedsItemFor"))), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.ifYouRetrieveIt.!random.capitalize>").Replace("*it*", fetchItem.them), HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.willYouDoIt.!random.capitalize>")), 
			_ => "ERROR: Failed to generate quest text from quest story type.", 
		});
		DynamicQuestConversationHelper.fabricateIntroAcceptChoice("Yes. I will find " + fetchItem.the + displayName + " as you ask.", questIntroNode, quest)["RevealMapNote"] = deliveryTarget.secretId;
		DynamicQuestConversationHelper.fabricateIntroRejectChoice("No, I will not.", questIntroNode);
		DynamicQuestConversationHelper.appendQuestCompletionSequence(conversationXMLBlueprint, quest, questIntroNode, "I've found " + fetchItem.the + displayName + ".", "I don't have " + fetchItem.the + displayName + " yet.", (ConversationXMLBlueprint questIntroChoiceFinalizer) =>
		{
		}, (ConversationXMLBlueprint gotoAcceptNodeFinalizer) =>
		{
			gotoAcceptNodeFinalizer["IfNotFinishedQuest"] = quest.ID;
			gotoAcceptNodeFinalizer["IfHaveItemWithID"] = deliveryItemID;
		}, (ConversationXMLBlueprint incompleteNodeFinalizer) =>
		{
			incompleteNodeFinalizer["IfNotFinishedQuest"] = quest.ID;
			incompleteNodeFinalizer["IfHaveItemWithID"] = "!" + deliveryItemID;
		}, (ConversationXMLBlueprint completeNodeFinalizer) =>
		{
			ConversationXMLBlueprint conversationXMLBlueprint2 = ConversationsAPI.AddPart(completeNodeFinalizer, "TakeItem");
			conversationXMLBlueprint2.SetAttribute("IDs", deliveryItemID);
			conversationXMLBlueprint2.SetAttribute("ClearQuest", "true");
			ConversationXMLBlueprint conversationXMLBlueprint3 = ConversationsAPI.AddPart(completeNodeFinalizer, "QuestHandler");
			conversationXMLBlueprint3.SetAttribute("Action", "Step");
			conversationXMLBlueprint3.SetAttribute("QuestID", quest.ID);
			conversationXMLBlueprint3.SetAttribute("StepID", "a_locate~b_return");
		}, (ConversationXMLBlueprint startNodeFinalizer) =>
		{
		});
	}

	public Quest fabricateFindASpecificItemQuest(GameObject giver, string objectToFetchCacheID)
	{
		Quest quest = QuestsAPI.fabricateEmptyQuest();
		GameObject gameObject = ZoneManager.instance.peekCachedObject(objectToFetchCacheID);
		sanctityOfSacredThing = HistoricStringExpander.ExpandString("<spice.commonPhrases.sanctity.!random> of *sacredThing*").Replace("*sacredThing*", questContext.getSacredThings().GetRandomElement());
		switch (QST)
		{
		case QuestStoryType_FindASpecificItem.SacredItem:
			if (Stat.Random(0, 1) == 0)
			{
				quest.Name = Grammar.MakeTitleCase(gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, true));
			}
			else
			{
				quest.Name = Grammar.MakeTitleCase("The " + sanctityOfSacredThing);
			}
			break;
		case QuestStoryType_FindASpecificItem.PersonalFavor:
			switch (Stat.Random(0, 2))
			{
			case 0:
				quest.Name = Grammar.MakeTitleCase(gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, true));
				break;
			case 1:
				quest.Name = Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<spice.commonPhrases.helping.!random> ") + giver.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " to find " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, true));
				break;
			default:
				quest.Name = Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<spice.commonPhrases.helping.!random> ") + giver.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true));
				break;
			}
			break;
		default:
			quest.Name = Grammar.MakeTitleCase(gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, true));
			break;
		}
		quest.Manager = new FindASpecificItemDynamicQuestManager(objectToFetchCacheID, quest.ID, "a_locate");
		quest.StepsByID = new Dictionary<string, QuestStep>();
		QuestStep questStep = new QuestStep();
		questStep.ID = "a_locate";
		questStep.Name = Grammar.MakeTitleCase("Find " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, true));
		questStep.Text = "Locate " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, true) + " at " + deliveryTarget.displayName + ".";
		questStep.XP = 100;
		questStep.Finished = false;
		quest.StepsByID.Add(questStep.ID, questStep);
		QuestStep questStep2 = new QuestStep();
		questStep2.ID = "b_return";
		questStep2.Name = Grammar.MakeTitleCase("Return " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, true) + " to " + questContext.getQuestOriginZone());
		questStep2.Text = "Return " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, true) + " to " + questContext.getQuestOriginZone() + " and speak with " + giver.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + ".";
		questStep2.XP = 100;
		questStep2.Finished = false;
		quest.StepsByID.Add(questStep2.ID, questStep2);
		quest.dynamicReward = questContext.getQuestReward();
		DynamicQuestsGameState.Add(quest);
		addQuestConversationToGiver(giver, quest, gameObject);
		return quest;
	}

	public FindASpecificItemDynamicQuestTemplate_FabricateQuestGiver()
	{
	}

	public FindASpecificItemDynamicQuestTemplate_FabricateQuestGiver(Func<GameObject, bool> questGiverFilter)
	{
		this.questGiverFilter = questGiverFilter;
	}

	public bool BuildZone(Zone zone)
	{
		List<GameObject> list = new List<GameObject>();
		base.zone = zone;
		questGiverFilter = questContext.getQuestGiverFilter();
		foreach (GameObject item in zone.GetObjects().ShuffleInPlace())
		{
			if (questGiverFilter(item))
			{
				fabricateFindASpecificItemQuest(item, deliveryItemID);
				list.Add(item);
				return true;
			}
		}
		return true;
	}
}
