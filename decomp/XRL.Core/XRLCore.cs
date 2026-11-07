using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Platform.IO;
using Platform.Performance;
using Qud.API;
using Qud.UI;
using Sheeter;
using UnityEngine;
using XRL.CharacterBuilds;
using XRL.Collections;
using XRL.Help;
using XRL.Messages;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World;
using XRL.World.AI;
using XRL.World.AI.Pathfinding;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using XRL.World.Conversations.Parts;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

namespace XRL.Core;

[Serializable]
[HasModSensitiveStaticCache]
public class XRLCore
{
	public class SortObjectBydistanceToPlayer : Comparer<XRL.World.GameObject>
	{
		public override int Compare(XRL.World.GameObject ox, XRL.World.GameObject oy)
		{
			Cell currentCell = The.Player.CurrentCell;
			if (currentCell == null)
			{
				return 0;
			}
			Cell currentCell2 = ox.CurrentCell;
			Cell currentCell3 = oy.CurrentCell;
			if (currentCell2 == null)
			{
				return 1;
			}
			if (currentCell3 == null)
			{
				return 0;
			}
			Point point = new Point(currentCell2.X, currentCell2.Y);
			Point point2 = new Point(currentCell3.X, currentCell3.Y);
			int x = currentCell.X;
			int y = currentCell.Y;
			int num = (x - point.X) * (x - point.X) + (y - point.Y) * (y - point.Y);
			int value = (x - point2.X) * (x - point2.X) + (y - point2.Y) * (y - point2.Y);
			return num.CompareTo(value);
		}
	}

	public class SortCellBydistanceToObject : Comparer<Cell>
	{
		private XRL.World.GameObject _Target;

		public SortCellBydistanceToObject(XRL.World.GameObject Target)
		{
			_Target = Target;
		}

		public override int Compare(Cell x, Cell y)
		{
			if (_Target.Physics.CurrentCell == null)
			{
				return 0;
			}
			if (x == y)
			{
				return 0;
			}
			if (x.X == y.X && x.Y == y.Y)
			{
				return 0;
			}
			int x2 = _Target.Physics.CurrentCell.X;
			int y2 = _Target.Physics.CurrentCell.Y;
			int num = (x2 - x.X) * (x2 - x.X) + (y2 - x.Y) * (y2 - x.Y);
			int num2 = (x2 - y.X) * (x2 - y.X) + (y2 - y.Y) * (y2 - y.Y);
			if (num == num2)
			{
				return Stat.Random(-1, 1);
			}
			return num.CompareTo(num2);
		}
	}

	public class SortPoint
	{
		public int X;

		public int Y;

		public SortPoint(int _x, int _y)
		{
			X = _x;
			Y = _y;
		}
	}

	public class SortBydistanceToPlayer : Comparer<SortPoint>
	{
		public override int Compare(SortPoint x, SortPoint y)
		{
			if (x.Equals(y))
			{
				return 0;
			}
			if (x == y)
			{
				return 0;
			}
			Cell currentCell = The.Player.CurrentCell;
			if (currentCell == null)
			{
				return 0;
			}
			if (x == y)
			{
				return 0;
			}
			if (x.X == y.X && x.Y == y.Y)
			{
				return 0;
			}
			int x2 = currentCell.X;
			int y2 = currentCell.Y;
			int num = (x2 - x.X) * (x2 - x.X) + (y2 - x.Y) * (y2 - x.Y);
			int num2 = (x2 - y.X) * (x2 - y.X) + (y2 - y.Y) * (y2 - y.Y);
			if (num == num2)
			{
				return Stat.Random(-1, 1);
			}
			return num.CompareTo(num2);
		}
	}

	[UIView("NewGame", true, true, false, "Menu", null, false, 0, false)]
	public class EmptyCoreUIs
	{
	}

	public static XRLCore Core;

	public XRLGame Game;

	public static XRLManual Manual;

	public static TextConsole _Console;

	public static ScreenBuffer _Buffer;

	public static ParticleManager ParticleManager;

	public static int CurrentFrame;

	public static int CurrentFrameLong;

	public static int CurrentFrame10;

	public static int CurrentFrameLong10;

	public static double CurrentFrameAccumulator;

	public static Stopwatch FrameTimer = new Stopwatch();

	private static int _PlayerVisibilityRadius = int.MinValue;

	public bool EnableAnimation = true;

	public bool ShowFPS;

	public bool VisAllToggle;

	public bool CheatMaxMod;

	public bool AllowWorldMapParticles;

	public bool HPWarning;

	private const int MAINMENU_INDEX_CONTROLREMAP = 0;

	private const int MAINMENU_INDEX_OPTIONS = 1;

	private const int MAINMENU_INDEX_WORLDINFO = 2;

	private const int MAINMENU_INDEX_SAVEANDEXIT = 3;

	private const int MAINMENU_INDEX_ABANDON = 4;

	[NonSerialized]
	public bool cool;

	[NonSerialized]
	public bool IDKFA;

	[NonSerialized]
	public bool IgnoreMe;

	[NonSerialized]
	public bool Calm;

	[NonSerialized]
	public static bool bThreadFocus = true;

	[NonSerialized]
	public string MoveConfirmDirection;

	[NonSerialized]
	public int RenderedObjects;

	public string _PlayerWalking = "";

	public SortPoint AutoexploreTarget;

	public CleanQueue<SortPoint> PlayerAvoid = new CleanQueue<SortPoint>();

	public SortPoint LastCell;

	[NonSerialized]
	public static int MemCheckTurns = 0;

	[NonSerialized]
	public const long InitialMemCheckHeadRoom = 2500000000L;

	[NonSerialized]
	public static long MemCheckHeadRoom = 2500000000L;

	[NonSerialized]
	public static bool sixArmsSet = false;

	[NonSerialized]
	[ModSensitiveStaticCache(true)]
	private static List<Action<XRLCore>> OnBeginPlayerTurnCallbacks = new List<Action<XRLCore>>();

	[NonSerialized]
	[ModSensitiveStaticCache(true)]
	private static List<Action<XRLCore>> OnEndPlayerTurnCallbacks = new List<Action<XRLCore>>();

	[NonSerialized]
	[ModSensitiveStaticCache(true)]
	private static List<Action<XRLCore>> OnEndPlayerTurnSingleCallbacks = new List<Action<XRLCore>>();

	[NonSerialized]
	[ModSensitiveStaticCache(true)]
	private static List<Action<XRLCore>> OnPassedTenPlayerTurnCallbacks = new List<Action<XRLCore>>();

	[NonSerialized]
	[ModSensitiveStaticCache(true)]
	private static List<Action<XRLCore, ScreenBuffer>> AfterRenderCallbacks = new List<Action<XRLCore, ScreenBuffer>>();

	[NonSerialized]
	[ModSensitiveStaticCache(true)]
	private static List<Action<string>> OnNewMessageLogEntryCallbacks = new List<Action<string>>();

	private static List<Cell> SmartUseCells = new List<Cell>();

	public static bool CludgeTargetRendered = false;

	public List<XRL.World.GameObject> CludgeCreaturesRendered;

	public static bool RenderFloorTextures = true;

	public static bool RenderHiddenPlayer = true;

	[NonSerialized]
	public System.Random ConfusionRng = new System.Random();

	private static int nFrame = 0;

	public List<XRL.World.GameObject> OldHostileWalkObjects = new List<XRL.World.GameObject>();

	public List<XRL.World.GameObject> HostileWalkObjects = new List<XRL.World.GameObject>();

	private bool _isNewGameModFlow;

	public static int lastWait = 10;

	public static bool waitForSegmentOnGameThread = false;

	public static Thread CoreThread = null;

	public static string OSXDLCPath = "";

	public static string DLCPath = "";

	public static string EmbeddedModsPath = "";

	public static string DataPath = "";

	public static string _SavePath = null;

	public static string _LocalPath = null;

	public static string _SyncedPath = null;

	public static bool bStarted = false;

	public static bool IsCoreThread => CoreThread == Thread.CurrentThread;

	public static XRL.World.GameObject player
	{
		get
		{
			if (Core == null)
			{
				return null;
			}
			if (The.Player == null)
			{
				return null;
			}
			return The.Player;
		}
	}

	public static int PlayerVisibilityRadius
	{
		get
		{
			if (_PlayerVisibilityRadius == int.MinValue)
			{
				_PlayerVisibilityRadius = The.Player?.GetVisibilityRadius() ?? int.MinValue;
			}
			return _PlayerVisibilityRadius;
		}
		set
		{
			_PlayerVisibilityRadius = value;
		}
	}

	[Obsolete("Don't use this use player.GetConfusion() instead! Will be removed ~Q3 2021")]
	public int ConfusionLevel
	{
		get
		{
			if (Game == null)
			{
				return 0;
			}
			if (The.Player == null)
			{
				return 0;
			}
			return The.Player.GetConfusion();
		}
		set
		{
			if (Game == null)
			{
				MetricsManager.LogWarning("Trying to set confusion level without a game.");
			}
			else if (The.Player == null)
			{
				MetricsManager.LogWarning("Trying to set confusion level without a player body.");
			}
			else
			{
				The.Player.SetIntProperty("ConfusionLevel", value);
			}
		}
	}

	[Obsolete("Don't use this use player.GetFuriousConfusion() instead! Will be removed ~Q3 2021")]
	public int FuriousConfusion
	{
		get
		{
			if (Game == null)
			{
				return 0;
			}
			if (The.Player == null)
			{
				return 0;
			}
			return The.Player.GetFuriousConfusion();
		}
		set
		{
			if (Game == null)
			{
				MetricsManager.LogWarning("Trying to set confusion level without a game.");
			}
			else if (The.Player == null)
			{
				MetricsManager.LogWarning("Trying to set confusion level without a player body.");
			}
			else
			{
				The.Player.SetIntProperty("FuriousConfusionLevel", value);
			}
		}
	}

	public float XPMul
	{
		get
		{
			return The.Game.GetFloatGameState("XPMul", 1f);
		}
		set
		{
			The.Game.SetFloatGameState("XPMul", value);
		}
	}

	public static long CurrentTurn
	{
		get
		{
			if (Core.Game == null)
			{
				return 0L;
			}
			return Core.Game.TimeTicks;
		}
	}

	public string PlayerWalking
	{
		get
		{
			return _PlayerWalking;
		}
		set
		{
			_PlayerWalking = value;
			if (value == "")
			{
				Loading.SetLoadingStatus(null);
				PlayerAvoid.Clear();
				AutoexploreTarget = null;
			}
		}
	}

	public static string SavePath
	{
		get
		{
			if (_SavePath == null)
			{
				return Path.PersistentDataPath();
			}
			return _SavePath;
		}
		set
		{
			_SavePath = value;
		}
	}

	public static string LocalPath
	{
		get
		{
			if (_LocalPath == null)
			{
				_LocalPath = Path.Combine(SavePath, "Local");
			}
			return _LocalPath;
		}
		set
		{
			_LocalPath = value;
		}
	}

	public static string SyncedPath
	{
		get
		{
			if (_SyncedPath == null)
			{
				_SyncedPath = Path.Combine(SavePath, "Synced");
			}
			return _SyncedPath;
		}
		set
		{
			_SyncedPath = value;
		}
	}

	public bool TilesEnabled => Globals.RenderMode == RenderModeType.Tiles;

	public void Reset()
	{
		FrameTimer.Reset();
		FrameTimer.Start();
		if (Core.Game.WallTime == null)
		{
			Core.Game.WallTime = new Stopwatch();
		}
		Core.Game.WallTime.Reset();
		Core.Game.WallTime.Start();
		JournalAPI.Init();
		GameManager.Instance.gameQueue.clear();
	}

	public static int GetCurrentFrameAtFPS(int fps, float speed = 1f)
	{
		return (int)((float)FrameTimer.ElapsedMilliseconds * speed / (float)(1000 / fps));
	}

