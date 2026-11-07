using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using ConsoleLib.Console;
using Genkit;
using Qud.UI;
using UnityEngine;
using XRL.Collections;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.AI.Pathfinding;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.Core;

[Serializable]
public class ActionManager
{
	[NonSerialized]
	public bool SkipPlayerTurn;

	[NonSerialized]
	public bool SkipSegment;

	[NonSerialized]
	public XRL.World.GameObject Actor;

	[NonSerialized]
	public RingDeque<XRL.World.GameObject> ActionQueue = new RingDeque<XRL.World.GameObject>(32);

	[NonSerialized]
	public List<XRL.World.GameObject> AbilityObjects = new List<XRL.World.GameObject>(32);

	[NonSerialized]
	public Rack<XRL.World.GameObject> LiveObjects = new Rack<XRL.World.GameObject>(32);

	[NonSerialized]
	public bool ProcessingTurnTick;

	[NonSerialized]
	public bool AllowCachedTurns;

	[NonSerialized]
	public Dictionary<string, int> ZoneSuspendFailures = new Dictionary<string, int>();

	[NonSerialized]
	public Rack<ActionCommandEntry> Commands = new Rack<ActionCommandEntry>();

	[NonSerialized]
	public int CommandChunk;

	public List<CommandQueueEntry> OldCommands;

	public bool RestingUntilHealed;

	public int RestingUntilHealedCount;

	public List<XRL.World.GameObject> RestingUntilHealedCompanions = new List<XRL.World.GameObject>();

	public bool RestingUntilHealedHaveCompanions;

	[NonSerialized]
	private InfluenceMap AutoexploreMap = new InfluenceMap(80, 25);

	[NonSerialized]
	private Cell AutoexploreCell;

	[NonSerialized]
	private Rack<XRL.World.GameObject> AutoexploreObjects = new Rack<XRL.World.GameObject>();

	[NonSerialized]
	private Color32[] minimapScratch;

	[NonSerialized]
	private static int SegCount = 0;

	[NonSerialized]
	private static Stopwatch AutomoveTimer = new Stopwatch();

	public void Save(SerializationWriter Writer)
	{
		Writer.WriteFields(this);
		Writer.WriteOptimized(ActionQueue.Count);
		foreach (XRL.World.GameObject item in ActionQueue)
		{
			Writer.WriteGameObject(item);
		}
		Writer.WriteOptimized(Commands.Count);
		foreach (ActionCommandEntry command in Commands)
		{
			command.Write(Writer);
		}
	}

	public static ActionManager Load(SerializationReader Reader)
	{
		ActionManager actionManager = Reader.ReadInstanceFields<ActionManager>();
		int num = Reader.ReadOptimizedInt32();
		bool flag = false;
		XRL.World.GameObject player = The.Player;
		actionManager.OldCommands = null;
		actionManager.ActionQueue.EnsureCapacity(num);
		for (int i = 0; i < num; i++)
		{
			XRL.World.GameObject gameObject = Reader.ReadGameObject("ActionQueue");
			if (gameObject == null)
			{
				if (!flag)
				{
					flag = true;
				}
			}
			else if (gameObject != player)
			{
				actionManager.ActionQueue.Enqueue(gameObject);
			}
		}
		if (!flag)
		{
			actionManager.ActionQueue.Enqueue(null);
		}
		if (player != null)
		{
			actionManager.ActionQueue.Push(player);
			Statistic energy = player.Energy;
			if (energy != null && energy.Value < 1000)
			{
				player.Energy.BaseValue = 1000;
			}
		}
		num = Reader.ReadOptimizedInt32();
		for (int j = 0; j < num; j++)
		{
			ActionCommandEntry item = default;
			item.Read(Reader);
			actionManager.Commands.Add(item);
		}
		return actionManager;
	}

	public void FinalizeRead()
	{
		int i = 0;
		for (int count = ActionQueue.Count; i < count; i++)
		{
			XRL.World.GameObject gameObject = ActionQueue[i];
			if (gameObject != null)
			{
				gameObject.ApplyActiveRegistrar();
				if (gameObject.Abilities != null)
				{
					AbilityObjects.Add(gameObject);
				}
			}
		}
	}

	public void Release()
	{
		ActionQueue.Clear();
		AbilityObjects.Clear();
		LiveObjects.Clear();
	}

