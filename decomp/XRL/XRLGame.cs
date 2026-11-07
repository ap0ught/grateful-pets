using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Genkit;
using HistoryKit;
using LaundryBear.PlatformServices;
using Platform;
using Platform.IO;
using Qud.API;
using UnityEngine;
using XRL.Collections;
using XRL.Core;
using XRL.Messages;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL;

[Serializable]
public class XRLGame : IEventSource, IEventHandler
{
	public const int FLAG_SEEDED = 1;

	public const int FLAG_CODA = 2;

	public static TextConsole Console;

	public static ScreenBuffer Buffer;

	public string GameID;

	public string PlayerName = "Player";

	public int Flags;

	public bool Running;

	public bool bZoned = true;

	public string DeathReason = "You quit.";

	public string _DeathCategory;

	public long Turns;

	public long TimeTicks;

	public long PlayerActionTicks;

	public int TimeOffset;

	public long Segments;

	public long ActionTicks;

	public long _walltime;

	public int GameObjectIDSequence = 1;

	public int FactionIDSequence;

	public int BodyPartIDSequence;

	[NonSerialized]
	public float frameDelta;

	[NonSerialized]
	public float realtimeSinceStartup;

	[NonSerialized]
	public bool forceNoDeath;

	[NonSerialized]
	public string lastFindId;

	[NonSerialized]
	public XRL.World.GameObject lastFind;

	[NonSerialized]
	public bool InForceFieldPush;

	[NonSerialized]
	public bool Transient;

	[NonSerialized]
	public string[] ExitViewStack;

	public static readonly string MarketingPostfix = "";

	public static readonly System.Version MarketingVersion = new System.Version("1.0.5");

	public static readonly System.Version CoreVersion = typeof(XRLGame).Assembly.GetName().Version;

	[NonSerialized]
	public const int SaveVersion = 409;

	[NonSerialized]
	public const int MinSaveVersion = 395;

	[NonSerialized]
	public const int MaxSaveVersion = 409;

	[NonSerialized]
	public History sultanHistory;

	[NonSerialized]
	public Stopwatch WallTime;

	[NonSerialized]
	public Dictionary<string, Maze> WorldMazes = new Dictionary<string, Maze>();

	[NonSerialized]
	public GamePlayer Player;

	[NonSerialized]
	public Reputation PlayerReputation;

	[NonSerialized]
	public ActionManager ActionManager;

	[NonSerialized]
	public ZoneManager ZoneManager;

	[NonSerialized]
	public List<XRL.World.GameObject> Objects = new List<XRL.World.GameObject>();

	[NonSerialized]
	public EventRegistry RegisteredEvents = EventRegistry.Get();

	[NonSerialized]
	public StringMap<Quest> Quests = new StringMap<Quest>();

	[NonSerialized]
	public StringMap<Quest> FinishedQuests = new StringMap<Quest>();

	[NonSerialized]
	public Task SaveTask;

	[NonSerialized]
	private bool? _AlternateStart;

	[NonSerialized]
	public List<IGameSystem> Systems = new List<IGameSystem>();

	[NonSerialized]
	private List<IGameSystem> RemovedSystems = new List<IGameSystem>();

	[NonSerialized]
	public bool HasRemovedSystems;

	[NonSerialized]
	public Dictionary<string, string> StringGameState = new Dictionary<string, string>();

	[NonSerialized]
	public Dictionary<string, int> IntGameState = new Dictionary<string, int>();

	[NonSerialized]
	public Dictionary<string, long> Int64GameState = new Dictionary<string, long>();

	[NonSerialized]
	public Dictionary<string, object> ObjectGameState = new Dictionary<string, object>();

	[NonSerialized]
	public Dictionary<string, bool> BooleanGameState = new Dictionary<string, bool>();

	[NonSerialized]
	public Dictionary<string, object> TransientGameState = new Dictionary<string, object>();

	[NonSerialized]
	public HashSet<string> BlueprintsSeen = new HashSet<string>();

	private static char[] spaceSplitter = new char[1] { ' ' };

	[NonSerialized]
	public string _CacheDirectory;

	[NonSerialized]
	public bool DontSaveThisIsAReplay;

	public string DeathCategory
	{
		get
		{
			return _DeathCategory;
		}
		set
		{
			_DeathCategory = value;
		}
	}

	public bool IsSeeded
	{
		get
		{
			return Flags.HasBit(1);
		}
		set
		{
			Flags.SetBit(1, value);
		}
	}

	public bool IsCoda
	{
		get
		{
			return Flags.HasBit(2);
		}
		set
		{
			Flags.SetBit(2, value);
		}
	}