	public bool InAvoidList(SortPoint P)
	{
		for (int i = 0; i < PlayerAvoid.Items.Count; i++)
		{
			SortPoint sortPoint = PlayerAvoid.Items[i];
			if (sortPoint.X == P.X && sortPoint.Y == P.Y)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckInventoryObject(XRL.World.GameObject GO)
	{
		if (GO.CurrentCell != null)
		{
			return false;
		}
		if (GO.Equipped != null)
		{
			return false;
		}
		if (GO.InInventory != The.Player)
		{
			return false;
		}
		return true;
	}

	/// <summary>Force all mod/game sensitive static caches to be loaded</summary>
	public void LoadEverything()
	{
		if (_isNewGameModFlow)
		{
			return;
		}
		foreach (Type item in ModManager.GetTypesWithAttribute(typeof(HasGameBasedStaticCacheAttribute)).Union(ModManager.GetTypesWithAttribute(typeof(HasModSensitiveStaticCacheAttribute))))
		{
			foreach (MethodInfo item2 in from mi in item.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				where mi.GetCustomAttributes(typeof(PreGameCacheInitAttribute), inherit: false).Count() > 0
				select mi)
			{
				item2.Invoke(null, new object[0]);
			}
		}
	}

	public void HotloadConfiguration(bool bGenerateCorpusData = false)
	{
		ModManager.ResetModSensitiveStaticCaches();
		LoadEverything();
		WorldFactory.Factory.Init();
		MutationFactory.CheckInit();
		MessageQueue.AddPlayerMessage("Configuration hotloaded...");
		WriteConsoleLine("Hot Loading Books...\n");
		BookUI.InitBooks(bGenerateCorpusData);
	}

	public bool AttemptSmartUse(Cell TargetCell, int MinPriority = 0)
	{
		XRL.World.GameObject gameObject = SmartUse.FindPlayerSmartUseObject(TargetCell, MinPriority);
		if ((gameObject == null || gameObject.IsOpenLiquidVolume()) && TargetCell == The.Player.CurrentCell && TargetCell.HasObjectWithPart("Physics", SmartUse.CanPlayerTake))
		{
			return The.Player.FireEvent(XRL.World.Event.New("CommandGet", "GetOne", false, "TargetCell", TargetCell, "SmartUse", true));
		}
		if (gameObject != null)
		{
			return SmartUse.PlayerPerformSmartUse(gameObject, MinPriority);
		}
		return false;
	}

	public void UpdateOverlay()
	{
	}

	public static void RegisterOnBeginPlayerTurnCallback(Action<XRLCore> action)
	{
		OnBeginPlayerTurnCallbacks.Add(action);
	}

	public static void RemoveOnBeginPlayerTurnCallback(Action<XRLCore> action)
	{
		OnBeginPlayerTurnCallbacks.Remove(action);
	}

	public static void RegisterOnEndPlayerTurnCallback(Action<XRLCore> action, bool Single = false)
	{
		if (Single)
		{
			OnEndPlayerTurnSingleCallbacks.Add(action);
		}
		else
		{
			OnEndPlayerTurnCallbacks.Add(action);
		}
	}

	public static void RemoveOnEndPlayerTurnCallback(Action<XRLCore> action, bool Single = false)
	{
		if (Single)
		{
			OnEndPlayerTurnSingleCallbacks.Remove(action);
		}
		else
		{
			OnEndPlayerTurnCallbacks.Remove(action);
		}
	}

	public static void RegisterOnPassedTenPlayerTurnCallback(Action<XRLCore> action)
	{
		OnPassedTenPlayerTurnCallbacks.Add(action);
	}

	public static void TenPlayerTurnsPassed()
	{
		int i = 0;
		for (int count = OnPassedTenPlayerTurnCallbacks.Count; i < count; i++)
		{
			OnPassedTenPlayerTurnCallbacks[i](Core);
		}
	}

	public static void RegisterAfterRenderCallback(Action<XRLCore, ScreenBuffer> action)
	{
		AfterRenderCallbacks.Add(action);
	}

	public static void RegisterNewMessageLogEntryCallback(Action<string> action)
	{
		if (!OnNewMessageLogEntryCallbacks.Contains(action))
		{
			OnNewMessageLogEntryCallbacks.Add(action);
		}
	}

	public static void CallNewMessageLogEntryCallbacks(string log)
	{
		try
		{
			if (OnNewMessageLogEntryCallbacks != null)
			{
				for (int i = 0; i < OnNewMessageLogEntryCallbacks.Count; i++)
				{
					OnNewMessageLogEntryCallbacks[i](log);
				}
			}
		}
		catch
		{
		}
	}

	public static void CallBeginPlayerTurnCallbacks()
	{
		for (int i = 0; i < OnBeginPlayerTurnCallbacks.Count; i++)
		{
			OnBeginPlayerTurnCallbacks[i](Core);
		}
	}

	public void PlayerTurn()
	{
		TutorialManager.GameSync();
		CallBeginPlayerTurnCallbacks();
		Sidebar.SidebarTick++;
		bool flag = false;
		if (The.Player != null)
		{
			if (!The.Player.HasEffect<Confused>() && !The.Player.HasEffect<HulkHoney_Tonic>())
			{
				ConfusionLevel = 0;
				FuriousConfusion = 0;
			}
			AllegianceSet allegianceSet = The.Player.Brain?.Allegiance;
			if (allegianceSet != null && allegianceSet.Count == 0)
			{
				allegianceSet["Player"] = 100;
			}
		}
		_ = FrameTimer.Elapsed.TotalMilliseconds;
		Game.Player.Messages.BeginPlayerTurn();
		if (!sixArmsSet && (The.Player?.GetBodyPartCount("Arm") ?? 0) >= 6)
		{
			Achievement.SIX_ARMS.Unlock();
			sixArmsSet = true;
		}
		if (Options.InventoryConsistencyCheck)
		{
			Inventory inventory = The.Player?.Inventory;
			if (inventory != null)
			{
				int i = 0;
				for (int count = inventory.Objects.Count; i < count; i++)
				{
					XRL.World.GameObject gameObject = inventory.Objects[i];
					if (!CheckInventoryObject(gameObject))
					{
						MessageQueue.AddPlayerMessage("Invalid inventory object: " + gameObject, 'R');
					}
				}
			}
		}
		Sidebar.UpdateState();
		Sidebar.Update();
		Keyboard.ClearMouseEvents(LeaveMovementEvents: true);
		while (The.Player.Energy.Value >= 1000 && Game.Running)
		{
			while (!GameManager.focused && Game.Running)
			{
				Thread.Sleep(200);
			}
			GameManager.Instance.CurrentGameView = Options.StageViewID;
			XRL.World.Event.ResetPool();
			int value = The.Player.Energy.Value;
			if (The.Player.CurrentZone != null)
			{
				The.Player.CurrentZone.SetActive();
			}
			if (HPWarning)
			{
				Popup.ShowSpace("{{R|Your health has dropped below {{C|" + Globals.HPWarningThreshold + "%}}!}}");
				HPWarning = false;
			}
			ActionManager actionManager = Game.ActionManager;
			if (Keyboard.kbhit() || Keyboard.HasMouseEvent() || !EnableAnimation || Core.PlayerWalking == "ReopenMissileUI")
			{
				string text = null;
				string text2 = "CmdNone";
				if (!The.Game.Running)
				{
					break;
				}
				CombatJuice.startTurn();
				if (Keyboard.HasMouseEvent(filterMetaCommands: true))
				{
					Keyboard.MouseEvent mouseEvent = Keyboard.PopMouseEvent();
					if (!TutorialManager.AllowCommand(mouseEvent?.Event))
					{
						break;
					}
					while (mouseEvent != null && The.Player.CurrentCell != null)
					{
						if (mouseEvent.Event == "AdventureMouseLook")
						{
							Look.ShowLooker(0, mouseEvent.x, mouseEvent.y);
						}
						if (mouseEvent.Event.StartsWith("Command:"))
						{
							string text3 = mouseEvent.Event;
							text2 = text3.Substring(8, text3.Length - 8);
							break;
						}
						if (mouseEvent.Event == "AdventureMouseForceMove" && Options.MouseMovement)
						{
							Cell currentCell = The.Player.CurrentCell;
							Cell cell = currentCell.ParentZone.GetCell(mouseEvent.x, mouseEvent.y);
							if (cell.PathDistanceTo(currentCell) != 1)
							{
								AutoAct.Setting = "M" + cell.X + "," + cell.Y;
								return;
							}
							text = currentCell.GetDirectionFromCell(cell);
							text2 = "CmdMove" + text;
						}
						else if (mouseEvent.Event == "AdventureMouseForceAttack")
						{
							Cell currentCell2 = The.Player.CurrentCell;
							Cell cell2 = currentCell2.ParentZone.GetCell(mouseEvent.x, mouseEvent.y);
							if (cell2 != null && cell2.IsExplored())
							{
								if (cell2.PathDistanceTo(currentCell2) != 1)
								{
									AutoAct.Setting = "M" + cell2.X + "," + cell2.Y;
									return;
								}
								text = currentCell2.GetDirectionFromCell(cell2);
								text2 = "CmdAttack" + text;
							}
						}
						else if (mouseEvent.Event == "AdventureMouseContextAction")
						{
							Cell currentCell3 = The.Player.CurrentCell;
							Cell cell3 = currentCell3.ParentZone.GetCell(mouseEvent.x, mouseEvent.y);
							if (cell3 != null && !cell3.IsExplored())
							{
								AutoAct.Setting = "M" + cell3.X + "," + cell3.Y;
								return;
							}
							if (cell3 != null && cell3.IsExplored())
							{
								if (cell3 == currentCell3)
								{
									text2 = "CmdWait";
								}
								else
								{
									if (cell3.PathDistanceTo(currentCell3) != 1)
									{
										XRL.World.GameObject gameObject2 = SmartUse.FindPlayerSmartUseObject(cell3, 100);
										if (gameObject2 != null && !gameObject2.IsOpenLiquidVolume())
										{
											AutoAct.Setting = "!" + gameObject2.ID;
										}
										else
										{
											AutoAct.Setting = "M" + cell3.X + "," + cell3.Y;
										}
										return;
									}
									text = currentCell3.GetDirectionFromCell(cell3);
									XRL.World.GameObject gameObject3 = SmartUse.FindPlayerSmartUseObject(cell3, 100);
									if (cell3.HasObjectWithPart("Brain", (XRL.World.GameObject GO) => GO.IsHostileTowards(The.Player)))
									{
										text2 = "CmdAttack" + text;
									}
									else if (gameObject3 != null && !gameObject3.IsOpenLiquidVolume())
									{
										text2 = "CmdNone";
										SmartUse.PlayerPerformSmartUse(gameObject3);
									}
									else
									{
										text2 = "CmdMove" + text;
									}
								}
							}
						}
						else if (mouseEvent.Event == "AdventureMouseInteract")
						{
							Cell cell4 = The.Player.CurrentCell.ParentZone.GetCell(mouseEvent.x, mouseEvent.y);
							if (cell4 != null && cell4.IsExplored())
							{
								XRL.World.GameObject highestRenderLayerObject = cell4.GetHighestRenderLayerObject();
								if (highestRenderLayerObject != null && highestRenderLayerObject.GetTagOrStringProperty("DefaultRightClickInventoryAction") != null)
								{
									InventoryActionEvent.Check(highestRenderLayerObject, The.Player, highestRenderLayerObject, highestRenderLayerObject.GetTagOrStringProperty("DefaultRightClickInventoryAction"));
								}
								else if (highestRenderLayerObject != null)
								{
									UnityEngine.Debug.Log($"Mouse: {mouseEvent.Vector2}");
									highestRenderLayerObject.Twiddle(MouseClick: true);
								}
								else
								{
									SoundManager.PlayUISound("Sounds/UI/ui_cursor_scroll", 1f, Combat: false, Interface: true);
								}
							}
						}
						else
						{
							if (!(mouseEvent.Event == "AdventureMouseInteractAll"))
							{
								text2 = mouseEvent.Event;
								break;
							}
							Cell currentCell4 = The.Player.CurrentCell;
							Cell cell5 = currentCell4.ParentZone.GetCell(mouseEvent.x, mouseEvent.y);
							if (cell5 != null && cell5.IsExplored())
							{
								The.Player.FireEvent(XRL.World.Event.New("CommandGetFrom", "Direction", currentCell4.GetDirectionFromCell(cell5)));
							}
						}
						mouseEvent = ((!Keyboard.HasMouseEvent(filterMetaCommands: true)) ? null : Keyboard.PopMouseEvent());
					}
				}
				else if (AutoAct.Setting == "ReopenMissileUI")
				{
					AutoAct.Interrupt();
					text2 = "CmdFire";
				}
				else if (Keyboard.kbhit() || !EnableAnimation)
				{
					text2 = CommandBindingManager.GetNextCommand(CommandBindingManager.CoreLoopExcludes);
				}
				if (!TutorialManager.AllowCommand(text2))
				{
					break;
				}
				if (text2 != null && text2 != null && text2.StartsWith("CmdMoveToBorder:"))
				{
					List<int> list = text2.Split(':')[1].Split(',').ToList().Map((string s) => int.Parse(s));
					Vector2i vector2i = new Vector2i(list[0], list[1]);
					Vector2i vector2i2 = new Vector2i(list[0], list[1]).Clamp();
					if (vector2i.DistanceTo(The.PlayerCell.Location.Vector2i) != 1)
					{
						PlayerAvoid.Clear();
						AutoAct.Setting = $"M{vector2i2.x},{vector2i2.y}";
						break;
					}
					text2 = "CmdMove" + The.PlayerCell.Location.Vector2i.DirectionTo(vector2i);
				}
				if (text2 == "CmdWaitMenu")
				{
					string[] array = new string[7] { "Wait 1 Turn", "Wait N Turns", "Wait 20 Turns", "Wait 100 Turns", "Wait Until Healed", "Wait Until Party Healed", "Wait Until Morning" };
					char[] array2 = new char[array.Length];
					char c = 'a';
					int num = 0;
					for (int num2 = array.Length; num < num2; num++)
					{
						array2[num] = c++;
					}
					switch (Popup.PickOption("Select Wait Style", null, "", "Sounds/UI/ui_notification", array, array2, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true))
					{
					case 0:
						text2 = "CmdWait";
						break;
					case 1:
						text2 = "CmdWaitN";
						break;
					case 2:
						text2 = "CmdWait20";
						break;
					case 3:
						text2 = "CmdWait100";
						break;
					case 4:
						text2 = "CmdWaitUntilHealed";
						break;
					case 5:
						text2 = "CmdWaitUntilPartyHealed";
						break;
					case 6:
						text2 = "CmdWaitUntilMorning";
						break;
					}
				}
				else if (text2 == "CmdMoveMenu")
				{
					GetMovementCapabilitiesEvent getMovementCapabilitiesEvent = GetMovementCapabilitiesEvent.GetFor(The.Player);
					string[] array3 = getMovementCapabilitiesEvent.Descriptions.ToArray();
					char[] array4 = new char[array3.Length];
					char c2 = 'a';
					int num3 = 0;
					for (int num4 = array3.Length; num3 < num4; num3++)
					{
						array4[num3] = c2++;
					}
					int num5 = Popup.PickOption("Select Move Style", null, "", "Sounds/UI/ui_notification", array3, array4, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
					if (num5 >= 0)
					{
						string text4 = getMovementCapabilitiesEvent.Abilities[num5]?.NotUsableDescription;
						if (text4 != null)
						{
							Popup.ShowFail(text4);
						}
						else
						{
							text2 = getMovementCapabilitiesEvent.Commands[num5];
						}
					}
				}
				switch (text2)
				{
				case "CmdSystemMenu":
				{
					int num14;
					int num15;
					while (true)
					{
						num14 = 0;
						num15 = 0;
						bool flag5 = CheckpointingSystem.IsCheckpointingEnabled();
						bool flag6 = The.Player.HasEffect(typeof(WakingDream));
						bool transient = Game.Transient;
						string text14 = ((flag5 && !flag6) ? "Quit Without Saving" : "Abandon Character");
						string text15 = (flag5 ? "QUIT" : "ABANDON");
						string[] options;
						char[] hotkeys;
						if (transient)
						{
							options = new string[4] { "Control Mapping", "Options", "Game Info", "End Test" };
							hotkeys = new char[4] { 'c', 'o', 'g', 'a' };
						}
						else if (flag5)
						{
							num14 = 2;
							if (CheckpointingSystem.IsPlayerInCheckpoint())
							{
								options = new string[7] { "Set Checkpoint", "&KRestore Checkpoint", "Control Mapping", "Options", "Game Info", "Save and Quit", text14 };
								hotkeys = new char[7] { 'k', 'r', 'c', 'o', 'g', 's', 'q' };
							}
							else
							{
								options = new string[7] { "&KSet Checkpoint", "Restore Checkpoint", "Control Mapping", "Options", "Game Info", "Save and Quit", text14 };
								hotkeys = new char[7] { 'k', 'r', 'c', 'o', 'g', 's', 'q' };
							}
						}
						else
						{
							options = new string[5] { "Control Mapping", "Options", "Game Info", "Save and Quit", text14 };
							hotkeys = new char[5] { 'c', 'o', 'g', 's', 'a' };
						}
						num15 = Popup.PickOption("", null, "", "Sounds/UI/ui_notification", options, hotkeys, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
						if (transient && num15 == 3)
						{
							num15 = 4;
						}
						if (num15 == 4 + num14)
						{
							bool flag7 = true;
							if (flag6)
							{
								The.Player.RemoveEffect<WakingDream>();
								return;
							}
							if (The.Game.DontSaveThisIsAReplay | transient)
							{
								Game.DeathReason = "<nodeath>";
								Game.DeathCategory = "exit";
								Game.forceNoDeath = true;
								Game.Running = false;
								The.Player.Energy.BaseValue = 0;
								return;
							}
							if (!Options.DisablePermadeath)
							{
								flag7 = false;
								string text16 = "\n\nType '" + text15 + "' to confirm.";
								string text17 = string.Empty;
								if (CapabilityManager.CurrentPlatformClassification() == CapabilityManager.PlatformClassification.Console)
								{
									text16 = string.Empty;
									text17 = null;
								}
								string text18 = (flag5 ? Popup.AskString("If you quit without saving, you will lose all your unsaved progress. Are you sure you want to QUIT and LOSE YOUR PROGRESS?" + text16, text17, "Sounds/UI/ui_notification", null, text15, text15.Length) : Popup.AskString("If you quit without saving, you will lose all your progress and your character will be lost. Are you sure you want to QUIT and LOSE YOUR PROGRESS?" + text16, text17, "Sounds/UI/ui_notification", null, text15, text15.Length));
								if (!text18.IsNullOrEmpty() && text18.ToUpper() == text15)
								{
									flag7 = true;
								}
							}
							if (flag7)
							{
								if (EndGame.IsAnyEnding)
								{
									CodaSystem.EndGamePrompt();
									return;
								}
								if (flag5)
								{
									Game.DeathReason = "<nodeath>";
									Game.DeathCategory = "exit";
									Game.forceNoDeath = true;
								}
								else
								{
									Game.DeathReason = "You abandoned all hope.";
									Game.DeathCategory = "exit";
									JournalAPI.AddAccomplishment("On the " + XRL.World.Calendar.GetDay() + " of " + XRL.World.Calendar.GetMonth() + ", you abandoned all hope.", null, null, null, "general", MuralCategory.Generic, MuralWeight.Nil, null, -1L);
								}
								Game.Running = false;
								The.Player.Energy.BaseValue = 0;
								return;
							}
						}
						if ((num15 == 0) & flag5)
						{
							if (CheckpointingSystem.IsPlayerInCheckpoint())
							{
								CheckpointingSystem.DoCheckpoint();
							}
							else
							{
								Popup.Show("You can only set your checkpoint in settlements.");
							}
						}
						if ((num15 == 1) & flag5)
						{
							if (CheckpointingSystem.IsPlayerInCheckpoint())
							{
								Popup.Show("You can only restore your checkpoint outside settlements.");
							}
							else if (Popup.ShowYesNoCancel("Are you sure you want to restore your checkpoint?") == DialogResult.Yes)
							{
								CheckpointingSystem.QueueRestore();
							}
						}
						if (num15 != 2 + num14)
						{
							break;
						}
						if (!Game.HasStringGameState("OriginalWorldSeed"))
						{
							Popup.ShowFail("This saved game predates world seed info.");
							continue;
						}
						Popup.ShowBlockWithCopy("\n\n           " + The.Game.GetStringGameState("GameMode") + " mode.\n\n           Turn " + The.Game.Turns + "\n\n          World seed: " + Game.GetStringGameState("OriginalWorldSeed") + "     \n\n\n   ", " {{W|C}} - Copy seed  {{W|ESC}} - Exit ", "", Game.GetStringGameState("OriginalWorldSeed"));
					}
					if (num15 == 3 + num14)
					{
						ZoneManager.instance.CheckCached(AllowFreeze: true, ForceFreeze: true);
						SaveGame("Primary");
						Popup.Show("Game saved!");
						Game.DeathReason = "<nodeath>";
						Game.DeathCategory = "exit";
						Game.forceNoDeath = true;
						Game.Running = false;
						return;
					}
					if (num15 == num14)
					{
						KeyMappingUI.Show();
					}
					if (num15 == 1 + num14)
					{
						OptionsUI.Show();
					}
					goto case "CmdNone";
				}
				case "CmdAutoMoveW":
					The.Player.AutoMove("W");
					goto case "CmdNone";
				case "CmdAutoMoveE":
					The.Player.AutoMove("E");
					goto case "CmdNone";
				case "CmdAutoMoveN":
					The.Player.AutoMove("N");
					goto case "CmdNone";
				case "CmdAutoMoveS":
					The.Player.AutoMove("S");
					goto case "CmdNone";
				case "CmdAutoMoveNW":
					The.Player.AutoMove("NW");
					goto case "CmdNone";
				case "CmdAutoMoveNE":
					The.Player.AutoMove("NE");
					goto case "CmdNone";
				case "CmdAutoMoveSW":
					The.Player.AutoMove("SW");
					goto case "CmdNone";
				case "CmdAutoMoveSE":
					The.Player.AutoMove("SE");
					goto case "CmdNone";
				case "CmdMoveW":
					AutoAct.ClearAutoMoveStop();
					The.Player.Move("W");
					RenderBase(UpdateSidebar: false);
					goto case "CmdNone";
				case "CmdMoveE":
					AutoAct.ClearAutoMoveStop();
					The.Player.Move("E");
					if (Options.DrawStepImmediately)
					{
						RenderBase(UpdateSidebar: false);
					}
					goto case "CmdNone";
				case "CmdMoveN":
					AutoAct.ClearAutoMoveStop();
					The.Player.Move("N");
					if (Options.DrawStepImmediately)
					{
						RenderBase(UpdateSidebar: false);
					}
					goto case "CmdNone";
				case "CmdMoveS":
					AutoAct.ClearAutoMoveStop();
					The.Player.Move("S");
					if (Options.DrawStepImmediately)
					{
						RenderBase(UpdateSidebar: false);
					}
					goto case "CmdNone";
				case "CmdMoveNW":
					AutoAct.ClearAutoMoveStop();
					The.Player.Move("NW");
					if (Options.DrawStepImmediately)
					{
						RenderBase(UpdateSidebar: false);
					}
					goto case "CmdNone";
				case "CmdMoveNE":
					AutoAct.ClearAutoMoveStop();
					The.Player.Move("NE");
					if (Options.DrawStepImmediately)
					{
						RenderBase(UpdateSidebar: false);
					}
					goto case "CmdNone";
				case "CmdMoveSW":
					AutoAct.ClearAutoMoveStop();
					The.Player.Move("SW");
					if (Options.DrawStepImmediately)
					{
						RenderBase(UpdateSidebar: false);
					}
					goto case "CmdNone";
				case "CmdMoveSE":
					AutoAct.ClearAutoMoveStop();
					The.Player.Move("SE");
					if (Options.DrawStepImmediately)
					{
						RenderBase(UpdateSidebar: false);
					}
					goto case "CmdNone";
				case "CmdMoveDirection":
				{
					string text13 = PickDirection.ShowPicker("Move in what direction?");
					if (!text13.IsNullOrEmpty() && text13 != "." && text13 != "?")
					{
						The.Player.Move(text13);
					}
					goto case "CmdNone";
				}
				case "CmdMoveTo":
				{
					Cell currentCell10 = The.Player.CurrentCell;
					if (currentCell10 != null)
					{
						Cell cell9 = PickTarget.ShowPicker(PickTarget.PickStyle.EmptyCell, 1, 9999, currentCell10.X, currentCell10.Y, Locked: false, AllowVis.OnlyExplored, null, null, null, null, "Move where?");
						if (cell9 != null && cell9 != currentCell10)
						{
							if (!cell9.IsAdjacentTo(currentCell10))
							{
								PlayerAvoid.Clear();
								AutoAct.Setting = "M" + cell9.X + "," + cell9.Y;
								return;
							}
							string directionFromCell2 = currentCell10.GetDirectionFromCell(cell9);
							if (!directionFromCell2.IsNullOrEmpty() && directionFromCell2 != "." && directionFromCell2 != "?")
							{
								The.Player.Move(directionFromCell2);
							}
						}
					}
					goto case "CmdNone";
				}
				case "CmdMoveToEdge":
				{
					string text8 = PickDirection.ShowPicker("Move to which edge? (\u0018\u0019\u001a\u001b)");
					switch (text8)
					{
					case "N":
					case "S":
					case "E":
					case "W":
						PlayerAvoid.Clear();
						AutoAct.Setting = "G" + text8;
						return;
					}
					goto case "CmdNone";
				}
				case "CmdMoveFarNW":
				case "CmdMoveFarN":
				case "CmdMoveFarNE":
				case "CmdMoveFarW":
				case "CmdMoveFarE":
				case "CmdMoveFarSW":
				case "CmdMoveFarS":
				case "CmdMoveFarSE":
				case "CmdMoveCenter":
				{
					if (The.Player.OnWorldMap())
					{
						Popup.ShowFail("You cannot do that on the world map.");
						return;
					}
					PlayerAvoid.Clear();
					int num10;
					int num11;
					switch (text2)
					{
					case "CmdMoveFarN":
						AutoAct.Setting = "GN";
						return;
					case "CmdMoveFarS":
						AutoAct.Setting = "GS";
						return;
					case "CmdMoveFarE":
						AutoAct.Setting = "GE";
						return;
					case "CmdMoveFarW":
						AutoAct.Setting = "GW";
						return;
					case "CmdMoveFarNW":
						num10 = 0;
						num11 = 0;
						break;
					case "CmdMoveFarNE":
						num10 = 79;
						num11 = 0;
						break;
					case "CmdMoveFarSW":
						num10 = 0;
						num11 = 24;
						break;
					case "CmdMoveFarSE":
						num10 = 79;
						num11 = 24;
						break;
					default:
						num10 = 40;
						num11 = 12;
						break;
					}
					AutoAct.Setting = "M" + num10 + "," + num11;
					return;
				}
				case "CmdMoveToPointOfInterest":
				{
					List<PointOfInterest> list2 = GetPointsOfInterestEvent.GetFor(The.Player);
					if (list2 == null || list2.Count <= 0)
					{
						Popup.ShowFail("You haven't found any points of interest nearby.");
						return;
					}
					list2.Sort(PointOfInterest.Compare);
					string[] array5 = new string[list2.Count];
					char[] array6 = new char[list2.Count];
					IRenderable[] array7 = new IRenderable[list2.Count];
					char c3 = 'a';
					int num8 = 0;
					for (int count3 = list2.Count; num8 < count3; num8++)
					{
						PointOfInterest pointOfInterest = list2[num8];
						array5[num8] = pointOfInterest.GetDisplayName(The.Player);
						array6[num8] = ((c3 <= 'z') ? c3++ : ' ');
						array7[num8] = pointOfInterest.GetIcon();
					}
					int num9 = Popup.PickOption("Go to which point of interest?", null, "", "Sounds/UI/ui_notification", array5, array6, array7, null, null, null, null, 1, 78, 0, -1, AllowEscape: true, RespectOptionNewlines: true, CenterIntro: false, CenterIntroIcon: true, ForceNewPopup: false, null, "CmdMoveToPointOfInterestMenu");
					if (num9 >= 0)
					{
						PlayerAvoid.Clear();
						list2[num9].NavigateTo(The.Player);
					}
					goto case "CmdNone";
				}
				case "CmdAttackW":
					The.Player.AttackDirection("W");
					goto case "CmdNone";
				case "CmdAttackE":
					The.Player.AttackDirection("E");
					goto case "CmdNone";
				case "CmdAttackN":
					The.Player.AttackDirection("N");
					goto case "CmdNone";
				case "CmdAttackS":
					The.Player.AttackDirection("S");
					goto case "CmdNone";
				case "CmdAttackNW":
					The.Player.AttackDirection("NW");
					goto case "CmdNone";
				case "CmdAttackNE":
					The.Player.AttackDirection("NE");
					goto case "CmdNone";
				case "CmdAttackSW":
					The.Player.AttackDirection("SW");
					goto case "CmdNone";
				case "CmdAttackSE":
					The.Player.AttackDirection("SE");
					goto case "CmdNone";
				case "CmdAttackU":
					if (The.Player.CurrentCell.HasObjectWithPart("StairsUp"))
					{
						The.Player.AttackDirection("U");
					}
					goto case "CmdNone";
				case "CmdAttackD":
					if (The.Player.CurrentCell.HasObjectWithPart("StairsDown"))
					{
						The.Player.AttackDirection("D");
					}
					goto case "CmdNone";
				case "CmdAttackDirection":
				{
					string text11 = PickDirection.ShowPicker("Attack in what direction?");
					if (!text11.IsNullOrEmpty() && text11 != "." && text11 != "?")
					{
						The.Player.AttackDirection(text11);
					}
					goto case "CmdNone";
				}
				case "CmdAttackCell":
				{
					Cell currentCell7 = The.Player.CurrentCell;
					if (currentCell7 != null)
					{
						Cell cell7 = PickTarget.ShowPicker(PickTarget.PickStyle.EmptyCell, 1, 1, currentCell7.X, currentCell7.Y, Locked: false, AllowVis.OnlyExplored, null, null, null, null, "Attack where?", EnforceRange: true);
						if (cell7 != null && cell7 != currentCell7 && cell7.IsAdjacentTo(currentCell7))
						{
							string directionFromCell = currentCell7.GetDirectionFromCell(cell7);
							if (!directionFromCell.IsNullOrEmpty() && directionFromCell != "." && directionFromCell != "?")
							{
								The.Player.AttackDirection(directionFromCell);
							}
						}
					}
					goto case "CmdNone";
				}
				case "CmdMoveU":
					if (The.Player.CurrentCell.FireEvent(XRL.World.Event.New("ClimbUp", "GO", The.Player)))
					{
						Cell currentCell8 = The.Player.CurrentCell;
						if (currentCell8 != null && !currentCell8.ParentZone.SpecialUpMessage().IsNullOrEmpty())
						{
							Popup.Show(currentCell8.ParentZone.SpecialUpMessage());
						}
						else if (currentCell8 != null && !currentCell8.ParentZone.IsWorldMap() && currentCell8.ParentZone.Z <= 10 && !currentCell8.ParentZone.IsInside())
						{
							if (The.Player.AreHostilesNearby() && !The.Player.IsFlying)
							{
								Popup.ShowFail("There are hostiles nearby!");
							}
							else
							{
								List<XRL.World.GameObject> list3 = currentCell8.ParentZone.FindObjects((XRL.World.GameObject gameObject9) => gameObject9.CurrentCell.IsExplored() && gameObject9.HasRegisteredEvent("ClimbUp") && gameObject9.Render != null && gameObject9.Render.Visible && !gameObject9.HasTagOrIntProperty("ExcludeFromAscentSelection") && GenericQueryEvent.Check(gameObject9, "AscentEligible", null, null, 0, BaseResult: true));
								int num12 = -1;
								if (list3.Count > 0)
								{
									string[] array8 = new string[list3.Count + 1];
									char[] array9 = new char[array8.Length];
									IRenderable[] array10 = new IRenderable[array8.Length];
									char c4 = 'b';
									array8[0] = "world map";
									array9[0] = 'a';
									array10[0] = new Renderable("Mutations/teleport_other.bmp", " ", "&W", null, 'Y');
									int num13 = 1;
									for (int count4 = list3.Count; num13 <= count4; num13++)
									{
										XRL.World.GameObject gameObject5 = list3[num13 - 1];
										array8[num13] = gameObject5.DisplayNameOnlyDirect + " [" + The.Player.DescribeDirectionToward(gameObject5, General: true, Short: true) + "]";
										array9[num13] = ((c4 <= 'z') ? c4++ : ' ');
										array10[num13] = gameObject5.RenderForUI();
									}
									num12 = Popup.PickOption("Select a destination", null, "", "Sounds/UI/ui_notification", array8, array9, array10, null, null, null, null, 1, 78, 0, -1, AllowEscape: true);
									if (num12 == -1)
									{
										goto case "CmdNone";
									}
									if (num12 > 0)
									{
										AutoAct.Setting = "M" + list3[num12 - 1].ID;
										return;
									}
								}
								if (The.Player.CanTravel() && (num12 != -1 || !Options.AskForWorldmap || Popup.ShowYesNoCancel("Are you sure you want to go to the world map?") == DialogResult.Yes))
								{
									try
									{
										string address = currentCell8.GetAddress();
										if (address.Contains("."))
										{
											Game.SetStringGameState("LastLocationOnSurface", address);
										}
									}
									catch (Exception ex)
									{
										LogError(ex);
									}
									string zoneWorld = Game.ZoneManager.ActiveZone.GetZoneWorld();
									int zonewX = Game.ZoneManager.ActiveZone.GetZonewX();
									int zonewY = Game.ZoneManager.ActiveZone.GetZonewY();
									Zone zone = Game.ZoneManager.GetZone(zoneWorld);
									Cell cell8 = zone.GetCell(zonewX, zonewY);
									XRL.World.GameObject firstObjectWithPart2 = cell8.GetFirstObjectWithPart("TerrainTravel");
									if (The.Player.DirectMoveTo(cell8, 0, Forced: false, IgnoreCombat: false, IgnoreGravity: false, firstObjectWithPart2))
									{
										The.Player.PlayWorldOrUISound("sfx_worldMap_enter");
										Cell currentCell9 = The.Player.CurrentCell;
										if (currentCell9 != null && currentCell9.ParentZone == zone)
										{
											The.ZoneManager.SetActiveZone(zone);
										}
										The.ZoneManager.ProcessGoToPartyLeader();
									}
								}
							}
						}
						else if (!The.Player.CurrentCell.ParentZone.IsWorldMap() && The.Player.GetTotalConfusion() <= 0 && (!Options.AskAutostair || Popup.ShowYesNoCancel("Would you like to walk to the nearest stairway up?") == DialogResult.Yes))
						{
							AutoAct.Setting = "<";
							return;
						}
					}
					goto case "CmdNone";
				case "CmdMoveD":
				{
					Cell currentCell5 = The.Player.CurrentCell;
					if (currentCell5 != null && currentCell5.ParentZone.IsWorldMap())
					{
						The.Player.PullDown(AllowAlternate: true);
					}
					else if (currentCell5.FireEvent(XRL.World.Event.New("ClimbDown", "GO", The.Player)) && !currentCell5.ParentZone.IsWorldMap() && The.Player.GetTotalConfusion() <= 0 && (!Options.AskAutostair || Popup.ShowYesNoCancel("Would you like to walk to the nearest stairway down?") == DialogResult.Yes))
					{
						AutoAct.Setting = ">";
						return;
					}
					goto case "CmdNone";
				}
				case "CmdWait":
				{
					int num7 = 1000;
					if (The.Player.IsAflame())
					{
						if (Firefighting.AttemptFirefighting(The.Player, The.Player, 1000, Automatic: true))
						{
							num7 = 0;
						}
					}
					else if (The.Player.IsFrozen())
					{
						The.Player.TemperatureChange(20);
					}
					else
					{
						The.Player.Physics.Search();
					}
					The.Player.FireEvent("PassedTurn");
					if (num7 > 0)
					{
						SoundManager.PlayUISound("sfx_wait", 0.1f, Combat: false, Interface: true);
						The.Player.PassTurn();
					}
					goto case "CmdNone";
				}
				case "CmdWait20":
					if (!AutoAct.ShouldHostilesInterrupt(".", null, logSpot: false, popSpot: true))
					{
						AutoAct.Setting = ".20";
						SoundManager.PlayUISound("sfx_wait", 0.1f, Combat: false, Interface: true);
						The.Player.PassTurn();
						Loading.SetLoadingStatus("Waiting...");
					}
					goto case "CmdNone";
				case "CmdWaitN":
					try
					{
						if (!AutoAct.ShouldHostilesInterrupt(".", null, logSpot: false, popSpot: true))
						{
							int highestActivatedAbilityCooldownRounds = The.Player.GetHighestActivatedAbilityCooldownRounds();
							int start = Math.Max(lastWait, highestActivatedAbilityCooldownRounds);
							int? num24 = Popup.AskNumber("How many turns would you like to wait?", "Sounds/UI/ui_notification", "", start);
							if (num24.HasValue)
							{
								int value2 = num24.Value;
								if (value2 > 0)
								{
									if (value2 != highestActivatedAbilityCooldownRounds)
									{
										lastWait = value2;
									}
									AutoAct.Setting = "." + value2;
									SoundManager.PlayUISound("sfx_wait", 0.1f, Combat: false, Interface: true);
									The.Player.PassTurn();
									Loading.SetLoadingStatus(The.StringBuilder.Append("Waiting for ").Append(value2.Things("turn")).Append("...")
										.ToString());
								}
								else
								{
									MessageQueue.AddPlayerMessage(value2 + " is not a valid number of turns to wait.", 'K');
								}
							}
						}
					}
					catch (Exception x)
					{
						MetricsManager.LogError("Encountered exception inside CmdWaitN.", x);
					}
					goto case "CmdNone";
				case "CmdWait100":
					if (!AutoAct.ShouldHostilesInterrupt(".", null, logSpot: false, popSpot: true))
					{
						AutoAct.Setting = ".100";
						SoundManager.PlayUISound("sfx_wait", 0.1f, Combat: false, Interface: true);
						The.Player.PassTurn();
						Loading.SetLoadingStatus("Waiting 100 turns...");
					}
					goto case "CmdNone";
				case "CmdWaitUntilHealed":
					if (!AutoAct.ShouldHostilesInterrupt("r", null, logSpot: false, popSpot: true))
					{
						AutoAct.Setting = "r";
						actionManager.RestingUntilHealed = true;
						actionManager.RestingUntilHealedCount = 0;
						SoundManager.PlayUISound("sfx_wait", 0.1f, Combat: false, Interface: true);
						The.Player.PassTurn();
						Loading.SetLoadingStatus("Resting until healed...");
					}
					goto case "CmdNone";
				case "CmdWaitUntilPartyHealed":
					if (!AutoAct.ShouldHostilesInterrupt("r", null, logSpot: false, popSpot: true))
					{
						AutoAct.Setting = "r+";
						actionManager.RestingUntilHealed = true;
						actionManager.RestingUntilHealedCount = 0;
						actionManager.RestingUntilHealedCompanions.Clear();
						actionManager.RestingUntilHealedHaveCompanions = false;
						SoundManager.PlayUISound("sfx_wait", 0.1f, Combat: false, Interface: true);
						The.Player.PassTurn();
						Loading.SetLoadingStatus("Resting until party healed...");
					}
					goto case "CmdNone";
				case "CmdWaitUntilMorning":
					if (!AutoAct.ShouldHostilesInterrupt("z", null, logSpot: false, popSpot: true))
					{
						AutoAct.Setting = "z";
						SoundManager.PlayUISound("sfx_wait", 0.1f, Combat: false, Interface: true);
						The.Player.PassTurn();
						Loading.SetLoadingStatus("Resting until morning...");
					}
					goto case "CmdNone";
				case "CmdShowFPS":
					ShowFPS = !ShowFPS;
					goto case "CmdNone";
				case "CmdShowReachability":
				{
					Zone activeZone2 = The.ZoneManager.ActiveZone;
					ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
					for (int num22 = 0; num22 < activeZone2.Height; num22++)
					{
						for (int num23 = 0; num23 < activeZone2.Width; num23++)
						{
							scrapBuffer.Goto(num23, num22);
							if (activeZone2.IsReachable(num23, num22))
							{
								scrapBuffer.Write(".");
							}
							else
							{
								scrapBuffer.Write("#");
							}
						}
					}
					Popup._TextConsole.DrawBuffer(scrapBuffer);
					Keyboard.getch();
					goto case "CmdNone";
				}
				case "CmdUseN":
					PerformSmartUse("N");
					goto case "CmdNone";
				case "CmdUseS":
					PerformSmartUse("S");
					goto case "CmdNone";
				case "CmdUseE":
					PerformSmartUse("E");
					goto case "CmdNone";
				case "CmdUseW":
					PerformSmartUse("W");
					goto case "CmdNone";
				case "CmdUseNE":
					PerformSmartUse("NE");
					goto case "CmdNone";
				case "CmdUseNW":
					PerformSmartUse("NW");
					goto case "CmdNone";
				case "CmdUseSW":
					PerformSmartUse("SW");
					goto case "CmdNone";
				case "CmdUseSE":
					PerformSmartUse("SE");
					goto case "CmdNone";
				case "CmdUse":
					PerformSmartUse(text, 100);
					goto case "CmdNone";
				case "CmdLook":
					if (The.Player.GetTotalConfusion() > 0)
					{
						if (The.Player.GetFuriousConfusion() > 0)
						{
							Popup.ShowFail("You cannot examine things while you are enraged.");
						}
						else
						{
							Popup.ShowFail("You cannot examine things while you are confused.");
						}
					}
					else
					{
						Cell currentCell12 = The.Player.CurrentCell;
						Look.ShowLooker(0, currentCell12.X, currentCell12.Y);
					}
					goto case "CmdNone";
				case "CmdHelp":
					Manual.ShowHelp("");
					goto case "CmdNone";
				case "CmdToggleAnimation":
					EnableAnimation = !EnableAnimation;
					goto case "CmdNone";
				case "CmdJournal":
					Screens.CurrentScreen = 6;
					Screens.Show(The.Player);
					goto case "CmdNone";
				case "CmdTinkering":
					Screens.CurrentScreen = 7;
					Screens.Show(The.Player);
					goto case "CmdNone";
				case "CmdExplore":
					The.Player.CurrentCell.ParentZone.ExploreAll();
					Core.VisAllToggle = !Core.VisAllToggle;
					goto case "CmdNone";
				case "CmdSaveAndQuit":
					if (Popup.ShowYesNoCancel("Are you sure you want to save and quit?") == DialogResult.Yes)
					{
						ZoneManager.instance.CheckCached(AllowFreeze: true, ForceFreeze: true);
						SaveGame("Primary");
						Game.Running = false;
						return;
					}
					goto case "CmdNone";
				case "CmdQuit":
					if (Popup.ShowYesNoCancel("Are you sure you want to quit?") == DialogResult.Yes)
					{
						DialogResult dialogResult = DialogResult.No;
						if (!The.Game.DontSaveThisIsAReplay)
						{
							Popup.ShowYesNoCancel("Do you want to save first?");
						}
						switch (dialogResult)
						{
						case DialogResult.Yes:
							ZoneManager.instance.CheckCached(AllowFreeze: true, ForceFreeze: true);
							SaveGame("Primary");
							Game.DeathReason = "<nodeath>";
							Game.DeathCategory = "exit";
							Game.forceNoDeath = true;
							Game.Running = false;
							return;
						case DialogResult.No:
						{
							bool flag4 = true;
							if (!Options.DisablePermadeath && !CheckpointingSystem.IsCheckpointingEnabled() && !The.Game.DontSaveThisIsAReplay)
							{
								flag4 = false;
								string text10 = Popup.AskString("If you quit without saving, you will lose all your unsaved progress. Are you sure you want to QUIT and LOSE YOUR PROGRESS?\n\nType 'ABANDON' to confirm.", "", "Sounds/UI/ui_notification", null, "ABANDON", 7, 0, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false);
								if (!text10.IsNullOrEmpty() && text10.ToUpper() == "ABANDON")
								{
									flag4 = true;
								}
							}
							if (!flag4)
							{
								break;
							}
							if (EndGame.IsAnyEnding && !The.Game.DontSaveThisIsAReplay)
							{
								CodaSystem.EndGamePrompt();
								return;
							}
							if (CheckpointingSystem.IsCheckpointingEnabled() || The.Game.DontSaveThisIsAReplay)
							{
								Game.DeathReason = "<nodeath>";
								Game.DeathCategory = "exit";
								Game.forceNoDeath = true;
							}
							else
							{
								Game.DeathReason = "You abandoned all hope.";
								JournalAPI.AddAccomplishment("On the " + XRL.World.Calendar.GetDay() + " of " + XRL.World.Calendar.GetMonth() + ", you abandoned all hope.", null, null, null, "general", MuralCategory.Generic, MuralWeight.Nil, null, -1L);
							}
							Game.Running = false;
							The.Player.Energy.BaseValue = 0;
							return;
						}
						}
					}
					goto case "CmdNone";
				case "CmdSave":
					if (Options.AllowSaveLoad && Popup.ShowYesNoCancel("Quick save the game?") == DialogResult.Yes)
					{
						The.Game.QuickSave();
						Loading.SetLoadingStatus("Game saved!");
						Loading.SetLoadingStatus(null);
					}
					goto case "CmdNone";
				case "CmdLoad":
					if (Options.AllowSaveLoad && Popup.ShowYesNoCancel("Load your last quick save?") == DialogResult.Yes)
					{
						Task<bool> task = The.Game.QuickLoad();
						task.Wait();
						if (task.Result)
						{
							Sidebar.UpdateState();
							Loading.SetLoadingStatus("Game loaded!");
							Loading.SetLoadingStatus(null);
							return;
						}
					}
					goto case "CmdNone";
				case "CmdAbilities":
				{
					The.Player.ModIntProperty("HasAccessedAbilities", 1);
					string text6 = AbilityManager.Show(The.Player);
					if (!text6.IsNullOrEmpty())
					{
						if (The.Player.OnWorldMap() && !AbilityManager.IsWorldMapUsable(text6))
						{
							The.Player.ShowFailure("You cannot do that on the world map.");
						}
						else
						{
							CommandEvent.Send(The.Player, text6);
						}
					}
					goto case "CmdNone";
				}
				case "CmdShowEffects":
					The.Player.ShowActiveEffects();
					goto case "CmdNone";
				case "CmdTarget":
				{
					Cell currentCell6 = The.Player.CurrentCell;
					Cell cell6 = PickTarget.ShowPicker(PickTarget.PickStyle.Burst, 0, 9999, currentCell6.X, currentCell6.Y, Locked: true, AllowVis.OnlyVisible, null, null, null, null, "Pick Target");
					if (cell6 != null)
					{
						SoundManager.PlayUISound("ui_cursor_scroll", 1f, Combat: false, Interface: true);
						Sidebar.CurrentTarget = cell6.GetCombatTarget(The.Player, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
					}
					goto case "CmdNone";
				}
				case "CmdThrow":
					Combat.ThrowWeapon(The.Player);
					goto case "CmdNone";
				case "CmdFire":
				{
					bool flag2 = false;
					bool flag3 = false;
					string text5 = null;
					List<XRL.World.GameObject> missileWeapons = The.Player.GetMissileWeapons();
					XRL.World.GameObject gameObject4 = null;
					if (missileWeapons != null && missileWeapons.Count > 0)
					{
						int num6 = 0;
						for (int count2 = missileWeapons.Count; num6 < count2; num6++)
						{
							if (missileWeapons[num6].TryGetPart<MissileWeapon>(out var Part))
							{
								flag3 = true;
								if (Part.ReadyToFire())
								{
									flag2 = true;
									break;
								}
								if (text5 == null)
								{
									text5 = Part.GetNotReadyToFireMessage();
									gameObject4 = missileWeapons[num6];
								}
							}
						}
					}
					if (!flag3)
					{
						Popup.ShowFail("You do not have a missile weapon equipped!");
					}
					else if (!flag2)
					{
						SoundManager.PlaySound(gameObject4?.GetSoundTag("NoAmmoSound"));
						Popup.ShowFail(text5 ?? ("You need to reload! (" + ControlManager.getCommandInputDescription("CmdReload", Options.ModernUI) + ")"));
					}
					else
					{
						Combat.FireMissileWeapon(The.Player);
					}
					goto case "CmdNone";
				}
				case "CmdWish":
					WishManager.AskWish();
					goto case "CmdNone";
				case "CmdWishMenu":
					WishMenu.OpenWishMenu();
					goto case "CmdNone";
				case "CmdFactions":
					Screens.ShowPopup("Factions", The.Player);
					goto case "CmdNone";
				case "CmdQuests":
					Screens.CurrentScreen = 5;
					Screens.Show(The.Player);
					goto case "CmdNone";
				case "CmdActiveEffects":
					The.Player.ShowActiveEffects();
					goto case "CmdNone";
				case "CmdLastStatusPage":
					Screens.Show(The.Player);
					goto case "CmdNone";
				case "CmdCharacter":
					Screens.CurrentScreen = 1;
					Screens.Show(The.Player);
					goto case "CmdNone";
				case "CmdInventory":
					InventoryAndEquipmentStatusScreen.EnteringFromEquipmentSide = false;
					Screens.CurrentScreen = 2;
					Screens.Show(The.Player);
					goto case "CmdNone";
				case "CmdEquipment":
					InventoryAndEquipmentStatusScreen.EnteringFromEquipmentSide = true;
					Screens.CurrentScreen = 3;
					Screens.Show(The.Player);
					goto case "CmdNone";
				case "CmdSkillsPowers":
					Screens.CurrentScreen = 0;
					Screens.Show(The.Player);
					goto case "CmdNone";
				case "CmdAutoExplore":
					if (!The.Player.CurrentZone.IsWorldMap())
					{
						The.Player.CurrentZone.FlushNavigationCaches();
						AutoAct.Setting = "?";
					}
					return;
				case "CmdAutoAttack":
					if (The.Player.Target == null)
					{
						Popup.ShowFail("You don't have a target.");
					}
					else if (The.Player.IsConfused && The.Player.GetFuriousConfusion() <= 0)
					{
						Popup.ShowFail("You cannot autoattack while you are confused.");
					}
					else
					{
						AutoAct.Setting = "a";
					}
					return;
				case "CmdAttackNearest":
				{
					if (The.Player.GetTotalConfusion() > 0)
					{
						if (The.Player.GetFuriousConfusion() > 0)
						{
							The.Player.FireEvent(XRL.World.Event.New("CmdMove" + Directions.GetRandomDirection()));
						}
						else
						{
							Popup.ShowFail("You cannot autoattack while you are confused.");
						}
						return;
					}
					Cell currentCell13 = The.Player.CurrentCell;
					XRL.World.GameObject gameObject7 = The.Player.Target;
					Engulfed effect = The.Player.GetEffect<Engulfed>();
					if (effect != null && XRL.World.GameObject.Validate(effect.EngulfedBy) && effect.EngulfedBy.IsHostileTowards(The.Player))
					{
						gameObject7 = effect.EngulfedBy;
					}
					if (gameObject7 != null && gameObject7.Brain != null && !gameObject7.IsHostileTowards(The.Player))
					{
						Popup.ShowFail("You do not autoattack " + gameObject7.t() + " because " + gameObject7.itis + " not hostile to you.");
						return;
					}
					bool flag8 = false;
					int num16 = ((gameObject7 == null) ? 9999999 : currentCell13.DistanceTo(gameObject7));
					switch (num16)
					{
					case 1:
					{
						string directionFromCell5 = currentCell13.GetDirectionFromCell(gameObject7.CurrentCell);
						if (!directionFromCell5.IsNullOrEmpty() && directionFromCell5 != ".")
						{
							try
							{
								AutoAct.Attacking = true;
								The.Player.AttackDirection(directionFromCell5);
								break;
							}
							finally
							{
								AutoAct.Attacking = false;
							}
						}
						goto default;
					}
					case 0:
					{
						Cell randomElement = currentCell13.GetLocalNavigableAdjacentCells(The.Player).GetRandomElement();
						if (randomElement != null)
						{
							string directionFromCell3 = currentCell13.GetDirectionFromCell(randomElement);
							if (!directionFromCell3.IsNullOrEmpty() && directionFromCell3 != ".")
							{
								int count5 = The.Game.Player.Messages.Messages.Count;
								if (The.Player.Move(directionFromCell3) || count5 < The.Game.Player.Messages.Messages.Count)
								{
									break;
								}
							}
						}
						flag8 = true;
						goto default;
					}
					default:
					{
						if (gameObject7 != null && !gameObject7.IsVisible())
						{
							Popup.ShowFail("You cannot see your target.");
							break;
						}
						List<Cell> localAdjacentCells = currentCell13.GetLocalAdjacentCells();
						int? num17 = null;
						int num18 = 0;
						for (int count6 = localAdjacentCells.Count; num18 < count6; num18++)
						{
							XRL.World.GameObject combatTarget = localAdjacentCells[num18].GetCombatTarget(The.Player);
							if (combatTarget != null && combatTarget.IsHostileTowards(The.Player) && combatTarget.IsVisible())
							{
								int? num19 = The.Player.Con(combatTarget);
								if (num19.HasValue && (!num17.HasValue || num19 < num17 || (num19 == num17 && combatTarget.Health() < gameObject7.Health())))
								{
									gameObject7 = combatTarget;
									num16 = 1;
								}
							}
						}
						if (gameObject7 != null && num16 <= 1)
						{
							string directionFromCell4 = currentCell13.GetDirectionFromCell(gameObject7.CurrentCell);
							if (!directionFromCell4.IsNullOrEmpty() && directionFromCell4 != ".")
							{
								try
								{
									AutoAct.Attacking = true;
									The.Player.AttackDirection(directionFromCell4);
									break;
								}
								finally
								{
									AutoAct.Attacking = false;
								}
							}
						}
						num16 = int.MaxValue;
						List<XRL.World.GameObject> list4 = currentCell13.ParentZone.FastSquareVisibility(currentCell13.X, currentCell13.Y, 80, "Brain", The.Player, VisibleToPlayerOnly: true, IncludeWalls: true);
						int num20 = 0;
						for (int count7 = list4.Count; num20 < count7; num20++)
						{
							XRL.World.GameObject gameObject8 = list4[num20];
							if (gameObject8.IsHostileTowards(The.Player))
							{
								int num21 = currentCell13.DistanceTo(gameObject8);
								bool flag9 = false;
								if (num21 < num16)
								{
									flag9 = true;
								}
								else if (num21 == num16 && The.Player.Con(gameObject8) < The.Player.Con(gameObject7))
								{
									flag9 = true;
								}
								if (flag9 && gameObject8.CurrentCell.GetCombatTarget(The.Player) == gameObject8)
								{
									gameObject7 = gameObject8;
									num16 = num21;
								}
							}
						}
						if (gameObject7 != null)
						{
							FindPath findPath = new FindPath(currentCell13, gameObject7.CurrentCell, PathGlobal: false, PathUnlimited: true, The.Player, 20, ExploredOnly: true);
							if (!findPath.Usable)
							{
								if (flag8)
								{
									Popup.ShowFail("You can't find a way to flee from " + gameObject7.t() + ".");
								}
								else
								{
									Popup.ShowFail("You can't find a way to reach " + gameObject7.t() + ".");
								}
							}
							else
							{
								The.Player.Move(findPath.Directions[0]);
							}
						}
						else if (flag8)
						{
							gameObject7 = The.Player.Target;
							Popup.ShowFail("You can't find a way to flee from " + gameObject7.t() + ".");
						}
						else
						{
							MessageQueue.AddPlayerMessage("You don't see any hostiles nearby.");
						}
						break;
					}
					}
					return;
				}
				case "CmdWalk":
				{
					Zone activeZone = The.ZoneManager.ActiveZone;
					if (activeZone != null && !activeZone.IsWorldMap())
					{
						string text12 = PickDirection.ShowPicker("Walk");
						if (text12 == null)
						{
							return;
						}
						Cell currentCell11 = The.Player.CurrentCell;
						if (currentCell11 != null)
						{
							Cell cellFromDirection5 = currentCell11.GetCellFromDirection(text12);
							if (cellFromDirection5 != null)
							{
								if (cellFromDirection5.IsEmpty())
								{
									AutoAct.Setting = text12;
								}
								else
								{
									if (cellFromDirection5.HasObjectWithPart("Combat", (XRL.World.GameObject GO) => GO.IsHostileTowards(The.Player)))
									{
										Popup.ShowFail("You may not {{Y|w}}alk into a hostile creature!");
										break;
									}
									AutoAct.Setting = text12;
								}
							}
						}
						if (AutoAct.Setting == ".")
						{
							AutoAct.Setting = ".20";
						}
						return;
					}
					goto case "CmdNone";
				}
				case "CmdGet":
					The.Player.FireEvent("CommandGet");
					goto case "CmdNone";
				case "CmdGetFrom":
					The.Player.FireEvent("CommandGetFrom");
					goto case "CmdNone";
				case "CmdGetFromN":
					The.Player.FireEvent(XRL.World.Event.New("CommandGetFrom", "Direction", "N"));
					goto case "CmdNone";
				case "CmdGetFromS":
					The.Player.FireEvent(XRL.World.Event.New("CommandGetFrom", "Direction", "S"));
					goto case "CmdNone";
				case "CmdGetFromE":
					The.Player.FireEvent(XRL.World.Event.New("CommandGetFrom", "Direction", "E"));
					goto case "CmdNone";
				case "CmdGetFromW":
					The.Player.FireEvent(XRL.World.Event.New("CommandGetFrom", "Direction", "W"));
					goto case "CmdNone";
				case "CmdGetFromNE":
					The.Player.FireEvent(XRL.World.Event.New("CommandGetFrom", "Direction", "NE"));
					goto case "CmdNone";
				case "CmdGetFromNW":
					The.Player.FireEvent(XRL.World.Event.New("CommandGetFrom", "Direction", "NW"));
					goto case "CmdNone";
				case "CmdGetFromSW":
					The.Player.FireEvent(XRL.World.Event.New("CommandGetFrom", "Direction", "SW"));
					goto case "CmdNone";
				case "CmdGetFromSE":
					The.Player.FireEvent(XRL.World.Event.New("CommandGetFrom", "Direction", "SE"));
					goto case "CmdNone";
				case "CmdOpen":
				{
					string text9 = PickDirection.ShowPicker("Open");
					if (text9 != null)
					{
						Cell cellFromDirection4 = The.Player.CurrentCell.GetCellFromDirection(text9);
						if (cellFromDirection4 != null)
						{
							List<XRL.World.GameObject> objectsWithRegisteredEvent = cellFromDirection4.GetObjectsWithRegisteredEvent("Open");
							if (objectsWithRegisteredEvent.Count == 1)
							{
								objectsWithRegisteredEvent[0].FireEvent(XRL.World.Event.New("Open", "Opener", The.Player));
							}
							else if (objectsWithRegisteredEvent.Count > 1)
							{
								Popup.PickGameObject("Open", objectsWithRegisteredEvent, AllowEscape: true)?.FireEvent(XRL.World.Event.New("Open", "Opener", The.Player));
							}
						}
					}
					goto case "CmdNone";
				}
				case "CmdXP":
				{
					XRL.World.GameObject gameObject6 = The.Player;
					Game.Player.Body = null;
					Popup.Suppress = true;
					gameObject6.AwardXP(250000);
					Popup.Suppress = false;
					ParticleManager.Banners.Clear();
					Game.Player.Body = gameObject6;
					goto case "CmdNone";
				}
				case "CmdReload":
					CommandReloadEvent.Execute(The.Player);
					goto case "CmdNone";
				case "CmdReplaceCell":
					CommandReplaceCellEvent.Execute(The.Player);
					goto case "CmdNone";
				case "CmdTalk":
				{
					string text7 = PickDirection.ShowPicker("Talk");
					if (text7 != null)
					{
						Cell cellFromDirection3 = The.Player.CurrentCell.GetCellFromDirection(text7);
						if (cellFromDirection3 != null)
						{
							XRL.World.GameObject firstObjectWithPart = cellFromDirection3.GetFirstObjectWithPart("ConversationScript", (XRL.World.GameObject GO) =>
							{
								if (GO.IsPlayer())
								{
									return false;
								}
								return (GO.Render == null || GO.Render.Visible) ? true : false;
							});
							if (firstObjectWithPart == null || !firstObjectWithPart.GetPart<ConversationScript>().AttemptConversation(Silent: true))
							{
								XRL.World.GameObject firstObjectWithRegisteredEvent = cellFromDirection3.GetFirstObjectWithRegisteredEvent("ObjectTalking", (XRL.World.GameObject GO) => !GO.IsPlayer());
								if (firstObjectWithRegisteredEvent == null || !firstObjectWithRegisteredEvent.FireEvent("ObjectTalking"))
								{
									firstObjectWithPart?.GetPart<ConversationScript>().AttemptConversation();
								}
							}
						}
					}
					goto case "CmdNone";
				}
				case "CmdToggleMessageVerbosity":
					Game.Player.Messages.Terse = !Game.Player.Messages.Terse;
					if (Game.Player.Messages.Terse)
					{
						MessageQueue.AddPlayerMessage("Set Terse messages");
					}
					if (!Game.Player.Messages.Terse)
					{
						MessageQueue.AddPlayerMessage("Set Verbose messages");
					}
					goto case "CmdNone";
				case "CmdShowSidebar":
					Sidebar.Hidden = !Sidebar.Hidden;
					goto case "CmdNone";
				case "CmdShowSidebarMessages":
					Sidebar.SidebarState++;
					if (Sidebar.SidebarState >= 4)
					{
						Sidebar.SidebarState = 0;
					}
					goto case "CmdNone";
				case "CmdNoclipU":
				{
					Cell cellFromDirection2 = The.Player.CurrentCell.GetCellFromDirection("U", BuiltOnly: false);
					if (cellFromDirection2 != null)
					{
						The.Player.SystemMoveTo(cellFromDirection2);
						The.ZoneManager.SetActiveZone(cellFromDirection2.ParentZone);
						The.ZoneManager.ProcessGoToPartyLeader();
					}
					goto case "CmdNone";
				}
				case "CmdNoclipD":
				{
					Cell cellFromDirection = The.Player.CurrentCell.GetCellFromDirection("D", BuiltOnly: false);
					if (cellFromDirection != null)
					{
						The.Player.SystemMoveTo(cellFromDirection);
						The.ZoneManager.SetActiveZone(cellFromDirection.ParentZone);
						The.ZoneManager.ProcessGoToPartyLeader();
					}
					goto case "CmdNone";
				}
				case "CmdDismemberLimb":
				{
					Body body = The.Player.Body;
					BodyPart dismemberableBodyPart = Axe_Dismember.GetDismemberableBodyPart(The.Player);
					if (dismemberableBodyPart != null)
					{
						body.Dismember(dismemberableBodyPart);
					}
					goto case "CmdNone";
				}
				case "CmdRegenerateLimb":
					RegenerateLimbEvent.Send(The.Player);
					goto case "CmdNone";
				case "CmdMessageHistory":
					The.Game.Player.Messages.Show();
					goto case "CmdNone";
				case "CmdCompanions":
				{
					Zone currentZone = The.Player.CurrentZone;
					if (currentZone != null)
					{
						List<XRL.World.GameObject> companionsReadonly = The.Player.GetCompanionsReadonly(currentZone.Width * 2);
						if (companionsReadonly.IsNullOrEmpty())
						{
							if (The.Player.Brain.PartyMembers.Count == 0)
							{
								Popup.ShowFail("You have no companions.");
							}
							else
							{
								Popup.ShowFail("None of your companions are nearby.");
							}
							break;
						}
						companionsReadonly.Sort((XRL.World.GameObject a, XRL.World.GameObject b) => a.SortVs(b, null, UseCategory: false));
						Popup.PickGameObject("Interact with which companion?", companionsReadonly, AllowEscape: true, ShowContext: false, UseYourself: true, (XRL.World.GameObject o) => GetCompanionStatusEvent.GetFor(o, The.Player))?.Twiddle();
					}
					goto case "CmdNone";
				}
				default:
					if (The.Player != null)
					{
						if (The.Player.OnWorldMap() && !AbilityManager.IsWorldMapUsable(text2))
						{
							The.Player.ShowFailure("You cannot do that on the world map.");
						}
						else
						{
							CommandEvent.Send(The.Player, text2);
						}
					}
					goto case "CmdNone";
				case "CmdNone":
				case "CmdShowBrainlog":
				case "CmdZoomIn":
				case "CmdZoomOut":
				case "CmdUnknown":
					Sidebar.UpdateState();
					Sidebar.Update();
					flag = true;
					UpdateOverlay();
					if (Thread.CurrentThread != CoreThread)
					{
						GameManager.runPlayerTurnOnUIThread = false;
					}
					break;
				}
			}
			else if (Thread.CurrentThread != CoreThread)
			{
				GameManager.runPlayerTurnOnUIThread = false;
			}
			int num25 = 0;
			for (int count8 = OnEndPlayerTurnCallbacks.Count; num25 < count8; num25++)
			{
				OnEndPlayerTurnCallbacks[num25](this);
			}
			_ = FrameTimer.Elapsed.TotalMilliseconds;
			if (The.Player.Energy.Value > 0)
			{
				if (value != The.Player.Energy.Value)
				{
					value = The.Player.Energy.Value;
					Game.Player.Messages.EndPlayerTurn();
				}
				RenderBase(UpdateSidebar: false);
			}
			else
			{
				Game.Player.Messages.LastMessage = Game.Player.Messages.Messages.Count;
				RenderBase(UpdateSidebar: false);
			}
			AmbientSoundsSystem.Update();
			if (flag)
			{
				Sidebar.SidebarTick++;
				actionManager.UpdateMinimap();
				actionManager.RunCommands(Decrement: false);
				int num26 = 0;
				for (int count9 = OnEndPlayerTurnSingleCallbacks.Count; num26 < count9; num26++)
				{
					OnEndPlayerTurnSingleCallbacks[num26](this);
				}
				flag = false;
				Core.Game.Player.EnqueuePlayerCell();
				Core.Game.Player.CheckPlayerLocations();
			}
			if (The.Player.Energy.Value >= 1000)
			{
				Keyboard.IdleWait();
				if (actionManager.SkipPlayerTurn)
				{
					actionManager.SkipPlayerTurn = false;
					break;
				}
			}
			if (Thread.CurrentThread != CoreThread)
			{
				GameManager.runPlayerTurnOnUIThread = false;
				break;
			}
		}
	}

	public void RenderMapToBuffer(ScreenBuffer Buffer)
	{
		_ = FrameTimer.Elapsed.TotalMilliseconds;
		Game.ZoneManager.ActiveZone.Render(Buffer);
		_ = FrameTimer.Elapsed.TotalMilliseconds;
		TimeSpan elapsed = FrameTimer.Elapsed;
		CurrentFrameAccumulator = (int)(elapsed.TotalMilliseconds / 16.666669845581055);
		CurrentFrame = (int)(elapsed.TotalMilliseconds % 1000.0) / 16;
		CurrentFrameLong = (int)(elapsed.TotalMilliseconds % 1000.0);
		CurrentFrame10 = (int)(elapsed.TotalMilliseconds % 10000.0) / 16;
		CurrentFrameLong10 = (int)(elapsed.TotalMilliseconds % 10000.0);
		if (Options.DisableTextAnimationEffects)
		{
			CurrentFrame = 8;
			CurrentFrameLong = 8;
			CurrentFrame10 = 8;
			CurrentFrameLong10 = 8;
		}
		if (Confusion.CurrentConfusionLevel > 0)
		{
			ConfusionShuffle(Buffer);
		}
		int i = 0;
		for (int count = AfterRenderCallbacks.Count; i < count; i++)
		{
			AfterRenderCallbacks[i](this, Buffer);
		}
	}

	public void RenderBaseToBuffer(ScreenBuffer Buffer)
	{
		Sidebar.UpdateState();
		CludgeTargetRendered = false;
		RenderFloorTextures = !Options.DisableFloorTextures;
		if (GameManager.bDraw == 7)
		{
			return;
		}
		Zone activeZone = Game.ZoneManager.ActiveZone;
		if (activeZone == null)
		{
			return;
		}
		if (activeZone.IsWorldMap())
		{
			XRL.World.Parts.Physics physics = The.Player.Physics;
			BeforeRenderEvent.Send(activeZone);
			if (physics != null)
			{
				activeZone.ExploreAll();
				activeZone.LightAll();
				activeZone.VisAll();
			}
			activeZone.Render(Buffer);
			int i = 0;
			for (int count = AfterRenderCallbacks.Count; i < count; i++)
			{
				AfterRenderCallbacks[i](this, _Buffer);
			}
			if (!CludgeTargetRendered && Sidebar.CurrentTarget != null && !Sidebar.CurrentTarget.IsVisible())
			{
				if (XRL.World.GameObject.Validate(Sidebar.CurrentTarget))
				{
					MessageQueue.AddPlayerMessage("You have lost sight of " + Sidebar.CurrentTarget.t() + ".");
				}
				Sidebar.CurrentTarget = null;
			}
			Sidebar.Render(Buffer);
			if (AllowWorldMapParticles)
			{
				ParticleManager.Render(Buffer);
			}
			else
			{
				ParticleManager.Particles.Clear();
			}
		}
		else
		{
			activeZone.ClearLightMap();
			activeZone.ClearVisiblityMap();
			BeforeRenderEvent.Send(activeZone);
			Cell currentCell = The.Player.CurrentCell;
			if (currentCell != null && activeZone != null)
			{
				PlayerVisibilityRadius = The.Player.GetVisibilityRadius();
				activeZone.AddVisibility(currentCell.X, currentCell.Y, PlayerVisibilityRadius);
			}
			if (Core.VisAllToggle)
			{
				activeZone.VisAll();
				activeZone.ExploreAll();
				activeZone.LightAll();
			}
			if (GameManager.bDraw == 11)
			{
				return;
			}
			activeZone.Render(Buffer);
			for (int j = 0; j < AfterRenderCallbacks.Count; j++)
			{
				AfterRenderCallbacks[j](this, _Buffer);
			}
			if ((GameManager.bDraw > 0 && GameManager.bDraw <= 20) || GameManager.bDraw == 21)
			{
				return;
			}
			if (!CludgeTargetRendered && Sidebar.CurrentTarget != null && !Sidebar.CurrentTarget.IsVisible())
			{
				if (XRL.World.GameObject.Validate(Sidebar.CurrentTarget))
				{
					MessageQueue.AddPlayerMessage("You have lost sight of " + Sidebar.CurrentTarget.t() + ".");
				}
				Sidebar.CurrentTarget = null;
			}
			ParticleManager.Render(Buffer);
			if (Confusion.CurrentConfusionLevel > 0)
			{
				ConfusionShuffle(Buffer);
			}
			if (GameManager.bDraw == 22)
			{
				return;
			}
			Sidebar.Render(Buffer);
		}
		RenderHiddenPlayer = true;
	}

	public void ConfusionShuffle(ScreenBuffer Buffer)
	{
		ConfusionRng = new System.Random((int)Core.Game.Turns);
		for (int i = 0; i < Buffer.Height; i++)
		{
			for (int j = 0; j < Buffer.Width; j++)
			{
				if (The.Player != null && The.Player.CurrentCell != null && (j != The.Player.CurrentCell.X || i != The.Player.CurrentCell.Y))
				{
					int num = j + ConfusionRng.Next(-1, 1);
					int num2 = i + ConfusionRng.Next(-1, 1);
					if ((num != The.Player.Physics.CurrentCell.X || num2 != The.Player.Physics.CurrentCell.Y) && num >= 0 && num < Buffer.Width - 1 && num2 >= 0 && num2 < Buffer.Height - 1)
					{
						ConsoleChar value = Buffer[j, i];
						Buffer[j, i] = Buffer[num, num2];
						Buffer[num, num2] = value;
						if (The.Player != null && The.Player.GetFuriousConfusion() > 0)
						{
							Buffer[j, i].SetColorsFromOldCharCode(ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Red, TextColor.Black));
							Buffer[num, num2].SetColorsFromOldCharCode(ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Red, TextColor.Black));
						}
					}
				}
				if (The.Player != null && The.Player.GetFuriousConfusion() > 0)
				{
					Buffer[j, i].SetColorsFromOldCharCode(ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Red, TextColor.Black));
				}
			}
		}
	}

	/// <returns>true if the type delay executed to completion; false if interrupted via input.</returns>
	public bool RenderDelay(int Milliseconds, bool Interruptible = true, bool SkipInput = true, bool AllowHiddenPlayer = false)
	{
		long num = FrameTimer.ElapsedMilliseconds + Milliseconds;
		while (FrameTimer.ElapsedMilliseconds < num)
		{
			if (Interruptible && Keyboard.kbhit())
			{
				if (SkipInput)
				{
					Keyboard.getch();
				}
				return false;
			}
			if (AllowHiddenPlayer)
			{
				RenderHiddenPlayer = false;
			}
			RenderBase();
		}
		return true;
	}

	public string GenerateRandomPlayerName(string Type = "")
	{
		return NameMaker.MakeName(null, null, Type);
	}

	public string GenerateRandomPlayerName(XRL.World.GameObject Player)
	{
		return NameMaker.MakeName(Player);
	}

	public void RenderBase(bool UpdateSidebar = true, bool DuringRestOkay = false)
	{
		if (Thread.CurrentThread == CoreThread)
		{
			GameManager.Instance.gameQueue.executeTasks();
		}
		if (GameManager.bDraw == 1)
		{
			return;
		}
		if (UpdateSidebar)
		{
			Sidebar.Update();
		}
		string setting = AutoAct.Setting;
		if (!DuringRestOkay && (setting == "r" || setting == "z" || (setting.Length > 0 && setting[0] == '.')))
		{
			return;
		}
		if (HostileWalkObjects.Count > 0)
		{
			HostileWalkObjects.Clear();
		}
		if (GameManager.bDraw == 2)
		{
			return;
		}
		ParticleManager.Frame();
		TimeSpan elapsed = FrameTimer.Elapsed;
		CurrentFrame = (int)(elapsed.TotalMilliseconds % 1000.0) / 16;
		CurrentFrameLong = (int)(elapsed.TotalMilliseconds % 1000.0);
		CurrentFrame10 = (int)(elapsed.TotalMilliseconds % 10000.0) / 16;
		CurrentFrameLong10 = (int)(elapsed.TotalMilliseconds % 10000.0);
		nFrame++;
		if (Options.DisableTextAnimationEffects)
		{
			CurrentFrame = 8;
			CurrentFrameLong = 8;
			CurrentFrame10 = 8;
			CurrentFrameLong10 = 8;
		}
		if (GameManager.bDraw == 3)
		{
			return;
		}
		RenderBaseToBuffer(_Buffer);
		if (GameManager.bDraw > 0 && GameManager.bDraw <= 15)
		{
			return;
		}
		if (ShowFPS)
		{
			_Buffer.Goto(0, 0);
			_Buffer.Write("Frame: " + nFrame + " GC:" + GC.CollectionCount(0) + " M:" + GC.GetTotalMemory(forceFullCollection: false));
			_Buffer.Goto(0, 1);
			_Buffer.Write("Objects Created: " + GameObjectFactory.Factory.ObjectsCreated);
			_Buffer.Goto(0, 2);
			_Buffer.Write("Rendered Objects: " + Core.RenderedObjects);
			_Buffer.Goto(0, 3);
			_Buffer.Write("FPS: " + nFrame / (FrameTimer.ElapsedMilliseconds / 1000));
		}
		if (GameManager.bDraw == 22)
		{
			return;
		}
		_Console.DrawBuffer(_Buffer, ImposterManager.getImposterUpdateFrame(_Buffer));
		if (GameManager.bDraw == 23 || !(setting != ""))
		{
			return;
		}
		if (OldHostileWalkObjects != null && HostileWalkObjects != null)
		{
			int i = 0;
			for (int count = HostileWalkObjects.Count; i < count; i++)
			{
				XRL.World.GameObject gameObject = HostileWalkObjects[i];
				if (!OldHostileWalkObjects.Contains(gameObject))
				{
					AutoAct.Interrupt(gameObject);
					OldHostileWalkObjects.Clear();
					OldHostileWalkObjects.AddRange(HostileWalkObjects);
					HostileWalkObjects.Clear();
					break;
				}
			}
		}
		if (setting != "")
		{
			OldHostileWalkObjects.Clear();
			OldHostileWalkObjects.AddRange(HostileWalkObjects);
			HostileWalkObjects.Clear();
		}
	}

	public void PerformSmartUse(string Direction = null, int MinPriority = 0)
	{
		Cell currentCell = The.Player.CurrentCell;
		List<Cell> list = null;
		if (Direction == null)
		{
			list = new List<Cell>(currentCell.GetAdjacentCells());
			list.Add(currentCell);
		}
		else
		{
			SmartUseCells.Clear();
			list = SmartUseCells;
			Cell cellFromDirection = currentCell.GetCellFromDirection(Direction);
			if (cellFromDirection != null)
			{
				list.Add(cellFromDirection);
			}
			list.Add(currentCell);
		}
		XRL.World.GameObject gameObject = null;
		bool flag = false;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			XRL.World.GameObject gameObject2 = SmartUse.FindPlayerSmartUseObject(list[i], MinPriority);
			if (gameObject2 != null)
			{
				if (gameObject != null)
				{
					flag = true;
				}
				else
				{
					gameObject = gameObject2;
				}
			}
		}
		bool flag2 = false;
		if (list.Contains(currentCell))
		{
			flag2 = currentCell.HasObjectWithPart("Physics", SmartUse.CanPlayerTake);
		}
		SmartUseCells.Clear();
		if (gameObject != null)
		{
			if (!flag && !flag2)
			{
				SmartUse.PlayerPerformSmartUse(gameObject);
				return;
			}
			string text = ((Direction != null) ? Direction : PickDirection.ShowPicker("Use"));
			if (text != null)
			{
				Cell cellFromDirection2 = currentCell.GetCellFromDirection(text);
				if (cellFromDirection2 != null)
				{
					AttemptSmartUse(cellFromDirection2);
				}
			}
		}
		else if (MinPriority > 0)
		{
			PerformSmartUse(Direction);
		}
		else
		{
			The.Player.FireEvent(XRL.World.Event.New("CommandGet", "GetOne", false, "SmartUse", true));
		}
	}

	public void ResetGameBasedStaticCaches()
	{
		Type typeFromHandle = typeof(HasGameBasedStaticCacheAttribute);
		Type typeFromHandle2 = typeof(GameBasedStaticCacheAttribute);
		Type typeFromHandle3 = typeof(GameBasedCacheInitAttribute);
		foreach (FieldInfo item in ModManager.GetFieldsWithAttribute(typeFromHandle2, typeFromHandle))
		{
			try
			{
				if (!item.IsStatic)
				{
					continue;
				}
				GameBasedStaticCacheAttribute customAttribute = item.GetCustomAttribute<GameBasedStaticCacheAttribute>();
				if (!customAttribute.ClearInstance)
				{
					goto IL_0103;
				}
				object value = item.GetValue(null);
				if (value == null)
				{
					goto IL_0103;
				}
				if (value is IDictionary dictionary)
				{
					dictionary.Clear();
					continue;
				}
				if (value is IList list)
				{
					list.Clear();
					continue;
				}
				try
				{
					MethodInfo method = value.GetType().GetMethod("Clear", Array.Empty<Type>());
					if (method != null)
					{
						method.Invoke(value, Array.Empty<object>());
						continue;
					}
				}
				catch (Exception ex)
				{
					MetricsManager.LogAssemblyError(item, "Error clearing field " + item.Name + ": " + ex);
				}
				goto IL_0103;
				IL_0103:
				value = ((customAttribute.CreateInstance || item.FieldType.IsValueType) ? Activator.CreateInstance(item.FieldType) : null);
				item.SetValue(null, value);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("resetting field " + item?.Name, x);
			}
		}
		foreach (MethodInfo item2 in ModManager.GetMethodsWithAttribute(typeFromHandle3, typeFromHandle))
		{
			try
			{
				if (item2.IsStatic)
				{
					item2.Invoke(null, Array.Empty<object>());
				}
			}
			catch (Exception x2)
			{
				MetricsManager.LogException($"Invoking game based cache init {item2.DeclaringType}.{item2.Name}", x2);
			}
		}
	}

	public XRLGame NewGame()
	{
		GameManager.Instance.gameQueue.clear();
		if (Game != null)
		{
			Game.Release();
		}
		using (CPUBoost.AutoScopeForceMainThread())
		{
			LoadEverything();
			ResetGameBasedStaticCaches();
			MemoryHelper.GCCollectMax();
		}
		Game = new XRLGame(_Console, _Buffer);
		Game.CreateNewGame();
		Reset();
		EmbarkInfo result = EmbarkBuilder.Begin().Result;
		if (result == null)
		{
			return null;
		}
		try
		{
			result.bootGame(Game);
			return Game;
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Booting game", x);
			GameManager.Instance.SetGameViewStack("MainMenu");
			return null;
		}
	}

	public void UpdateGlobalChoices()
	{
		if (Game != null && !Game.StringGameState.ContainsKey("SeekerEnemyFaction"))
		{
			for (int i = 0; i < 1000; i++)
			{
				Faction randomFaction = Factions.GetRandomFaction("Seekers");
				if (randomFaction != null)
				{
					Game.SetStringGameState("SeekerEnemyFaction", randomFaction.DisplayName);
					Factions.Get("Seekers").SetFactionFeeling(randomFaction.Name, -100);
					break;
				}
			}
		}
		Factions.RequireCachedHeirlooms();
		BookUI.InitBooks();
	}

	public void CreateMarkOfDeath()
	{
		char[] list = new char[9] { '!', '@', '#', '$', '%', '*', '(', ')', '/' };
		string text = "";
		for (int i = 0; i < 3; i++)
		{
			text += list.GetRandomElement();
		}
		for (int num = 2; num >= 0; num--)
		{
			text += text[num];
		}
		Game.SetStringGameState("MarkOfDeath", text);
		JournalAPI.AddObservation("The lost Mark of Death from the late sultanate was " + text + ".", "MarkOfDeathSecret", "Gossip", "MarkOfDeathSecret", new string[2] { "gossip", "old" }, revealed: false, -1L, "You recover the Mark of Death of the late sultanate.");
	}

	public void CreateCures()
	{
		List<string> list = new List<string>();
		list.Add("asphalt");
		list.Add("oil");
		list.Add("honey");
		list.Add("blood");
		list.Add("wine");
		list.Add("salt");
		list.Add("cider");
		list.Add("sap");
		List<string> list2 = new List<string>();
		list2.Add("wine");
		list2.Add("honey");
		list2.Add("water");
		list2.Add("cider");
		list2.Add("sap");
		int num = 0;
		for (int i = 1; i <= 2; i++)
		{
			num = Stat.Random(0, list.Count - 1);
			Game.SetStringGameState("GlotrotCure" + i, list[num]);
			if (list2.CleanContains(list[num]))
			{
				list2.Remove(list[num]);
			}
			list.RemoveAt(num);
		}
		num = Stat.Random(0, list2.Count - 1);
		Game.SetStringGameState("GlotrotCure3", list2[num]);
		List<string> list3 = new List<string>();
		list3.Add("blood");
		list3.Add("honey");
		list3.Add("wine");
		list3.Add("oil");
		list3.Add("asphalt");
		list3.Add("sap");
		Game.SetStringGameState("IronshankCure", list3.GetRandomElement());
		List<string> list4 = new List<string>();
		list4.Add("salt");
		list4.Add("cider");
		list4.Add("ink");
		Game.SetStringGameState("MonochromeCure", list4.GetRandomElement());
		GenerateFungalCure();
	}

	public void GenerateFungalCure()
	{
		string randomElement = new List<string>
		{
			"cider", "honey", "wine", "oil", "asphalt", "blood", "slime", "acid", "putrid", "convalessence",
			"proteangunk", "sap"
		}.GetRandomElement();
		Game.SetStringGameState("FungalCureLiquid", randomElement);
		Game.SetStringGameState("FungalCureLiquidDisplay", LiquidVolume.GetLiquid(randomElement).GetName(null).Strip());
		List<string> list = new List<string>();
		for (int i = 0; i < GameObjectFactory.Factory.BlueprintList.Count; i++)
		{
			GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.BlueprintList[i];
			if (!gameObjectBlueprint.DescendsFrom("BaseWorm") || gameObjectBlueprint.Tags.ContainsKey("NoCure") || !gameObjectBlueprint.HasPart("Corpse") || gameObjectBlueprint.GetPartParameter("Corpse", "CorpseChance", 0) <= 0)
			{
				continue;
			}
			string partParameter = gameObjectBlueprint.GetPartParameter<string>("Corpse", "CorpseBlueprint");
			if (GameObjectFactory.Factory.GetBlueprint(partParameter).DescendsFrom("Corpse"))
			{
				list.Add(partParameter);
				if (XRL.World.GameObject.CreateSample(partParameter).DisplayName.Strip().Contains("object"))
				{
					MetricsManager.LogError("Invalid fungal worm selected: " + partParameter);
				}
			}
		}
		string randomElement2 = list.GetRandomElement();
		Game.SetStringGameState("FungalCureWorm", randomElement2);
		Game.SetStringGameState("FungalCureWormDisplay", XRL.World.GameObject.Create(randomElement2).DisplayName.Strip());
	}

	public void RunGame()
	{
		The.ParticleManager.reset();
		if (Game.Player.Body.GetHPPercent() < Globals.HPWarningThreshold)
		{
			HPWarning = true;
		}
		RenderBase();
		UpdateGlobalChoices();
		while (Game.Running)
		{
			try
			{
				if (GameManager.runWholeTurnOnUIThread > 0 && Thread.CurrentThread == CoreThread)
				{
					waitForSegmentOnGameThread = true;
					GameManager.Instance.uiQueue.queueTask(() =>
					{
						Thread.Sleep(100);
						while (GameManager.runWholeTurnOnUIThread > 0)
						{
							Game.ActionManager.RunSegment();
						}
						waitForSegmentOnGameThread = false;
					}, 10);
					while (waitForSegmentOnGameThread)
					{
					}
				}
				else
				{
					Game.ActionManager.RunSegment();
				}
			}
			catch (ThreadAbortException)
			{
				MetricsManager.LogInfo("Game thread ended");
			}
			catch (Exception x)
			{
				MetricsManager.LogError("TurnError", x);
			}
		}
		The.ParticleManager.reset();
		FungalVisionary.VisionLevel = 0;
		GameManager.Instance.GreyscaleLevel = 0;
		Sidebar.MaxHP = 0;
		HPWarning = false;
		if (Game.DeathReason != "<nodeath>" && !Game.forceNoDeath)
		{
			BuildScore();
			if (!Options.DisablePermadeath)
			{
				DataManager.DeleteSaveDirectory(Game.GetCacheDirectory());
			}
		}
	}

	public void StashScore()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int value = ConstructScore(The.Game.GetStringGameState("EndgameCause"), stringBuilder, out var Cause);
		The.Game.SetIntGameState("StashedScore", value);
		The.Game.SetStringGameState("StashedScoreDetails", stringBuilder.ToString());
		The.Game.SetStringGameState("StashedScoreCause", Cause);
		The.Game.SetStringGameState("StashedReferenceDisplayName", The.Player?.GetReferenceDisplayName());
		The.Game.SetIntGameState("StashedLevel", The.Player?.Level ?? 0);
	}

