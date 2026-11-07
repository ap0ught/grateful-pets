using System;
using System.Collections.Generic;
using System.Reflection;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.World.Conversations.Parts;
using XRL.World.Parts;
using XRL.World.Quests;

namespace XRL.World.Conversations;

[HasModSensitiveStaticCache]
[HasConversationDelegate]
public static class ConversationDelegates
{
	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, PredicateReceiver> _Predicates;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, ActionReceiver> _Actions;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, PartGeneratorReceiver> _PartGenerators;

	public static Dictionary<string, PredicateReceiver> Predicates
	{
		get
		{
			if (_Predicates == null)
			{
				LoadDelegates();
			}
			return _Predicates;
		}
	}

	public static Dictionary<string, ActionReceiver> Actions
	{
		get
		{
			if (_Actions == null)
			{
				LoadDelegates();
			}
			return _Actions;
		}
	}

	public static Dictionary<string, PartGeneratorReceiver> PartGenerators
	{
		get
		{
			if (_PartGenerators == null)
			{
				LoadDelegates();
			}
			return _PartGenerators;
		}
	}

	public static void CreatePredicate(MethodInfo Method, Dictionary<string, PredicateReceiver> Predicates)
	{
		CreatePredicate(Method.GetCustomAttribute<ConversationDelegate>(), Method, Predicates);
	}

	public static void CreatePredicate(ConversationDelegate Attribute, MethodInfo Method, Dictionary<string, PredicateReceiver> Predicates)
	{
		string text = Attribute.Key ?? Method.Name;
		int startIndex = text.UpToNthIndex(char.IsUpper, 2, 0);
		ConversationPredicate predicate = (ConversationPredicate)Method.CreateDelegate(typeof(ConversationPredicate));
		Predicates[text] = (IConversationElement e, string x) => predicate(DelegateContext.Set(e, x, The.Listener));
		if (Attribute.Inverse)
		{
			if (Attribute.InverseKey != null)
			{
				Predicates[Attribute.InverseKey] = (IConversationElement e, string x) => !predicate(DelegateContext.Set(e, x, The.Listener));
			}
			else
			{
				Predicates[text.Insert(startIndex, "Not")] = (IConversationElement e, string x) => !predicate(DelegateContext.Set(e, x, The.Listener));
			}
		}
		if (!Attribute.Speaker)
		{
			return;
		}
		if (Attribute.SpeakerKey != null)
		{
			Predicates[Attribute.SpeakerKey] = (IConversationElement e, string x) => predicate(DelegateContext.Set(e, x, The.Speaker));
		}
		else
		{
			Predicates[text.Insert(startIndex, "Speaker")] = (IConversationElement e, string x) => predicate(DelegateContext.Set(e, x, The.Speaker));
		}
		if (!Attribute.Inverse)
		{
			return;
		}
		if (Attribute.SpeakerInverseKey != null)
		{
			Predicates[Attribute.SpeakerInverseKey] = (IConversationElement e, string x) => !predicate(DelegateContext.Set(e, x, The.Speaker));
		}
		else
		{
			Predicates[text.Insert(startIndex, "SpeakerNot")] = (IConversationElement e, string x) => !predicate(DelegateContext.Set(e, x, The.Speaker));
		}
	}

	public static void CreateAction(MethodInfo Method, Dictionary<string, ActionReceiver> Actions)
	{
		CreateAction(Method.GetCustomAttribute<ConversationDelegate>(), Method, Actions);
	}

	public static void CreateAction(ConversationDelegate Attribute, MethodInfo Method, Dictionary<string, ActionReceiver> Actions)
	{
		string text = Attribute.Key ?? Method.Name;
		int startIndex = text.UpToNthIndex(char.IsUpper, 2, 0);
		ConversationAction action = Method.CreateDelegate(typeof(ConversationAction)) as ConversationAction;
		Actions[text] = (IConversationElement e, string x) =>
		{
			action(DelegateContext.Set(e, x, The.Listener));
		};
		if (!Attribute.Speaker)
		{
			return;
		}
		if (Attribute.SpeakerKey != null)
		{
			Actions[Attribute.SpeakerKey] = (IConversationElement e, string x) =>
			{
				action(DelegateContext.Set(e, x, The.Speaker));
			};
		}
		else
		{
			Actions[text.Insert(startIndex, "Speaker")] = (IConversationElement e, string x) =>
			{
				action(DelegateContext.Set(e, x, The.Speaker));
			};
		}
	}