	public bool AlternateStart
	{
		get
		{
			bool valueOrDefault = _AlternateStart == true;
			if (!_AlternateStart.HasValue)
			{
				valueOrDefault = !GetStringGameState("embark", "Joppa").EqualsNoCase("Joppa");
				_AlternateStart = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public string gameMode
	{
		get
		{
			return GetStringGameState("GameMode");
		}
		set
		{
			SetStringGameState("GameMode", value);
		}
	}

	[Obsolete("Use XRLGame.Player without underscore")]
	public GamePlayer _Player
	{
		get
		{
			return Player;
		}
		set
		{
			Player = value;
		}
	}

	public XRLGame()
		: this(XRLCore._Console, XRLCore._Buffer)
	{
	}

	public XRLGame(TextConsole _Console, ScreenBuffer _Buffer)
	{
		Console = _Console;
		Buffer = _Buffer;
		ActionManager = new ActionManager();
		ActionManager.ActionQueue.Enqueue(null);
		ZoneManager = new ZoneManager(this);
		PlayerReputation = new Reputation();
		WallTime = new Stopwatch();
	}

	public T GetSystem<T>() where T : IGameSystem
	{
		return GetSystem(typeof(T)) as T;
	}

	public IGameSystem GetSystem(Type Type)
	{
		int i = 0;
		for (int count = Systems.Count; i < count; i++)
		{
			if ((object)Systems[i].GetType() == Type)
			{
				return Systems[i];
			}
		}
		return null;
	}

	public T RequireSystem<T>(Func<T> Generator) where T : IGameSystem
	{
		int i = 0;
		for (int count = Systems.Count; i < count; i++)
		{
			if (Systems[i].GetType() == typeof(T))
			{
				if (RemovedSystems != null && RemovedSystems.Contains(Systems[i]))
				{
					RemovedSystems.Remove(Systems[i]);
				}
				return Systems[i] as T;
			}
		}
		return AddSystem(Generator()) as T;
	}

	public T RequireSystem<T>() where T : IGameSystem, new()
	{
		int i = 0;
		for (int count = Systems.Count; i < count; i++)
		{
			if (Systems[i].GetType() == typeof(T))
			{
				return Systems[i] as T;
			}
		}
		return (T)AddSystem(new T());
	}

	public IGameSystem RequireSystem(string TypeID)
	{
		Type type = ModManager.ResolveType(TypeID);
		if ((object)type == null)
		{
			throw new TypeUnloadedException(TypeID);
		}
		return RequireSystem(type);
	}

	public IGameSystem RequireSystem(Type Type)
	{
		int i = 0;
		for (int count = Systems.Count; i < count; i++)
		{
			if (Systems[i].GetType() == Type)
			{
				return Systems[i];
			}
		}
		return AddSystem((IGameSystem)Activator.CreateInstance(Type, nonPublic: true));
	}

	public void RegisterEvent(IEventHandler Handler, int EventID, int Order = 0, bool Serialize = false)
	{
		RegisteredEvents.Register(Handler, EventID, Order, Serialize);
	}

	public void UnregisterEvent(IEventHandler Handler, int EventID)
	{
		RegisteredEvents.Unregister(Handler, EventID);
	}

	public bool WantEvent(int ID, int Cascade)
	{
		return RegisteredEvents.ContainsKey(ID);
	}

	public bool HandleEvent(MinEvent E)
	{
		if (!RegisteredEvents.Dispatch(E))
		{
			return false;
		}
		return true;
	}

	public bool HandleEvent(MinEvent E, bool DispatchPlayer)
	{
		if (!RegisteredEvents.Dispatch(E))
		{
			return false;
		}
		if (DispatchPlayer && Player._Body != null)
		{
			Player._Body.HandleEvent(E);
		}
		return true;
	}

	public void RemoveFlaggedSystems()
	{
		for (int i = 0; i < RemovedSystems.Count; i++)
		{
			RemoveSystem(RemovedSystems[i]);
		}
		RemovedSystems.Clear();
		HasRemovedSystems = false;
	}

	public void FlagSystemForRemoval(IGameSystem System)
	{
		RemovedSystems.Add(System);
		HasRemovedSystems = true;
	}

	public void FlagSystemsForRemoval(Type Type)
	{
		for (int i = 0; i < Systems.Count; i++)
		{
			if ((object)Systems[i].GetType() == Type)
			{
				FlagSystemForRemoval(Systems[i]);
			}
		}
	}

	public IGameSystem AddSystem(string system)
	{
		IGameSystem gameSystem = ModManager.CreateInstance<IGameSystem>(system);
		if (gameSystem == null)
		{
			MetricsManager.LogError("Unknown system class: " + system);
			return null;
		}
		return AddSystem(gameSystem);
	}

	public IGameSystem AddSystem(IGameSystem System, bool DoRegistration = true)
	{
		Systems.Add(System);
		System.Removed = false;
		System.OnAdded();
		if (DoRegistration)
		{
			System.ApplyRegistrar(this);
		}
		return System;
	}

	public void RemoveSystem(IGameSystem System)
	{
		if (Systems.Remove(System))
		{
			System.Removed = true;
			System.OnRemoved();
			System.ApplyUnregistrar(this);
		}
	}

	public void Release()
	{
		Running = false;
		if (ZoneManager != null)
		{
			ZoneManager.Release();
		}
		if (ActionManager != null)
		{
			ActionManager.Release();
		}
	}

	public void Clean()
	{
		RegisteredEvents.Clean();
		Player?._Body?.RegisteredEvents?.Clean();
	}

	public bool HasObjectGameState(string Value)
	{
		return ObjectGameState.ContainsKey(Value);
	}

	public bool HasIntGameState(string Value)
	{
		return IntGameState.ContainsKey(Value);
	}

	public bool HasInt64GameState(string Value)
	{
		return Int64GameState.ContainsKey(Value);
	}

	public bool HasStringGameState(string Value)
	{
		return StringGameState.ContainsKey(Value);
	}

	public bool HasBooleanGameState(string Value)
	{
		return BooleanGameState.ContainsKey(Value);
	}

	public bool HasBlueprintBeenSeen(string Name)
	{
		return BlueprintsSeen.Contains(Name);
	}

	public void BlueprintSeen(string Name)
	{
		BlueprintsSeen.Add(Name);
	}

	public void IncrementIntGameState(string Value, int Amount)
	{
		if (!IntGameState.ContainsKey(Value))
		{
			IntGameState.Add(Value, 0);
		}
		IntGameState[Value] += Amount;
	}

	public void IncrementInt64GameState(string Value, long Amount)
	{
		if (!Int64GameState.ContainsKey(Value))
		{
			Int64GameState.Add(Value, 0L);
		}
		Int64GameState[Value] += Amount;
	}

	public bool ToggleBooleanGameState(string Value)
	{
		if (!BooleanGameState.ContainsKey(Value))
		{
			BooleanGameState.Add(Value, value: false);
		}
		return BooleanGameState[Value] = !BooleanGameState[Value];
	}

	public void PopupStartQuest(Quest Quest)
	{
		IQuestSystem system = Quest.System;
		if (system != null)
		{
			system.ShowStartPopup();
		}
		else
		{
			Quest.ShowStartPopup();
		}
	}

	public Quest StartQuest(Quest Quest, string QuestGiverName = null, string QuestGiverLocationName = null, string QuestGiverLocationZoneID = null)
	{
		if (TryGetQuest(Quest.ID, out var Quest2))
		{
			return Quest2;
		}
		SoundManager.PlayUISound("Sounds/Misc/sfx_quest_gain");
		Quests.Add(Quest.ID, Quest);
		IQuestSystem questSystem = Quest.InitializeSystem();
		if (QuestGiverName != null)
		{
			Quest.QuestGiverName = QuestGiverName;
		}
		if (QuestGiverLocationName != null)
		{
			Quest.QuestGiverLocationName = QuestGiverLocationName;
		}
		if (QuestGiverLocationZoneID != null)
		{
			Quest.QuestGiverLocationZoneID = QuestGiverLocationZoneID;
		}
		if (Quest.QuestGiverLocationName == null)
		{
			Quest.QuestGiverLocationName = The.Player.CurrentZone?.DisplayName;
		}
		if (Quest.QuestGiverLocationZoneID == null)
		{
			Quest.QuestGiverLocationZoneID = The.Player.CurrentZone?.ZoneID;
		}
		PopupStartQuest(Quest);
		questSystem?.Start();
		Quest.Manager?.OnQuestAdded();
		QuestStartedEvent.Send(Quest);
		MetricsManager.LogEvent("Quest:Start:" + Quest.ID);
		Quest.Manager?.AfterQuestAdded();
		return Quest;
	}

	public Quest StartQuest(string QuestID, string QuestGiverName = null, string QuestGiverLocationName = null, string QuestGiverLocationZoneID = null)
	{
		if (TryGetQuest(QuestID, out var Quest))
		{
			return Quest;
		}
		if (QuestLoader.Loader.QuestsByID.TryGetValue(QuestID, out var value) || DynamicQuestsGameState.Instance.Quests.TryGetValue(QuestID, out value))
		{
			return StartQuest(value.Copy(), QuestGiverName, QuestGiverLocationName, QuestGiverLocationZoneID);
		}
		return null;
	}

	public void CompleteQuest(string questID)
	{
		if (FinishedQuests.ContainsKey(questID))
		{
			return;
		}
		if (!HasQuest(questID))
		{
			if (!QuestLoader.Loader.QuestsByID.ContainsKey(questID))
			{
				return;
			}
			StartQuest(questID);
		}
		foreach (string key in Quests[questID].StepsByID.Keys)
		{
			FinishQuestStep(questID, key);
		}
	}

	public long GetQuestFinishTime(string QuestID)
	{
		return GetInt64GameState("QuestFinishedTime_" + QuestID, -1L);
	}

	/// <summary>
	/// This adds a quest to finished even if it was never started.
	/// </summary>
	/// <param name="QuestID" />
	public void ForceFinishQuest(string QuestID)
	{
		if (Quests.TryGetValue(QuestID, out var Value))
		{
			FinishQuest(Value);
		}
		else
		{
			FinishedQuests.Add(QuestID, new Quest());
		}
	}

	public void FinishQuest(string QuestID)
	{
		if (Quests.TryGetValue(QuestID, out var Value))
		{
			FinishQuest(Value);
		}
	}

	public void PopupFinishQuest(Quest Quest)
	{
		IQuestSystem system = Quest.System;
		if (system != null)
		{
			system.ShowFinishPopup();
		}
		else
		{
			Quest.ShowFinishPopup();
		}
	}

	public void FinishQuest(Quest Quest)
	{
		if (Quest.Finished || FinishedQuests.ContainsKey(Quest.ID))
		{
			return;
		}
		SoundManager.PlayUISound("Sounds/Misc/sfx_quest_total_complete");
		Quest.Finished = true;
		FinishedQuests.Add(Quest.ID, Quest);
		SetInt64GameState("QuestFinishedTime_" + Quest.ID, XRL.World.Calendar.TotalTimeTicks);
		PopupFinishQuest(Quest);
		try
		{
			Quest.Finish();
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Quest::FinishQuest@" + Quest.ID, x);
		}
		Quest.Manager?.OnQuestComplete();
		if (The.Player != null)
		{
			ItemNaming.Opportunity(The.Player, null, Quest.GetInfluencer(), null, "Quest", 6, 0, 0, 1);
		}
		try
		{
			Quest.FinishPost();
		}
		catch (Exception x2)
		{
			MetricsManager.LogException("Quest::FinishQuest@" + Quest.ID, x2);
		}
		if (!Quest.Accomplishment.IsNullOrEmpty())
		{
			IPronounProvider pronounProvider = The.Player.GetPronounProvider();
			string text = ((!Quest.Hagiograph.IsNullOrEmpty()) ? Quest.Hagiograph : Quest.Accomplishment);
			text = text.StartReplace().AddReplacer("them", pronounProvider.Objective).AddReplacer("their", pronounProvider.PossessiveAdjective)
				.ToString();
			string value = ((!Quest.HagiographCategory.IsNullOrEmpty()) ? Quest.HagiographCategory : "DoesSomethingRad");
			MuralWeight muralWeight = MuralWeight.High;
			if (Quest.Hagiograph.IsNullOrEmpty())
			{
				muralWeight = MuralWeight.Nil;
			}
			if (Quest.ID.Equals("O Glorious Shekhinah!"))
			{
				if (The.Game.GetIntGameState("VisitedSixDayStilt") != 1)
				{
					string accomplishment = Quest.Accomplishment;
					string muralText = text;
					MuralCategory muralCategory = MuralCategoryHelpers.parseCategory(value);
					MuralWeight muralWeight2 = muralWeight;
					JournalAPI.AddAccomplishment(accomplishment, muralText, Quest.Gospel, null, "general", muralCategory, muralWeight2, null, -1L);
					XRLCore.Core.Game.SetIntGameState("VisitedSixDayStilt", 1);
					Achievement.SIX_DAY_STILT.Unlock();
				}
			}
			else
			{
				string accomplishment2 = Quest.Accomplishment;
				string muralText2 = text;
				MuralCategory muralCategory = MuralCategoryHelpers.parseCategory(value);
				MuralWeight muralWeight2 = muralWeight;
				JournalAPI.AddAccomplishment(accomplishment2, muralText2, Quest.Gospel, null, "general", muralCategory, muralWeight2, null, -1L);
			}
		}
		if (!Quest.Factions.IsNullOrEmpty() && !Quest.Reputation.IsNullOrEmpty())
		{
			int amount = int.Parse(Quest.Reputation);
			string[] array = Quest.Factions.Split(',');
			foreach (string faction in array)
			{
				The.Game.PlayerReputation.Modify(faction, amount, "Quest");
			}
		}
		if (!Quest.Achievement.IsNullOrEmpty())
		{
			AchievementManager.SetAchievement(Quest.Achievement);
		}
		QuestFinishedEvent.Send(Quest);
		MetricsManager.LogEvent("Quest:Complete:" + Quest.ID);
		XRL.World.GameObject player = The.Player;
		if (player != null)
		{
			if (player.HasStat("Level"))
			{
				MetricsManager.LogEvent("Quest:Complete:Level:" + Quest.ID + ":" + player.Stat("Level"));
			}
			if (player.HasStat("HP"))
			{
				MetricsManager.LogEvent("Quest:Complete:HP:" + Quest.ID + ":" + player.Stat("Level"));
			}
			MetricsManager.LogEvent("Quest:Complete:Turns:" + Quest.ID + ":" + XRLCore.Core.Game.Turns);
			MetricsManager.LogEvent("Quest:Complete:Walltime:" + Quest.ID + ":" + XRLCore.Core.Game._walltime);
		}
	}

	public bool FinishQuestStep(string QuestID, string QuestStepList, int XP = -1, bool CanFinishQuest = true, string ZoneID = null)
	{
		if (Quests.TryGetValue(QuestID, out var Value))
		{
			return FinishQuestStep(Value, QuestStepList, XP, CanFinishQuest, ZoneID);
		}
		return false;
	}

	public void PopupFinishQuestStep(Quest Quest, QuestStep Step)
	{
		IQuestSystem system = Quest.System;
		if (system != null)
		{
			system.ShowFinishStepPopup(Step);
		}
		else
		{
			Quest.ShowFinishStepPopup(Step);
		}
	}

	public bool FinishQuestStep(Quest Quest, string QuestStepList, int XP = -1, bool CanFinishQuest = true, string ZoneID = null)
	{
		bool result = false;
		try
		{
			if (!HasQuest(Quest.ID))
			{
				return result;
			}
			string[] array = QuestStepList.Split('~');
			foreach (string text in array)
			{
				if (!Quest.StepsByID.TryGetValue(text, out var value))
				{
					MetricsManager.LogError("FinishQuestStep", "Invalid quest step ID: " + Quest.ID + "@" + text);
					continue;
				}
				result = true;
				if (value.Finished)
				{
					continue;
				}
				SoundManager.PlayUISound("Sounds/Misc/sfx_quest_step_complete");
				value.Finished = true;
				value.Failed = false;
				if (!value.Awarded)
				{
					if (XP >= 0)
					{
						value.XP = XP;
					}
					PopupFinishQuestStep(Quest, value);
					if (value.XP > 0)
					{
						The.Player.AwardXP(value.XP, -1, 0, int.MaxValue, null, Quest.GetInfluencer(), null, null, ZoneID ?? Quest.Manager?.GetQuestZoneID());
					}
					value.Awarded = true;
				}
				Quest.FinishStep(value);
				Quest.Manager?.OnStepComplete(text);
				QuestStepFinishedEvent.Send(Quest, value);
			}
			if (CanFinishQuest)
			{
				CheckQuestFinishState(Quest);
			}
		}
		catch (Exception ex)
		{
			MetricsManager.LogError("FinishQuestStep", ex);
			MessageQueue.AddPlayerMessage("Error finishing quest step " + Quest.ID + " @ " + QuestStepList + " : " + ex.ToString(), 'R');
		}
		return result;
	}

	public void CheckQuestFinishState(string QuestID)
	{
		if (Quests.TryGetValue(QuestID, out var Value))
		{
			FinishQuest(Value);
		}
	}

	public void CheckQuestFinishState(Quest Quest)
	{
		if (Quest.Finished)
		{
			return;
		}
		foreach (var (_, questStep2) in Quest.StepsByID)
		{
			if (!questStep2.Finished && !questStep2.Optional)
			{
				return;
			}
		}
		FinishQuest(Quest);
	}

	public void PopupFailQuestStep(Quest Quest, QuestStep Step)
	{
		IQuestSystem system = Quest.System;
		if (system != null)
		{
			system.ShowFailStepPopup(Step);
		}
		else
		{
			Quest.ShowFailStepPopup(Step);
		}
	}

	public void FailQuestStep(string QuestID, string StepID, bool ShowMessage = true)
	{
		if (Quests.TryGetValue(QuestID, out var Value))
		{
			FailQuestStep(Value, StepID, ShowMessage);
		}
	}

	public void FailQuestStep(Quest Quest, string StepID, bool ShowMessage = true)
	{
		if (!HasQuest(Quest.ID))
		{
			return;
		}
		if (!Quest.StepsByID.TryGetValue(StepID, out var value))
		{
			MetricsManager.LogError("FinishQuestStep", "Invalid quest step ID: " + Quest.ID + "@" + StepID);
		}
		else if (!value.Failed)
		{
			SoundManager.PlayUISound("Sounds/Misc/sfx_quest_step_fail");
			value.Finished = false;
			value.Failed = true;
			if (ShowMessage)
			{
				PopupFailQuestStep(Quest, value);
			}
			Quest.FailStep(value);
		}
	}

	public bool FinishedQuest(string questID)
	{
		return FinishedQuests.ContainsKey(questID);
	}

	public bool FinishedQuestStep(string questID)
	{
		string[] array = questID.Split('~');
		if (!Quests.ContainsKey(array[0]))
		{
			return false;
		}
		if (!Quests[array[0]].StepsByID.ContainsKey(array[1]))
		{
			return false;
		}
		return Quests[array[0]].StepsByID[array[1]].Finished;
	}

	public bool HasGameState(string Name)
	{
		if (!HasStringGameState(Name) && !HasIntGameState(Name) && !HasObjectGameState(Name) && !HasInt64GameState(Name))
		{
			return HasBooleanGameState(Name);
		}
		return true;
	}

	public bool TestGameState(string Spec)
	{
		string[] array = Spec.Split(spaceSplitter, 3);
		string name = array[0];
		string text = ((array.Length >= 2) ? array[1] : null);
		string val = ((array.Length >= 3) ? array[2] : null);
		if (text != null && text[0] == '!')
		{
			return !TestGameStateInternal(name, text.Substring(1), val);
		}
		return TestGameStateInternal(name, text, val);
	}

	private bool TestGameStateInternal(string name, string op, string val)
	{
		if (op != null && val != null && StringGameState.TryGetValue(name, out var value))
		{
			switch (op)
			{
			case "=":
				if (value == val)
				{
					return true;
				}
				break;
			case "~":
				if (value.EqualsNoCase(val))
				{
					return true;
				}
				break;
			case "contains":
				if (value.Contains(val))
				{
					return true;
				}
				break;
			case "~contains":
				if (value.Contains(val, CompareOptions.IgnoreCase))
				{
					return true;
				}
				break;
			case "isin":
				if (val.Contains(value))
				{
					return true;
				}
				break;
			case "~isin":
				if (val.Contains(value, CompareOptions.IgnoreCase))
				{
					return true;
				}
				break;
			}
		}
		if (op != null && val != null && IntGameState.TryGetValue(name, out var value2) && int.TryParse(val, out var result))
		{
			switch (op)
			{
			case "=":
				break;
			case ">":
				goto IL_01c8;
			case ">=":
				goto IL_01ce;
			case "<":
				goto IL_01d4;
			case "<=":
				goto IL_01da;
			case "%":
				goto IL_01e0;
			case "&":
				goto IL_01e7;
			default:
				goto IL_01f5;
			case null:
				goto IL_032d;
			}
			if (value2 == result)
			{
				return true;
			}
		}
		goto IL_01ef;
		IL_01f5:
		if (val != null && Int64GameState.TryGetValue(name, out var value3) && long.TryParse(val, out var result2))
		{
			switch (op)
			{
			case "=":
				if (value3 == result2)
				{
					return true;
				}
				break;
			case ">":
				if (value3 > result2)
				{
					return true;
				}
				break;
			case ">=":
				if (value3 >= result2)
				{
					return true;
				}
				break;
			case "<":
				if (value3 < result2)
				{
					return true;
				}
				break;
			case "<=":
				if (value3 <= result2)
				{
					return true;
				}
				break;
			case "%":
				if (value3 % result2 == 0L)
				{
					return true;
				}
				break;
			case "&":
				if ((value3 & result2) == result2)
				{
					return true;
				}
				break;
			}
		}
		goto IL_032d;
		IL_01ce:
		if (value2 >= result)
		{
			return true;
		}
		goto IL_01ef;
		IL_01d4:
		if (value2 < result)
		{
			return true;
		}
		goto IL_01ef;
		IL_01ef:
		if (op != null)
		{
			goto IL_01f5;
		}
		goto IL_032d;
		IL_01da:
		if (value2 <= result)
		{
			return true;
		}
		goto IL_01ef;
		IL_032d:
		if (BooleanGameState.TryGetValue(name, out var value4))
		{
			bool result3;
			if (op == null && val == null)
			{
				if (value4)
				{
					return true;
				}
			}
			else if (op != null && val != null && bool.TryParse(val, out result3) && op == "=" && value4 == result3)
			{
				return true;
			}
		}
		return false;
		IL_01c8:
		if (value2 > result)
		{
			return true;
		}
		goto IL_01ef;
		IL_01e7:
		if ((value2 & result) == result)
		{
			return true;
		}
		goto IL_01ef;
		IL_01e0:
		if (value2 % result == 0)
		{
			return true;
		}
		goto IL_01ef;
	}

	public void PopupFailQuest(Quest Quest)
	{
		IQuestSystem system = Quest.System;
		if (system != null)
		{
			system.ShowFailPopup();
		}
		else
		{
			Quest.ShowFailPopup();
		}
	}

	public bool FailQuest(string QuestID)
	{
		if (!Quests.TryGetValue(QuestID, out var Value))
		{
			return false;
		}
		SoundManager.PlayUISound("Sounds/Misc/sfx_quest_total_fail");
		FinishedQuests.Add(QuestID, Value);
		Quests.Remove(QuestID);
		PopupFailQuest(Value);
		Value.Fail();
		return true;
	}

	public bool TryGetQuest(string QuestID, out Quest Quest)
	{
		if (!Quests.TryGetValue(QuestID, out Quest))
		{
			return FinishedQuests.TryGetValue(QuestID, out Quest);
		}
		return true;
	}

	public bool HasQuest(string ID)
	{
		if (Quests.ContainsKey(ID))
		{
			return true;
		}
		if (FinishedQuests.ContainsKey(ID))
		{
			return true;
		}
		return false;
	}

	public int GetQuestGiverState(XRL.World.GameObject Object)
	{
		if (Object == null)
		{
			return -1;
		}
		string value = Object.GetTagOrStringProperty("QuestGiver") ?? Object.GetTagOrStringProperty("GivesDynamicQuest");
		if (value.IsNullOrEmpty())
		{
			return -1;
		}
		int result = 0;
		DelimitedEnumeratorChar enumerator = value.DelimitedBy(';').GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			if (FinishedQuests.ContainsKey(current))
			{
				result = 2;
				break;
			}
			if (Quests.ContainsKey(current))
			{
				result = 1;
			}
		}
		return result;
	}

	public bool HasFinishedQuest(string ID)
	{
		return FinishedQuests.ContainsKey(ID);
	}

	public bool HasFinishedQuestStep(string questID, string questStepID)
	{
		if (FinishedQuests.ContainsKey(questID))
		{
			return true;
		}
		if (HasQuest(questID) && Quests[questID].StepsByID.ContainsKey(questStepID) && Quests[questID].StepsByID[questStepID].Finished)
		{
			return true;
		}
		return false;
	}

	public bool HasUnfinishedQuest(string questID)
	{
		if (HasQuest(questID))
		{
			return !HasFinishedQuest(questID);
		}
		return false;
	}

	public bool HasQuestProperty(string QuestID, string Key)
	{
		if (Quests.TryGetValue(QuestID, out var Value) || FinishedQuests.TryGetValue(QuestID, out Value))
		{
			return Value.HasProperty(Key);
		}
		return false;
	}

	public string GetStringGameState(string State, string Default = "")
	{
		if (StringGameState.TryGetValue(State, out var value))
		{
			return value;
		}
		return Default;
	}

	public void AppendStringGameState(string State, string Value)
	{
		if (!StringGameState.ContainsKey(State))
		{
			StringGameState.Add(State, Value);
		}
		else
		{
			StringGameState[State] += Value;
		}
	}

	public void AppendStringGameState(string State, string Value, string Separator)
	{
		if (!StringGameState.ContainsKey(State))
		{
			StringGameState.Add(State, Value);
		}
		else
		{
			StringGameState[State] = StringGameState[State] + Separator + Value;
		}
	}

	public void SetStringGameState(string State, string Value)
	{
		StringGameState[State] = Value;
	}

	public void RemoveStringGameState(string State)
	{
		StringGameState.Remove(State);
	}

	public bool HasDelimitedGameState(string State, char Separator, string Value)
	{
		if (StringGameState.TryGetValue(State, out var value))
		{
			return value.HasDelimitedSubstring(Separator, Value);
		}
		return false;
	}

	public bool HasAnyDelimitedGameState(string State, char Separator, string First, string Second)
	{
		if (StringGameState.TryGetValue(State, out var value))
		{
			if (!value.HasDelimitedSubstring(Separator, First))
			{
				return value.HasDelimitedSubstring(Separator, Second);
			}
			return true;
		}
		return false;
	}

	public bool TryAddDelimitedGameState(string State, char Separator, string Value)
	{
		if (StringGameState.TryGetValue(State, out var value))
		{
			if (value.HasDelimitedSubstring(Separator, Value))
			{
				return false;
			}
			StringGameState[State] = value + Separator + Value;
		}
		else
		{
			StringGameState[State] = Value;
		}
		return true;
	}

	public int GetIntGameState(string State, int Default = 0)
	{
		if (IntGameState.TryGetValue(State, out var value))
		{
			return value;
		}
		return Default;
	}

	public void SetIntGameState(string State, int Value)
	{
		IntGameState[State] = Value;
	}

	public int ModIntGameState(string State, int Value)
	{
		int num = GetIntGameState(State) + Value;
		IntGameState[State] = num;
		return num;
	}

	public void RemoveIntGameState(string State)
	{
		IntGameState.Remove(State);
	}

	public long GetInt64GameState(string State, long Default = 0L)
	{
		if (Int64GameState.TryGetValue(State, out var value))
		{
			return value;
		}
		return Default;
	}

	public void SetInt64GameState(string State, long Value)
	{
		Int64GameState[State] = Value;
	}

	public void RemoveInt64GameState(string State)
	{
		Int64GameState.Remove(State);
	}

	public bool TryGetFloatGameState(string State, out float Value)
	{
		if (IntGameState.TryGetValue(State, out var value))
		{
			Value = BitConverter.Int32BitsToSingle(value);
			return true;
		}
		Value = 0f;
		return false;
	}

	public float GetFloatGameState(string State, float Default = 0f)
	{
		if (IntGameState.TryGetValue(State, out var value))
		{
			return BitConverter.Int32BitsToSingle(value);
		}
		return Default;
	}

	public void SetFloatGameState(string State, float Value)
	{
		IntGameState[State] = BitConverter.SingleToInt32Bits(Value);
	}

	public void RemoveFloatGameState(string State)
	{
		IntGameState.Remove(State);
	}

	public bool TryGetDoubleGameState(string State, out double Value)
	{
		if (Int64GameState.TryGetValue(State, out var value))
		{
			Value = BitConverter.Int64BitsToDouble(value);
			return true;
		}
		Value = 0.0;
		return false;
	}

	public double GetDoubleGameState(string State, double Default = 0.0)
	{
		if (Int64GameState.TryGetValue(State, out var value))
		{
			return BitConverter.Int64BitsToDouble(value);
		}
		return Default;
	}

	public void SetDoubleGameState(string State, float Value)
	{
		Int64GameState[State] = BitConverter.DoubleToInt64Bits(Value);
	}

	public void RemoveDoubleGameState(string State)
	{
		Int64GameState.Remove(State);
	}

	public bool GetBooleanGameState(string State, bool Default = false)
	{
		if (BooleanGameState.TryGetValue(State, out var value))
		{
			return value;
		}
		return Default;
	}

	public bool TryGetBooleanGameState(string State, out bool Result)
	{
		return BooleanGameState.TryGetValue(State, out Result);
	}

	public void SetBooleanGameState(string State, bool Value)
	{
		BooleanGameState[State] = Value;
	}

	public void RemoveBooleanGameState(string State)
	{
		BooleanGameState.Remove(State);
	}

	public object GetObjectGameState(string State)
	{
		if (ObjectGameState.TryGetValue(State, out var value))
		{
			return value;
		}
		return null;
	}

	public void SetObjectGameState(string State, Location2D _Value)
	{
		throw new InvalidDataException("Don't set Location2D game states!");
	}

	public T RequireGameState<T>(string StateID, Func<T> Generator) where T : class
	{
		if (ObjectGameState.TryGetValue(StateID, out var value))
		{
			return value as T;
		}
		T val = Generator();
		ObjectGameState[StateID] = val;
		return val;
	}

	public T RequireTransientGameState<T>(string StateID, Func<T> Generator) where T : class
	{
		if (TransientGameState.TryGetValue(StateID, out var value))
		{
			return value as T;
		}
		T val = Generator();
		TransientGameState[StateID] = val;
		return val;
	}

	public T RequireTransientGameState<T>(string StateID) where T : class, new()
	{
		if (TransientGameState.TryGetValue(StateID, out var value))
		{
			return value as T;
		}
		T val = new T();
		TransientGameState[StateID] = val;
		return val;
	}

	public void SetObjectGameState(string State, object Value)
	{
		ObjectGameState[State] = Value;
	}

	public int GetWorldSeed(string Key = null)
	{
		_ = IsSeeded;
		if (!IntGameState.TryGetValue("WorldSeed", out var value))
		{
			value = ((!StringGameState.TryGetValue("OriginalWorldSeed", out var value2) || value2.IsNullOrEmpty() || value2[0] != '#' || !int.TryParse(value2.AsSpan(1), out var result)) ? new System.Random(Hash.String("WorldSeed" + value2)).Next(0, int.MaxValue) : result);
			IntGameState["WorldSeed"] = value;
		}
		if (Key == null)
		{
			return value;
		}
		return (int)Hash.FNV1A32(Key, (uint)value);
	}

	public void CreateNewGame()
	{
		Player = new GamePlayer();
		PlayerName = "";
		Quests = new StringMap<Quest>();
		FinishedQuests = new StringMap<Quest>();
		StringGameState = new Dictionary<string, string>();
		IntGameState = new Dictionary<string, int>();
		Int64GameState = new Dictionary<string, long>();
		ObjectGameState = new Dictionary<string, object>();
		BooleanGameState = new Dictionary<string, bool>();
		BlueprintsSeen = new HashSet<string>();
		WorldMazes = new Dictionary<string, Maze>();
		Segments = 0L;
		Turns = 0L;
		FactionIDSequence = 0;
		TimeOffset = Stat.Random(0, 365) * 1200 + 325;
		TimeTicks = TimeOffset;
		WallTime = new Stopwatch();
		WallTime.Start();
		_walltime = 0L;
		MemoryHelper.GCCollectMax();
	}

	public void SaveQuests(SerializationWriter Writer)
	{
		Writer.Write(Quests.Count);
		foreach (KeyValuePair<string, Quest> quest in Quests)
		{
			Writer.Write(quest.Key);
			quest.Value.Save(Writer);
		}
		Writer.Write(FinishedQuests.Count);
		foreach (KeyValuePair<string, Quest> finishedQuest in FinishedQuests)
		{
			Writer.Write(finishedQuest.Key);
			finishedQuest.Value.Save(Writer);
		}
	}

	public void LoadQuests(SerializationReader Reader)
	{
		int num = Reader.ReadInt32();
		Quests.Clear();
		Quests.EnsureCapacity(num);
		for (int i = 0; i < num; i++)
		{
			string key = Reader.ReadString();
			Quests.Add(key, Quest.Load(Reader));
		}
		num = Reader.ReadInt32();
		FinishedQuests.Clear();
		FinishedQuests.EnsureCapacity(num);
		for (int j = 0; j < num; j++)
		{
			string key2 = Reader.ReadString();
			FinishedQuests.Add(key2, Quest.Load(Reader));
		}
	}

	public void SaveSystems(SerializationWriter Writer)
	{
		RemoveFlaggedSystems();
		Writer.Write(Systems.Count);
		foreach (IGameSystem system in Systems)
		{
			system?.BeforeSave();
			Writer.Write(system);
			system?.AfterSave();
		}
	}

	public void LoadSystems(SerializationReader Reader)
	{
		int num = Reader.ReadInt32();
		Systems.EnsureCapacity(num);
		for (int i = 0; i < num; i++)
		{
			if (Reader.ReadComposite() is IGameSystem item)
			{
				Systems.Add(item);
			}
		}
	}

	public void SaveObjectGameState(SerializationWriter Writer)
	{
		Writer.WriteOptimized(ObjectGameState.Count);
		foreach (var (value, value2) in ObjectGameState)
		{
			Writer.WriteOptimized(value);
			Writer.WriteObject(value2);
		}
	}

	public void LoadObjectGameState(SerializationReader Reader)
	{
		int num = Reader.ReadOptimizedInt32();
		ObjectGameState = new Dictionary<string, object>(num);
		for (int i = 0; i < num; i++)
		{
			string key = Reader.ReadOptimizedString();
			object obj = Reader.ReadObject();
			if (obj != null)
			{
				ObjectGameState[key] = obj;
			}
		}
	}

	public string GetCacheDirectory(string FileName = null)
	{
		if (_CacheDirectory == null)
		{
			string text = DataManager.SyncedPath("Saves/" + GameID);
			try
			{
				Platform.IO.Directory.CreateDirectory(text);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("GetCacheDirectory", x);
				return null;
			}
			_CacheDirectory = text;
		}
		if (!FileName.IsNullOrEmpty())
		{
			return Platform.IO.Path.Combine(_CacheDirectory, FileName);
		}
		return _CacheDirectory;
	}

	public static XRLGame LoadCurrentGame(string GameName, bool ShowPopup = false, Dictionary<string, object> GameState = null)
	{
		Task<XRLGame> task = LoadGame(XRLCore.Core.Game.GetCacheDirectory(GameName), Session: false, ShowPopup, GameState);
		task.Wait();
		return task.Result;
	}

	private static async Task<bool> RestoreBackup(string originalPath, string backupSuffix)
	{
		if (await Platform.IO.File.ExistsAsync(originalPath + backupSuffix))
		{
			try
			{
				await Platform.IO.File.CopyAsync(originalPath + backupSuffix, originalPath, overwrite: true);
				return true;
			}
			catch
			{
				return false;
			}
		}
		return false;
	}

	public static async Task<XRLGame> LoadGame(string Path, bool Session = false, bool ShowPopup = false, Dictionary<string, object> GameState = null)
	{
		XRLGame xRLGame = null;
		XRLGame Return = null;
		string filePath = Path + ".sav.gz";
		int fileVersion = -1;
		string gameVersion = "{unknown}";
		try
		{
			if (!(await Platform.IO.File.ExistsAsync(filePath)) && !(await RestoreBackup(filePath, ".bak")))
			{
				filePath = Path + ".sav";
				if (!(await Platform.IO.File.ExistsAsync(filePath)))
				{
					if (ShowPopup)
					{
						Popup.Show("No saved game exists. (" + DataManager.SanitizePathForDisplay(Path) + ")");
					}
					return null;
				}
			}
			if (!SerializationReader.TryGetShared(out var Reader))
			{
				return null;
			}
			Loading.Status status = Loading.StartTask("Loading game");
			try
			{
				using Stream stream = Platform.IO.File.OpenRead(filePath);
				MemoryStream memory = Reader.Stream;
				if (stream.Length >= 2 && stream.ReadByte() == 31)
				{
					stream.ReadByte();
					_ = 139;
				}
				stream.Position = 0L;
				if (1 != 0)
				{
					using GZipStream decompressingStream = new GZipStream(stream, CompressionMode.Decompress);
					await decompressingStream.CopyToAsync(memory);
				}
				else
				{
					await stream.CopyToAsync(memory);
					stream.Dispose();
				}
				memory.Position = 0L;
				Reader.Start(SerializePlayer: true);
				XRL.World.GameObject.ExternalLoadBindings.Clear();
				if (Reader.ReadInt32() != 123457)
				{
					gameVersion = "2.0.167.0 or prior";
					throw new Exception("Save file is the incorrect version.");
				}
				fileVersion = Reader.FileVersion;
				gameVersion = Reader.ReadString();
				if (CapabilityManager.CurrentPlatformClassification() == CapabilityManager.PlatformClassification.PC)
				{
					try
					{
						if (fileVersion != 409 && Path.EndsWith("Primary"))
						{
							string text = filePath + $"_upgradebackup_{fileVersion}.gz";
							if (!Platform.IO.File.Exists(text))
							{
								Platform.IO.File.Copy(filePath, text);
								string text2 = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filePath), "Cache.db");
								string text3 = text2 + $"_upgradebackup_{fileVersion}.gz";
								if (Platform.IO.File.Exists(text2) && !Platform.IO.File.Exists(text3))
								{
									Platform.IO.File.Copy(text2, text3);
								}
							}
						}
					}
					catch (Exception x)
					{
						MetricsManager.LogException("save upgrade backup", x);
					}
				}
				if (Reader.FileVersion < 395 || Reader.FileVersion > 409)
				{
					throw new Exception("Save file is the incorrect version (" + gameVersion + ").");
				}
				The.Core.ResetGameBasedStaticCaches();
				UnityEngine.Debug.Log("Load game object...");
				Return = Reader.ReadInstanceFields<XRLGame>();
				xRLGame = XRLCore.Core.Game;
				XRLCore.Core.Game = Return;
				UnityEngine.Debug.Log("Load player...");
				Return.Player = GamePlayer.Load(Reader);
				UnityEngine.Debug.Log("Load systems...");
				Return.LoadSystems(Reader);
				if (Reader.ReadInt32() != 111111)
				{
					throw new Exception("checkval (1) wasn't correct");
				}
				UnityEngine.Debug.Log("Load zone manager...");
				try
				{
					Return.ZoneManager = ZoneManager.Load(Return, Reader);
				}
				catch (Exception x2)
				{
					Reader.Errors++;
					Reader.UnspoolTo(222222, Prior: true);
					MetricsManager.LogException("Exception loading zone manager", x2);
				}
				if (Reader.ReadInt32() != 222222)
				{
					throw new Exception("checkval (2) wasn't correct");
				}
				Return.ActionManager = ActionManager.Load(Reader);
				Return.LoadQuests(Reader);
				if (Reader.ReadInt32() != 333333)
				{
					throw new Exception("checkval (3) wasn't correct");
				}
				Return.PlayerReputation = new Reputation();
				Return.PlayerReputation.Read(Reader);
				if (Reader.ReadInt32() != 333444)
				{
					throw new Exception("checkval (34 wasn't correct");
				}
				UnityEngine.Debug.Log("Load globals...");
				Examiner.LoadGlobals(Reader);
				if (Reader.ReadInt32() != 444444)
				{
					throw new Exception("checkval (4) wasn't correct");
				}
				TinkerItem.LoadGlobals(Reader);
				Return.WorldMazes = Reader.ReadDictionary<string, Maze>();
				Factions.Load(Reader);
				XRLCore.Core.Game.sultanHistory = History.Load(Reader);
				if (Reader.ReadInt32() != 555555)
				{
					throw new Exception("checkval (5) wasn't correct");
				}
				Gender.Clear();
				PronounSet.Clear();
				Gender.LoadAll(Reader);
				PronounSet.LoadAll(Reader);
				if (Reader.ReadInt32() != 666666)
				{
					throw new Exception("checkval (6) wasn't correct");
				}
				Return.StringGameState = Reader.ReadDictionary<string, string>();
				Return.IntGameState = Reader.ReadDictionary<string, int>();
				Return.Int64GameState = Reader.ReadDictionary<string, long>();
				Return.BooleanGameState = Reader.ReadDictionary<string, bool>();
				Return.LoadObjectGameState(Reader);
				JournalAPI.Load(Reader);
				int num = Reader.ReadInt32();
				Return.BlueprintsSeen = new HashSet<string>(num);
				for (int i = 0; i < num; i++)
				{
					Return.BlueprintSeen(Reader.ReadString());
				}
				UnityEngine.Debug.Log("Read game objects...");
				Reader.FinalizeRead();
				Return.ActionManager.FinalizeRead();
				Return.RequireSystem(() => new PsychicHunterSystem());
				Return.RequireSystem(() => new AmbientSoundsSystem());
				Return.RequireSystem<NorthShevaSystem>();
				Return.Systems.ForEach((IGameSystem s) =>
				{
					s?.AfterLoad(Return);
				});
				if (Reader.Errors > 0)
				{
					Popup.DisplayLoadError(Reader, "save", Reader.Errors);
				}
				if (Reader.FileVersion < 396)
				{
					try
					{
						string stringGameState = Return.GetStringGameState("TauChime");
						if (!stringGameState.IsNullOrEmpty() && !stringGameState.HasDelimitedSubstring(',', "Dead"))
						{
							List<XRL.World.GameObject> inventoryEquipmentAndCyberneticsReadonly = Return.Player.Body.GetInventoryEquipmentAndCyberneticsReadonly();
							bool flag = false;
							foreach (XRL.World.GameObject item in inventoryEquipmentAndCyberneticsReadonly)
							{
								if (item.Blueprint == "TauChime")
								{
									flag = true;
									item.RemovePart<GameUnique>();
									break;
								}
							}
							if (!flag)
							{
								XRL.World.GameObject gameObject = XRL.World.GameObject.Create("TauChime");
								Return.SetStringGameState("TauChime", gameObject.ID);
								Return.Player.Body.ReceiveObject(gameObject);
							}
						}
					}
					catch (Exception x3)
					{
						MetricsManager.LogException("TauChimeCompat", x3);
					}
				}
			}
			finally
			{
				SerializationReader.ReleaseShared();
				status.Dispose();
			}
			The.Core.HostileWalkObjects = new List<XRL.World.GameObject>();
			The.Core.OldHostileWalkObjects = new List<XRL.World.GameObject>();
			The.Core.CludgeCreaturesRendered = new List<XRL.World.GameObject>();
			UnityEngine.Debug.Log("Collect...");
			MemoryHelper.GCCollect();
			The.Core.Reset();
			int intGameState = Return.GetIntGameState("NextRandomSeed");
			if (intGameState == 0)
			{
				intGameState = Return.GetIntGameState("RandomSeed");
			}
			Stat.ReseedFrom(Return.ZoneManager?.ActiveZone?.ZoneID + intGameState);
			MarkovBook.CorpusData.Clear();
			Gender.Init();
			PronounSet.Init();
			NameStyles.CheckInit();
			try
			{
				FungalVisionary.VisionLevel = 0;
				GameManager.Instance.GreyscaleLevel = Return.GetIntGameState("GreyscaleLevel");
			}
			catch (Exception)
			{
			}
			Return.ImportGameState(GameState);
			UnityEngine.Debug.Log("Seed: " + Return.GetStringGameState("OriginalWorldSeed"));
			Return.Player._Body.Inventory?.Validate();
			Return.Player.Messages.Cache_0_12Valid = false;
		}
		catch (Exception ex2)
		{
			string message = "That save file appears to be corrupt, you can try to restore the backup in your save directory (" + DataManager.SanitizePathForDisplay(Path) + ".sav.gz.bak) by removing the 'bak' file extension.";
			if (ModManager.TryGetStackMod(ex2, out var Mod, out var Frame))
			{
				MethodBase method = Frame.GetMethod();
				string text4 = method.DeclaringType?.FullName + "." + method.Name;
				Mod.Error(text4 + "::" + ex2);
				message = "That save file is likely not loading because of a mod error from " + Mod.DisplayTitleStripped + " (" + text4 + "), make sure the correct mods are enabled or contact the mod author.";
			}
			else
			{
				if (fileVersion < 409)
				{
					message = "That save file looks like it's from an older save format revision (" + gameVersion + "). Sorry!\n\nYou can probably change to a previous branch in your game client and get it to load if you want to finish it off.";
				}
				else if (fileVersion > 409)
				{
					message = "That save file looks like it's from a newer save format revision (" + gameVersion + ").\n\nYou can probably change to a newer branch in your game client and get it to load if you want to finish it off.";
				}
				MetricsManager.LogException("XRLGame.LoadGame::", ex2, "serialization_error");
			}
			if (ShowPopup)
			{
				Popup.Show(message);
			}
			throw;
		}
		try
		{
			int num2 = 0;
			for (int count = Return.Systems.Count; num2 < count; num2++)
			{
				Return.Systems[num2].ApplyRegistrar(Return);
			}
			AbilityManager.UpdateFavorites();
			The.Player.FireEvent("GameRestored");
			AfterGameLoadedEvent.Send(Return);
			Return.PlayerReputation.AfterGameLoaded();
			ModManager.CallAfterGameLoaded();
			if (Return.ZoneManager?.ActiveZone != null)
			{
				ZoneManager.PaintWalls(Return.ZoneManager.ActiveZone);
				ZoneManager.PaintWater(Return.ZoneManager.ActiveZone);
				Return.ZoneManager.ActiveZone.Activated();
				ZoneManager.ActivateObjects(Return.ZoneManager.ActiveZone);
			}
			MemoryHelper.GCCollectMax();
			xRLGame?.Release();
			if (Session)
			{
				Return.StartSession();
			}
			if (fileVersion < 398)
			{
				PartRack partRack = Return?.Player?._Body?.PartsList;
				if (!partRack.IsNullOrEmpty())
				{
					foreach (IPart item2 in partRack)
					{
						if (item2 is BaseMutation baseMutation && !baseMutation.Variant.IsNullOrEmpty() && !GameObjectFactory.Factory.HasBlueprint(baseMutation.Variant) && int.TryParse(baseMutation.Variant, out var result))
						{
							List<string> variants = baseMutation.GetVariants();
							if (variants.IsNullOrEmpty())
							{
								baseMutation.Variant = null;
							}
							else if (result < 0 || result >= variants.Count)
							{
								baseMutation.Variant = variants.GetRandomElement(new System.Random());
							}
							else
							{
								baseMutation.Variant = variants[result];
							}
						}
					}
				}
			}
			if (fileVersion < 399)
			{
				XRL.World.GameObject body = Return.Player.Body;
				if (body.Render.RenderLayer < 100)
				{
					body.Render.RenderLayer = 100;
				}
			}
		}
		catch (Exception x4)
		{
			MetricsManager.LogException("AfterGameLoaded", x4);
		}
		return Return;
	}

