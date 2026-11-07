using System.Collections.Generic;
using XRL.UI;
using XRL.World;
using XRL.World.Conversations;
using XRL.World.Parts;

namespace Qud.API;

public static class ConversationsAPI
{
	public static ConversationXMLBlueprint AddConversation(GameObject Object, string Filter = null, string FilterExtras = null, string Append = null, bool ClearLost = false, bool ClearOriginal = true)
	{
		ConversationScript part = Object.GetPart<ConversationScript>();
		if (part?.Blueprint != null && !ClearOriginal)
		{
			return part.Blueprint;
		}
		Object.RemovePart<ConversationScript>();
		part = Object.AddPart(new ConversationScript());
		part.ClearLost = ClearLost;
		part.Filter = Filter;
		part.FilterExtras = FilterExtras;
		part.Append = Append;
		part.Blueprint = new ConversationXMLBlueprint
		{
			ID = "CustomConversation::" + Object.ID,
			Name = "Conversation"
		};
		Object?.SetStringProperty("SuppressPowerSwitchTwiddle", "1");
		return part.Blueprint;
	}

	public static void AddDynamicShim(ConversationXMLBlueprint Conversation)
	{
		ConversationXMLBlueprint conversationXMLBlueprint = XRL.World.Conversations.Conversation.Blueprints["BaseDynamicShim"];
		ConversationXMLBlueprint child = conversationXMLBlueprint.GetChild("DynamicStart");
		ConversationXMLBlueprint conversationXMLBlueprint2;
		if (!child.Children.IsNullOrEmpty() && !Conversation.Children.IsNullOrEmpty())
		{
			foreach (ConversationXMLBlueprint child3 in Conversation.Children)
			{
				if (child3.Name != child.Name)
				{
					continue;
				}
				conversationXMLBlueprint2 = child3;
				if (conversationXMLBlueprint2.Children == null)
				{
					conversationXMLBlueprint2.Children = new List<ConversationXMLBlueprint>();
				}
				foreach (ConversationXMLBlueprint child4 in child.Children)
				{
					child3.Children.Add(child4);
				}
			}
		}
		ConversationXMLBlueprint child2 = conversationXMLBlueprint.GetChild("DynamicNode");
		if (!child2.Children.IsNullOrEmpty() && !Conversation.Children.IsNullOrEmpty())
		{
			foreach (ConversationXMLBlueprint child5 in Conversation.Children)
			{
				if (child5.Name != child2.Name)
				{
					continue;
				}
				conversationXMLBlueprint2 = child5;
				if (conversationXMLBlueprint2.Children == null)
				{
					conversationXMLBlueprint2.Children = new List<ConversationXMLBlueprint>();
				}
				foreach (ConversationXMLBlueprint child6 in child2.Children)
				{
					child5.Children.Add(child6);
				}
			}
		}
		ConversationXMLBlueprint conversationXMLBlueprint3 = XRL.World.Conversations.Conversation.Blueprints["BaseConversation"];
		conversationXMLBlueprint2 = Conversation;
		if (conversationXMLBlueprint2.Children == null)
		{
			conversationXMLBlueprint2.Children = new List<ConversationXMLBlueprint>();
		}
		Conversation.Children.AddRange(conversationXMLBlueprint3.Children);
	}

	public static void RemoveDynamicShim(ConversationXMLBlueprint Conversation)
	{
		ConversationXMLBlueprint conversationXMLBlueprint = XRL.World.Conversations.Conversation.Blueprints["BaseDynamicShim"];
		ConversationXMLBlueprint child = conversationXMLBlueprint.GetChild("DynamicStart");
		if (!child.Children.IsNullOrEmpty() && !Conversation.Children.IsNullOrEmpty())
		{
			foreach (ConversationXMLBlueprint child3 in Conversation.Children)
			{
				if (child3.Name != child.Name || child3.Children.IsNullOrEmpty())
				{
					continue;
				}
				foreach (ConversationXMLBlueprint child4 in child.Children)
				{
					child3.Children.Remove(child4);
				}
			}
		}
		ConversationXMLBlueprint child2 = conversationXMLBlueprint.GetChild("DynamicNode");
		if (!child2.Children.IsNullOrEmpty() && !Conversation.Children.IsNullOrEmpty())
		{
			foreach (ConversationXMLBlueprint child5 in Conversation.Children)
			{
				if (child5.Name != child2.Name || child5.Children.IsNullOrEmpty())
				{
					continue;
				}
				foreach (ConversationXMLBlueprint child6 in child2.Children)
				{
					child5.Children.Remove(child6);
				}
			}
		}
		ConversationXMLBlueprint inherit = XRL.World.Conversations.Conversation.Blueprints["BaseConversation"];
		if (Conversation.Children == null)
		{
			Conversation.Children = new List<ConversationXMLBlueprint>();
		}
		Conversation.Children.RemoveAll((ConversationXMLBlueprint x) => inherit.Children.Contains(x));
	}