	public int ConstructScore(string FakeDeathReason, StringBuilder SB, out string Cause)
	{
		int num = 0;
		XRLGame game = The.Game;
		XRL.World.GameObject gameObject = The.Player;
		num += (int)((double)gameObject.Stat("XP") * 0.334);
		XRL.World.GameObject gameObject2 = null;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		using ScopeDisposedList<XRL.World.GameObject> scopeDisposedList = ScopeDisposedList<XRL.World.GameObject>.GetFromPool();
		gameObject.GetContents(scopeDisposedList);
		foreach (XRL.World.GameObject item in scopeDisposedList)
		{
			if (item.HasTag("ExcludeFromGameScore"))
			{
				continue;
			}
			num += item.Count;
			if (!item.TryGetPart<Examiner>(out var Part) || Part.Complexity <= 0)
			{
				continue;
			}
			int techTier = item.GetTechTier();
			int tier = item.GetTier();
			int num7 = techTier + tier + Part.Complexity + Part.Difficulty;
			bool flag = false;
			if (num7 > num6)
			{
				flag = true;
			}
			else if (num7 == num6)
			{
				if (techTier > num5)
				{
					flag = true;
				}
				else if (techTier == num5)
				{
					if (tier > num4)
					{
						flag = true;
					}
					else if (tier == num4)
					{
						if (Part.Complexity > num2)
						{
							flag = true;
						}
						else if (Part.Complexity == num2 && Part.Difficulty > num3)
						{
							flag = true;
						}
					}
				}
			}
			if (flag)
			{
				gameObject2 = item;
				num5 = techTier;
				num4 = tier;
				num2 = Part.Complexity;
				num3 = Part.Difficulty;
				num6 = num7;
			}
			num += Math.Max(techTier, Part.Complexity) * 250 * item.Count;
		}
		num += game.FinishedQuests.Count * 733;
		num += game.Quests.Count * 133;
		num += (int)((double)game.Turns * 0.01);
		num += game.ZoneManager.VisitedTime.Keys.Count * 35;
		num += 1211 * game.GetIntGameState("ArtifactsGenerated");
		int intGameState = game.GetIntGameState("LairsFound");
		num += intGameState * 75;
		num -= 387;
		num -= 35;
		char value = 'ê';
		if (!string.IsNullOrEmpty(game.GetStringGameState("EndType")))
		{
			if (EndGame.IsUltimate)
			{
				num = (int)((float)num * 10f);
				value = 'í';
			}
			else if (EndGame.IsAnyNorthSheva)
			{
				num *= 5;
				value = 'í';
			}
			else if (EndGame.IsBrightsheol)
			{
				num = (int)((double)num * 2.5);
				value = 'í';
			}
			else
			{
				num += 1000000;
				value = 'í';
			}
		}
		SB.Clear();
		SB.Append("{{C|").Append(value).Append("}} Game summary for {{W|")
			.Append(game.PlayerName)
			.Append("}} {{C|")
			.Append(value)
			.Append("}}\n\n");
		SB.Append("This game ended on ").Append(DateTime.Now.ToLocalTime().ToLongDateString()).Append(" at ")
			.Append(DateTime.Now.ToLocalTime().ToLongTimeString())
			.Append(".\n");
		Cause = "";
		if (!FakeDeathReason.IsNullOrEmpty())
		{
			Cause = FakeDeathReason;
			SB.Append(FakeDeathReason).Append('\n');
		}
		else
		{
			Cause = game.DeathReason.Replace("!", ".");
			SB.Append(Cause).Append('\n');
		}
		SB.Append("You were level {{C|" + gameObject.Stat("Level") + "}}.\n");
		SB.Append("You scored {{C|").Append(num).Append("}} ")
			.Append((num == 1) ? "point" : "points")
			.Append(".\n");
		MetricsManager.LogEvent("Death:score", num);
		SB.Append("You survived for {{C|").Append(game.Turns).Append("}} ")
			.Append((game.Turns == 1) ? "turn" : "turns")
			.Append(".\n");
		MetricsManager.LogEvent("Death:Turns", game.Turns);
		MetricsManager.LogEvent("Death:Zones", game.ZoneManager.VisitedTime.Keys.Count);
		if (intGameState > 0)
		{
			SB.Append("You found {{C|").Append(intGameState).Append("}} ")
				.Append((intGameState == 1) ? "lair" : "lairs")
				.Append(".\n");
			MetricsManager.LogEvent("Death:Lairs", intGameState);
		}
		int intGameState2 = game.GetIntGameState("PlayerItemNamingDone");
		if (intGameState2 > 0)
		{
			SB.Append("You named {{C|").Append(intGameState2).Append("}} ")
				.Append((intGameState2 == 1) ? "item" : "items")
				.Append(".\n");
			MetricsManager.LogEvent("Death:ItemsNamed", intGameState2);
		}
		if (game.HasIntGameState("ArtifactsGenerated"))
		{
			int intGameState3 = game.GetIntGameState("ArtifactsGenerated");
			SB.Append("You generated {{C|").Append(intGameState3).Append("}} storied ")
				.Append((intGameState3 == 1) ? "item" : "items")
				.Append(".\n");
			MetricsManager.LogEvent("Death:Artifacts", intGameState3);
		}
		game.HasIntGameState("MetempsychosisCount");
		if (gameObject2 != null)
		{
			string text = gameObject2.an(int.MaxValue, null, null, AsIfKnown: true, Single: true, NoConfusion: true);
			SB.Append("The most advanced artifact in your possession was " + text + ".\n");
			MetricsManager.LogEvent("Death:artifact:" + text + " [" + gameObject2.Blueprint + "]");
		}
		SB.Append("This game was played in ").Append(game.gameMode).Append(" mode.\n");
		SB.Append("\n\n");
		SB.Append("{{C|").Append(value).Append("}} Chronology for {{W|")
			.Append(game.PlayerName)
			.Append("}} {{C|")
			.Append(value)
			.Append("}}\n\n");
		foreach (JournalAccomplishment accomplishment in JournalAPI.Accomplishments)
		{
			SB.Append("{{C|").Append('ú').Append("}} ")
				.Append(accomplishment.GetDisplayText())
				.Append("\n");
		}
		SB.Append("\n\n");
		SB.Append("{{C|").Append(value).Append("}} Final messages for {{W|")
			.Append(game.PlayerName)
			.Append("}} {{C|")
			.Append(value)
			.Append("}}\n\n");
		int num8 = 0;
		foreach (string lines in The.Game.Player.Messages.GetLinesList(0, 30))
		{
			if (num8 != 0)
			{
				SB.Append("\n");
			}
			num8++;
			SB.Append(lines);
		}
		return num;
	}