	public bool RestoreCheckpoint()
	{
		if (LoadCurrentGame("Checkpoint") == null)
		{
			return false;
		}
		Task task = SaveCopy("Checkpoint", "Primary");
		Task task2 = CacheCopy("CheckpointCache", "Cache", Reset: true);
		task.Wait();
		task2.Wait();
		return true;
	}

	public void Checkpoint()
	{
		SaveGame("Checkpoint", "Saving game", CopyCache: true, CopyPrimary: true);
	}

	public void QuickSave()
	{
		SaveGame("Quick", "Saving game", CopyCache: true, CopyPrimary: true);
	}

	public async Task<bool> QuickLoad()
	{
		if (!Platform.IO.File.Exists(The.Game.GetCacheDirectory("Quick.sav.gz")))
		{
			return LoadCurrentGame("Primary") != null;
		}
		if (LoadCurrentGame("Quick") == null)
		{
			return false;
		}
		Task task = SaveCopy("Quick", "Primary");
		Task cacheCopy = CacheCopy("QuickCache", "Cache", Reset: true);
		await task;
		await cacheCopy;
		return true;
	}

	public async Task CacheCopy(string From, string To, bool Reset = false)
	{
		From += ".db";
		To += ".db";
		DataManager.CacheOperation operation = await DataManager.StartCacheOperationAsync(Exclusive: true);
		try
		{
			if (Reset)
			{
				DataManager.ResetCacheConnection();
			}
			else
			{
				DataManager.CloseCacheConnection();
			}
			string path = GetCacheDirectory(From);
			if (await Platform.IO.File.ExistsAsync(path))
			{
				if (CapabilityManager.HasCapability(CapabilityManager.CapabilityType.IsSwitch))
				{
					await Platform.IO.File.CopyOverwriteByChunkAsync(path, GetCacheDirectory(To));
					return;
				}
				PlatformIOResult platformIOResult = await Blob.CopyAsync(path, GetCacheDirectory(To), overwrite: true);
				if (platformIOResult.result == StorageResult.InUse)
				{
					using (Loading.StartTask("File in use, retrying..."))
					{
						DataManager.ResetCacheConnection();
						await Task.Delay(5000);
						DataManager.ResetCacheConnection();
						platformIOResult = await Blob.CopyAsync(path, GetCacheDirectory(To), overwrite: true);
					}
				}
				platformIOResult.ThrowIfFailed();
				return;
			}
			StorageResult storageResult = await Blob.DeleteAsync(GetCacheDirectory(To));
			if (storageResult == StorageResult.InUse)
			{
				using (Loading.StartTask("File in use, retrying..."))
				{
					DataManager.ResetCacheConnection();
					await Task.Delay(5000);
					DataManager.ResetCacheConnection();
					storageResult = await Blob.DeleteAsync(GetCacheDirectory(To));
				}
			}
			storageResult.ThrowIfFailed();
		}
		catch (Exception ex)
		{
			MetricsManager.LogException("Exception copying cache", ex);
			ExceptionDispatchInfo.Throw(ex);
		}
		finally
		{
			operation.Dispose();
		}
	}