	public static ConversationXMLBlueprint AddText(ConversationXMLBlueprint Blueprint, string ID = null, string Text = null)
	{
		ConversationXMLBlueprint conversationXMLBlueprint = new ConversationXMLBlueprint
		{
			ID = (ID ?? "Text"),
			Name = "Text",
			Text = Text
		};
		Blueprint.AddChild(conversationXMLBlueprint);
		return conversationXMLBlueprint;
	}

	public static ConversationXMLBlueprint AddElement(ConversationXMLBlueprint Blueprint, string Name, string ID = null, string Text = null)
	{
		ConversationXMLBlueprint conversationXMLBlueprint = new ConversationXMLBlueprint
		{
			ID = (ID ?? Name),
			Name = Name
		};
		if (!Text.IsNullOrEmpty())
		{
			AddText(conversationXMLBlueprint, null, Text);
		}
		Blueprint.AddChild(conversationXMLBlueprint);
		return conversationXMLBlueprint;
	}

	public static ConversationXMLBlueprint AddNode(ConversationXMLBlueprint Blueprint, string ID = null, string Text = null)
	{
		return AddElement(Blueprint, "Node", ID, Text);
	}

	public static ConversationXMLBlueprint AddStart(ConversationXMLBlueprint Blueprint, string Text = null)
	{
		return AddElement(Blueprint, "Start", null, Text);
	}

	public static ConversationXMLBlueprint AddChoice(ConversationXMLBlueprint Blueprint, string ID = null, string Target = null, string Text = null)
	{
		ConversationXMLBlueprint conversationXMLBlueprint = AddElement(Blueprint, "Choice", ID, Text);
		if (!Target.IsNullOrEmpty())
		{
			conversationXMLBlueprint["Target"] = Target;
		}
		return conversationXMLBlueprint;
	}

	public static ConversationXMLBlueprint AddChoice(ConversationXMLBlueprint Blueprint, string ID = null, ConversationXMLBlueprint Target = null, string Text = null)
	{
		return AddChoice(Blueprint, ID, Target.CardinalID, Text);
	}

	public static ConversationXMLBlueprint DistributeChoice(ConversationXMLBlueprint Blueprint, string ParentName, ConversationXMLBlueprint Choice)
	{
		if (Blueprint.Children.IsNullOrEmpty())
		{
			return null;
		}
		foreach (ConversationXMLBlueprint child in Blueprint.Children)
		{
			if (child.Name == ParentName)
			{
				child.AddChild(Choice);
			}
		}
		return Choice;
	}

	public static ConversationXMLBlueprint DistributeChoice(ConversationXMLBlueprint Blueprint, string ParentName, string ID = null, string Target = null, string Text = null)
	{
		if (Blueprint.Children.IsNullOrEmpty())
		{
			return null;
		}
		ConversationXMLBlueprint conversationXMLBlueprint = null;
		foreach (ConversationXMLBlueprint child in Blueprint.Children)
		{
			if (child.Name == ParentName)
			{
				if (conversationXMLBlueprint == null)
				{
					conversationXMLBlueprint = AddChoice(child, ID, Target, Text);
				}
				else
				{
					child.AddChild(conversationXMLBlueprint);
				}
			}
		}
		return conversationXMLBlueprint;
	}