	public void BuildScore(bool Real = true, string FakeDeathReason = null, bool showUI = true)
	{
		int score = 0;
		string text = "";
		string Cause = "";
		string text2 = The.Player.GetReferenceDisplayName();
		int num = The.Player.Level;
		if (The.Game.HasIntGameState("StashedScore") && The.Game.HasStringGameState("StashedScoreDetails") && The.Game.HasStringGameState("StashedScoreCause"))
		{
			score = The.Game.GetIntGameState("StashedScore");
			text = The.Game.GetStringGameState("StashedScoreDetails");
			Cause = The.Game.GetStringGameState("StashedScoreCause");
			text2 = The.Game.GetStringGameState("StashedReferenceDisplayName", text2);
			num = The.Game.GetIntGameState("StashedLevel", num);
		}
		else
		{
			StringBuilder stringBuilder = new StringBuilder();
			score = ConstructScore(FakeDeathReason, stringBuilder, out Cause);
			text = stringBuilder.ToString();
		}
		if (string.IsNullOrEmpty(FakeDeathReason) && The.Game.HasStringGameState("EndgameCause"))
		{
			FakeDeathReason = The.Game.GetStringGameState("EndgameCause");
			Cause = FakeDeathReason;
		}
		MetricsManager.LogEvent("Death:score:" + score);
		string leaderboardID = null;
		if (Real)
		{
			try
			{
				if (!Game.GetStringGameState("leaderboardMode").IsNullOrEmpty())
				{
					string prefID = "leaderboardresult_" + Game.GetStringGameState("leaderboardMode");
					if (!Prefs.HasString(prefID))
					{
						Func<string> submitResult = null;
						submitResult = () => LeaderboardManager.SubmitResult(Game.GetStringGameState("leaderboardMode"), score, (LeaderboardScores result) =>
						{
							if (result != null)
							{
								Prefs.SetString(prefID, score.ToString());
								StringBuilder stringBuilder2 = new StringBuilder();
								int num2 = Math.Min(result.GetEntryCount(), 10);
								for (int i = 0; i < num2; i++)
								{
									LeaderboardEntry leaderboardEntry = result.GetLeaderboardEntry(i);
									stringBuilder2.Append(leaderboardEntry.rank + ": {{Y|" + leaderboardEntry.displayName + "}} ({{W|" + leaderboardEntry.score + "}})\n");
								}
								if (!LeaderboardManager.leaderboardresults.ContainsKey(leaderboardID))
								{
									LeaderboardManager.leaderboardresults.Add(leaderboardID, "");
								}
								LeaderboardManager.leaderboardresults[leaderboardID] = stringBuilder2.ToString();
								UnityEngine.Debug.Log(stringBuilder2.ToString());
								Keyboard.PushMouseEvent("LeaderboardResultsUpdated");
							}
							else
							{
								UnityEngine.Debug.Log("Could not get online for leaderboards.");
								Popup.ShowYesNo("An error occurred while posting to the leaderbaord, would you like to try again?", "Sounds/UI/ui_notification", AllowEscape: true, DialogResult.Yes, (DialogResult dialogResult) =>
								{
									switch (dialogResult)
									{
									case DialogResult.Yes:
									case DialogResult.OK:
										submitResult();
										break;
									default:
										Prefs.SetString(prefID, score.ToString());
										PlatformManager.SetOfflineMode(isOffline: true);
										break;
									}
								});
							}
						});
						leaderboardID = submitResult();
					}
				}
			}
			catch (Exception ex)
			{
				leaderboardID = null;
				LogError(ex);
			}
			if (!The.Game.DontSaveThisIsAReplay)
			{
				Scores.Scoreboard.Add(score, text, Game.Turns, Game.GameID, Game.GetStringGameState("GameMode"), CheckpointingSystem.IsCheckpointingEnabled(), num, text2);
			}
			if (leaderboardID != null)
			{
				text = "<%leaderboard%>\n\n" + text;
			}
		}
		if (showUI)
		{
			GameSummaryUI.Show(score, text, Cause, Game.PlayerName, leaderboardID, Real);
		}
	}