	public async Task SaveCopy(string srcName, string dstName)
	{
		string srcPath = GetCacheDirectory(srcName + ".sav.gz");
		string dstPath = GetCacheDirectory(dstName + ".sav.gz");
		if (!(await Platform.IO.File.ExistsAsync(srcPath)))
		{
			return;
		}
		bool isDstBackedUp = false;
		PlatformIOResult platformIOResult2;
		if (await Platform.IO.File.ExistsAsync(dstPath))
		{
			PlatformIOResult platformIOResult = await Blob.CopyAsync(dstPath, dstPath + ".bak", overwrite: true);
			if (platformIOResult.WasSuccessful())
			{
				isDstBackedUp = true;
			}
			else
			{
				platformIOResult2 = platformIOResult;
				UnityEngine.Debug.LogError("Exception making save backup: " + platformIOResult2.ToString());
			}
		}
		PlatformIOResult copyBlob = await Blob.CopyAsync(srcPath, dstPath, overwrite: true);
		if (copyBlob.WasSuccessful())
		{
			PlatformIOResult platformIOResult3 = await Blob.CopyAsync(GetCacheDirectory(srcName + ".json"), GetCacheDirectory(dstName + ".json"), overwrite: true);
			if (!platformIOResult3.WasSuccessful())
			{
				platformIOResult2 = platformIOResult3;
				UnityEngine.Debug.LogError("Exception copying save info: " + platformIOResult2.ToString());
			}
			return;
		}
		platformIOResult2 = copyBlob;
		UnityEngine.Debug.LogError("Exception making save backup: " + platformIOResult2.ToString());
		if (isDstBackedUp)
		{
			if ((await Blob.CopyAsync(dstPath + ".bak", dstPath, overwrite: true)).WasSuccessful())
			{
				UnityEngine.Debug.LogWarning("Backup restored to: " + dstName);
				return;
			}
			platformIOResult2 = copyBlob;
			UnityEngine.Debug.LogError("Exception making save backup: " + platformIOResult2.ToString());
		}
	}

