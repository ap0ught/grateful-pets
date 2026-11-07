using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class TalkingItem : IPart
{
	public string ConversationID;

	public bool ClearLost;

	public TalkingItem()
	{
	}

	public TalkingItem(string ID)
		: this()
	{
		ConversationID = ID;
	}

	public override bool SameAs(IPart p)
	{
		TalkingItem talkingItem = p as TalkingItem;
		if (talkingItem.ConversationID != ConversationID)
		{
			return false;
		}
		if (talkingItem.ClearLost != ClearLost)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != CanGiveDirectionsEvent.ID || !ClearLost) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanGiveDirectionsEvent E)
	{
		if (E.SpeakingWith == ParentObject && ClearLost && !E.PlayerCompanion && !E.SpeakingWith.HasEffect<Lost>())
		{
			E.CanGiveDirections = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (E.Actor != ParentObject)
		{
			E.AddAction("Chat", "chat", "Chat", null, 'h', FireOnActor: false, 10, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Chat" && E.Actor != ParentObject)
		{
			ConversationUI.HaveConversation(ConversationID, ParentObject, null, null, null, TradeEnabled: false);
		}
		return base.HandleEvent(E);
	}
}
