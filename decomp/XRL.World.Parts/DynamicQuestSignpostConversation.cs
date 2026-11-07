using System;
using System.Collections.Generic;
using System.Linq;
using HistoryKit;
using XRL.World.Conversations;

namespace XRL.World.Parts;

[Serializable]
public class DynamicQuestSignpostConversation : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeConversationEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeConversationEvent E)
	{
		if (ParentObject.GetCurrentCell() == null)
		{
			return false;
		}
		List<(GameObject, Quest)> list = new List<(GameObject, Quest)>();
		foreach (GameObject @object in ParentObject.GetCurrentCell().ParentZone.GetObjects((GameObject o) => o.HasProperty("GivesDynamicQuest")))
		{
			string stringProperty = @object.GetStringProperty("GivesDynamicQuest");
			if (!The.Game.HasQuest(stringProperty))
			{
				if (The.Game.Quests.TryGetValue(stringProperty, out var Value))
				{
					list.Add((@object, Value));
				}
				else
				{
					list.Add((@object, null));
				}
			}
		}
		if (list.Count <= 0 || list.Any(((GameObject, Quest) t) => t.Item1 == ParentObject))
		{
			return base.HandleEvent(E);
		}
		foreach (Node start in E.Conversation.Starts)
		{
			if (!start.Elements.Any((IConversationElement n) => n is Choice choice && choice.Target == "*DynamicQuestSignpostConversationIntro"))
			{
				start.AddChoice(null, HistoricStringExpander.ExpandString("<spice.quests.intro.!random>"), "*DynamicQuestSignpostConversationIntro");
			}
		}
		string text = "";
		string text2 = null;
		bool flag = false;
		int num = 0;
		for (int count = list.Count; num < count; num++)
		{
			GameObject item = list[num].Item1;
			string text3 = ParentObject.DescribeDirectionToward(item, General: true);
			if (num > 0)
			{
				if (num != list.Count - 1)
				{
					text += ", ";
				}
				else
				{
					text = ((!flag) ? (text + " or ") : (text + ", or "));
				}
			}
			text = text + "{{Y|" + item.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: true, ForSort: false, Short: false, BaseOnly: true) + "}}";
			if (!string.IsNullOrEmpty(text3))
			{
				text = text + ", " + ((text3 == text2) ? "also " : "") + text3;
				flag = true;
				text2 = text3;
			}
		}
		Node node = E.Conversation.AddNode("*DynamicQuestSignpostConversationIntro", HistoricStringExpander.ExpandString("<spice.instancesOf.speakTo.!random.capitalize> ") + text + ".");
		node.AddChoice(null, "Thank you. I have more to ask.", "Start");
		node.AddChoice(null, "Live and drink.", "End");
		return base.HandleEvent(E);
	}
}