	public void SaveGame(string GameName)
	{
		Game.SaveGame(GameName);
	}

	[Obsolete("ShowThinker does nothing and will be removed in a future version")]
	public static void ShowThinker()
	{
	}

	public static void SetClipboard(string Msg)
	{
		UnityEngine.Debug.LogError(Msg);
	}

	public static void LogError(string Error)
	{
		UnityEngine.Debug.LogError("ERROR:" + Error);
		if (!Keyboard.Closed)
		{
			MetricsManager.LogException("Unknown", new Exception(Error));
		}
		if (Keyboard.Closed)
		{
			Thread.CurrentThread.Abort();
			throw new Exception("Stopping game thread with an exception!");
		}
	}

	public static void Log(string Info)
	{
		UnityEngine.Debug.Log(Info);
	}

	public static void LogError(Exception ex)
	{
		UnityEngine.Debug.LogError("ERROR:" + ex.ToString());
		if (Keyboard.Closed)
		{
			Thread.CurrentThread.Abort();
			throw new Exception("Stopping game thread with an exception!");
		}
	}

	public static void LogError(string Category, Exception ex)
	{
		UnityEngine.Debug.LogError("ERROR:" + Category + ":" + ex.ToString());
		if (Keyboard.Closed)
		{
			Thread.CurrentThread.Abort();
			throw new Exception("Stopping game thread with an exception!");
		}
	}

