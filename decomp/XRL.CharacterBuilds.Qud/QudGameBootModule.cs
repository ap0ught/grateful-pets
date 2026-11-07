using System;
using System.Collections.Generic;
using System.Reflection;
using ConsoleLib.Console;
using Platform.Performance;
using Qud.UI;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace XRL.CharacterBuilds.Qud;

public class QudGameBootModule : AbstractEmbarkBuilderModule
{
	/// <summary>
	/// fires before anything else happens during game bootup
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_BEGINBOOT = "BeginBoot";

	/// <summary>
	/// world progress UI is now setup and the game has an ID
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_AFTERBEGINBOOT = "AfterBeginBoot";

	/// <summary>
	/// caches will be reset
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_CACHERESET = "CacheReset";

	/// <summary>
	/// caches are now reset
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_AFTERCACHERESET = "AfterCacheReset";

	/// <summary>
	/// game seeds will be setup
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_GENERATESEEDS = "GenerateSeeds";

	/// <summary>
	/// game seeds are now setup
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_AFTERGENERATESEEDS = "AfterGenerateSeeds";

	/// <summary>
	/// typically used to use game.AddSystem(...) your systems
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_INITIALIZESYSTEMS = "InitializeSystems";

	/// <summary>
	/// typically used to use game.AddSystem(...) your systems
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_AFTERINITIALIZESYSTEMS = "AfterInitializeSystems";

	/// <summary>
	/// GamestateSingletons are about to be initialized. This will probably be handled rarely.
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_INITIALIZEGAMESTATESINGLETONS = "InitializeGamestateSingletons";

	/// <summary>
	/// GamestateSingletons have been initialized. This will probably be handled rarely.
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_AFTERINITIALIZEGAMESTATESINGLETONS = "AfterInitializeGamestateSingletons";

	/// <summary>
	/// worlds are about to be built
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_BEFOREINITIALIZEHISTORY = "BeforeInitializeHistory";

	/// <summary>
	/// worlds are about to be built
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_INITIALIZEHISTORY = "InitializeHistory";

	/// <summary>
	/// worlds have been built
	/// element is a HistoryKit.History
	/// </summary>
	public static readonly string BOOTEVENT_AFTERINITIALIZEHISTORY = "AfterInitializeHistory";

	/// <summary>
	/// worlds are about to be built
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_INITIALIZESULTANHISTORY = "InitializeSultanHistory";

	/// <summary>
	/// worlds have been built
	/// element is a HistoryKit.History
	/// </summary>
	public static readonly string BOOTEVENT_AFTERINITIALIZESULTANHISTORY = "AfterInitializeSultanHistory";

	/// <summary>
	/// worlds are about to be built
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_INITIALIZEWORLDS = "InitializeWorlds";

	/// <summary>
	/// worlds have been built
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_AFTERINITIALIZEWORLDS = "AfterInitializeWorlds";

	/// <summary>
	/// element is a GlobalLocation that will be the player's starting cell
	/// </summary>
	public static readonly string BOOTEVENT_BOOTSTARTINGLOCATION = "BootStartingLocation";

	/// <summary>
	/// generate the player's name if the player didn't choose one
	/// element is a string that will be the player's name
	/// </summary>
	public static readonly string BOOTEVENT_GENERATERANDOMPLAYERNAME = "GenerateRandomPlayerName";

	/// <summary>
	/// Generate the blueprint for the player's GameObject
	/// element is a string that will be the player's body
	/// </summary>
	public static readonly string BOOTEVENT_BOOTPLAYEROBJECTBLUEPRINT = "BootPlayerObjectBlueprint";

	/// <summary>
	/// create the player's GameObject
	/// element is a GameObject that will be the player's body
	/// </summary>
	public static readonly string BOOTEVENT_BEFOREBOOTPLAYEROBJECT = "BeforeBootPlayerObject";

	/// <summary>
	/// create the player's GameObject
	/// element is a GameObject that will be the player's body
	/// </summary>
	public static readonly string BOOTEVENT_BOOTPLAYEROBJECT = "BootPlayerObject";

	/// <summary>
	/// the player's GameObject has been created
	/// element is a GameObject that will be the player's body
	/// </summary>
	public static readonly string BOOTEVENT_AFTERBOOTPLAYEROBJECT = "AfterBootPlayerObject";

	/// <summary>
	/// Generate the player's tile.
	/// Element is a string path to the tile texture.
	/// </summary>
	public static readonly string BOOTEVENT_BOOTPLAYERTILE = "BootPlayerTile";

	/// <summary>
	/// Generate the player's foreground color.
	/// Element is a string color character.
	/// </summary>
	public static readonly string BOOTEVENT_BOOTPLAYERTILEFOREGROUND = "BootPlayerTileForeground";