	public void StartSession()
	{
		using (DelayShutdown.AutoScopeForceMainThread())
		{
			string cacheDirectory = GetCacheDirectory("Primary.sav.gz");
			if (!Platform.IO.File.Exists(cacheDirectory) || !int.TryParse(Options.GetOption("OptionSessionBackups", "3"), out var result) || result <= 0)
			{
				return;
			}
			string cacheDirectory2 = GetCacheDirectory("Cache.db");
			string text = DataManager.LocalPath("Session/");
			string format = text + "Session{0}.sav.gz";
			string format2 = text + "SessionCache{0}.db";
			try
			{
				Platform.IO.Directory.CreateDirectory(text);
				for (int num = result; num > 0; num--)
				{
					string text2 = string.Format(format, num);
					string text3 = string.Format(format2, num);
					if (Platform.IO.File.Exists(text2))
					{
						if (num + 1 > result)
						{
							Platform.IO.File.Delete(text2);
						}
						else
						{
							string text4 = string.Format(format, num + 1);
							Platform.IO.File.Delete(text4);
							Platform.IO.File.Move(text2, text4);
						}
					}
					if (Platform.IO.File.Exists(text3))
					{
						if (num + 1 > result)
						{
							Platform.IO.File.Delete(text3);
						}
						else
						{
							string text5 = string.Format(format2, num + 1);
							Platform.IO.File.Delete(text5);
							Platform.IO.File.Move(text3, text5);
						}
					}
				}
				Platform.IO.File.Copy(cacheDirectory, string.Format(format, 1));
				if (Platform.IO.File.Exists(cacheDirectory2))
				{
					using (DataManager.StartCacheOperation(Exclusive: true))
					{
						DataManager.CloseCacheConnection();
						Platform.IO.File.Copy(cacheDirectory2, string.Format(format2, 1));
						return;
					}
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Exception rolling session backups", x);
			}
		}
	}

	public SaveGameJSON SaveGameInfo()
	{
		XRL.World.GameObject player = The.Player;
		RenderEvent renderEvent = player.RenderForUI("SaveGameInfo");
		TimeSpan timeSpan = TimeSpan.FromTicks(_walltime);
		return new SaveGameJSON
		{
			SaveVersion = 409,
			GameVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
			ID = GameID,
			Name = PlayerName,
			Level = player.Statistics["Level"].Value,
			GenoSubType = "",
			GameMode = GetStringGameState("GameMode", "Classic"),
			CharIcon = renderEvent.Tile,
			FColor = renderEvent.GetForegroundColorChar(),
			DColor = renderEvent.GetDetailColorChar(),
			Location = ZoneManager.GetZoneDisplayName(player.CurrentZone.ZoneID),
			InGameTime = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}",
			Turn = Turns,
			SaveTime = DateTime.Now.ToLongDateString() + " at " + DateTime.Now.ToLongTimeString(),
			ModsEnabled = ModManager.GetRunningMods().ToList()
		};
	}