	public static ConversationXMLBlueprint DistributeChoice(ConversationXMLBlueprint Blueprint, string ParentName, string ID = null, ConversationXMLBlueprint Target = null, string Text = null)
	{
		return DistributeChoice(Blueprint, ParentName, ID, Target?.CardinalID, Text);
	}

	public static ConversationXMLBlueprint AddPart(ConversationXMLBlueprint Blueprint, string Name)
	{
		ConversationXMLBlueprint conversationXMLBlueprint = AddElement(Blueprint, "Part");
		conversationXMLBlueprint["Name"] = Name;
		return conversationXMLBlueprint;
	}

	public static ConversationXMLBlueprint GetConversation(GameObject Object)
	{
		return Object.GetPart<ConversationScript>()?.Blueprint;
	}

	public static ConversationXMLBlueprint addSimpleRootInformationOption(GameObject go, string optionText, string choiceText)
	{
		ConversationXMLBlueprint conversation = GetConversation(go);
		ConversationXMLBlueprint conversationXMLBlueprint = AddNode(conversation, null, choiceText);
		AddChoice(conversation.GetChild("Start"), null, conversationXMLBlueprint, optionText);
		AddChoice(conversationXMLBlueprint, null, "Start", "I have more to ask.");
		return conversation;
	}

	public static GameObject chooseOneItem(List<GameObject> objects, string title, bool allowEscape)
	{
		List<GameObject> list = new List<GameObject>();
		List<string> list2 = new List<string>();
		List<char> list3 = new List<char>();
		char c = 'a';
		foreach (GameObject @object in objects)
		{
			list.Add(@object);
			list2.Add(@object.DisplayName);
			list3.Add(c);
			c = (char)(c + 1);
		}
		int num = Popup.PickOption("Choose a reward", null, "", "Sounds/UI/ui_notification", list2.ToArray(), list3.ToArray(), null, null, null, null, null, 1, 60, 0, -1, allowEscape);
		if (num < 0)
		{
			return null;
		}
		return list[num];
	}

	public static ConversationXMLBlueprint addSimpleConversationToObject(GameObject Object, string Text, string Goodbye, string Filter = null, string FilterExtras = null, string Append = null, bool ClearLost = false, bool ClearOriginal = true)
	{
		ConversationXMLBlueprint conversationXMLBlueprint = AddConversation(Object, Filter, FilterExtras, Append, ClearLost, ClearOriginal);
		AddChoice(AddStart(conversationXMLBlueprint, Text), null, "End", Goodbye);
		return conversationXMLBlueprint;
	}

	public static ConversationXMLBlueprint appendSimpleConversationToObject(GameObject Object, string Text, string Goodbye, string Filter = null, string FilterExtras = null, string Append = null, bool ClearLost = false)
	{
		return addSimpleConversationToObject(Object, Text, Goodbye, Filter, FilterExtras, Append, ClearLost, ClearOriginal: false);
	}

	public static ConversationXMLBlueprint addSimpleConversationToObject(GameObject Object, string Text, string Goodbye, string Question, string Answer, string Filter = null, string FilterExtras = null, string Append = null, bool ClearLost = false, bool ClearOriginal = true)
	{
		ConversationXMLBlueprint conversationXMLBlueprint = addSimpleConversationToObject(Object, Text, Goodbye, Filter, FilterExtras, Append, ClearLost, ClearOriginal);
		ConversationXMLBlueprint conversationXMLBlueprint2 = AddNode(conversationXMLBlueprint, null, Answer);
		AddChoice(conversationXMLBlueprint2, null, "End", Goodbye);
		AddChoice(conversationXMLBlueprint.GetChild("Start"), null, conversationXMLBlueprint2, Question);
		return conversationXMLBlueprint;
	}

	public static ConversationXMLBlueprint appendSimpleConversationToObject(GameObject Object, string Text, string Goodbye, string Question, string Answer, string Filter = null, string FilterExtras = null, string Append = null, bool ClearLost = false)
	{
		return addSimpleConversationToObject(Object, Text, Goodbye, Question, Answer, Filter, FilterExtras, Append, ClearLost, ClearOriginal: false);
	}
}