	/// <summary>
	/// Generate the player's background color.
	/// Element is a string color character.
	/// </summary>
	public static readonly string BOOTEVENT_BOOTPLAYERTILEBACKGROUND = "BootPlayerTileBackground";

	/// <summary>
	/// Generate the player's detail color.
	/// Element is a string color character.
	/// </summary>
	public static readonly string BOOTEVENT_BOOTPLAYERTILEDETAIL = "BootPlayerTileDetail";

	/// <summary>
	/// Generate the player's reputation.
	/// Has no element.
	/// </summary>
	public static readonly string BOOTEVENT_BOOTPLAYERREPUTATION = "BootPlayerReputation";

	/// <summary>
	/// the game is just about to start this is the last event before it does
	/// has no element
	/// </summary>
	public static readonly string BOOTEVENT_GAMESTARTING = "GameStarting";

	public override void InitFromSeed(string seed)
	{
		builder.info.GameSeed = seed;
	}

	public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
	{
		if (id == BOOTEVENT_BOOTPLAYEROBJECT)
		{
			GameObject obj = element as GameObject;
			obj.Brain.Goals.Clear();
			obj.DisplayName = The.Game.PlayerName;
			obj.HasProperName = true;
			obj.RequirePart<Description>().Short = "It's you.";
			obj.AddPart(new OpeningStory());
			return obj;
		}
		return base.handleBootEvent(id, game, info, element);
	}

	public override void Init()
	{
		builder.info.GameSeed = Guid.NewGuid().ToString();
	}

	public static void SeedGame(XRLGame Game, EmbarkInfo Info)
	{
		if (!Game.IsSeeded)
		{
			Info.fireBootEvent(BOOTEVENT_GENERATESEEDS, Game);
			Game.SetStringGameState("OriginalWorldSeed", Info.GameSeed);
			Game.IsSeeded = true;
			Stat.ReseedFrom(Info.GameSeed, includeLifetimeSeeds: true);
			Game.RemoveIntGameState("WorldSeed");
			Game.GetWorldSeed();
			Info.fireBootEvent(BOOTEVENT_AFTERGENERATESEEDS, Game);
		}
	}