	public Task SaveGame(string GameName, string Message = "Saving game", bool CopyCache = false, bool CopyPrimary = false)
	{
		if (Transient)
		{
			return null;
		}
		string primaryFilePath;
		IStorage storage;
		long totalStorage;
		long journalSize;
		long totalStorageMax;
		long journalSizeMax;
		using (DelayShutdown.AutoScopeForceMainThread())
		{
			if (DontSaveThisIsAReplay)
			{
				return null;
			}
			Task saveTask = SaveTask;
			if (saveTask != null && !saveTask.IsCompleted)
			{
				return null;
			}
			if (!Running)
			{
				return SaveTask = null;
			}
			string fileName = GameName + ".sav.gz";
			primaryFilePath = GetCacheDirectory(fileName);
			MemoryHelper.GCCollectMax();
			if (!SerializationWriter.TryGetShared(out var Writer))
			{
				return SaveTask = null;
			}
			Loading.Status status = Loading.StartTask(Message);
			try
			{
				if (WallTime != null)
				{
					_walltime += WallTime.ElapsedTicks;
					WallTime.Reset();
					WallTime.Start();
				}
				else
				{
					WallTime = new Stopwatch();
					WallTime.Start();
				}
				SaveGameJSON obj = SaveGameInfo();
				SetIntGameState("GreyscaleLevel", GameManager.Instance.GreyscaleLevel);
				SetIntGameState("NextRandomSeed", Stat.Rnd.Next());
				Writer.Start(409, SerializePlayer: true);
				Writer.Write(123457);
				Writer.Write(Assembly.GetExecutingAssembly().GetName().Version.ToString());
				Writer.WriteFields(this);
				Player.Save(Writer);
				SaveSystems(Writer);
				Writer.Write(111111);
				ZoneManager.Save(Writer);
				Writer.Write(222222);
				ActionManager.Save(Writer);
				SaveQuests(Writer);
				Writer.Write(333333);
				PlayerReputation.Write(Writer);
				Writer.Write(333444);
				Examiner.SaveGlobals(Writer);
				Writer.Write(444444);
				TinkerItem.SaveGlobals(Writer);
				Writer.Write(WorldMazes);
				Factions.Save(Writer);
				sultanHistory.Save(Writer);
				Writer.Write(555555);
				Gender.SaveAll(Writer);
				PronounSet.SaveAll(Writer);
				Writer.Write(666666);
				Writer.Write(StringGameState);
				Writer.Write(IntGameState);
				Writer.Write(Int64GameState);
				Writer.Write(BooleanGameState);
				SaveObjectGameState(Writer);
				JournalAPI.Save(Writer);
				Writer.Write(BlueprintsSeen.Count);
				foreach (string item in BlueprintsSeen)
				{
					Writer.Write(item);
				}
				Writer.FinalizeWrite();
				MetricsManager.LogInfo($"Done {Message} in {WallTime.ElapsedMilliseconds}ms");
				string cacheDirectory = GetCacheDirectory(GameName + ".json");
				bool restoreBackup = false;
				try
				{
					string content = JsonUtility.ToJson(obj, prettyPrint: true);
					Platform.IO.File.WriteAllText(cacheDirectory, content);
					if (Platform.IO.File.Exists(primaryFilePath))
					{
						Platform.IO.File.Copy(primaryFilePath, primaryFilePath + ".bak", overwrite: true);
						restoreBackup = true;
					}
					using (MemoryStream memoryStream = new MemoryStream())
					{
						using (GZipStream gZipStream = new GZipStream(memoryStream, System.IO.Compression.CompressionLevel.Fastest, leaveOpen: true))
						{
							byte[] buffer = Writer.Stream.GetBuffer();
							gZipStream.Write(buffer, 0, (int)Writer.Stream.Length);
						}
						byte[] content2 = memoryStream.ToArray();
						Platform.IO.File.WriteAllBytes(primaryFilePath, content2);
					}
					MemoryHelper.GCCollect();
					CheckSave(primaryFilePath);
					Task task = null;
					Task task2 = null;
					if (CopyPrimary)
					{
						task = SaveCopy(GameName, "Primary");
					}
					if (CopyCache)
					{
						task2 = CacheCopy("Cache", GameName + "Cache");
					}
					task?.Wait();
					task2?.Wait();
				}
				catch (Exception exception)
				{
					SaveGameError(primaryFilePath, exception, restoreBackup);
				}
				finally
				{
					SerializationWriter.ReleaseShared();
				}
				return null;
			}
			catch (Exception exception2)
			{
				SerializationWriter.ReleaseShared();
				SaveGameError(primaryFilePath, exception2);
				return SaveTask = Task.FromException(exception2);
			}
			finally
			{
				MemoryHelper.GCCollectMax();
				status.Dispose();
				if (CapabilityManager.HasCapability(CapabilityManager.CapabilityType.RequiresStorageExpansion))
				{
					storage = State.GetStorage();
					storage.GetTotalStorageQuota(out totalStorage, out journalSize);
					storage.GetMaxStorageQuota(out totalStorageMax, out journalSizeMax);
					storage.GetUsedStorageQuota(OnUsedStorageReceived);
				}
			}
		}
		void OnUsedStorageReceived(StorageResult result, long quotaUsed)
		{
			bool flag = false;
			bool flag2 = false;
			_ = totalStorage;
			long num = journalSize;
			if (ShouldExpandStorage(totalStorage, quotaUsed))
			{
				long num2 = Math.Min(totalStorageMax, totalStorage + 33554432);
				flag = num2 != totalStorage;
				flag2 = num2 == totalStorage;
			}
			Platform.IO.FileInfo info = Platform.IO.File.GetInfo(GetCacheDirectory("Cache.db"));
			if (ShouldExpandJournal(info.Length, journalSize))
			{
				num = Math.Min(journalSizeMax, journalSize + 16777216);
				flag |= num != journalSize;
				_ = journalSize;
			}
			if (flag && storage.ExpandStorageQuota(Math.Min(totalStorageMax, totalStorage * 2), Math.Min(journalSizeMax, journalSize * 2)) != StorageResult.Success)
			{
				OutOfStorageException exception3 = new OutOfStorageException("Storage space is running low. Please make more space to avoid loss of data.");
				SaveGameError(primaryFilePath, exception3);
			}
			if (flag2)
			{
				OutOfStorageException exception4 = new OutOfStorageException("Storage space is running low. Please make more space to avoid loss of data.");
				SaveGameError(primaryFilePath, exception4);
			}
		}
	}

