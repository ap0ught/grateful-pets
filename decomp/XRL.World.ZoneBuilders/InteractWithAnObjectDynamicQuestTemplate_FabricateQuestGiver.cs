using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.World.Conversations;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class InteractWithAnObjectDynamicQuestTemplate_FabricateQuestGiver : ZoneBuilderSandbox
{
	public DynamicQuestDeliveryTarget deliveryTarget;

	public string deliveryItemID;

	public Func<GameObject, bool> questGiverFilter;

	public DynamicQuestContext questContext;

	public string plan;

	public QuestStoryType_InteractWithAnObject QST;

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
			go.Brain.WandersRandomly = false;
		}
		string text = null;
		string stringProperty = fetchItem.GetStringProperty("QuestVerb");
		switch (QST)
		{
		case QuestStoryType_InteractWithAnObject.HolyItem:
			text = ((!(stringProperty != "desecrate")) ? Grammar.ConvertAtoAn(string.Format("{0}? {1}. {2}. {3}? {4}.", HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.intro.!random.capitalize>").Replace("*itemName*", fetchItem.DisplayNameOnlyDirectAndStripped).Replace("*deliveryTarget*", ColorUtility.StripFormatting(deliveryTarget.displayName)), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.itIsDespicable.!random.capitalize>").Replace("*It*", fetchItem.It).Replace("*sacredThing*", questContext.getSacredThings().GetRandomElement()), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.honorUsDesecrate.!random.capitalize>").Replace("*it*", fetchItem.it).Replace("*verb*", stringProperty), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.willYou.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.rewardYou.!random.capitalize>"))) : Grammar.ConvertAtoAn(string.Format("{0}? {1}. {2}. {3}. {4}? {5}.", HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.intro.!random.capitalize>").Replace("*itemName*", fetchItem.DisplayNameOnlyDirectAndStripped).Replace("*deliveryTarget*", ColorUtility.StripFormatting(deliveryTarget.displayName)), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.itIsHoly.!random.capitalize>").Replace("*It*", fetchItem.It), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.willInteract.!random.capitalize>").Replace("*it*", fetchItem.it).Replace("*verb*", stringProperty)
				.Replace("*sacredThing*", questContext.getSacredThings().GetRandomElement()), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.honorUs.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.willYou.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.holyitem.rewardYou.!random.capitalize>"))));
			break;
		case QuestStoryType_InteractWithAnObject.StrangePlan:
			if (If.CoinFlip())
			{
				quest.Name = Grammar.MakeTitleCase(Grammar.MakePossessive(go.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: true, ForSort: false, Short: false, BaseOnly: true)) + " Strange " + plan);
			}
			text = string.Format("{0}\n\n{1} {2}. {3}. {4}. {5} {6}.\n\n{7}.", HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.strangeplan.intro.!random>"), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.strangeplan.comeClose.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.strangeplan.myPlan.!random.capitalize>").Replace("*plan*", plan), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.strangeplan.goTo.!random.capitalize>").Replace("*verb*", stringProperty).Replace("*deliveryTarget*", ColorUtility.StripFormatting(deliveryTarget.displayName))
				.Replace("*itemName*", fetchItem.DisplayNameOnlyDirectAndStripped), "No, I cannot tell you why", HistoricStringExpander.ExpandString("<spice.quests.FindaSpecificItem.personal.willYouDoIt.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.strangeplan.IRewardYou.!random.capitalize>"), HistoricStringExpander.ExpandString("<spice.quests.InteractWithAnObject.strangeplan.byThe_TellNoOne.!random.capitalize>").Replace("*sacredThing*", questContext.getSacredThings().GetRandomElement()));
			break;
		default:
			text = "ERROR: Failed to generate quest text from quest story type.";
			break;
		}
		ConversationXMLBlueprint questIntroNode = ConversationsAPI.AddNode(conversationXMLBlueprint, null, Grammar.ConvertAtoAn(text));
		DynamicQuestConversationHelper.fabricateIntroAcceptChoice("Yes. I will " + stringProperty + " " + fetchItem.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: true) + " as you ask.", questIntroNode, quest)["RevealMapNote"] = deliveryTarget.secretId;
		DynamicQuestConversationHelper.fabricateIntroRejectChoice("No, I will not.", questIntroNode);
		string[] array = stringProperty.Split(' ');
		string text2 = ((array.Length > 1) ? (Grammar.PastTenseOf(array[0]) + " " + string.Join(" ", array.Skip(1).ToArray())) : Grammar.PastTenseOf(stringProperty));
		DynamicQuestConversationHelper.appendQuestCompletionSequence(conversationXMLBlueprint, quest, questIntroNode, "I've " + text2 + " " + fetchItem.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: true) + ".", "I haven't " + text2 + " " + fetchItem.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: true) + " yet.", (ConversationXMLBlueprint questIntroChoiceFinalizer) =>
		{
		}, (ConversationXMLBlueprint gotoAcceptNodeFinalizer) =>
		{
			gotoAcceptNodeFinalizer["IfNotFinishedQuest"] = quest.ID;
			gotoAcceptNodeFinalizer["IfFinishedQuestStep"] = quest.ID + "~a_use";
		}, (ConversationXMLBlueprint incompleteNodeFinalizer) =>
		{
			incompleteNodeFinalizer["IfNotFinishedQuest"] = quest.ID;
			incompleteNodeFinalizer["IfNotFinishedQuestStep"] = quest.ID + "~a_use";
		}, (ConversationXMLBlueprint completeNodeFinalizer) =>
		{
			completeNodeFinalizer["CompleteQuestStep"] = quest.ID + "~b_return";
		}, (ConversationXMLBlueprint startNodeFinalizer) =>
		{
		});
	}

	public Quest fabricateInteractWithAnObjectQuest(GameObject giver, string objectToFetchCacheID)
	{
		Quest quest = QuestsAPI.fabricateEmptyQuest();
		GameObject gameObject = ZoneManager.instance.peekCachedObject(objectToFetchCacheID);
		string stringProperty = gameObject.GetStringProperty("QuestVerb");
		plan = HistoricStringExpander.ExpandString("<spice.commonPhrases.plan.!random>");
		quest.Name = (If.CoinFlip() ? Grammar.MakeTitleCase(stringProperty + " " + gameObject.t()) : Grammar.MakeTitleCase(gameObject.T()));
		quest.Manager = new InteractWithAnObjectDynamicQuestManager(objectToFetchCacheID, quest.ID, "a_use");
		quest.StepsByID = new Dictionary<string, QuestStep>();
		QuestStep questStep = new QuestStep();
		questStep.ID = "a_use";
		questStep.Name = Grammar.MakeTitleCase(stringProperty + " " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: true));
		questStep.Text = "Travel to " + deliveryTarget.displayName + " and " + stringProperty + " " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: true) + ".";
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
		addQuestConversationToGiver(giver, quest, gameObject);
		return quest;
	}

	public InteractWithAnObjectDynamicQuestTemplate_FabricateQuestGiver()
	{
	}

	public InteractWithAnObjectDynamicQuestTemplate_FabricateQuestGiver(Func<GameObject, bool> questGiverFilter)
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
				fabricateInteractWithAnObjectQuest(item, deliveryItemID);
				list.Add(item);
				return true;
			}
		}
		return true;
	}
}
