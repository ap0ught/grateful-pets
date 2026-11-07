using System;
using ConsoleLib.Console;
using Qud.API;
using XRL.Core;
using XRL.UI;
using XRL.World.Conversations;
using XRL.World.Conversations.Parts;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class ConversationScript : IPart
{
	public ConversationXMLBlueprint Blueprint;

	public bool RecordConversationAsProperty;

	public string ConversationID;

	public string Quest;

	public string PreQuestConversationID;

	public string InQuestConversationID;

	public string PostQuestConversationID;

	public bool ClearLost;

	public int ChargeUse;

	public string Filter;

	public string FilterExtras;

	public string Color;

	public string Append;

	[NonSerialized]
	private static Event eCanHaveSmartUseConversation = new Event("CanHaveSmartUseConversation");

	public ConversationScript()
	{
	}

	public ConversationScript(string ConversationID)
		: this()
	{
		this.ConversationID = ConversationID;
	}

	public ConversationScript(string ConversationID, bool ClearLost)
		: this(ConversationID)
	{
		this.ClearLost = ClearLost;
	}

	public override bool SameAs(IPart p)
	{
		ConversationScript conversationScript = p as ConversationScript;
		if (conversationScript.ConversationID != ConversationID)
		{
			return false;
		}
		if (conversationScript.Quest != Quest)
		{
			return false;
		}
		if (conversationScript.PreQuestConversationID != PreQuestConversationID)
		{
			return false;
		}
		if (conversationScript.PostQuestConversationID != PostQuestConversationID)
		{
			return false;
		}
		if (conversationScript.ClearLost != ClearLost)
		{
			return false;
		}
		if (conversationScript.ChargeUse != ChargeUse)
		{
			return false;
		}
		if (conversationScript.Filter != Filter)
		{
			return false;
		}
		if (conversationScript.FilterExtras != FilterExtras)
		{
			return false;
		}
		if (conversationScript.Color != Color)
		{
			return false;
		}
		if (conversationScript.Append != Append)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginConversationEvent.ID && (ID != CanGiveDirectionsEvent.ID || !ClearLost) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "RecordConversationAsProperty", RecordConversationAsProperty);
		E.AddEntry(this, "ConversationID", ConversationID);
		E.AddEntry(this, "Quest", Quest);
		E.AddEntry(this, "PreQuestConversationID", PreQuestConversationID);
		E.AddEntry(this, "InQuestConversationID", InQuestConversationID);
		E.AddEntry(this, "PostQuestConversationID", PostQuestConversationID);
		E.AddEntry(this, "ClearLost", ClearLost);
		E.AddEntry(this, "ChargeUse", ChargeUse);
		E.AddEntry(this, "Filter", Filter);
		E.AddEntry(this, "FilterExtras", FilterExtras);
		E.AddEntry(this, "Color", Color);
		E.AddEntry(this, "Append", Append);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginConversationEvent E)
	{
		if (E.SpeakingWith == ParentObject)
		{
			if (ChargeUse > 0)
			{
				ParentObject.UseCharge(ChargeUse, LiveOnly: false, 0L);
			}
			if (RecordConversationAsProperty)
			{
				E.Actor.ModIntProperty(ParentObject.Blueprint + "Chat", 1);
			}
			if (!Filter.IsNullOrEmpty())
			{
				The.Conversation?.AddPart(new TextFilter(Filter, FilterExtras));
			}
			if (!Append.IsNullOrEmpty())
			{
				The.Conversation?.AddPart(new TextInsert
				{
					Text = Append
				});
			}
			int intProperty = E.Actor.GetIntProperty("PronounSetTick");
			bool speakerGivePronouns = false;
			bool speakerGetPronouns = false;
			bool speakerGetNewPronouns = false;
			ParentObject.ModIntProperty("ConversationCount", 1);
			if (PronounSet.EnableConversationalExchange && ParentObject.Brain != null && !ParentObject.HasProperty("FugueCopy"))
			{
				if (E.Actor.GetPronounSet() != null)
				{
					if (ParentObject.GetIntProperty("ConversationCount") == 1)
					{
						speakerGetPronouns = true;
					}
					else if (ParentObject.GetIntProperty("KnowsPlayerPronounsAsOfTick") < intProperty)
					{
						speakerGetPronouns = true;
						speakerGetNewPronouns = true;
					}
					ParentObject.SetIntProperty("KnowsPlayerPronounsAsOfTick", intProperty);
				}
				if (ParentObject.GetPronounSet() != null && !ParentObject.IsPronounSetKnown())
				{
					ParentObject.SetPronounSetKnown(Value: true);
					speakerGivePronouns = true;
				}
			}
			string text = PronounExchangeDescription(E.Actor, ParentObject, speakerGivePronouns, speakerGetPronouns, speakerGetNewPronouns);
			if (text != null)
			{
				string text2 = "[" + ColorUtility.CapitalizeExceptFormatting(text) + ".]\n\n";
				ConversationUI.StartNode?.AddPart(new TextInsert
				{
					Text = text2,
					Prepend = true,
					Spoken = false
				});
			}
		}
		return base.HandleEvent(E);
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
		if (ParentObject != E.Actor)
		{
			E.AddAction("Chat", "chat", "Chat", null, 'h', FireOnActor: false, 10, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Chat")
		{
			if (AttemptConversation(E.Actor, ParentObject, null, null, GetActiveConversationBlueprint(), 0, Silent: false, Blueprint != null, null, E))
			{
				E.RequestInterfaceExit();
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer() && (!ParentObject.IsPlayerLed() || !ParentObject.IsMobile()) && ParentObject.FireEvent(eCanHaveSmartUseConversation))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer() && (!ParentObject.IsPlayerLed() || !ParentObject.IsMobile()) && ParentObject.FireEvent(eCanHaveSmartUseConversation) && AttemptConversation())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static bool IsPhysicalConversationPossible(GameObject Who, GameObject With, bool ShowPopup = true, bool AllowCombat = false, bool AllowFrozen = false, int ChargeUse = 0)
	{
		if (Who.HasEffect<Stun>() || Who.HasEffect<Exhausted>() || Who.HasEffect<Paralyzed>() || Who.HasEffect<Asleep>())
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail("You are in no shape to start a conversation.");
			}
			return false;
		}
		if (Who.HasEffect<Confused>())
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail("You can't seem to make out what " + With.t() + With.Is + " saying.");
			}
			return false;
		}
		if (!IsConversationallyResponsiveEvent.Check(With, Who, out var Message, Physical: true))
		{
			if ((!string.IsNullOrEmpty(Message) & ShowPopup) && Who.IsPlayer())
			{
				Popup.ShowFail(Message);
			}
			return false;
		}
		if (ChargeUse > 0 && !With.TestCharge(ChargeUse, LiveOnly: false, 0L))
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail(With.T() + With.Is + " utterly unresponsive.");
			}
			return false;
		}
		if (!AllowFrozen && With.IsFrozen())
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail("You hear a muffled grunting coming from inside the block of ice.");
			}
			return false;
		}
		if (!AllowCombat && With.IsHostileTowards(Who))
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail(With.T() + With.GetVerb("refuse") + " to speak to you.");
			}
			return false;
		}
		if (!AllowCombat && With.IsEngagedInMelee())
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail(With.Does("are") + " engaged in hand-to-hand combat and" + With.Is + " too busy to have a conversation with you.");
			}
			return false;
		}
		if (!AllowCombat && With.IsAflame())
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail(With.Does("are") + " on fire and" + With.Is + " too busy to have a conversation with you.");
			}
			return false;
		}
		if (!CanStartConversationEvent.Check(Who, With, out var FailureMessage, Physical: true))
		{
			if (ShowPopup && Who.IsPlayer())
			{
				if (!string.IsNullOrEmpty(FailureMessage))
				{
					Popup.ShowFail(FailureMessage);
				}
				else
				{
					Popup.ShowFail("You cannot seem to engage " + With.T() + " in conversation.");
				}
			}
			return false;
		}
		return true;
	}

	public static bool IsPhysicalConversationPossible(GameObject With, bool ShowPopup = false, bool AllowCombat = false, bool AllowFrozen = false, int ChargeUse = 0)
	{
		return IsPhysicalConversationPossible(IComponent<GameObject>.ThePlayer, With, ShowPopup, AllowCombat, AllowFrozen, ChargeUse);
	}

	public bool IsPhysicalConversationPossible(bool ShowPopup = false, bool AllowCombat = false, bool AllowFrozen = false)
	{
		return IsPhysicalConversationPossible(ParentObject, ShowPopup, AllowCombat, AllowFrozen, ChargeUse);
	}

	public static bool IsMentalConversationPossible(GameObject Who, GameObject With, bool ShowPopup = true, bool AllowCombat = false, int ChargeUse = 0)
	{
		if (!Who.HasPart<Telepathy>())
		{
			return false;
		}
		if (!Who.CanMakeTelepathicContactWith(With) || (ChargeUse > 0 && !With.TestCharge(ChargeUse, LiveOnly: false, 0L)))
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail("You can sense nothing from " + With.t() + ".");
			}
			return false;
		}
		if (!IsConversationallyResponsiveEvent.Check(With, Who, out var Message, Physical: false, Mental: true))
		{
			if ((!string.IsNullOrEmpty(Message) & ShowPopup) && Who.IsPlayer())
			{
				Popup.ShowFail(Message);
			}
			return false;
		}
		if (!AllowCombat && With.IsHostileTowards(Who))
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail("You sense only hostility from " + With.t() + ".");
			}
			return false;
		}
		if (!CanStartConversationEvent.Check(Who, With, out var FailureMessage, Physical: false, Mental: true))
		{
			if (ShowPopup && Who.IsPlayer())
			{
				if (!string.IsNullOrEmpty(FailureMessage))
				{
					Popup.ShowFail(FailureMessage);
				}
				else
				{
					Popup.ShowFail("You cannot seem to make contact with " + With.t() + ".");
				}
			}
			return false;
		}
		return true;
	}

	public static bool IsMentalConversationPossible(GameObject With, bool ShowPopup = false, bool AllowCombat = false, int ChargeUse = 0)
	{
		return IsMentalConversationPossible(IComponent<GameObject>.ThePlayer, With, ShowPopup, AllowCombat, ChargeUse);
	}

	public bool IsMentalConversationPossible(bool ShowPopup = false, bool AllowCombat = false)
	{
		return IsMentalConversationPossible(ParentObject, ShowPopup, AllowCombat, ChargeUse);
	}

	public string GetActiveConversationID()
	{
		if (!string.IsNullOrEmpty(Quest))
		{
			if (!string.IsNullOrEmpty(PostQuestConversationID) && XRLCore.Core.Game.FinishedQuest(Quest))
			{
				return PostQuestConversationID;
			}
			if (!string.IsNullOrEmpty(PreQuestConversationID) && !XRLCore.Core.Game.HasQuest(Quest))
			{
				return PreQuestConversationID;
			}
			if (!string.IsNullOrEmpty(InQuestConversationID) && XRLCore.Core.Game.HasQuest(Quest) && !XRLCore.Core.Game.FinishedQuest(Quest))
			{
				return InQuestConversationID;
			}
		}
		return ConversationID;
	}

	public ConversationXMLBlueprint GetActiveConversationBlueprint()
	{
		if (Blueprint != null)
		{
			return Blueprint;
		}
		string activeConversationID = GetActiveConversationID();
		if (Conversation.Blueprints.TryGetValue(activeConversationID, out var value))
		{
			return value;
		}
		return null;
	}

	public static bool IsDistant(GameObject Listener, GameObject Speaker, GameObject Transmitter = null, GameObject Receiver = null)
	{
		if (!Listener.InSameOrAdjacentCellTo(Speaker) && !Listener.InSameOrAdjacentCellTo(Transmitter) && !Speaker.InSameOrAdjacentCellTo(Receiver))
		{
			if (Receiver != null)
			{
				return !Receiver.InSameOrAdjacentCellTo(Transmitter);
			}
			return true;
		}
		return false;
	}

	public static bool AttemptConversation(GameObject Listener, GameObject Speaker, GameObject Transmitter = null, GameObject Receiver = null, ConversationXMLBlueprint Blueprint = null, int ChargeUse = 0, bool Silent = false, bool Dynamic = false, bool? Mental = null, IEvent ParentEvent = null)
	{
		if (!GameObject.Validate(Listener) || !GameObject.Validate(Speaker) || Listener == Speaker)
		{
			return false;
		}
		if (!AttemptConversationEvent.Check(ref Speaker, ref Listener, ref Transmitter, ref Receiver, ref Blueprint, Silent))
		{
			return false;
		}
		if (Blueprint == null)
		{
			if (!Silent)
			{
				return Listener.ShowFailure(Speaker.Does("have") + " nothing to say.");
			}
			return false;
		}
		if (Dynamic)
		{
			ConversationsAPI.AddDynamicShim(Blueprint);
		}
		bool flag3;
		try
		{
			bool flag = IsDistant(Listener, Speaker, Transmitter, Receiver);
			if (!Mental.HasValue & flag)
			{
				Mental = true;
			}
			bool? flag2 = Mental;
			flag3 = true;
			if (flag2 == flag3 && !IsMentalConversationPossible(Listener, Speaker, !Silent, AllowCombat: false, ChargeUse))
			{
				flag3 = false;
			}
			else
			{
				if (!flag && IsPhysicalConversationPossible(Listener, Speaker, !Silent, AllowCombat: false, AllowFrozen: false, ChargeUse))
				{
					goto IL_0128;
				}
				if (!Mental.HasValue)
				{
					if (IsMentalConversationPossible(Listener, Speaker, !Silent, AllowCombat: false, ChargeUse))
					{
						goto IL_0128;
					}
					flag3 = false;
				}
				else
				{
					if (Mental != false)
					{
						goto IL_0128;
					}
					flag3 = false;
				}
			}
			goto end_IL_007e;
			IL_0128:
			Speaker?.PlayWorldSoundTag("ChatSound", (Mental == true) ? "sfx_interact_chat_telepathic" : "sfx_interact_chat", Listener?.CurrentCell);
			ConversationUI.HaveConversation(Blueprint, Speaker, Listener, Transmitter, Receiver, TradeEnabled: true, Mental != true, Mental == true);
			goto IL_01b7;
			end_IL_007e:;
		}
		finally
		{
			if (Dynamic)
			{
				ConversationsAPI.RemoveDynamicShim(Blueprint);
			}
		}
		return flag3;
		IL_01b7:
		return true;
	}

	public bool AttemptConversation(bool Silent = false, bool? Mental = null, IEvent ParentEvent = null)
	{
		return AttemptConversation(The.Player, ParentObject, null, null, GetActiveConversationBlueprint(), 0, Silent, Blueprint != null, Mental, ParentEvent);
	}

	public static string PronounExchangeDescription(GameObject Player, GameObject Speaker, bool SpeakerGivePronouns, bool SpeakerGetPronouns, bool SpeakerGetNewPronouns)
	{
		if (Speaker.Brain == null)
		{
			return null;
		}
		if (Speaker.HasProperty("FugueCopy"))
		{
			return null;
		}
		if (SpeakerGivePronouns & SpeakerGetPronouns)
		{
			return "you and " + Speaker.t() + " exchange pronouns; " + Speaker.its + " are " + Speaker.GetPronounSet().GetShortName();
		}
		if (SpeakerGivePronouns)
		{
			return Speaker.t() + Speaker.GetVerb("give") + " you " + Speaker.its + " pronouns, which are " + Speaker.GetPronounSet().GetShortName();
		}
		if (SpeakerGetNewPronouns)
		{
			return "you give " + Speaker.t() + " your new pronouns";
		}
		if (SpeakerGetPronouns)
		{
			return "you give " + Speaker.t() + " your pronouns";
		}
		return null;
	}
}