	public static void CreatePart(ConversationDelegate Attribute, MethodInfo Method, Dictionary<string, PartGeneratorReceiver> PartGenerators)
	{
		string text = Attribute.Key ?? Method.Name;
		int startIndex = text.UpToNthIndex(char.IsUpper, 2, 0);
		ConversationPartGenerator generator = Method.CreateDelegate(typeof(ConversationPartGenerator)) as ConversationPartGenerator;
		PartGenerators[text] = (IConversationElement e, string x) => generator(DelegateContext.Set(e, x, The.Listener));
		if (!Attribute.Speaker)
		{
			return;
		}
		if (Attribute.SpeakerKey != null)
		{
			PartGenerators[Attribute.SpeakerKey] = (IConversationElement e, string x) => generator(DelegateContext.Set(e, x, The.Speaker));
		}
		else
		{
			PartGenerators[text.Insert(startIndex, "Speaker")] = (IConversationElement e, string x) => generator(DelegateContext.Set(e, x, The.Speaker));
		}
	}

	[PreGameCacheInit]
	public static void LoadDelegates()
	{
		_Predicates = new Dictionary<string, PredicateReceiver>();
		_Actions = new Dictionary<string, ActionReceiver>();
		_PartGenerators = new Dictionary<string, PartGeneratorReceiver>();
		Type typeFromHandle = typeof(ConversationDelegate);
		Type typeFromHandle2 = typeof(HasConversationDelegate);
		foreach (MethodInfo item in ModManager.GetMethodsWithAttribute(typeFromHandle, typeFromHandle2, Cache: false))
		{
			try
			{
				ConversationDelegate customAttribute = item.GetCustomAttribute<ConversationDelegate>();
				string text = customAttribute.Key ?? item.Name;
				if (item.ReturnType == typeof(bool))
				{
					CreatePredicate(customAttribute, item, Predicates);
				}
				else if (item.ReturnType == typeof(void))
				{
					CreateAction(customAttribute, item, Actions);
				}
				else if (item.ReturnType == typeof(IConversationPart))
				{
					CreatePart(customAttribute, item, PartGenerators);
				}
				else
				{
					MetricsManager.LogAssemblyError(item, "Conversation delegate " + text + " does not match signature.");
				}
			}
			catch (Exception message)
			{
				MetricsManager.LogAssemblyError(item, message);
			}
		}
	}

