using System;
using HistoryKit;
using Qud.API;
using XRL.World.Conversations;

namespace XRL.World;

public static class DynamicQuestConversationHelper
{
	public static ConversationXMLBlueprint fabricateIntroAcceptChoice(string text, ConversationXMLBlueprint questIntroNode, Quest quest)
	{
		ConversationXMLBlueprint conversationXMLBlueprint = ConversationsAPI.AddChoice(questIntroNode, null, "End", text);
		conversationXMLBlueprint["IfNotHaveQuest"] = quest.ID;
		ConversationXMLBlueprint conversationXMLBlueprint2 = ConversationsAPI.AddPart(conversationXMLBlueprint, "QuestHandler");
		conversationXMLBlueprint2["QuestID"] = quest.ID;
		conversationXMLBlueprint2["Action"] = "Start";
		string text2 = quest.dynamicReward?.getRewardAcceptQuestText();
		if (!text2.IsNullOrEmpty())
		{
			conversationXMLBlueprint2.Text = "{{W|[" + text2 + "]}}";
		}
		return conversationXMLBlueprint;
	}

	public static ConversationXMLBlueprint fabricateIntroRejectChoice(string text, ConversationXMLBlueprint questIntroNode)
	{
		return ConversationsAPI.AddChoice(questIntroNode, null, "End", text);
	}

	public static ConversationXMLBlueprint fabricateIntroAdditionalChoice(string text, ConversationXMLBlueprint questIntroNode)
	{
		return ConversationsAPI.AddChoice(questIntroNode, null, "End", text);
	}

	public static void appendQuestCompletionSequence(ConversationXMLBlueprint conversation, Quest quest, ConversationXMLBlueprint questIntroNode, string completeText, string incompleteText, Action<ConversationXMLBlueprint> questIntroChoiceFinalizer = null, Action<ConversationXMLBlueprint> gotoAcceptNodeFinalizer = null, Action<ConversationXMLBlueprint> incompleteNodeFinalizer = null, Action<ConversationXMLBlueprint> completeNodeFinalizer = null, Action<ConversationXMLBlueprint> startNodeFinalizer = null)
	{
		ConversationXMLBlueprint conversationXMLBlueprint = null;
		string text = "Choice";
		if (quest.dynamicReward != null)
		{
			text = quest.dynamicReward.getRewardConversationType();
		}
		if (text == "Choice")
		{
			string text2 = HistoricStringExpander.ExpandString("<spice.quests.thanks.!random.capitalize>. Our village owes you a debt. For now, please choose a reward from our stockpile as payment for your service.");
			conversationXMLBlueprint = ConversationsAPI.AddNode(conversation, null, text2);
			ConversationXMLBlueprint obj = ConversationsAPI.AddChoice(conversationXMLBlueprint, null, "End", "Live and drink.");
			completeNodeFinalizer(obj);
		}
		else if (text == "VillageZeroMainQuest")
		{
			ConversationXMLBlueprint conversationXMLBlueprint2 = ConversationsAPI.AddNode(conversation, null, "They are disciples of Barathrum. Mostly they are Urshiib, like their mentor. Mutant albino cave bears. With quills to boot! A thousand years ago Barathrum and his kin crossed the Homs Delta into the heart of Qud. He has spent centuries fiddling with the tokens of antiquity in his underground workshops.");
			ConversationXMLBlueprint conversationXMLBlueprint3 = ConversationsAPI.AddChoice(conversationXMLBlueprint2, null, "End", "I will take the disk to Grit Gate and speak with the Barathrumites.");
			conversationXMLBlueprint3["StartQuest"] = "A Signal in the Noise";
			conversationXMLBlueprint3["GiveItem"] = "Stamped Data Disk";
			ConversationsAPI.AddChoice(conversationXMLBlueprint2, null, "End", "I must pass on this offer, for now. Live and drink.");
			ConversationXMLBlueprint conversationXMLBlueprint4 = ConversationsAPI.AddNode(conversation, null, "Are you seeking more work, =player.formalAddressTerm=? Recently we came into possession of a data disk bearing a peculiar stamp and encoded with a strange signal. The signal means nothing to us, but there's a sect of tinkers called the Barathrumites who might be interested in it. They are friends to our village and often trade for the scrap we tow out of the earth. Would you carry the disk to their enclave at Grit Gate, along the western rim of the jungle? In exchange for the delivery, you might seek an apprenticeship with them.\n\nIf you are interested, take the disk now, and travel safely.");
			ConversationsAPI.AddChoice(conversationXMLBlueprint4, null, conversationXMLBlueprint2, "Who are these Barathrumites?");
			conversationXMLBlueprint4.AddChild(conversationXMLBlueprint3);
			conversationXMLBlueprint = ConversationsAPI.AddNode(conversation, null, HistoricStringExpander.ExpandString("<spice.quests.thanks.!random.capitalize>. You've proven =player.reflexive= a friend to our village. Take this recoiler and return whenever your throat is dry."));
			ConversationXMLBlueprint obj2 = ConversationsAPI.AddChoice(conversationXMLBlueprint, null, conversationXMLBlueprint4, "My thanks, =pronouns.formalAddressTerm=.");
			completeNodeFinalizer(obj2);
			if (!The.Game.HasQuest("More Than a Willing Spirit"))
			{
				ConversationXMLBlueprint conversationXMLBlueprint5 = ConversationsAPI.DistributeChoice(conversation, "Start", null, conversationXMLBlueprint4, "Who are these Barathrumites?");
				conversationXMLBlueprint5["IfFinishedQuest"] = quest.ID;
				conversationXMLBlueprint5["IfNotHaveQuest"] = "A Signal in the Noise";
			}
		}
		string text3 = HistoricStringExpander.ExpandString("<spice.quests.intro.!random.capitalize>");
		ConversationXMLBlueprint conversationXMLBlueprint6 = ConversationsAPI.DistributeChoice(conversation, "Start", null, questIntroNode, text3);
		conversationXMLBlueprint6["IfNotHaveQuest"] = quest.ID;
		conversationXMLBlueprint6["IfNotFinishedQuest"] = quest.ID;
		questIntroChoiceFinalizer(conversationXMLBlueprint6);
		text3 = completeText;
		ConversationXMLBlueprint conversationXMLBlueprint7 = ConversationsAPI.DistributeChoice(conversation, "Start", null, conversationXMLBlueprint, text3);
		conversationXMLBlueprint7["IfHaveQuest"] = quest.ID;
		conversationXMLBlueprint7["IfNotFinishedQuest"] = quest.ID;
		gotoAcceptNodeFinalizer(conversationXMLBlueprint7);
		ConversationXMLBlueprint conversationXMLBlueprint8 = ConversationsAPI.DistributeChoice(conversation, "Start", null, "End", incompleteText);
		conversationXMLBlueprint8["IfHaveQuest"] = quest.ID;
		incompleteNodeFinalizer(conversationXMLBlueprint8);
	}
}