	private bool ShouldExpandStorage(long totalStorage, long storageUsed)
	{
		return (float)(totalStorage - storageUsed) < (float)totalStorage * 0.125f;
	}

	private bool ShouldExpandJournal(long cacheSize, long journalSize)
	{
		return (float)(journalSize - cacheSize) < (float)journalSize * 0.125f;
	}

	public void CheckSave(string Path)
	{
		using Stream stream = Platform.IO.File.OpenRead(Path);
		if (stream.Length < 2)
		{
			throw new IOException("Save file is less than two bytes.");
		}
		Span<byte> buffer = stackalloc byte[2];
		stream.Read(buffer);
		if (buffer[0] != 31 || buffer[1] != 139)
		{
			throw new IOException("Save file is missing gzip header.");
		}
	}

	public void SaveGameError(string Path, Exception Exception, bool RestoreBackup = false)
	{
		MetricsManager.LogException("SaveGame", Exception);
		if (RestoreBackup && Platform.IO.File.Exists(Path + ".bak"))
		{
			try
			{
				Platform.IO.File.Copy(Path + ".bak", Path, overwrite: true);
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogWarning($"Failed to restore backup save file. {arg}");
			}
		}
		Popup.ShowFailAsync("There was a fatal exception attempting to save your game. Caves of Qud attempted to recover your prior save. You probably want to close the game and reload your most recent save. It'd be helpful to send the save and logs to support@freeholdgames.com");
	}

	public void ImportGameState(Dictionary<string, object> GameState)
	{
		if (GameState == null)
		{
			return;
		}
		foreach (KeyValuePair<string, object> item in GameState)
		{
			if (item.Value is string value)
			{
				SetStringGameState(item.Key, value);
				continue;
			}
			int? num = item.Value as int?;
			if (num.HasValue)
			{
				SetIntGameState(item.Key, num.Value);
				continue;
			}
			long? num2 = item.Value as long?;
			if (num2.HasValue)
			{
				SetInt64GameState(item.Key, num2.Value);
				continue;
			}
			bool? flag = item.Value as bool?;
			if (flag.HasValue)
			{
				SetBooleanGameState(item.Key, flag.Value);
			}
			else
			{
				SetObjectGameState(item.Key, item.Value);
			}
		}
	}
}