	public static void LogError(string Category, string error)
	{
		UnityEngine.Debug.LogError("ERROR:" + Category + ":" + error);
		if (Keyboard.Closed)
		{
			Thread.CurrentThread.Abort();
			throw new Exception("Stopping game thread with an exception!");
		}
	}

	public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
	{
		LogError("FATAL thread exception:" + e.ToString());
	}

	public bool RestoreModsLoaded(List<string> Enabled)
	{
		return RestoreModsLoadedAsync(Enabled).Result;
	}

	public async Task<bool> RestoreModsLoadedAsync(List<string> Enabled)
	{
		List<string> list = new List<string>(Enabled.Except(ModManager.GetAvailableMods()));
		List<string> extraLoaded = new List<string>(ModManager.GetRunningMods().Except(Enabled));
		List<string> notLoaded = new List<string>(Enabled.Except(ModManager.GetRunningMods()).Except(list));
		if (list.Count > 0 && (await Popup.NewPopupMessageAsync(XRL.World.Event.NewStringBuilder("One or more mods enabled in this save are {{red|not available}}:{{red|").Compound(list.Select(ModManager.GetModTitle), "\n{{y|:}} ").Compound("}}Do you still wish to try to load this save?", "\n\n")
			.ToString(), PopupMessage.YesNoButton, null, "Incomplete Mod Configuration")).command != PopupMessage.YesNoButton[0].command)
		{
			return false;
		}
		if (extraLoaded.Count > 0 || notLoaded.Count > 0)
		{
			StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
			if (extraLoaded.Count > 0)
			{
				stringBuilder.Compound("These mods are {{red|disabled}} in the save:{{red|", '\n').Compound(extraLoaded.Select(ModManager.GetModTitle), "\n{{y|:}} ").Append("}}")
					.AppendLine();
			}
			if (notLoaded.Count > 0)
			{
				stringBuilder.Compound("These mods are {{green|enabled}} in the save:{{green|", '\n').Compound(notLoaded.Select(ModManager.GetModTitle), "\n{{y|:}} ").Append("}}")
					.AppendLine();
			}
			stringBuilder.AppendLine();
			string[] options = new string[2] { "Restart using save game's mod configuration", "Load keeping current mod configuration" };
			switch (await Popup.PickOptionAsync("Mod Configuration Differs", stringBuilder.ToString(), "", options, null, null, null, null, 0, 60, 0, -1, RespectOptionNewlines: false, AllowEscape: true))
			{
			case -1:
				return false;
			case 0:
				foreach (string item in extraLoaded)
				{
					ModManager.GetMod(item).IsEnabled = false;
				}
				foreach (string item2 in notLoaded)
				{
					ModManager.GetMod(item2).IsEnabled = true;
				}
				ModManager.WriteModSettings();
				GameManager.Restart();
				break;
			}
		}
		return true;
	}