	public override void bootGame(XRLGame game, EmbarkInfo info)
	{
		using (CPUBoost.AutoScopeForceMainThread())
		{
			TutorialManager.OnBootGame(game, info);
			MetricsManager.LogInfo("Beginning world build for seed: " + info.GameSeed);
			if (Options.ModernUI)
			{
				_ = WorldGenerationScreen.ShowWorldGenerationScreen(258).Result;
			}
			XRLCore core = The.Core;
			info.fireBootEvent(BOOTEVENT_BEGINBOOT, game);
			GameManager.Instance.PushGameView("WorldCreationProgress");
			Loading.SetHideLoadStatus(hidden: true);
			WorldCreationProgress.Begin(7);
			WorldCreationProgress.NextStep("Initializing protocols...", 2);
			WorldCreationProgress.StepProgress("Hardening math...");
			game.GameID = Guid.NewGuid().ToString();
			info.fireBootEvent(BOOTEVENT_AFTERBEGINBOOT, game);
			info.fireBootEvent(BOOTEVENT_CACHERESET, game);
			WorldCreationProgress.StepProgress("Planting world seeds...");
			game.GetCacheDirectory();
			game.bZoned = false;
			The.Core.ResetGameBasedStaticCaches();
			info.fireBootEvent(BOOTEVENT_AFTERCACHERESET, game);
			SeedGame(game, info);
			GameObjectFactory.Factory.DispatchLoadBlueprints(Task: false);
			info.fireBootEvent(BOOTEVENT_INITIALIZESYSTEMS, game);
			info.fireBootEvent(BOOTEVENT_AFTERINITIALIZESYSTEMS, game);
			info.fireBootEvent(BOOTEVENT_INITIALIZEGAMESTATESINGLETONS, game);
			foreach (Type item in ModManager.GetTypesWithAttribute(typeof(GameStateSingleton), Cache: false))
			{
				string text = item.GetCustomAttribute<GameStateSingleton>().ID ?? item.Name;
				Stat.ReseedFrom("GAMESTATE" + text);
				object obj = Activator.CreateInstance(item);
				game.SetObjectGameState(text, obj);
				if (obj is IGameStateSingleton gameStateSingleton)
				{
					gameStateSingleton.Initialize();
					info.GameStateSingletons.Add(gameStateSingleton);
				}
			}
			info.fireBootEvent(BOOTEVENT_AFTERINITIALIZEGAMESTATESINGLETONS, game);
			Stat.ReseedFrom("HISTORYINIT");
			info.fireBootEvent(BOOTEVENT_BEFOREINITIALIZEHISTORY, game);
			info.fireBootEvent(BOOTEVENT_INITIALIZEHISTORY, game);
			info.fireBootEvent(BOOTEVENT_AFTERINITIALIZEHISTORY, game);
			info.fireBootEvent(BOOTEVENT_INITIALIZEWORLDS, game);
			WorldCreationProgress.NextStep("Generating topography...", WorldFactory.Factory.countWorlds() * 40);
			Stat.ReseedFrom("NAMEMAP");
			WorldFactory.Factory.BuildZoneNameMap();
			Stat.ReseedFrom("BUILDWORLDS");
			WorldFactory.Factory.BuildWorlds();
			info.fireBootEvent(BOOTEVENT_AFTERINITIALIZEWORLDS, game);
			WorldCreationProgress.NextStep("Resolving faction relationships...", 2);
			game.Running = true;
			WorldCreationProgress.StepProgress("Adding player to world...");
			game.PlayerReputation.Init();
			info.fireBootEvent(BOOTEVENT_BOOTPLAYERREPUTATION, game);
			GlobalLocation globalLocation = info.fireBootEvent(BOOTEVENT_BOOTSTARTINGLOCATION, game, new GlobalLocation());
			if (string.IsNullOrEmpty(game.PlayerName?.Trim()))
			{
				game.PlayerName = info.fireBootEvent(BOOTEVENT_GENERATERANDOMPLAYERNAME, game, core.GenerateRandomPlayerName(info.getModule<QudSubtypeModule>().data.Subtype));
			}
			GameObject element = info.fireBootEvent<GameObject>(BOOTEVENT_BEFOREBOOTPLAYEROBJECT, game, null);
			element = info.fireBootEvent(BOOTEVENT_BOOTPLAYEROBJECT, game, element);
			Render render = element.Render;
			string tile = info.fireBootEvent<string>(BOOTEVENT_BOOTPLAYERTILE, game, null) ?? render.Tile;
			string text2 = info.fireBootEvent<string>(BOOTEVENT_BOOTPLAYERTILEFOREGROUND, game, null) ?? render.GetForegroundColor();
			string text3 = info.fireBootEvent<string>(BOOTEVENT_BOOTPLAYERTILEBACKGROUND, game, null) ?? render.GetBackgroundColor();
			string detailColor = info.fireBootEvent<string>(BOOTEVENT_BOOTPLAYERTILEDETAIL, game, null) ?? render.DetailColor;
			render.Tile = tile;
			render.ColorString = "&" + text2 + "^" + text3;
			render.DetailColor = detailColor;
			foreach (Type item2 in ModManager.GetTypesWithAttribute(typeof(PlayerMutator)))
			{
				Stat.ReseedFrom("PLAYERMUTATOR" + item2.Name);
				(Activator.CreateInstance(item2) as IPlayerMutator)?.mutate(element);
			}
			info.fireBootEvent(BOOTEVENT_AFTERBOOTPLAYEROBJECT, game, element);
			MetricsManager.SendTelemetryWithPayload("game_start", "funnel.stages", new Dictionary<string, string>
			{
				{ "GameMode", game.gameMode },
				{
					"ControlType",
					(ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad) ? "gamepad" : "keyboard"
				}
			});
			WorldCreationProgress.StepProgress("Starting game!");
			WorldCreationProgress.StepProgress(" ");
			WorldCreationProgress.StepProgress("  ");
			Stat.ReseedFrom("InitialSeeds");
			game.SetIntGameState("RandomSeed", Stat.Rnd.Next());
			Stat.Rnd = new Random(game.GetIntGameState("RandomSeed"));
			game.SetIntGameState("RandomSeed2", Stat.Rnd.Next());
			Stat.Rnd2 = new Random(game.GetIntGameState("RandomSeed2"));
			game.SetIntGameState("RandomSeed3", Stat.Rnd.Next());
			game.SetIntGameState("RandomSeed4", Stat.Rnd.Next());
			Stat.Rnd4 = new Random(game.GetIntGameState("RandomSeed4"));
			Loading.SetHideLoadStatus(hidden: false);
			MetricsManager.LogInfo("Cached objects: " + game.ZoneManager.CachedObjects.Count);
			MemoryHelper.GCCollect();
			Cell cell = globalLocation.ResolveCell();
			if (!cell.IsReachable())
			{
				cell = cell.getClosestReachableCell();
			}
			cell.AddObject(element);
			game.Player.Body = element;
			Keyboard.ClearInput();
			game.PlayerReputation.InitFeeling();
			info.fireBootEvent(BOOTEVENT_GAMESTARTING, game);
			Stat.ReseedFrom("GameStart");
		}
	}
}