	public bool HasActionDescendedFrom<T>() where T : IActionCommand
	{
		ActionCommandEntry[] array = Commands.GetArray();
		int i = 0;
		for (int count = Commands.Count; i < count; i++)
		{
			if (array[i].Command is T)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAction<T>() where T : IActionCommand
	{
		return HasAction(typeof(T));
	}

	public bool HasAction(Type Type)
	{
		ActionCommandEntry[] array = Commands.GetArray();
		int i = 0;
		for (int count = Commands.Count; i < count; i++)
		{
			if ((object)array[i].Command.GetType() == Type)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAction(Guid ID)
	{
		ActionCommandEntry[] array = Commands.GetArray();
		int i = 0;
		for (int count = Commands.Count; i < count; i++)
		{
			if (array[i].ID == ID)
			{
				return true;
			}
		}
		return false;
	}

	public void EnqueueAction(IActionCommand Command, int Delay = 0)
	{
		EnqueueAction(new ActionCommandEntry(Command, Delay));
	}

	public void EnqueueAction(Guid ID, IActionCommand Command, int Delay = 0)
	{
		EnqueueAction(new ActionCommandEntry(ID, Command, Delay));
	}

	public void EnqueueAction(ActionCommandEntry Entry)
	{
		int num = Commands.Count;
		int commandChunk = CommandChunk;
		ActionCommandEntry[] array = Commands.GetArray();
		int priority = Entry.Command.Priority;
		while (num > commandChunk && array[num - 1].Command.Priority < priority)
		{
			num--;
		}
		Commands.Insert(num, Entry);
	}

	public void DequeueAction(int Index)
	{
		Commands.RemoveAt(Index);
		if (Index < CommandChunk)
		{
			CommandChunk--;
		}
	}

	public int DequeueActionsDescendedFrom<T>() where T : IActionCommand
	{
		int num = 0;
		ActionCommandEntry[] array = Commands.GetArray();
		for (int num2 = Commands.Count - 1; num2 >= 0; num2--)
		{
			if (array[num2].Command is T)
			{
				DequeueAction(num2);
				num++;
			}
		}
		return num;
	}

	public int DequeueActions<T>() where T : IActionCommand
	{
		return DequeueActions(typeof(T));
	}

	public int DequeueActions(Type Type)
	{
		int num = 0;
		ActionCommandEntry[] array = Commands.GetArray();
		for (int num2 = Commands.Count - 1; num2 >= 0; num2--)
		{
			if ((object)array[num2].Command.GetType() == Type)
			{
				DequeueAction(num2);
				num++;
			}
		}
		return num;
	}

	public int DequeueActions(Guid ID)
	{
		int num = 0;
		ActionCommandEntry[] array = Commands.GetArray();
		for (int num2 = Commands.Count - 1; num2 >= 0; num2--)
		{
			if (array[num2].ID == ID)
			{
				DequeueAction(num2);
				num++;
			}
		}
		return num;
	}

	public void AddActiveObject(XRL.World.GameObject GO)
	{
		if (GO.IsInGraveyard())
		{
			MetricsManager.LogError("Attempting to add graveyard object '" + GO.DebugName + "' to action queue.");
			return;
		}
		if (!XRL.World.GameObject.Validate(GO))
		{
			MetricsManager.LogError("Attempting to add invalid object '" + GO.DebugName + "' to action queue.");
			return;
		}
		if (GO.Energy == null)
		{
			MetricsManager.LogError("Attempting to add object with no energy stat '" + GO.DebugName + "' to action queue.");
			return;
		}
		Zone currentZone = GO.CurrentZone;
		if ((AllowCachedTurns || currentZone == null || currentZone == The.ZoneManager.ActiveZone) && !ActionQueue.Contains(GO))
		{
			if (GO.Abilities != null)
			{
				AbilityObjects.Add(GO);
			}
			ActionQueue.Enqueue(GO);
			GO.ApplyActiveRegistrar();
			AfterAddActiveObjectEvent.Send(GO);
		}
	}

	public void RemoveExternalObjects()
	{
		Zone activeZone = The.ActiveZone;
		for (int num = ActionQueue.Count - 1; num >= 0; num--)
		{
			XRL.World.GameObject gameObject = ActionQueue[num];
			if (gameObject != null && gameObject.CurrentZone != activeZone)
			{
				RemoveActiveObject(num);
			}
		}
	}

	public void RemoveActiveObject(XRL.World.GameObject Object)
	{
		if (ActionQueue.Remove(Object))
		{
			AbilityObjects.Remove(Object);
			Object.ApplyActiveUnregistrar();
			AfterRemoveActiveObjectEvent.Send(Object);
		}
	}

	public void RemoveActiveObject(int Index)
	{
		XRL.World.GameObject gameObject = ActionQueue.RemoveAt(Index);
		AbilityObjects.Remove(gameObject);
		try
		{
			gameObject.ApplyActiveUnregistrar();
			AfterRemoveActiveObjectEvent.Send(gameObject);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Error after removing active object", x);
		}
	}

	public void UpdateMinimap()
	{
		if (minimapScratch == null)
		{
			minimapScratch = new Color32[4000];
		}
		if (!GameManager.Instance.DisplayMinimap)
		{
			return;
		}
		try
		{
			Zone zone = The.Player?.CurrentZone;
			if (zone == null)
			{
				return;
			}
			for (int i = 0; i < zone.Height; i++)
			{
				for (int j = 0; j < zone.Width; j++)
				{
					Cell cell = zone.GetCell(j, i);
					int num = j + (24 - i) * 2 * 80;
					int num2 = j + ((24 - i) * 2 + 1) * 80;
					cell.RefreshMinimapColor();
					minimapScratch[num].r = (minimapScratch[num2].r = cell.minimapCacheColor.r);
					minimapScratch[num].g = (minimapScratch[num2].g = cell.minimapCacheColor.g);
					minimapScratch[num].b = (minimapScratch[num2].b = cell.minimapCacheColor.b);
					minimapScratch[num].a = (minimapScratch[num2].a = cell.minimapCacheColor.a);
				}
			}
			lock (GameManager.minimapColors)
			{
				for (int k = 0; k < 4000; k++)
				{
					GameManager.minimapColors[k] = minimapScratch[k];
				}
			}
			GameManager.Instance.uiQueue.queueSingletonTask("updateMinimap", () =>
			{
				GameManager.Instance.UpdateMinimap();
			});
		}
		catch (Exception x)
		{
			MetricsManager.LogError("minimap update", x);
		}
	}

	public void AddLiveObject(XRL.World.GameObject Object)
	{
		LiveObjects.Add(Object);
	}

	public void TickAbilityCooldowns(int Segments = 1)
	{
		foreach (XRL.World.GameObject abilityObject in AbilityObjects)
		{
			abilityObject.Abilities.TickCooldowns(Segments);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool ShouldRemove(XRL.World.GameObject Object)
	{
		if (Object.Live && Object.IsValid() && Object.CurrentZone != null)
		{
			return Object.CurrentZone.Suspended;
		}
		return true;
	}

	public void ProcessSingleTurn(long TimeTick, int Amount)
	{
		ProcessingTurnTick = true;
		EndTurnEvent instance = SingletonEvent<EndTurnEvent>.Instance;
		foreach (var (_, zone2) in The.ZoneManager.CachedZones)
		{
			if (!zone2.Suspended && !zone2.Stale)
			{
				zone2.TurnTick(TimeTick, Amount);
				zone2.HandleEvent(instance);
			}
		}
		Rack<XRL.World.GameObject> liveObjects = LiveObjects;
		XRL.World.GameObject[] array = liveObjects.GetArray();
		liveObjects.ShuffleInPlace();
		for (int num = liveObjects.Count - 1; num >= 0; num--)
		{
			XRL.World.GameObject gameObject = array[num];
			if (ShouldRemove(gameObject))
			{
				liveObjects.RemoveAt(num);
				gameObject.Live = false;
			}
			else
			{
				gameObject.TurnTick(TimeTick, Amount);
				gameObject.HandleEvent(instance);
			}
		}
		ProcessingTurnTick = false;
	}

	public void ProcessTurnTick(long TimeTick, int Amount)
	{
		ProcessingTurnTick = true;
		foreach (var (_, zone2) in The.ZoneManager.CachedZones)
		{
			if (!zone2.Suspended && !zone2.Stale)
			{
				zone2.TurnTick(TimeTick, Amount);
			}
		}
		Rack<XRL.World.GameObject> liveObjects = LiveObjects;
		XRL.World.GameObject[] array = liveObjects.GetArray();
		liveObjects.ShuffleInPlace();
		for (int num = liveObjects.Count - 1; num >= 0; num--)
		{
			XRL.World.GameObject gameObject = array[num];
			if (ShouldRemove(gameObject))
			{
				liveObjects.RemoveAt(num);
				gameObject.Live = false;
			}
			else
			{
				gameObject.TurnTick(TimeTick, Amount);
			}
		}
		ProcessingTurnTick = false;
	}

	private XRL.World.GameObject FindAutoexploreObjectToOpen(XRL.World.GameObject Actor, Cell Cell, ref bool? Hostiles, Func<bool> CheckHostiles)
	{
		bool? flag = null;
		int i = 0;
		for (int count = Cell.Objects.Count; i < count; i++)
		{
			bool valueOrDefault = flag == true;
			bool flag2;
			if (!flag.HasValue)
			{
				valueOrDefault = Options.AutoexploreChests;
				flag = valueOrDefault;
				flag2 = valueOrDefault;
			}
			else
			{
				flag2 = valueOrDefault;
			}
			if (flag2 && Cell.Objects[i].ShouldAutoexploreAsChest())
			{
				valueOrDefault = Hostiles == true;
				bool flag3;
				if (!Hostiles.HasValue)
				{
					valueOrDefault = CheckHostiles();
					Hostiles = valueOrDefault;
					flag3 = valueOrDefault;
				}
				else
				{
					flag3 = valueOrDefault;
				}
				if (!flag3)
				{
					return Cell.Objects[i];
				}
			}
			if (flag == false || Hostiles == true)
			{
				return null;
			}
		}
		List<Cell> localAdjacentCells = Cell.GetLocalAdjacentCells();
		int j = 0;
		for (int count2 = localAdjacentCells.Count; j < count2; j++)
		{
			Cell cell = localAdjacentCells[j];
			bool flag4 = cell.IsSolidFor(Actor);
			int k = 0;
			for (int count3 = cell.Objects.Count; k < count3; k++)
			{
				XRL.World.GameObject gameObject = cell.Objects[k];
				if (!flag4 || gameObject.CanInteractInCellWithSolid(Actor))
				{
					bool valueOrDefault = flag == true;
					bool flag5;
					if (!flag.HasValue)
					{
						valueOrDefault = Options.AutoexploreChests;
						flag = valueOrDefault;
						flag5 = valueOrDefault;
					}
					else
					{
						flag5 = valueOrDefault;
					}
					if (flag5 && gameObject.ShouldAutoexploreAsChest())
					{
						valueOrDefault = Hostiles == true;
						bool flag6;
						if (!Hostiles.HasValue)
						{
							valueOrDefault = CheckHostiles();
							Hostiles = valueOrDefault;
							flag6 = valueOrDefault;
						}
						else
						{
							flag6 = valueOrDefault;
						}
						if (!flag6)
						{
							return gameObject;
						}
					}
				}
				if (flag == false || Hostiles == true)
				{
					return null;
				}
			}
		}
		return null;
	}

	private XRL.World.GameObject FindAutoexploreObjectToProcess(XRL.World.GameObject Actor, Cell Cell, out string Command, out bool AllowRetry, ref bool? Hostiles, Func<bool> Check, bool AutogetOnly = false)
	{
		Command = null;
		AllowRetry = false;
		bool flag = Cell.IsSolidFor(Actor);
		int i = 0;
		for (int count = Cell.Objects.Count; i < count; i++)
		{
			XRL.World.GameObject gameObject = Cell.Objects[i];
			if ((!flag || gameObject.CanInteractInCellWithSolid(Actor)) && AutoexploreObjectEvent.CheckForAdjacent(out Command, out AllowRetry, Actor, gameObject, AutoAct.Setting, AutoAct.Action, AutogetOnly))
			{
				bool valueOrDefault = Hostiles == true;
				bool flag2;
				if (!Hostiles.HasValue)
				{
					valueOrDefault = Check();
					Hostiles = valueOrDefault;
					flag2 = valueOrDefault;
				}
				else
				{
					flag2 = valueOrDefault;
				}
				if (!flag2)
				{
					return gameObject;
				}
				return null;
			}
		}
		if (AutogetOnly && !Options.AutogetFromNearby)
		{
			return null;
		}
		List<Cell> localAdjacentCells = Cell.GetLocalAdjacentCells();
		int j = 0;
		for (int count2 = localAdjacentCells.Count; j < count2; j++)
		{
			Cell cell = localAdjacentCells[j];
			flag = cell.IsSolidFor(Actor);
			int k = 0;
			for (int count3 = cell.Objects.Count; k < count3; k++)
			{
				XRL.World.GameObject gameObject2 = cell.Objects[k];
				if ((!flag || gameObject2.CanInteractInCellWithSolid(Actor)) && AutoexploreObjectEvent.CheckForAdjacent(out Command, out AllowRetry, Actor, gameObject2, AutoAct.Setting, AutoAct.Action, AutogetOnly))
				{
					bool valueOrDefault = Hostiles == true;
					bool flag3;
					if (!Hostiles.HasValue)
					{
						valueOrDefault = Check();
						Hostiles = valueOrDefault;
						flag3 = valueOrDefault;
					}
					else
					{
						flag3 = valueOrDefault;
					}
					if (!flag3)
					{
						return gameObject2;
					}
					return null;
				}
			}
		}
		return null;
	}

	public bool ValidateActor(XRL.World.GameObject Actor)
	{
		Cell currentCell = Actor.CurrentCell;
		if (!Actor.IsValid())
		{
			MetricsManager.LogError("ActionQueueInconsistency", Actor.IsPlayer() ? "Removing invalid player object" : "Removing invalid object");
		}
		else if (currentCell == null)
		{
			if (Actor.IsPlayer())
			{
				MetricsManager.LogError("ActionQueueInconsistency", "Player object has no current cell, returning to active spawn cell");
				currentCell = The.ZoneManager?.ActiveZone?.GetSpawnCell();
				if (currentCell != null)
				{
					currentCell.AddObject(Actor);
					return true;
				}
			}
			MetricsManager.LogError("ActionQueueInconsistency", "Removing object with no current cell " + Actor.DebugName);
		}
		else if (currentCell.ParentZone == null)
		{
			MetricsManager.LogError("ActionQueueInconsistency", "Removing zoneless cell object " + Actor.DebugName);
		}
		else if (!The.ZoneManager.CachedZonesContains(currentCell.ParentZone.ZoneID))
		{
			MetricsManager.LogError("ActionQueueInconsistency", "Removing non-cached zone object " + Actor.DebugName);
		}
		else
		{
			if (Actor.Energy != null)
			{
				return true;
			}
			MetricsManager.LogError("ActionQueueInconsistency", "Removing object with no energy stat " + Actor.DebugName);
		}
		return false;
	}

	public void RunSegment()
	{
		try
		{
			XRLGame game = The.Game;
			GameManager.Instance.CurrentGameView = Options.StageViewID;
			XRL.World.Event.ResetPool();
			The.Core.AllowWorldMapParticles = false;
			XRL.World.GameObject gameObject = null;
			bool flag = false;
			int num = 0;
			XRL.World.GameObject LastDoor = null;
			if (The.Player != null && !ActionQueue.Contains(The.Player))
			{
				ActionQueue.Enqueue(The.Player);
			}
			if (ActionQueue.LastIndexOf(null) == -1)
			{
				ActionQueue.Enqueue(null);
			}
			while (ActionQueue.TryDequeue(out Actor))
			{
				ActionQueue.Enqueue(Actor);
				if (Actor == null)
				{
					break;
				}
				if (!game.Running)
				{
					return;
				}
				if (!ValidateActor(Actor))
				{
					RemoveActiveObject(Actor);
					continue;
				}
				Actor.Energy.BaseValue += Actor.Speed;
				if (Actor.Energy != null && Actor.Energy.Value >= 1000)
				{
					if (!EarlyBeforeBeginTakeActionEvent.Check(Actor) || !BeforeBeginTakeActionEvent.Check(Actor) || !BeginTakeActionEvent.Check(Actor))
					{
						Actor.Energy.BaseValue = 0;
					}
					if (Actor.IsPlayer() && Options.LogTurnSeparator && Actor.ArePerceptibleHostilesNearby())
					{
						game.Player.Messages.Add("[--turn start--]");
					}
				}
				Cell currentCell = Actor.CurrentCell;
				bool flag2 = false;
				int num2 = 0;
				while (Actor.Energy != null && Actor.Energy.Value >= 1000 && game.Running && !SkipSegment)
				{
					game.ActionTicks++;
					if (GameManager.runWholeTurnOnUIThread > 0 && Actor != null && Actor.IsPlayer())
					{
						Actor.Energy.BaseValue = 0;
						GameManager.runWholeTurnOnUIThread--;
						if (GameManager.runWholeTurnOnUIThread <= 0)
						{
							return;
						}
						continue;
					}
					XRL.World.Event.ResetPool();
					flag2 = true;
					num2++;
					try
					{
						if (Actor.IsPlayer())
						{
							UpdateMinimap();
						}
						Actor.CleanEffects();
						_ = Actor.Energy.Value;
						if (!BeforeTakeActionEvent.Check(Actor))
						{
							flag2 = true;
							if (Actor.Energy != null)
							{
								Actor.Energy.BaseValue = 0;
							}
						}
						else
						{
							if (!CommandTakeActionEvent.Check(Actor))
							{
								continue;
							}
							Actor.CleanEffects();
							if (Actor.IsPlayer() && AutoAct.IsInterruptable() && AutoAct.Setting[0] != 'M' && AutoAct.Setting[0] != 'U' && AutoAct.Setting[0] != 'P' && AutoAct.Setting[0] != '!')
							{
								AutoAct.CheckHostileInterrupt();
							}
							if (Actor?.Energy != null && Actor.Energy.Value >= 1000 && Actor.IsPlayer())
							{
								currentCell = Actor.CurrentCell;
								if (currentCell != null && currentCell.ParentZone != null && !currentCell.ParentZone.IsActive())
								{
									currentCell.ParentZone.SetActive();
								}
								if (AutoAct.IsActive())
								{
									Sidebar.UpdateState();
									XRLCore.CallBeginPlayerTurnCallbacks();
									The.Core.RenderBase(UpdateSidebar: false);
									if (AutoAct.Setting != "g" && Keyboard.kbhit())
									{
										AutoAct.Interrupt();
										Keyboard.getch();
									}
									else if (currentCell.X == 0 && (AutoAct.Setting == "W" || AutoAct.Setting == "NW" || AutoAct.Setting == "SW"))
									{
										AutoAct.Interrupt();
									}
									else if (currentCell.X == 79 && (AutoAct.Setting == "E" || AutoAct.Setting == "NE" || AutoAct.Setting == "SE"))
									{
										AutoAct.Interrupt();
									}
									else if (currentCell.Y == 0 && (AutoAct.Setting == "N" || AutoAct.Setting == "NW" || AutoAct.Setting == "NE"))
									{
										AutoAct.Interrupt();
									}
									else if (currentCell.Y == 24 && (AutoAct.Setting == "S" || AutoAct.Setting == "SW" || AutoAct.Setting == "SE"))
									{
										AutoAct.Interrupt();
									}
								}
								if (RestingUntilHealed && !AutoAct.IsActive())
								{
									SingletonWindowBase<LoadingStatusWindow>.instance.SetLoadingStatus(null);
									RestingUntilHealed = false;
									RestingUntilHealedCompanions.Clear();
									RestingUntilHealedHaveCompanions = false;
								}
								if (AutoAct.IsActive())
								{
									CombatJuice.startTurn();
									if (AutoAct.IsRateLimited())
									{
										AutomoveTimer.Reset();
										AutomoveTimer.Start();
									}
									string setting = AutoAct.Setting;
									char c = setting[0];
									XRL.World.GameObject Object;
									bool flag3;
									bool AllowRetry;
									List<XRL.World.GameObject> objects;
									int num5;
									XRL.World.GameObject gameObject4;
									switch (c)
									{
									case '!':
									case 'M':
									case 'P':
									case 'U':
									case 'X':
									{
										string text2 = setting.Substring(1);
										int num8 = 0;
										if (c == 'P')
										{
											int num9 = text2.IndexOf(':');
											if (num9 != -1)
											{
												num8 = Convert.ToInt32(text2.Substring(0, num9));
												text2 = text2.Substring(num9 + 1);
											}
										}
										XRL.World.GameObject gameObject8 = null;
										Cell cell = null;
										int x;
										int y;
										if (text2.Contains(","))
										{
											string[] array = text2.Split(',');
											x = Convert.ToInt32(array[0]);
											y = Convert.ToInt32(array[1]);
											cell = currentCell.ParentZone.GetCell(x, y);
										}
										else
										{
											gameObject8 = XRL.World.GameObject.FindByID(text2);
											cell = gameObject8?.CurrentCell;
											if (cell == null || cell.ParentZone != currentCell.ParentZone)
											{
												AutoAct.Interrupt();
												break;
											}
											x = cell.X;
											y = cell.Y;
										}
										if (c == '!' && cell.DistanceTo(currentCell) <= 1)
										{
											AutoAct.Interrupt();
											The.Core.AttemptSmartUse(cell, 100);
											break;
										}
										if (c == 'U' && cell.DistanceTo(currentCell) <= 1)
										{
											AutoAct.Interrupt();
											The.Core.AttemptSmartUse(cell);
											break;
										}
										if (currentCell == cell)
										{
											AutoAct.Interrupt();
											break;
										}
										if (c == 'P' && cell.DistanceTo(Actor) <= num8)
										{
											AutoAct.Interrupt();
											break;
										}
										if (currentCell.X == cell.X && currentCell.Y == cell.Y)
										{
											MetricsManager.LogError("Autoact " + setting + " had cells " + currentCell.ToString() + ", " + cell.ToString() + " with same coordinates but different objects");
											AutoAct.Interrupt();
											break;
										}
										bool flag7 = true;
										if (!cell.IsExplored() && c == 'M')
										{
											flag7 = false;
											AutoexploreMap.ClearSeeds();
											currentCell.ParentZone.SetMouseMoveAutoexploreSeeds(AutoexploreMap, cell, out var _, out var _);
											AutoexploreMap.UsingWeights();
											currentCell.ParentZone.SetInfluenceMapAutoexploreWeightsAndWalls(AutoexploreMap.Weights, AutoexploreMap.Walls);
											AutoexploreMap.RecalculateCostOnly();
											string lowestWeightedCostDirectionFrom2 = AutoexploreMap.GetLowestWeightedCostDirectionFrom(currentCell.Pos2D);
											if (lowestWeightedCostDirectionFrom2 == ".")
											{
												flag7 = true;
											}
											else
											{
												if (Keyboard.kbhit())
												{
													AutoAct.Interrupt();
													break;
												}
												AutoAct.TryToMove(Actor, currentCell, ref LastDoor, null, lowestWeightedCostDirectionFrom2);
											}
										}
										if (flag7)
										{
											if (AutoAct.TryFindPathStep(cell, out var Step3))
											{
												AutoAct.TryToMove(Actor, currentCell, ref LastDoor, null, Step3, AllowDigging: true, OpenDoors: true, Peaceful: true, c == 'M' || c == 'P' || c == 'S' || c == 'U' || c == '!');
											}
											else if (gameObject8 != null)
											{
												Popup.Show("You cannot find a path to " + gameObject8.t() + ".");
												AutoAct.Interrupt(null, null, gameObject8);
											}
											else
											{
												Popup.Show("You cannot find a path to your destination.");
												AutoAct.Interrupt(null, currentCell.ParentZone.GetCell(x, y));
											}
										}
										break;
									}
									case 'G':
									{
										char c2 = setting[1];
										if (AutoAct.TryFindEdgeStep(setting[1], out var Step))
										{
											if (Step == ".")
											{
												AutoAct.Interrupt();
											}
											else
											{
												AutoAct.TryToMove(Actor, currentCell, ref LastDoor, null, Step);
											}
										}
										else
										{
											Popup.Show("You cannot find a path toward the " + Directions.GetExpandedDirection(c2.ToString() ?? "") + ".");
											AutoAct.Interrupt();
										}
										break;
									}
									case 'd':
									{
										string[] array2 = setting.Substring(1).Split(',');
										int x2 = Convert.ToInt32(array2[0]);
										int y2 = Convert.ToInt32(array2[1]);
										int num10 = Convert.ToInt32(array2[2]);
										int num11 = Convert.ToInt32(array2[3]);
										if (currentCell.X == num10 && currentCell.Y == num11)
										{
											AutoAct.Interrupt();
											break;
										}
										List<Point> list = Zone.Line(x2, y2, num10, num11, ReadOnly: true);
										Point point = null;
										for (int num12 = list.Count - 2; num12 >= 0; num12--)
										{
											if (list[num12].X == currentCell.X && list[num12].Y == currentCell.Y)
											{
												point = list[num12 + 1];
												break;
											}
										}
										if (point == null)
										{
											AutoAct.Interrupt();
										}
										else
										{
											AutoAct.TryToMove(Actor, currentCell, ref LastDoor, currentCell.ParentZone.GetCell(point), null, AllowDigging: true, OpenDoors: false);
										}
										break;
									}
									case '?':
									{
										bool? Hostiles2 = null;
										AutoexploreMap.ClearSeeds();
										XRL.World.GameObject gameObject6 = FindAutoexploreObjectToProcess(Actor, currentCell, out var Command2, out var AllowRetry2, ref Hostiles2, AutoAct.CheckHostileInterrupt);
										if (gameObject6 != null)
										{
											if (gameObject6 == gameObject && (!AllowRetry2 || num >= 10))
											{
												MetricsManager.LogError("setting autoexplore suppression on " + gameObject6.DebugName + " from autoexplore, count " + num + ", allow retry " + flag);
												AutoAct.SetAutoexploreSuppression(gameObject6, Flag: true);
												AutoAct.Interrupt();
											}
											else
											{
												if (gameObject == gameObject6 && flag == AllowRetry2)
												{
													num++;
												}
												else
												{
													gameObject = gameObject6;
													flag = AllowRetry2;
													num = 1;
												}
												AutoexploreObject(Actor, gameObject6, Command2, AllowRetry2);
											}
										}
										else
										{
											if (Hostiles2 == true)
											{
												break;
											}
											XRL.World.GameObject gameObject7 = FindAutoexploreObjectToOpen(Actor, currentCell, ref Hostiles2, AutoAct.CheckHostileInterrupt);
											if (gameObject7 != null)
											{
												gameObject7.SetIntProperty("Autoexplored", 1);
												gameObject7.FireEvent(XRL.World.Event.New("Open", "Opener", Actor));
											}
											else
											{
												if (Hostiles2 == true)
												{
													break;
												}
												string Step2 = ".";
												bool Blackout = false;
												if (Options.GetOption("OptionAutoexploreMode") == "Standard")
												{
													FasterDMapAutoexplore.FindAutoexploreStep(out Step2, out Blackout);
												}
												else
												{
													AutoAct.FindAutoexploreStep(out Step2, out Blackout);
												}
												if (Step2 == ".")
												{
													if (!currentCell.ParentZone.AnyUnexplored())
													{
														Popup.Show("There doesn't seem to be anywhere else to explore.");
													}
													else if (Blackout)
													{
														Popup.Show("There is only darkness from an unusual source left to explore.");
													}
													else
													{
														Popup.Show("There doesn't seem to be anywhere else to explore from here.");
													}
													AutoAct.Interrupt();
												}
												else if (Hostiles2.HasValue || !AutoAct.CheckHostileInterrupt())
												{
													AutoAct.TryToMove(Actor, currentCell, ref LastDoor, null, Step2);
												}
											}
										}
										break;
									}
									case '<':
									case '>':
									{
										AutoexploreMap.ClearSeeds();
										int num7 = 0;
										string text = "stairways";
										if (c == '<')
										{
											if (currentCell.HasObjectWithPart("StairsUp"))
											{
												AutoAct.Interrupt();
												break;
											}
											num7 = currentCell.ParentZone.SetInfluenceMapStairsUp(AutoexploreMap);
											text = "stairways leading upward";
										}
										else if (c == '>')
										{
											if (currentCell.HasObjectWithPart("StairsDown") && !currentCell.HasObject("Pit") && !currentCell.HasObject("LazyPit"))
											{
												AutoAct.Interrupt();
												break;
											}
											num7 = currentCell.ParentZone.SetInfluenceMapStairsDown(AutoexploreMap);
											text = "stairways leading downward";
										}
										if (num7 == 0)
										{
											Popup.ShowFail("There are no " + text + " nearby.");
											AutoAct.Interrupt();
											break;
										}
										AutoexploreMap.UsingWeights();
										currentCell.ParentZone.SetInfluenceMapAutoexploreWeightsAndWalls(AutoexploreMap.Weights, AutoexploreMap.Walls);
										AutoexploreMap.RecalculateCostOnly();
										string lowestWeightedCostDirectionFrom = AutoexploreMap.GetLowestWeightedCostDirectionFrom(currentCell.Pos2D);
										if (lowestWeightedCostDirectionFrom == ".")
										{
											MessageQueue.AddPlayerMessage("You can't figure out how to safely reach the stairs from here.");
											AutoAct.Interrupt();
										}
										else if (!AutoAct.CheckHostileInterrupt())
										{
											if (Keyboard.kbhit())
											{
												AutoAct.Interrupt();
											}
											else
											{
												AutoAct.TryToMove(Actor, currentCell, ref LastDoor, null, lowestWeightedCostDirectionFrom);
											}
										}
										break;
									}
									case 'g':
									{
										Object = null;
										flag3 = false;
										bool? Hostiles = null;
										AllowRetry = false;
										if (setting.Length == 1)
										{
											flag3 = true;
											XRL.World.GameObject gameObject2 = FindAutoexploreObjectToProcess(Actor, currentCell, out var Command, out AllowRetry, ref Hostiles, AutoAct.CheckHostileInterrupt, AutogetOnly: true);
											if (gameObject2 != null)
											{
												AutoexploreObject(Actor, gameObject2, Command, AllowRetry);
												break;
											}
											if (Hostiles == true)
											{
												break;
											}
										}
										else if (setting[1] == 'o')
										{
											XRL.World.GameObject gameObject3 = XRL.World.GameObject.FindByID(setting.Substring(2));
											if (gameObject3 != null && gameObject3.InSameOrAdjacentCellTo(Actor))
											{
												Inventory inventory = gameObject3.Inventory;
												Cell currentCell2 = gameObject3.GetCurrentCell();
												if (inventory != null && currentCell2 != null && (!currentCell2.IsSolidFor(Actor) || gameObject3.CanInteractInCellWithSolid(Actor)))
												{
													objects = inventory.GetObjects();
													num5 = 0;
													int count = objects.Count;
													while (num5 < count)
													{
														if (!objects[num5].ShouldTakeAll())
														{
															num5++;
															continue;
														}
														goto IL_0f92;
													}
												}
											}
										}
										else if (setting[1] == 'd')
										{
											Cell cellFromDirection2 = currentCell.GetCellFromDirection(setting.Substring(2));
											if (cellFromDirection2 != null)
											{
												bool flag4 = cellFromDirection2.IsSolidFor(Actor);
												int num6 = 0;
												int count2 = cellFromDirection2.Objects.Count;
												while (num6 < count2)
												{
													gameObject4 = cellFromDirection2.Objects[num6];
													if ((flag4 && !gameObject4.CanInteractInCellWithSolid(Actor)) || !gameObject4.ShouldTakeAll())
													{
														num6++;
														continue;
													}
													goto IL_1035;
												}
											}
										}
										goto IL_1055;
									}
									case 'o':
										if (AutoAct.Action == null)
										{
											MetricsManager.LogError("had autoact o but no ongoing action");
											AutoAct.Interrupt();
										}
										else if (AutoAct.Action.CanComplete())
										{
											AutoAct.Action.Complete();
											AutoAct.Action.End();
											AutoAct.Resume();
										}
										else if (AutoAct.Action.Continue())
										{
											if (AutoAct.Action.CanComplete())
											{
												AutoAct.Action.Complete();
												AutoAct.Action.End();
												AutoAct.Resume();
											}
										}
										else
										{
											AutoAct.Interrupt();
										}
										break;
									case 'z':
										if (Calendar.IsDay())
										{
											AutoAct.Interrupt();
											break;
										}
										Actor.PassTurn();
										if (++The.ActionManager.RestingUntilHealedCount % 10 == 0)
										{
											XRLCore.TenPlayerTurnsPassed();
											The.Core.RenderBase(UpdateSidebar: false, DuringRestOkay: true);
										}
										break;
									case 'r':
									{
										The.ActionManager.RestingUntilHealedCount++;
										bool flag5 = setting.Length > 1 && setting[1] == '+';
										if (flag5)
										{
											Loading.SetLoadingStatus("Resting until party healed... Turn: " + The.ActionManager.RestingUntilHealedCount);
										}
										else
										{
											Loading.SetLoadingStatus("Resting until healed... Turn: " + The.ActionManager.RestingUntilHealedCount);
										}
										Actor.PassTurn();
										bool flag6 = true;
										if (Actor.isDamaged() && Actor.HealsNaturally())
										{
											flag6 = false;
										}
										else if (flag5)
										{
											if (RestingUntilHealedHaveCompanions)
											{
												foreach (XRL.World.GameObject restingUntilHealedCompanion in RestingUntilHealedCompanions)
												{
													if (!XRL.World.GameObject.Validate(restingUntilHealedCompanion) || restingUntilHealedCompanion.IsNowhere() || !restingUntilHealedCompanion.IsLedBy(Actor))
													{
														RestingUntilHealedHaveCompanions = false;
														RestingUntilHealedCompanions.Clear();
														break;
													}
												}
											}
											if (!RestingUntilHealedHaveCompanions)
											{
												List<XRL.World.GameObject> companionsReadonly = Actor.GetCompanionsReadonly(Actor.CurrentZone.Width * 2);
												if (!companionsReadonly.IsNullOrEmpty())
												{
													RestingUntilHealedCompanions.AddRange(companionsReadonly);
												}
												RestingUntilHealedHaveCompanions = true;
											}
											int i = 0;
											for (int count3 = RestingUntilHealedCompanions.Count; i < count3; i++)
											{
												XRL.World.GameObject gameObject5 = RestingUntilHealedCompanions[i];
												if (gameObject5.isDamaged() && gameObject5.HealsNaturally())
												{
													flag6 = false;
													break;
												}
											}
										}
										if (flag6)
										{
											AutoAct.Interrupt();
										}
										else if (The.ActionManager.RestingUntilHealedCount % 10 == 0)
										{
											XRLCore.TenPlayerTurnsPassed();
											The.Core.RenderBase(UpdateSidebar: false, DuringRestOkay: true);
										}
										break;
									}
									case 'a':
									{
										XRL.World.GameObject target = Actor.Target;
										if (target == null || (The.Player.GetConfusion() > 0 && The.Player.GetFuriousConfusion() <= 0))
										{
											AutoAct.Interrupt();
											break;
										}
										if (target.Brain != null && !target.IsHostileTowards(Actor))
										{
											MessageQueue.AddPlayerMessage("You will not auto-attack " + target.t() + " because " + target.itis + " not hostile to you.");
											AutoAct.Interrupt();
											break;
										}
										if (The.Player.GetConfusion() > 0 && The.Player.GetFuriousConfusion() > 0)
										{
											try
											{
												AutoAct.Attacking = true;
												Actor.FireEvent(XRL.World.Event.New("CmdMove" + Directions.GetRandomDirection()));
											}
											finally
											{
												AutoAct.Attacking = false;
											}
											break;
										}
										if (!target.IsVisible())
										{
											MessageQueue.AddPlayerMessage("You cannot see your target.");
											AutoAct.Interrupt();
											break;
										}
										Cell currentCell3 = target.CurrentCell;
										switch (Actor.DistanceTo(target))
										{
										case 0:
										{
											Cell randomElement = currentCell.GetLocalNavigableAdjacentCells(Actor).GetRandomElement();
											if (randomElement == null)
											{
												MessageQueue.AddPlayerMessage("You can't find a way to navigate to " + target.t() + ".");
												AutoAct.Interrupt(null, null, target);
												break;
											}
											string directionFromCell2 = currentCell.GetDirectionFromCell(randomElement);
											if (directionFromCell2.IsNullOrEmpty() || directionFromCell2 == ".")
											{
												AutoAct.Interrupt();
											}
											else if (!Actor.Move(directionFromCell2, Forced: false, System: false, IgnoreGravity: false, NoStack: false, AllowDashing: true, DoConfirmations: true, null, null, NearestAvailable: false, null, null, null, Peaceful: true) || Actor.CurrentCell != randomElement)
											{
												AutoAct.Interrupt(null, randomElement);
											}
											break;
										}
										case 1:
										{
											if (currentCell3.GetCombatTarget(Actor) != target)
											{
												MessageQueue.AddPlayerMessage("You are unable to attack " + target.t() + ".");
												AutoAct.Interrupt(null, null, target, IsThreat: true);
												break;
											}
											string directionFromCell = currentCell.GetDirectionFromCell(currentCell3);
											if (directionFromCell.IsNullOrEmpty() || directionFromCell == ".")
											{
												AutoAct.Interrupt();
												break;
											}
											try
											{
												AutoAct.Attacking = true;
												if (!Actor.AttackDirection(directionFromCell))
												{
													AutoAct.Interrupt(null, currentCell3, null, IsThreat: true);
												}
												else if (target.IsInvalid() || target != Actor.Target)
												{
													AutoAct.Interrupt();
												}
											}
											finally
											{
												AutoAct.Attacking = false;
											}
											break;
										}
										default:
										{
											FindPath findPath = new FindPath(currentCell, currentCell3, PathGlobal: false, PathUnlimited: true, Actor, 20);
											if (!findPath.Usable)
											{
												MessageQueue.AddPlayerMessage("You can't seem to find a way to reach " + target.t() + ".");
												AutoAct.Interrupt(null, null, target);
											}
											else
											{
												AutoAct.TryToMove(Actor, currentCell, findPath.Steps[1], findPath.Directions[0]);
											}
											break;
										}
										}
										break;
									}
									default:
										{
											if (setting.Contains("."))
											{
												int num3 = Convert.ToInt32(setting.Split('.')[1]) - 1;
												if (num3 > 0)
												{
													Actor.PassTurn();
													Loading.SetLoadingStatus(The.StringBuilder.Append("Waiting for ").Append(num3.Things("turn")).Append("...")
														.ToString());
													AutoAct.Setting = "." + num3;
													if (num3 % 10 == 0)
													{
														XRLCore.TenPlayerTurnsPassed();
														The.Core.RenderBase(UpdateSidebar: false, DuringRestOkay: true);
													}
												}
												else
												{
													Loading.SetLoadingStatus("Done waiting");
													AutoAct.Interrupt();
												}
											}
											else if (setting.StartsWith(":"))
											{
												if (long.TryParse(setting.AsSpan(1), out var result) && game.Turns < result)
												{
													Actor.PassTurn();
													int num4 = (int)(result - game.Turns);
													Loading.SetLoadingStatus(The.StringBuilder.Append("Waiting for ").Append(num4.Things("round")).Append("...")
														.ToString());
													if (num4 % 10 == 0)
													{
														The.Core.RenderBase(UpdateSidebar: false, DuringRestOkay: true);
													}
												}
												else
												{
													Loading.SetLoadingStatus("Done waiting");
													AutoAct.Interrupt();
												}
											}
											else
											{
												if (AutoAct.CheckHostileInterrupt())
												{
													break;
												}
												Cell cellFromDirection = currentCell.GetCellFromDirection(setting);
												if (InterruptAutowalkEvent.Check(Actor, cellFromDirection, out var Because, out var IndicateObject, out var IndicateCell, out var AsThreat))
												{
													if (Because == null && XRL.World.GameObject.Validate(ref IndicateObject))
													{
														Because = "of " + IndicateObject.t() + " " + Actor.DescribeDirectionToward(IndicateObject);
													}
													string because = Because;
													XRL.World.GameObject indicateObject = IndicateObject;
													AutoAct.Interrupt(because, IndicateCell, indicateObject, AsThreat);
												}
												else
												{
													AutoAct.TryToMove(Actor, currentCell, ref LastDoor, null, setting, AllowDigging: false);
												}
											}
											break;
										}
										IL_1035:
										if (AutoAct.CheckHostileInterrupt(logSpot: true))
										{
											break;
										}
										Object = gameObject4;
										AllowRetry = true;
										goto IL_1055;
										IL_1055:
										if (Object == null)
										{
											AutoAct.Resume();
											break;
										}
										if (Object == gameObject && (!flag || num >= 10))
										{
											MetricsManager.LogError("setting autoexplore suppression on " + Object.DebugName + " from autoget, count " + num + ", allow retry " + flag);
											AutoAct.SetAutoexploreSuppression(Object, Flag: true);
											AutoAct.Interrupt();
											break;
										}
										if (gameObject == Object && flag == AllowRetry)
										{
											num++;
										}
										else
										{
											gameObject = Object;
											flag = AllowRetry;
											num = 1;
										}
										if (!flag3 || Object.CanAutoget())
										{
											if (Actor.TakeObject(Object, NoStack: false, Silent: false, null, "Autoget"))
											{
												if (flag3 && XRL.World.GameObject.Validate(ref Object))
												{
													Sidebar.AddAutogotItem(Object);
													Sidebar.Update();
												}
											}
											else
											{
												AutoAct.Interrupt();
											}
										}
										else
										{
											MetricsManager.LogError("invalid object for autoget, " + Object.DebugName);
											AutoAct.Interrupt();
										}
										break;
										IL_0f92:
										if (AutoAct.CheckHostileInterrupt(logSpot: true))
										{
											break;
										}
										Object = objects[num5];
										AllowRetry = true;
										goto IL_1055;
									}
									if (!AutomoveTimer.IsRunning)
									{
										continue;
									}
									AutomoveTimer.Stop();
									if (!AutoAct.IsRateLimited())
									{
										continue;
									}
									int autoexploreRate = Options.AutoexploreRate;
									if (autoexploreRate != 0)
									{
										int num13 = 1000 / autoexploreRate;
										long elapsedMilliseconds = AutomoveTimer.ElapsedMilliseconds;
										if (elapsedMilliseconds < num13)
										{
											Thread.Sleep((int)(num13 - elapsedMilliseconds));
										}
									}
									continue;
								}
								if (Actor.Brain.Goals.Count > 0)
								{
									Actor.Brain.FireEvent(XRL.World.Event.New("CommandTakeAction"));
									The.Core.RenderBase();
								}
								if (SkipPlayerTurn)
								{
									SkipPlayerTurn = false;
									continue;
								}
								if (GameManager.runWholeTurnOnUIThread > 0 && Thread.CurrentThread != XRLCore.CoreThread)
								{
									The.Player.Energy.BaseValue = 0;
									return;
								}
								if (GameManager.runPlayerTurnOnUIThread && Thread.CurrentThread == XRLCore.CoreThread)
								{
									while (!Keyboard.kbhit())
									{
									}
									bool turnWaitFlag = true;
									GameManager.Instance.uiQueue.queueTask(() =>
									{
										The.Core.PlayerTurn();
										turnWaitFlag = false;
									}, 1);
									while (turnWaitFlag)
									{
									}
								}
								else if (Thread.CurrentThread == XRLCore.CoreThread)
								{
									The.Core.PlayerTurn();
									if (!game.Running)
									{
										return;
									}
								}
							}
							else if (Actor.IsPlayer())
							{
								The.Core.RenderBase();
							}
							else if (num2 > 5 && Actor?.Energy != null)
							{
								Actor.Energy.BaseValue -= 1000;
							}
							continue;
						}
					}
					catch (Exception x3)
					{
						MetricsManager.LogException("RunSegment", x3);
						if (Actor?.Energy != null && !Actor.IsPlayer())
						{
							Actor.Energy.BaseValue = 0;
						}
					}
				}
				if (flag2)
				{
					EndActionEvent.Send(Actor);
				}
				if (!SkipSegment)
				{
					continue;
				}
				SkipSegment = false;
				break;
			}
			SegCount++;
			game.Segments++;
			TickAbilityCooldowns();
			The.ZoneManager.CheckEventQueue();
			if (SegCount >= 10)
			{
				if (game.HasRemovedSystems)
				{
					game.RemoveFlaggedSystems();
				}
				EndTurnEvent.Send(game);
				ProcessSingleTurn(game.TimeTicks, 1);
				SegCount = 0;
				The.ZoneManager.CheckEventQueue();
				The.ZoneManager.Tick(AllowFreeze: false);
				game.Turns++;
				game.TimeTicks++;
				Zone zone = game.Player.Body?.CurrentZone;
				if (zone != null)
				{
					zone.LastPlayerPresence = game.TimeTicks;
				}
			}
			RunCommands();
		}
		catch (Exception x4)
		{
			MetricsManager.LogException("RunSegment (final)", x4);
		}
	}

	public void RunCommands(bool Decrement = true)
	{
		CommandChunk = Commands.Count;
		try
		{
			XRLGame game = The.Game;
			for (int i = 0; i < CommandChunk; i++)
			{
				if (!game.Running)
				{
					break;
				}
				ActionCommandEntry reference = Commands.GetReference(i);
				if (Decrement)
				{
					reference.SegmentDelay--;
				}
				if (reference.SegmentDelay <= 0)
				{
					IActionCommand command = reference.Command;
					DequeueAction(i--);
					try
					{
						command.Execute(game, this);
					}
					catch (Exception ex)
					{
						MetricsManager.LogAssemblyError(command.GetType(), "Exception executing command action\n" + ex);
					}
				}
			}
		}
		finally
		{
			CommandChunk = 0;
		}
	}

	public void ProcessIndependentEndTurn(XRL.World.GameObject obj)
	{
		EndTurnEvent.Send(obj);
		obj.CleanEffects();
	}

	public bool IsPlayerTurn()
	{
		if (Actor != null)
		{
			return XRLCore.Core.Game?.Player._Body == Actor;
		}
		return false;
	}

	private bool AutoexploreObject(XRL.World.GameObject Actor, XRL.World.GameObject Item, string Command, bool AllowRetry)
	{
		if (!AllowRetry)
		{
			AutoAct.SetAutoexploreActionProperty(Item, Command, 1);
		}
		try
		{
			AutoAct.Attacking = Command == "Attack";
			bool flag = InventoryActionEvent.Check(out InventoryActionEvent E, Item, Actor, Item, Command, true, false, false, false, false, 0, 0, 0, (XRL.World.GameObject)null, (Cell)null, (Cell)null, (IInventory)null);
			if (E != null)
			{
				foreach (XRL.World.GameObject item in E.Generated)
				{
					if (XRL.World.GameObject.Validate(item))
					{
						Sidebar.AddAutogotItem(item);
						Sidebar.Update();
					}
				}
			}
			if (!flag & AllowRetry)
			{
				MetricsManager.LogError("setting autoexplore suppression on " + Item.DebugName + " from failed command " + Command + ", allow retry " + AllowRetry);
				AutoAct.SetAutoexploreSuppression(Item, Flag: true);
			}
			return flag;
		}
		finally
		{
			AutoAct.Attacking = false;
		}
	}
}