	public void WriteConsoleLine(string s)
	{
		try
		{
			UnityEngine.Debug.Log(s);
		}
		catch
		{
		}
	}

	public static void Stop()
	{
		UnityEngine.Debug.Log("Stopping...");
		GameManager.bDraw = 0;
		Keyboard.Closed = true;
		CoreThread.Interrupt();
		CoreThread.Abort();
		Core.Game.Running = false;
	}

	private static string ArgumentPath(string Argument)
	{
		string fullPath = Path.GetFullPath(Argument);
		int length = Argument.Length;
		if (length == 0 || !Argument[length - 1].IsDirectorySeparator())
		{
			Argument += Path.DirectorySeparatorChar;
		}
		if (!Directory.Exists(Argument))
		{
			Directory.CreateDirectory(Argument);
		}
		return fullPath;
	}

	public static void InitializePaths()
	{
		Log("Initializing paths...");
		DataPath = Path.Combine(Application.streamingAssetsPath, "Base");
		SavePath = Path.PersistentDataPath();
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (commandLineArgs != null && commandLineArgs.Length > 0)
		{
			for (int i = 0; i < commandLineArgs.Length - 1; i++)
			{
				switch (commandLineArgs[i].ToUpper())
				{
				case "-SAVEPATH":
					SavePath = ArgumentPath(commandLineArgs[++i]);
					break;
				case "-SHAREDPATH":
					LocalPath = ArgumentPath(commandLineArgs[++i]);
					break;
				case "-SYNCEDPATH":
					SyncedPath = ArgumentPath(commandLineArgs[++i]);
					break;
				}
			}
		}
		Log("Data path: " + DataPath);
		Log("Save path: " + SavePath);
		Log("Shared path: " + LocalPath);
		Log("Synced path: " + SyncedPath);
		Folder.CreateDirectory(DataManager.LocalPath("Mods")).LogIfErrored();
		Folder.CreateDirectory(DataManager.SyncedPath("Saves")).LogIfErrored();
		DataManager.PerformFileLocationChangesAsAppropriate().Wait();
	}

	public static Thread Start()
	{
		UnityEngine.Debug.Log("Starting core...");
		CoreThread = new Thread(_ThreadStart)
		{
			Name = "XRLCore Thread"
		};
		CoreThread.CurrentCulture = CultureInfo.InvariantCulture;
		CoreThread.Priority = System.Threading.ThreadPriority.Highest;
		CoreThread.Start();
		bStarted = true;
		UnityEngine.Debug.Log("Started!");
		return CoreThread;
	}