	[ConversationDelegate]
	public static bool IfHaveQuest(DelegateContext Context)
	{
		return The.Game.HasQuest(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveActiveQuest(DelegateContext Context)
	{
		return The.Game.HasUnfinishedQuest(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveQuestProperty(DelegateContext Context)
	{
		Context.Value.Split('~', out var First, out var Second);
		return The.Game.HasQuestProperty(First, Second);
	}

	[ConversationDelegate]
	public static bool IfFinishedQuest(DelegateContext Context)
	{
		return The.Game.HasFinishedQuest(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfFinishedQuestStep(DelegateContext Context)
	{
		return The.Game.FinishedQuestStep(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveObservation(DelegateContext Context)
	{
		return JournalAPI.HasObservation(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveObservationWithTag(DelegateContext Context)
	{
		return JournalAPI.HasObservationWithTag(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveSultanNoteWithTag(DelegateContext Context)
	{
		return JournalAPI.HasSultanNoteWithTag(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveVillageNote(DelegateContext Context)
	{
		return JournalAPI.HasVillageNote(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveMapNote(DelegateContext Context)
	{
		return JournalAPI.IsMapOrVillageNoteRevealed(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveState(DelegateContext Context)
	{
		return The.Game.HasGameState(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveConversationState(DelegateContext Context)
	{
		return The.Conversation.HasState(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfTestState(DelegateContext Context)
	{
		return The.Game.TestGameState(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveDelimitedState(DelegateContext Context)
	{
		int num = Context.Value.LastIndexOf(':');
		if (num == -1)
		{
			return false;
		}
		ReadOnlySpan<char> value = Context.Value.AsSpan(0, num);
		ReadOnlySpan<char> value2 = Context.Value.AsSpan(num + 1);
		char separator = ',';
		if (value[0] == '[')
		{
			separator = Context.Value[1];
			value = value.Slice(3);
		}
		return The.Game.HasDelimitedGameState(new string(value), separator, new string(value2));
	}

	[ConversationDelegate]
	public static bool IfHaveDelimitedConversationState(DelegateContext Context)
	{
		int num = Context.Value.LastIndexOf(':');
		if (num == -1)
		{
			return false;
		}
		ReadOnlySpan<char> value = Context.Value.AsSpan(0, num);
		ReadOnlySpan<char> value2 = Context.Value.AsSpan(num + 1);
		char separator = ',';
		if (value[0] == '[')
		{
			separator = Context.Value[1];
			value = value.Slice(3);
		}
		return The.Conversation.HasDelimitedState(new string(value), separator, new string(value2));
	}

	[ConversationDelegate]
	public static bool IfLastChoice(DelegateContext Context)
	{
		return ConversationUI.LastChoice?.ID == Context.Value;
	}

	[ConversationDelegate]
	public static bool IfHaveText(DelegateContext Context)
	{
		return (ConversationUI.CurrentNode?.Text)?.Contains(Context.Value) ?? false;
	}

	[ConversationDelegate]
	public static bool IfTime(DelegateContext Context)
	{
		if (!Context.Value.TryParseRange(out var Low, out var High))
		{
			return false;
		}
		long num = Calendar.TotalTimeTicks % 1200;
		if (High < Low)
		{
			if (num < Low)
			{
				return num <= High;
			}
			return true;
		}
		if (num >= Low)
		{
			return num <= High;
		}
		return false;
	}

	[ConversationDelegate]
	public static bool IfLedBy(DelegateContext Context)
	{
		if (The.Speaker.Brain?.PartyLeader == null)
		{
			return false;
		}
		if (Context.Value == "*")
		{
			return true;
		}
		if (Context.Value.EqualsNoCase("Player"))
		{
			return The.Speaker.IsPlayerLed();
		}
		return The.Speaker.Brain.IsLedBy(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfZoneName(DelegateContext Context)
	{
		if (The.ActiveZone != null)
		{
			string zoneBaseDisplayName = The.ZoneManager.GetZoneBaseDisplayName(The.ActiveZone.ZoneID, Mutate: false);
			if (zoneBaseDisplayName != null && zoneBaseDisplayName.Contains(Context.Value))
			{
				return true;
			}
			string zoneNameContext = The.ZoneManager.GetZoneNameContext(The.ActiveZone.ZoneID);
			if (zoneNameContext != null && zoneNameContext.Contains(Context.Value))
			{
				return true;
			}
		}
		return false;
	}

	[ConversationDelegate]
	public static bool IfZoneLevel(DelegateContext Context)
	{
		if (The.ActiveZone == null)
		{
			return false;
		}
		int z = The.ActiveZone.Z;
		if (Context.Value.TryParseRange(out var Low, out var High) && Low <= z)
		{
			return High >= z;
		}
		return false;
	}

	[ConversationDelegate]
	public static bool IfZoneTier(DelegateContext Context)
	{
		if (The.ActiveZone == null)
		{
			return false;
		}
		int newTier = The.ActiveZone.NewTier;
		if (Context.Value.TryParseRange(out var Low, out var High) && Low <= newTier)
		{
			return High >= newTier;
		}
		return false;
	}

	[ConversationDelegate]
	public static bool IfZoneWorld(DelegateContext Context)
	{
		return The.ActiveZone?.ZoneWorld == Context.Value;
	}

	[ConversationDelegate]
	public static bool IfZoneID(DelegateContext Context)
	{
		if (The.ActiveZone?.ZoneID == null)
		{
			return false;
		}
		return The.ActiveZone.ZoneID.StartsWith(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfZoneVisited(DelegateContext Context)
	{
		Dictionary<string, long> dictionary = The.ZoneManager?.VisitedTime;
		if (dictionary.IsNullOrEmpty())
		{
			return false;
		}
		return dictionary.ContainsKey(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfZoneHaveObject(DelegateContext Context)
	{
		Zone zone = The.Speaker?.CurrentZone;
		if (zone == null)
		{
			return false;
		}
		Context.Value.AsDelimitedSpans(',', out var First, out var Second);
		if (First.Length == 0)
		{
			return false;
		}
		if (Second.Length == 0)
		{
			return zone.HasObject((First.Length == Context.Value.Length) ? Context.Value : First.ToString());
		}
		if (!int.TryParse(Second, out var result))
		{
			MetricsManager.LogError("IfZoneBlueprint::Unable to parse " + Second.ToString() + " as distance integer");
			return false;
		}
		GameObject gameObject = zone.FindClosestObject(The.Speaker, First.ToString());
		if (gameObject != null)
		{
			return gameObject.DistanceTo(The.Speaker) <= result;
		}
		return false;
	}

	[ConversationDelegate]
	public static bool IfZoneHaveQuestGiver(DelegateContext Context)
	{
		Zone zone = The.Speaker?.CurrentZone;
		if (zone == null || The.Speaker.Brain == null)
		{
			return false;
		}
		Predicate<int> predicate;
		string key;
		switch (Context.Value)
		{
		case "Finished":
			predicate = (int x) => x == 2;
			key = "ZoneHaveQuestGiverFinished";
			break;
		case "Active":
			predicate = (int x) => x == 1;
			key = "ZoneHaveQuestGiverActive";
			break;
		case "Started":
			predicate = (int x) => x >= 1;
			key = "ZoneHaveQuestGiverStarted";
			break;
		case "Unstarted":
			predicate = (int x) => x == 0;
			key = "ZoneHaveQuestGiverUnstarted";
			break;
		case "Invalid":
			predicate = (int x) => x == -1;
			key = "ZoneHaveQuestGiverInvalid";
			break;
		default:
			return false;
		}
		if (The.Conversation.TryGetState<bool>(key, out var Value))
		{
			return Value;
		}
		bool flag = false;
		XRLGame game = The.Game;
		Brain brain = The.Speaker.Brain;
		Zone.ObjectEnumerator enumerator = zone.IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (brain.InSameFactionAs(current))
			{
				int questGiverState = game.GetQuestGiverState(current);
				if (predicate(questGiverState))
				{
					flag = true;
					break;
				}
			}
		}
		The.Conversation.State[key] = flag;
		return flag;
	}

	[ConversationDelegate(Inverse = false)]
	public static bool IfCheckpoint(DelegateContext Context)
	{
		if (Context.Value.EqualsNoCase("false"))
		{
			GameObject speaker = The.Speaker;
			if (speaker == null)
			{
				return true;
			}
			return speaker.CurrentZone?.IsCheckpoint() != true;
		}
		GameObject speaker2 = The.Speaker;
		if (speaker2 == null)
		{
			return false;
		}
		return speaker2.CurrentZone?.IsCheckpoint() == true;
	}

	[ConversationDelegate]
	public static bool IfUnderstood(DelegateContext Context)
	{
		if (Examiner.UnderstandingTable == null)
		{
			Loading.LoadTask("Loading medication names", Examiner.ResetGlobals);
		}
		if (Examiner.UnderstandingTable.TryGetValue(Context.Value, out var value))
		{
			return value == 2;
		}
		return false;
	}

	[ConversationDelegate]
	public static bool IfCommand(DelegateContext Context)
	{
		return PredicateEvent.GetFor(Context.Element, "IfCommand", Context.Value);
	}

	[ConversationDelegate]
	public static bool IfReputationAtLeast(DelegateContext Context)
	{
		int num = Context.Value.ToUpperInvariant() switch
		{
			"LOVED" => 2, 
			"LIKED" => 1, 
			"INDIFFERENT" => 0, 
			"DISLIKED" => -1, 
			"HATED" => -2, 
			_ => int.MaxValue, 
		};
		return The.Game.PlayerReputation.GetLevel(The.Speaker.GetPrimaryFaction()) >= num;
	}

	[ConversationDelegate]
	public static bool IfSlynthCandidate(DelegateContext Context)
	{
		LandingPadsSystem system = The.Game.GetSystem<LandingPadsSystem>();
		if (system == null)
		{
			return false;
		}
		if (Context.Value.EqualsNoCase("Mayor"))
		{
			string propertyOrTag = The.Speaker.GetPropertyOrTag("Mayor");
			if (propertyOrTag != null)
			{
				return system.candidateFactions.Contains(propertyOrTag);
			}
			return false;
		}
		if (Context.Value.EqualsNoCase("Primary"))
		{
			string primaryFaction = The.Speaker.GetPrimaryFaction();
			if (primaryFaction != null)
			{
				return system.candidateFactions.Contains(primaryFaction);
			}
			return false;
		}
		return system.candidateFactions.Contains(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfSlynthChosen(DelegateContext Context)
	{
		if (!The.Game.StringGameState.TryGetValue("SlynthSettlementFaction", out var value))
		{
			return false;
		}
		if (Context.Value.EqualsNoCase("Mayor"))
		{
			return value == The.Speaker.GetPropertyOrTag("Mayor");
		}
		if (Context.Value.EqualsNoCase("Primary"))
		{
			return value == The.Speaker.GetPrimaryFaction();
		}
		return value == Context.Value;
	}

	[ConversationDelegate]
	public static bool IfHindriarch(DelegateContext Context)
	{
		if (!The.Game.StringGameState.TryGetValue("HindrenMysteryOutcomeHindriarch", out var value))
		{
			return false;
		}
		return value == Context.Value;
	}

	[ConversationDelegate(Inverse = false)]
	public static bool IfAllowEscape(DelegateContext Context)
	{
		bool flag = Context.Value.EqualsNoCase("true");
		return (!(Context.Element.GetAncestor((IConversationElement x) => x is Node) is Node node) || node.AllowEscape) == flag;
	}

	[ConversationDelegate(Inverse = false)]
	public static bool IfIn100(DelegateContext Context)
	{
		if (int.TryParse(Context.Value, out var result))
		{
			if (result > 0)
			{
				if (result < 100)
				{
					return Stat.Random(1, 100) <= result;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfGenotype(DelegateContext Context)
	{
		return Context.Value.HasDelimitedSubstring(',', Context.Target.GetGenotype());
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfSubtype(DelegateContext Context)
	{
		return Context.Value.HasDelimitedSubstring(',', Context.Target.GetSubtype());
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfTrueKin(DelegateContext Context)
	{
		return Context.Target.IsTrueKin();
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfMutant(DelegateContext Context)
	{
		return Context.Target.IsMutant();
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHaveItem(DelegateContext Context)
	{
		return Context.Target.HasObjectInInventory(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHaveItemWithID(DelegateContext Context)
	{
		return Context.Target.HasObjectInInventory((GameObject o) => o.IDMatch(Context.Value));
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHavePart(DelegateContext Context)
	{
		return Context.Target.HasPart(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfWearingBlueprint(DelegateContext Context)
	{
		return Context.Target.HasObjectEquipped(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHaveBlueprint(DelegateContext Context)
	{
		Inventory inventory = Context.Target.Inventory;
		Body body = Context.Target.Body;
		DelimitedEnumeratorChar enumerator = Context.Value.DelimitedBy(',').GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			if (inventory != null)
			{
				foreach (GameObject @object in inventory.Objects)
				{
					if (current.SequenceEqual(@object.Blueprint))
					{
						return true;
					}
				}
			}
			if (body == null)
			{
				continue;
			}
			foreach (GameObject item in body.GetEquippedObjectsReadonly())
			{
				if (!item.IsNatural() && item.Physics.IsReal && current.SequenceEqual(item.Blueprint))
				{
					return true;
				}
			}
		}
		return false;
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHaveItemDescendsFrom(DelegateContext Context)
	{
		Inventory inventory = Context.Target.Inventory;
		Body body = Context.Target.Body;
		if (inventory != null)
		{
			foreach (GameObject @object in inventory.Objects)
			{
				if (@object.Physics.IsReal && @object.GetBlueprint().DescendsFrom(Context.Value))
				{
					return true;
				}
			}
		}
		if (body != null)
		{
			foreach (GameObject item in body.GetEquippedObjectsReadonly())
			{
				if (!item.IsNatural() && item.Physics.IsReal && item.GetBlueprint().DescendsFrom(Context.Value))
				{
					return true;
				}
			}
		}
		return false;
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHaveTag(DelegateContext Context)
	{
		return Context.Target.HasTag(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHaveProperty(DelegateContext Context)
	{
		return Context.Target.HasProperty(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHaveTagOrProperty(DelegateContext Context)
	{
		return Context.Target.HasTagOrProperty(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfLevelLessOrEqual(DelegateContext Context)
	{
		return Context.Target.Stat("Level") <= Convert.ToInt32(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHaveLiquid(DelegateContext Context)
	{
		Context.Value.AsDelimitedSpans(':', out var First, out var Second);
		if (!First.IsEmpty)
		{
			int freeDrams = Context.Target.GetFreeDrams(new string(First));
			if (freeDrams > 0)
			{
				if (Second.IsEmpty)
				{
					return true;
				}
				if (int.TryParse(Second, out var result) && freeDrams >= result)
				{
					return true;
				}
			}
		}
		return false;
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfAtLeastRank(DelegateContext Context)
	{
		return Context.Target.IsAtLeastFactionRank(The.Speaker.GetPrimaryFaction(), Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfAtLeastStanding(DelegateContext Context)
	{
		return Context.Target.IsAtLeastFactionStanding(The.Speaker.GetPrimaryFaction(), Convert.ToInt32(Context.Value));
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHasWorshipped(DelegateContext Context)
	{
		return Context.Target.HasWorshippedBySpec(Context.Value, The.Speaker.GetPrimaryFaction());
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHasBlasphemed(DelegateContext Context)
	{
		return Context.Target.HasBlasphemedBySpec(Context.Value, The.Speaker.GetPrimaryFaction());
	}

	[ConversationDelegate]
	public static bool IfQuestGiverState(DelegateContext Context)
	{
		int questGiverState = The.Game.GetQuestGiverState(The.Speaker);
		return Context.Value switch
		{
			"Finished" => questGiverState == 2, 
			"Active" => questGiverState == 1, 
			"Started" => questGiverState >= 1, 
			"Unstarted" => questGiverState == 0, 
			"Invalid" => questGiverState == -1, 
			_ => false, 
		};
	}

	[ConversationDelegate]
	public static bool IfQuestSignpost(DelegateContext Context)
	{
		Zone zone = The.Speaker?.CurrentZone;
		if (zone == null || The.Speaker.HasTagOrIntProperty("VillagePet") || The.Speaker.HasTagOrIntProperty("NoSignpost") || (Context.Value.EqualsNoCase("checkpoint") && !zone.IsCheckpoint()))
		{
			return false;
		}
		Context.Value = "Unstarted";
		if (!IfQuestGiverState(Context))
		{
			return IfZoneHaveQuestGiver(Context);
		}
		return false;
	}

	[ConversationDelegate(Speaker = true)]
	public static void GiveLiquid(DelegateContext Context)
	{
		Context.Value.AsDelimitedSpans(':', out var First, out var Second);
		if (!First.IsEmpty)
		{
			if (Second.IsEmpty || !int.TryParse(Second, out var result))
			{
				result = 1;
			}
			Context.Target.GiveDrams(result, new string(First));
		}
	}

	[ConversationDelegate(Speaker = true)]
	public static void UseLiquid(DelegateContext Context)
	{
		Context.Value.Split(':', out var First, out var Second);
		if (!First.IsNullOrEmpty())
		{
			if (Second.IsNullOrEmpty() || !int.TryParse(Second, out var result))
			{
				result = 1;
			}
			Context.Target.UseDrams(result, First);
		}
	}

	[ConversationDelegate]
	[Obsolete("TODO: Replace usage with parts")]
	public static void Execute(DelegateContext Context)
	{
		try
		{
			Context.Value.Split(':', out var First, out var Second);
			ModManager.ResolveType(First).GetMethod(Second).Invoke(null, null);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Error invoking command:" + Context.Value, x);
		}
	}

	[ConversationDelegate(Speaker = true)]
	public static void AwardXP(DelegateContext Context)
	{
		if (Context.Value.IsNullOrEmpty())
		{
			return;
		}
		ReadOnlySpan<char> s = Context.Value;
		bool flag = true;
		if (s.Length > 0 && s[0] == '!')
		{
			flag = false;
			s = s.Slice(1);
		}
		if (int.TryParse(s, out var result))
		{
			if (flag && Context.Target.IsPlayerControlled())
			{
				Context.Target.Physics.DidX("gain", "{{rules|" + result + "}} XP", "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup: true);
			}
			Context.Target.AwardXP(result, -1, 0, int.MaxValue, null, (Context.Target == The.Speaker) ? The.Listener : The.Speaker);
		}
	}

	[ConversationDelegate]
	public static void FinishQuest(DelegateContext Context)
	{
		The.Game.FinishQuest(Context.Value);
	}

	[ConversationDelegate]
	public static void Achievement(DelegateContext Context)
	{
		AchievementManager.SetAchievement(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static void FireEvent(DelegateContext Context)
	{
		string[] array = Context.Value.Split(',');
		Event obj = new Event(array[0]);
		for (int i = 1; i < array.Length; i++)
		{
			array[i].Split(':', out var First, out var Second);
			if (Second.IsNullOrEmpty())
			{
				obj.SetParameter(First, 1);
			}
			else
			{
				obj.SetParameter(First, Second);
			}
		}
		Context.Target.FireEvent(obj);
	}

	[ConversationDelegate]
	public static void SetStringState(DelegateContext Context)
	{
		Context.Value.Split(',', out var First, out var Second);
		if (Second.IsNullOrEmpty())
		{
			The.Game.SetStringGameState(First, Second);
		}
		else
		{
			The.Game.RemoveStringGameState(First);
		}
	}

	[ConversationDelegate]
	public static void SetIntState(DelegateContext Context)
	{
		Context.Value.AsDelimitedSpans(',', out var First, out var Second);
		int result;
		if (Second.IsEmpty)
		{
			The.Game.RemoveIntGameState(new string(First));
		}
		else if (int.TryParse(Second, out result))
		{
			The.Game.SetIntGameState(new string(First), result);
		}
	}

	[ConversationDelegate]
	public static void AddIntState(DelegateContext Context)
	{
		Context.Value.AsDelimitedSpans(',', out var First, out var Second);
		if (!Second.IsEmpty && int.TryParse(Second, out var result))
		{
			string key = new string(First);
			The.Game.IntGameState.TryGetValue(key, out var value);
			The.Game.IntGameState[key] = value + result;
		}
	}

	[ConversationDelegate]
	public static void SetBooleanState(DelegateContext Context)
	{
		Context.Value.AsDelimitedSpans(',', out var First, out var Second);
		bool result;
		if (Second.IsEmpty)
		{
			The.Game.RemoveBooleanGameState(Context.Value);
		}
		else if (bool.TryParse(Second, out result))
		{
			The.Game.SetBooleanGameState(new string(First), result);
		}
	}

	[ConversationDelegate]
	public static void ToggleBooleanState(DelegateContext Context)
	{
		The.Game.SetBooleanGameState(Context.Value, !The.Game.GetBooleanGameState(Context.Value));
	}

	[ConversationDelegate(Speaker = true)]
	public static void SetStringProperty(DelegateContext Context)
	{
		Context.Value.Split(',', out var First, out var Second);
		if (Second.IsNullOrEmpty())
		{
			Context.Target.RemoveStringProperty(First);
		}
		else
		{
			Context.Target.SetStringProperty(First, Second);
		}
	}

	[ConversationDelegate(Speaker = true)]
	public static void SetIntProperty(DelegateContext Context)
	{
		Context.Value.AsDelimitedSpans(',', out var First, out var Second);
		int result;
		if (Second.IsEmpty)
		{
			Context.Target.RemoveIntProperty(Context.Value);
		}
		else if (int.TryParse(Second, out result))
		{
			Context.Target.SetIntProperty(new string(First), result);
		}
	}

	[ConversationDelegate]
	public static void SetStringConversationState(DelegateContext Context)
	{
		Context.Value.Split(',', out var First, out var Second);
		if (Second.IsNullOrEmpty())
		{
			The.Conversation.RemoveState(Context.Value);
		}
		else
		{
			The.Conversation[First] = Second;
		}
	}

	[ConversationDelegate]
	public static void SetIntConversationState(DelegateContext Context)
	{
		Context.Value.AsDelimitedSpans(',', out var First, out var Second);
		int result;
		if (Second.IsEmpty)
		{
			The.Conversation.RemoveState(Context.Value);
		}
		else if (int.TryParse(Second, out result))
		{
			The.Conversation[new string(First)] = result;
		}
	}

	[ConversationDelegate]
	public static void SetBooleanConversationState(DelegateContext Context)
	{
		Context.Value.AsDelimitedSpans(',', out var First, out var Second);
		bool result;
		if (Second.IsEmpty)
		{
			The.Conversation.RemoveState(Context.Value);
		}
		else if (bool.TryParse(Second, out result))
		{
			The.Conversation[new string(First)] = result;
		}
	}

	[ConversationDelegate]
	public static void SetLeader(DelegateContext Context)
	{
		Brain brain = The.Speaker?.Brain;
		if (brain == null)
		{
			return;
		}
		if (Context.Value == "Listener")
		{
			brain.PartyLeader = The.Listener;
			return;
		}
		if (Context.Value == "Player")
		{
			brain.PartyLeader = The.Player;
			return;
		}
		GameObject gameObject = The.Speaker.CurrentZone?.FindClosestObject(The.Speaker, (GameObject x) => x.Blueprint == Context.Value);
		if (gameObject != null)
		{
			brain.PartyLeader = gameObject;
		}
	}

	[ConversationDelegate]
	public static void CallScript(DelegateContext Context)
	{
		int num = Context.Value.LastIndexOf('.');
		if (num != -1)
		{
			ModManager.ResolveType(Context.Value.Substring(0, num)).GetMethod(Context.Value.Substring(num + 1))?.Invoke(null, null);
		}
	}

	[ConversationDelegate]
	public static void ClearOwner(DelegateContext Context)
	{
		Zone activeZone = The.ActiveZone;
		for (int i = 0; i < activeZone.Height; i++)
		{
			for (int j = 0; j < activeZone.Width; j++)
			{
				int k = 0;
				for (int count = activeZone.Map[j][i].Objects.Count; k < count; k++)
				{
					if (activeZone.Map[j][i].Objects[k].HasTagOrProperty(Context.Value))
					{
						activeZone.Map[j][i].Objects[k].Physics.Owner = null;
					}
				}
			}
		}
	}

	[ConversationDelegate]
	public static void RevealMapNoteByID(DelegateContext Context)
	{
		RevealMapNote(Context);
	}

	[ConversationDelegate]
	public static void RevealMapNote(DelegateContext Context)
	{
		JournalMapNote mapNote = JournalAPI.GetMapNote(Context.Value);
		string text = The.Speaker?.Brain?.GetPrimaryFaction();
		if (mapNote != null && !text.IsNullOrEmpty())
		{
			Faction faction = Factions.Get(text);
			mapNote.Attributes.Add(faction.NoBuySecretString);
			if (faction.Visible)
			{
				mapNote.AppendHistory(" {{K|-learned from " + faction.GetFormattedName() + "}}");
			}
			JournalAPI.RevealMapNote(mapNote);
		}
	}

	[ConversationDelegate]
	public static void RevealObservation(DelegateContext Context)
	{
		JournalObservation observation = JournalAPI.GetObservation(Context.Value);
		string text = The.Speaker?.Brain?.GetPrimaryFaction();
		if (observation != null && !text.IsNullOrEmpty())
		{
			Faction faction = Factions.Get(text);
			observation.Attributes.Add(faction.NoBuySecretString);
			if (faction.Visible)
			{
				observation.AppendHistory(" {{K|-learned from " + faction.GetFormattedName() + "}}");
			}
			JournalAPI.RevealObservation(observation);
		}
	}

	[ConversationDelegate(Speaker = true)]
	public static void Notify(DelegateContext Context)
	{
		GenericNotifyEvent.Send(Context.Target, Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static void PromoteIfBelow(DelegateContext Context)
	{
		Context.Target.PromoteIfBelow(The.Speaker.GetPrimaryFaction(), Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static void EnableWorship(DelegateContext Context)
	{
		if (!Context.Value.IsNullOrEmpty())
		{
			Dictionary<string, string> dictionary = Context.Value.CachedDictionaryExpansion();
			dictionary.TryGetValue("Name", out var value);
			dictionary.TryGetValue("Faction", out var value2);
			dictionary.TryGetValue("Power", out var value3);
			int power = value3?.RollCached() ?? 0;
			if (value2.IsNullOrEmpty() || value2 == "*context*")
			{
				value2 = The.Speaker.GetPrimaryFaction();
			}
			if (!value.IsNullOrEmpty() && !value2.IsNullOrEmpty())
			{
				Factions.RegisterWorshippable(value, value2, power);
			}
		}
	}

	[ConversationDelegate(Speaker = true)]
	public static void PerformWorship(DelegateContext Context)
	{
		if (Context.Value.IsNullOrEmpty())
		{
			return;
		}
		Dictionary<string, string> dictionary = Context.Value.CachedDictionaryExpansion();
		dictionary.TryGetValue("Name", out var value);
		dictionary.TryGetValue("Faction", out var value2);
		if (value2.IsNullOrEmpty() || value2 == "*context*")
		{
			value2 = The.Speaker.GetPrimaryFaction();
		}
		if (!value.IsNullOrEmpty() && !value2.IsNullOrEmpty())
		{
			Worshippable worshippable = Factions.FindWorshippable(value2, value);
			if (worshippable == null)
			{
				dictionary.TryGetValue("Power", out var value3);
				int power = value3?.RollCached() ?? 0;
				worshippable = Factions.RegisterWorshippable(value, value2, power);
			}
			if (worshippable != null)
			{
				WorshipPerformedEvent.Send(Context.Target, The.Speaker, worshippable);
			}
		}
	}

	[ConversationDelegate(Speaker = true)]
	public static void PerformBlasphemy(DelegateContext Context)
	{
		if (Context.Value.IsNullOrEmpty())
		{
			return;
		}
		Dictionary<string, string> dictionary = Context.Value.CachedDictionaryExpansion();
		dictionary.TryGetValue("Name", out var value);
		dictionary.TryGetValue("Faction", out var value2);
		if (value2.IsNullOrEmpty() || value2 == "*context*")
		{
			value2 = The.Speaker.GetPrimaryFaction();
		}
		if (!value.IsNullOrEmpty() && !value2.IsNullOrEmpty())
		{
			Worshippable worshippable = Factions.FindWorshippable(value2, value);
			if (worshippable == null)
			{
				dictionary.TryGetValue("Power", out var value3);
				int power = value3?.RollCached() ?? 0;
				worshippable = Factions.RegisterWorshippable(value, value2, power);
			}
			if (worshippable != null)
			{
				BlasphemyPerformedEvent.Send(Context.Target, The.Speaker, worshippable);
			}
		}
	}

	[ConversationDelegate]
	public static IConversationPart StartQuest(DelegateContext Context)
	{
		return new QuestHandler(Context.Value);
	}

	[ConversationDelegate]
	public static IConversationPart IfHaveSecret(DelegateContext Context)
	{
		return new SecretHandler(Context.Value);
	}

	[ConversationDelegate]
	public static IConversationPart RequireSecret(DelegateContext Context)
	{
		return new SecretHandler(Context.Value, Require: true);
	}

	[ConversationDelegate]
	public static IConversationPart FlagSecret(DelegateContext Context)
	{
		return new SecretHandler(Context.Value, Require: false, Override: true);
	}

	[ConversationDelegate]
	public static IConversationPart GiveItem(DelegateContext Context)
	{
		return new ReceiveItem(Context.Value);
	}

	[ConversationDelegate]
	public static IConversationPart TakeItem(DelegateContext Context)
	{
		return new TakeItem(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static IConversationPart RemoveItem(DelegateContext Context)
	{
		return new TakeItem
		{
			FromSpeaker = !Context.Target.IsPlayer(),
			Blueprints = Context.Value,
			Remove = true
		};
	}

	[ConversationDelegate]
	public static IConversationPart CompleteQuestStep(DelegateContext Context)
	{
		string[] array = Context.Value.Split('~');
		int result = -1;
		int num = array[1].IndexOf('|');
		if (num >= 0)
		{
			if (!int.TryParse(array[1].Substring(num + 1), out result))
			{
				result = -1;
			}
			array[1] = array[1].Substring(0, num);
		}
		return new QuestHandler(array[0], array[1], result, 2);
	}
}