	public static void _ThreadStart()
	{
		try
		{
			SynchronizationContext synchronizationContext = new SynchronizationContext();
			SynchronizationContext.SetSynchronizationContext(synchronizationContext);
			GameManager.Instance.gameThreadSynchronizationContext = synchronizationContext;
			Core = new XRLCore();
			Core._Start();
		}
		catch (ThreadInterruptedException)
		{
			UnityEngine.Debug.Log("Core thread shut down...");
		}
		catch (Exception message)
		{
			UnityEngine.Debug.LogError(message);
		}
	}

	public void ReloadUIViews()
	{
		WriteConsoleLine("Init Console UIs...\n");
		foreach (Type item in ModManager.GetTypesWithAttribute(typeof(UIView)))
		{
			if (typeof(IWantsTextConsoleInit).IsAssignableFrom(item) && Activator.CreateInstance(item) is IWantsTextConsoleInit wantsTextConsoleInit)
			{
				wantsTextConsoleInit.Init(_Console, _Buffer);
			}
		}
	}

	public void _Start()
	{
		Core = this;
		if (TextConsole.bExtended)
		{
			_Buffer = ScreenBuffer.create(80, 33);
			if (TextConsole.Mode == TerminalMode.OpenGL)
			{
			}
		}
		else
		{
			_Buffer = ScreenBuffer.create(80, 25);
			_ = TextConsole.Mode;
			_ = 1;
		}
		_Console = new TextConsole();
		_Console.ShowCursor(bShow: false);
		WriteConsoleLine("Cleaning up...\n");
		WriteConsoleLine("Starting up XRL...\n");
		using CPUBoost cPUBoost = CPUBoost.AutoScopeForceMainThread();
		WriteConsoleLine("Initialize Genders and Pronoun Sets...\n");
		Gender.Init();
		PronounSet.Init();
		WriteConsoleLine("Initialize Name Styles...\n");
		NameStyles.CheckInit();
		WriteConsoleLine("Loading World Blueprints...\n");
		WorldFactory.Factory.Init();
		WriteConsoleLine("Loading Object Blueprints...\n");
		GameObjectFactory.Init();
		WriteConsoleLine("Loading Help...\n");
		Manual = new XRLManual(_Console);
		WriteConsoleLine("Init Pathfinder...\n");
		FindPath.Initalize();
		ReloadUIViews();
		ParticleManager = new ParticleManager();
		ParticleManager.Init(_Console, _Buffer);
		if (TextConsole.Mode == TerminalMode.OpenGL)
		{
			_Console.Hide();
			_Console.FocusUI();
		}
		int num = 0;
		string s = "{{K|" + XRLGame.CoreVersion.ToString() + "}}";
		LoadEverything();
		WriteConsoleLine("Starting Game...");
		SoundManager.PlayMusic("Music/Pilgrims Path");
		bool flag = SavesAPI.HasSavedGameInfo();
		cPUBoost.Dispose();
		while (true)
		{
			if (GameManager.Instance.CurrentGameView != "MainMenu")
			{
				GameManager.Instance.SetGameViewStack("MainMenu");
			}
			GameManager.Instance.ClearRegions();
			XRL.World.Event.ResetPool();
			_Buffer.Clear();
			_Buffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			_Buffer.Goto(35, 9);
			_Buffer.Write(CapabilityManager.AllowKeyboardHotkeys ? "{{W|N}}ew game" : "New game");
			if (num == 0)
			{
				_Buffer.WriteAt(33, 9, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|New game}}");
			}
			GameManager.Instance.AddRegion(35, 9, 43, 9, "Pick:New Game", null, "Select:0");
			_Buffer.Goto(35, 10);
			if (flag)
			{
				_Buffer.Write(CapabilityManager.AllowKeyboardHotkeys ? "{{W|C}}ontinue" : "Continue");
			}
			else
			{
				_Buffer.Write("{{K|Continue}}");
			}
			GameManager.Instance.AddRegion(35, 10, 43, 10, "Pick:Continue", null, "Select:1");
			if (num == 1)
			{
				_Buffer.WriteAt(33, 10, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|Continue}}");
			}
			_Buffer.Goto(35, 12);
			_Buffer.Write(CapabilityManager.AllowKeyboardHotkeys ? "{{W|O}}ptions" : "Options");
			if (num == 2)
			{
				_Buffer.WriteAt(33, 12, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|Options}}");
			}
			GameManager.Instance.AddRegion(35, 12, 43, 12, "Pick:Options", null, "Select:2");
			_Buffer.Goto(35, 13);
			_Buffer.Write(CapabilityManager.AllowKeyboardHotkeys ? "{{W|H}}igh Scores" : "High Scores");
			if (num == 3)
			{
				_Buffer.WriteAt(33, 13, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|High Scores}}");
			}
			GameManager.Instance.AddRegion(35, 13, 43, 13, "Pick:High Scores", null, "Select:3");
			_Buffer.Goto(35, 14);
			_Buffer.Write(CapabilityManager.AllowKeyboardHotkeys ? "[{{W|?}}] Help" : "Help");
			if (num == 4)
			{
				_Buffer.WriteAt(33, 14, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|Help}}");
			}
			GameManager.Instance.AddRegion(35, 14, 43, 14, "Pick:Help", null, "Select:4");
			_Buffer.Goto(35, 15);
			_Buffer.Write(CapabilityManager.AllowKeyboardHotkeys ? "{{W|Q}}uit" : "Quit");
			if (num == 5)
			{
				_Buffer.WriteAt(33, 15, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|Quit}}");
			}
			GameManager.Instance.AddRegion(35, 15, 43, 15, "Pick:Quit", null, "Select:5");
			_Buffer.Goto(35, 18);
			_Buffer.Write(CapabilityManager.AllowKeyboardHotkeys ? "{{W|R}}edeem Code" : "Redeem Code");
			if (num == 6)
			{
				_Buffer.WriteAt(33, 18, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|Redeem Code}}");
			}
			GameManager.Instance.AddRegion(35, 18, 43, 18, "Pick:Redeem Code", null, "Select:6");
			_Buffer.Goto(35, 17);
			_Buffer.Write("{{g|Dromad Edition}}");
			if (ModManager.Mods != null && ModManager.Mods.Count > 0)
			{
				_Buffer.WriteAt(35, 21, CapabilityManager.AllowKeyboardHotkeys ? "{{W|M}}ods" : "Mods");
				if (num == 7)
				{
					_Buffer.WriteAt(33, 21, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|Mods}}");
				}
				GameManager.Instance.AddRegion(35, 21, 43, 21, "ModManager", null, "Select:7");
			}
			if (ModManager.IsAnyModFailed())
			{
				_Buffer.WriteAt(40, 21, "{{y|-}} {{R|You have mods with errors.}}");
			}
			else if (ModManager.IsAnyModMissingDependency())
			{
				_Buffer.WriteAt(40, 21, "{{y|-}} {{R|You have mods with missing dependencies.}}");
			}
			else if (ModManager.IsScriptingUndetermined())
			{
				_Buffer.WriteAt(40, 21, "{{y|-}} {{R|You have unapproved scripting mods.}}");
			}
			_Buffer.WriteAt(32, 0, "  {{C|Caves of Qud}}  ");
			_Buffer.WriteAt(55, 0, s);
			_Buffer.WriteAt(27, 24, " {{Y|Copyright ({{w|c}}) Freehold Games({{w|tm}})}} ");
			_Console.DrawBuffer(_Buffer);
			while (true)
			{
				Keys keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
				int num2 = 6;
				if (ModManager.Mods != null && ModManager.Mods.Count > 0)
				{
					num2 = 7;
				}
				if (keys == Keys.Enter)
				{
					keys = Keys.Space;
				}
				if (keys == Keys.NumPad8 && num > 0)
				{
					num--;
				}
				if (keys == Keys.NumPad2 && num < num2)
				{
					num++;
				}
				if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("Select:"))
				{
					num = Convert.ToInt32(Keyboard.CurrentMouseEvent.Event.Split(':')[1]);
				}
				if ((keys == Keys.Space || keys == Keys.Escape) && Options.ModernUI)
				{
					break;
				}
				if (keys == Keys.M || (keys == Keys.Space && num == 7) || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Pick:Installed Mod Configuration"))
				{
					UIManager.pushWindow("ModManager");
					SingletonWindowBase<ModManagerUI>.instance.nextHideCallback.AddListener(() =>
					{
						Keyboard.PushMouseEvent("Refresh");
					});
				}
				if (keys == Keys.R || (keys == Keys.Space && num == 6) || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Pick:Redeem Code"))
				{
					string text = Popup.AskString("Redeem a Code", "", "Sounds/UI/ui_notification", null, null, 32);
					if (!text.IsNullOrEmpty())
					{
						CodeRedemptionManager.redeem(text);
					}
				}
				if (keys != Keys.Q && (keys != Keys.Space || num != 5) && (keys != Keys.MouseEvent || !(Keyboard.CurrentMouseEvent.Event == "Quit")))
				{
					if (keys == Keys.N || (keys == Keys.Space && num == 0) || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Pick:New Game"))
					{
						NewGame();
					}
					XRLGame game;
					if ((keys == Keys.C || (keys == Keys.Space && num == 1) || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Pick:Continue")) && flag)
					{
						if (Options.ModernUI)
						{
							try
							{
								GameManager.Instance.PushGameView("ModernSaveManagement");
								SingletonWindowBase<MainMenu>.instance.DisableNavContext();
								SingletonWindowBase<Qud.UI.SaveManagement>.instance.ContinueMenu().Wait();
							}
							finally
							{
								GameManager.Instance.PopGameView();
								game = Game;
								if (game == null || !game.Running)
								{
									SingletonWindowBase<MainMenu>.instance.Reshow();
								}
							}
						}
						else
						{
							SaveManagement().Wait();
						}
						flag = SavesAPI.HasSavedGameInfo();
					}
					if (keys == Keys.O || (keys == Keys.Space && num == 2) || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Pick:Options"))
					{
						OptionsUI.Show();
					}
					if (keys == Keys.H || (keys == Keys.Space && num == 3) || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Pick:High Scores"))
					{
						if (Options.ModernUI)
						{
							try
							{
								GameManager.Instance.PushGameView("ModernHighScores");
								SingletonWindowBase<MainMenu>.instance.DisableNavContext();
								SingletonWindowBase<HighScoresScreen>.instance.ShowScreen().Wait();
								game = Game;
								if (game == null || !game.Running)
								{
									SingletonWindowBase<MainMenu>.instance.Reshow();
								}
							}
							finally
							{
								GameManager.Instance.PopGameView();
								game = Game;
								if (game == null || !game.Running)
								{
									SingletonWindowBase<MainMenu>.instance.Reshow();
								}
							}
						}
						else
						{
							Scores.Show();
						}
					}
					game = Game;
					if (game != null && game.Running)
					{
						bool transient = Game.Transient;
						string[] exitViewStack = Game.ExitViewStack;
						try
						{
							RunGame();
							flag = SavesAPI.HasSavedGameInfo();
							SoundManager.PlayMusic("Music/Pilgrims Path", "music", Crossfade: true, 2f);
							AmbientSoundsSystem.PlayAmbientBeds();
							if (transient && !exitViewStack.IsNullOrEmpty())
							{
								GameManager.Instance.SetGameViewStack(exitViewStack);
								Thread.Sleep(2000);
								ControlManager.ResetInput(disableLayers: false, LeaveMovementEvents: false);
								Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
								Keyboard.getvk(Options.MapDirectionsToKeypad, pumpActions: false, wait: false);
								continue;
							}
						}
						catch (Exception x)
						{
							MetricsManager.LogException("RunGame", x);
						}
					}
					else
					{
						if (keys == Keys.OemQuestion || (keys & Keys.OemQuestion) == Keys.OemQuestion || (keys == Keys.Space && num == 4) || keys == Keys.F1 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Pick:Help"))
						{
							Manual.ShowHelp("");
							MainMenu.EnsureReselect = true;
						}
						if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Pick:Credits")
						{
							Manual.ShowHelp("Credits");
							MainMenu.EnsureReselect = true;
						}
					}
					break;
				}
				Popup._TextConsole.Close();
				return;
			}
		}
	}

	public async Task<XRLGame> SaveManagement()
	{
		GameManager.Instance.PushGameView("LegacySaveManagement");
		bool bFirst = true;
		while (true)
		{
			int nSelected = 0;
			int nStart = 0;
			Task<List<SaveGameInfo>> savedGameInfo = SavesAPI.GetSavedGameInfo();
			Loading.Status saveGameInfosTaskStatus = new Loading.Status("Indexing saved games...");
			List<SaveGameInfo> list = await savedGameInfo;
			saveGameInfosTaskStatus.Dispose();
			if (list.Count == 0)
			{
				break;
			}
			bFirst = false;
			Keys keys;
			do
			{
				XRL.World.Event.ResetPool();
				_Buffer.Clear();
				_Buffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
				for (int i = nStart; i < nStart + 4 && i < list.Count; i++)
				{
					int num = (i - nStart) * 5 + 3;
					if (nSelected == i)
					{
						_Buffer.Goto(3, num);
						_Buffer.Write("{{Y|>}} {{W|" + list[i].Name + ", " + list[i].Description + "}}");
					}
					else
					{
						_Buffer.Goto(5, num);
						_Buffer.Write("{{w|" + list[i].Name + ", " + list[i].Description + "}}");
					}
					_Buffer.Goto(5, num + 1);
					_Buffer.Write(list[i].Info);
					_Buffer.Goto(5, num + 2);
					if (!list[i].SaveTime.IsNullOrEmpty())
					{
						_Buffer.Write(list[i].SaveTime);
					}
					else
					{
						_Buffer.Write("{{K|{" + list[i].ID + "} }}");
					}
					_Buffer.Goto(5, num + 3);
					_Buffer.Write("{{K|" + list[i].Size + " {" + list[i].ID + "} }}");
				}
				_Buffer.Goto(5, 24);
				_Buffer.Write(" [" + ControlManager.getCommandInputFormatted("Accept") + "]-Load Game [" + ControlManager.getCommandInputFormatted("CmdDelete") + "]-Delete Game ");
				_Console.DrawBuffer(_Buffer, null, bSkipIfOverlay: true);
				keys = Keyboard.getvk(Options.MapDirectionsToKeypad, pumpActions: true);
				switch (keys)
				{
				case Keys.Escape:
					GameManager.Instance.PopGameView();
					return null;
				case Keys.NumPad8:
					if (nSelected > 0)
					{
						nSelected--;
						if (nSelected < nStart)
						{
							nStart--;
						}
					}
					break;
				}
				if (keys == Keys.NumPad2 && nSelected < list.Count - 1)
				{
					nSelected++;
				}
				if (nSelected - nStart >= 4)
				{
					nStart++;
				}
				if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("Play:"))
				{
					list = await SavesAPI.GetSavedGameInfo();
					nSelected = Convert.ToInt32(Keyboard.CurrentMouseEvent.Event.Split(':')[1]);
					keys = Keys.Space;
				}
				if (list.Count == 0)
				{
					GameManager.Instance.PopGameView();
					return null;
				}
				if (nSelected >= list.Count)
				{
					nSelected = list.Count - 1;
				}
				if (keys != Keys.Space && keys != Keys.Enter)
				{
					continue;
				}
				try
				{
					SaveGameInfo saveGameInfo = list[nSelected];
					if (saveGameInfo.json.SaveVersion < 395)
					{
						Popup.Show("That save file looks like it's from an older save format revision (" + saveGameInfo.json.GameVersion + "). Sorry!\n\nYou can probably change to a previous branch in your game client and get it to load if you want to finish it off.");
					}
					else if (saveGameInfo.TryRestoreModsAndLoadAsync().Result)
					{
						GameManager.Instance.PopGameView();
						SingletonWindowBase<MainMenu>.instance.Reshow();
						return The.Game;
					}
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError(ex.ToString());
				}
			}
			while (keys != Keys.MouseEvent || !(Keyboard.CurrentMouseEvent.Event == "Command:CmdDelete") || Popup.ShowYesNoCancel("Are you sure you want to delete this saved game?") != DialogResult.Yes);
			list[nSelected].Delete();
		}
		if (bFirst)
		{
			Popup.ShowFail("You have no existing saved games.");
		}
		GameManager.Instance.PopGameView();
		return null;
	}

	public static bool WantEvent(int ID, int cascade)
	{
		return Core?.Game?.WantEvent(ID, cascade) == true;
	}

	public static bool HandleEvent<T>(T E) where T : MinEvent
	{
		return Core?.Game?.HandleEvent(E) == true;
	}

	public static bool HandleEvent<T>(T E, IEvent ParentEvent) where T : MinEvent
	{
		bool result = HandleEvent(E);
		ParentEvent.ProcessChildEvent(E);
		return result;
	}
}
