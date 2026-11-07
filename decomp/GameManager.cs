#define NLOG_ALL
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Assets.Game.UI.Windows.Stage;
using ConsoleLib.Console;
using Cysharp.Text;
using Genkit;
using Kobold;
using LaundryBear;
using LaundryBear.PlatformServices;
using ModelShark;
using Platform.Performance;
using Qud.API;
using Qud.UI;
using QupKit;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL;
using XRL.Collections;
using XRL.Core;
using XRL.Rules;
using XRL.Serialization;
using XRL.UI;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;

public class GameManager : BaseGameController<GameManager>
{
	public enum PreferredSidebarPosition
	{
		None,
		Left,
		Right
	}

	public class ViewInfo
	{
		public string NavCategory;

		public string UICanvas;

		public int UICanvasHost;

		public bool WantsTileOver;

		public bool ForceFullscreen;

		public bool ForceFullscreenInLegacy;

		public bool KeepCurrentFullscreen;

		public bool ExecuteActions;

		public int OverlayMode;

		public bool TakesScroll;

		public ViewInfo(bool WantsTileOver, bool ForceFullscreen, string NavCategory = null, string UICanvas = null, int OverlayMode = 0, bool ExecuteActions = false, bool TakesScroll = false, int UICanvasHost = 0, bool IgnoreForceFullscreen = false, bool ForceFullscreenInLegacy = false)
		{
			this.WantsTileOver = WantsTileOver;
			this.ForceFullscreen = ForceFullscreen;
			this.ForceFullscreenInLegacy = ForceFullscreenInLegacy;
			KeepCurrentFullscreen = IgnoreForceFullscreen;
			this.UICanvas = UICanvas;
			this.UICanvasHost = UICanvasHost;
			this.OverlayMode = OverlayMode;
			this.ExecuteActions = ExecuteActions;
			this.NavCategory = NavCategory;
			this.TakesScroll = TakesScroll;
		}

		public bool IsSame(ViewInfo other)
		{
			if (WantsTileOver == other.WantsTileOver && ForceFullscreen == other.ForceFullscreen && ForceFullscreenInLegacy == other.ForceFullscreenInLegacy && UICanvas == other.UICanvas && UICanvasHost == other.UICanvasHost && OverlayMode == other.OverlayMode && ExecuteActions == other.ExecuteActions && NavCategory == other.NavCategory)
			{
				return TakesScroll == other.TakesScroll;
			}
			return false;
		}
	}

	public class ClickRegion
	{
		public int x1;

		public int y1;

		public int x2;

		public int y2;

		public string OverCommand;

		public string LeftClickCommand;

		public string RightClickCommand;

		public bool Contains(int x, int y)
		{
			if (x >= x1 && x <= x2 && y >= y1)
			{
				return y <= y2;
			}
			return false;
		}
	}

	public static bool _focused = true;

	public static float mouseDisable = 0f;

	public static UnityEngine.GameObject MainCamera;

	public static Camera cameraMainCamera;

	public static LetterboxCamera MainCameraLetterbox;

	public static Stopwatch Time = new Stopwatch();

	public static bool Playing;

	public UnityEngine.GameObject AudioListener;

	public static bool OverlayRootForceoff = false;

	public UnityEngine.GameObject Backdrop;

	public UnityEngine.GameObject TileRoot;

	public UnityEngine.GameObject OverlayRoot;

	public static GameManager Instance;

	public UnityEngine.GameObject Minimap;

	public static ManualResetEvent focusedEvent = new ManualResetEvent(initialState: true);

	public ThreadTaskQueue uiQueue = new ThreadTaskQueue();

	public ThreadTaskQueue gameQueue = new ThreadTaskQueue();

	public static bool bCapInputBuffer = false;

	public int tileWidth = 16;

	public int tileHeight = 24;

	private Point2D CurrentPlayerCell = new Point2D(-1, -1);

	public bool pauseUpdates;

	public static List<Action<PreferredSidebarPosition>> OnPreferredSidebarPositionUpdatedCallbacks = new List<Action<PreferredSidebarPosition>>();

	public PreferredSidebarPosition currentSidebarPosition;

	public Dictionary<string, UnityEngine.GameObject> findCache = new Dictionary<string, UnityEngine.GameObject>();

	public UIManager uiManager;

	public static List<Resolution> resolutions;

	public SynchronizationContext uiSynchronizationContext;

	public SynchronizationContext gameThreadSynchronizationContext;

	public PlayerInputManager player;

	private PlatformService platformService;

	private IPlatform platform;

	public UnityEngine.GameObject serviceLocatorPrefabForStandalone;

	public UITextSkin minimapLabel;

	public Texture2D minimapTexture;

	public static Color32[] minimapColors = new Color32[4000];

	public bool _bAlt;

	private bool _destroyed;

	public exTextureInfo[] CharInfos = new exTextureInfo[256];

	public ex3DSprite2[,] ConsoleCharacter = new ex3DSprite2[80, 25];

	public Transform[,] ConsoleCharacterTransform = new Transform[80, 25];

	public ex3DSprite2[,] OverlayCharacter = new ex3DSprite2[82, 27];

	public int[,] CurrentShadermode = new int[80, 25];

	public string[,] CurrentTile = new string[80, 25];

	public bool MouseInput;

	private Dictionary<string, ViewInfo> _ViewData = new Dictionary<string, ViewInfo>
	{
		{
			"*Default",
			new ViewInfo(WantsTileOver: false, ForceFullscreen: true, "Menu")
		},
		{
			"ModernPopup*",
			new ViewInfo(WantsTileOver: false, ForceFullscreen: false, "Menu")
		}
	};

	public Point2D LastTileOver = new Point2D(-1, -1);

	public UnityEngine.GameObject consoleTileAreaHandler;

	public float TargetZoomFactor = 1f;

	public Vector3 TargetCameraLocation = new Vector3(0f, 0f, -10f);

	private bool _overlayOptionsUpdated;

	public bool ModernUI;

	public double StageScale = 1.0;

	private float TargetIntensity;

	private CC_AnalogTV TV;

	public string _ActiveGameView;

	public string __CurrentGameView = "MainMenu";

	private RingDeque<string> GameViewStack = new RingDeque<string>();

	private List<ClickRegion> Regions = new List<ClickRegion>();

	private int nRegions;

	public bool bViewUpdated;

	public TooltipTrigger generalTooltip;

	public TooltipTrigger lookerTooltip;

	public TooltipTrigger tileTooltip;

	public TooltipTrigger compareLookerTooltip;

	public Vector2i lastHover = new Vector2i(-1, -1);

	public List<string> LeftGameViews = new List<string>();

	public string LastGameView = "";

	public ViewInfo currentUIView;

	private List<string> HiddenNongameViews = new List<string>();

	public static bool capslock = false;

	public static int bDraw = 0;

	public static bool AwakeComplete = false;

	public bool bInitComplete;

	public bool bInitStarted;

	public bool bEditorMode;

	public bool bEditorInit;

	public bool FadeSplash;

	public UnityEngine.GameObject SplashCanvas;

	public float minimapScale = 1f;

	public float nearbyObjectsListScale = 1f;

	public float compassScale = 1f;

	public static bool dirtyFocused = false;

	private bool bFirstUpdate = true;

	public bool DisplayMinimap;

	public int LastWidth;

	public int LastHeight;

	private int _TileScale;

	private int _DockMovable;

	public bool Hallucinating;

	public float _Spin;

	public float _CurrentSpin;

	public float MAX_THIN_WORLD_DISTORTING = 3f;

	public float _ThinWorldDistoring;

	private int _StaticEffecting;

	private float _StaticEffectTime;

	private float _NextStaticEffectGlitch;

	public bool Greyscaling;

	public int GreyscaleLevel;

	public float _spacefoldingT;

	public bool _spacefolding;

	public float _fuzzingT;

	public bool _fuzzing;

	public RectTransform stageRect;

	public StageDock stageDock;

	public bool ShouldForceFullscreen;

	public int _selectedAbility = -1;

	public TextMeshProUGUI selectedAbilityText;

	public ActivatedAbilityEntry currentlySelectedAbility;

	public UnityEngine.GameObject playerTrackerPrefab;

	public PlayerTracker playerTracker;

	public MonoBehaviour InputManagerModuleNew;

	public MonoBehaviour InputManagerModuleOld;

	public string currentNavCategory;

	public string currentNavDirection;

	public StringBuilder selectedAbilitybuilder = new StringBuilder();

	public string DebugNavLayers;

	public string DebugNavCategory;

	private static int TakeAStepFilterMode = 0;

	public float lookDelay;

	public float lookRepeat;

	public UnityEngine.UI.Text currentNavDirectionDisplay;

	public static bool _runPlayerTurnOnUIThread = false;

	private ScreenBuffer ClearBuffer;

	public bool _fadingToBlack;

	public int fadeToBlackStage;

	public float originalBrightness = float.MinValue;

	public float fadingToBlackTimer;

	public static int runWholeTurnOnUIThread = 0;

	public Vector3 lastCameraPosition = Vector3.zero;

	private bool lastFullscreen = true;

	public static int UpdateCountFromChangedToActiveView = 0;

	public static Color backdropBlackColor = new Color(13f / 255f, 47f / 256f, 23f / 128f, 1f);

	public static ulong frame = 0uL;

	public string LastViewTag;

	private List<UnityEngine.GameObject> Backdrops = new List<UnityEngine.GameObject>();

	private Dictionary<string, UnityEngine.GameObject> BackdropsByName = new Dictionary<string, UnityEngine.GameObject>();

	private HashSet<string> RequestedBackdrops = new HashSet<string>();

	public static bool focused
	{
		get
		{
			return _focused;
		}
		set
		{
			if (!_focused & value)
			{
				Keyboard.ClearInput();
				Keyboard.ClearMouseEvents();
				mouseDisable = 0.25f;
			}
			_focused = value;
			if (value)
			{
				focusedEvent.Set();
			}
			else
			{
				focusedEvent.Reset();
			}
		}
	}

	public bool bAlt
	{
		get
		{
			return _bAlt;
		}
		set
		{
			_bAlt = value;
			if (_bAlt)
			{
				GameObjectFind("AltButton").GetComponent<Image>().color = Color.red;
			}
			if (!_bAlt)
			{
				GameObjectFind("AltButton").GetComponent<Image>().color = Color.white;
			}
		}
	}

	[Obsolete("The ViewData property will soon be removed, update to using XRL.UI.UIView on a class.")]
	public Dictionary<string, ViewInfo> ViewData => _ViewData;

	public string _CurrentGameView
	{
		get
		{
			return __CurrentGameView;
		}
		set
		{
			if (value != __CurrentGameView)
			{
				UpdateCountFromChangedToActiveView = 0;
				__CurrentGameView = value;
			}
		}
	}

	public string CurrentGameView
	{
		get
		{
			return _CurrentGameView;
		}
		set
		{
			if (!(_CurrentGameView == value))
			{
				_CurrentGameView = value;
				ClearRegions();
			}
		}
	}

	public int TileScale
	{
		get
		{
			return _TileScale;
		}
		set
		{
			if (_TileScale != value)
			{
				_TileScale = value;
				if ((bool)MainCameraLetterbox)
				{
					MainCameraLetterbox.Refresh();
				}
			}
		}
	}

	public int DockMovable
	{
		get
		{
			return _DockMovable;
		}
		set
		{
			if (_DockMovable != value)
			{
				_DockMovable = value;
				if ((bool)MainCameraLetterbox)
				{
					MainCameraLetterbox.Refresh();
				}
			}
		}
	}

	public float Spin
	{
		set
		{
			if (_Spin <= 0f)
			{
				_Spin = value;
				_CurrentSpin = 0f;
			}
		}
	}

	public float ThinWorldDistorting
	{
		set
		{
			_ThinWorldDistoring = value;
		}
	}

	public int StaticEffecting
	{
		set
		{
			if (value > 0 && _StaticEffectTime > 0f)
			{
				_StaticEffectTime = 0f;
				_NextStaticEffectGlitch = 0f;
			}
			if (value > _StaticEffecting)
			{
				_StaticEffecting = value;
			}
		}
	}

	public bool Spacefolding
	{
		get
		{
			return _spacefolding;
		}
		set
		{
			_spacefolding = value;
			if (value)
			{
				_spacefoldingT = 0f;
			}
		}
	}

	public bool Fuzzing
	{
		get
		{
			return _fuzzing;
		}
		set
		{
			_fuzzing = value;
			if (value)
			{
				_fuzzingT = 0f;
			}
		}
	}

	public int selectedAbility
	{
		get
		{
			return _selectedAbility;
		}
		set
		{
			_selectedAbility = value;
		}
	}

	public static bool runPlayerTurnOnUIThread
	{
		get
		{
			return _runPlayerTurnOnUIThread;
		}
		set
		{
			_runPlayerTurnOnUIThread = value;
			UnityEngine.Debug.LogWarning("runPlayerTurnSet:" + _runPlayerTurnOnUIThread);
		}
	}

	public bool fadeToBlack
	{
		get
		{
			return _fadingToBlack;
		}
		set
		{
			if (value)
			{
				originalBrightness = float.MinValue;
				fadeToBlackStage = 0;
				fadingToBlackTimer = 0f;
			}
			_fadingToBlack = value;
		}
	}

	public bool NeedsToPan
	{
		get
		{
			if (!(TargetZoomFactor > 1f))
			{
				if (Options.PlayScale != Options.PlayAreaScaleTypes.Fit)
				{
					return MainCameraLetterbox.IsOverflowing;
				}
				return false;
			}
			return true;
		}
	}

	public bool NeedsArrowBorder
	{
		get
		{
			if (!(TargetZoomFactor > 1f))
			{
				return Options.PlayScale != Options.PlayAreaScaleTypes.Fit;
			}
			return true;
		}
	}

	public static Camera MapCamera => MainCamera.GetComponent<Camera>();

	public void SetPlayerCell(Point2D C, bool updateCamera = false)
	{
		playerTracker.transform.position = getTileCenter(C.x, C.y, 20);
		if (CurrentPlayerCell != C)
		{
			TargetCameraLocation = CellToWorldspace(C);
			CurrentPlayerCell = C;
		}
		AudioListener.transform.position = getTileCenter(C.x, C.y);
		UpdatePreferredSidebarPosition();
		if (updateCamera)
		{
			RefreshLayout();
			MainCameraLetterbox.OnUpdate();
		}
	}

	public void cinematicPan(string id)
	{
		MainCamera.AddComponent<CinematicPan>().Begin(id);
		UIManager.getWindow("Stage").gameObject.SetActive(value: false);
		UIManager.getWindow("AbilityBar").gameObject.SetActive(value: false);
		UIManager.getWindow("PlayerStatusBar").gameObject.SetActive(value: false);
		pauseUpdates = true;
	}

	public Vector3 CellToWorldspace(Point2D C)
	{
		return new Vector3(40 * -tileWidth + C.x * tileWidth + 8, 12.5f * (float)tileHeight - (float)(C.y * tileHeight) - 12f, -10f);
	}

	public Vector2 WorldspaceToCanvasSpace(Vector3 WorldSpace)
	{
		return UIManager.instance.WorldspaceToCanvasSpace(WorldSpace, cameraMainCamera);
	}

	public void UpdatePreferredSidebarPosition()
	{
		if (DockMovable > 0 && DockMovable != 3)
		{
			return;
		}
		Vector2 vector = WorldspaceToCanvasSpace(CellToWorldspace(CurrentPlayerCell));
		float num = 0.45f;
		float num2 = 0.55f;
		PreferredSidebarPosition preferredSidebarPosition = currentSidebarPosition;
		float num3 = vector.x / (float)Screen.width;
		if (currentSidebarPosition == PreferredSidebarPosition.None)
		{
			preferredSidebarPosition = PreferredSidebarPosition.Left;
		}
		else if (currentSidebarPosition == PreferredSidebarPosition.Right)
		{
			if (num3 > num2)
			{
				preferredSidebarPosition = PreferredSidebarPosition.Left;
			}
		}
		else if (currentSidebarPosition == PreferredSidebarPosition.Left && num3 < num)
		{
			preferredSidebarPosition = PreferredSidebarPosition.Right;
		}
		if (preferredSidebarPosition == currentSidebarPosition)
		{
			return;
		}
		if (preferredSidebarPosition == PreferredSidebarPosition.Left)
		{
			UnityEngine.Debug.Log("Sidebar swap to left");
		}
		if (preferredSidebarPosition == PreferredSidebarPosition.Right)
		{
			UnityEngine.Debug.Log("Sidebar swap to right");
		}
		currentSidebarPosition = preferredSidebarPosition;
		if (DockMovable != 3)
		{
			for (int i = 0; i < OnPreferredSidebarPositionUpdatedCallbacks.Count; i++)
			{
				OnPreferredSidebarPositionUpdatedCallbacks[i](preferredSidebarPosition);
			}
		}
	}

	private UnityEngine.GameObject GameObjectFind(string id)
	{
		if (findCache.ContainsKey(id))
		{
			if (!(findCache[id] == null))
			{
				return findCache[id];
			}
			findCache.Remove(id);
		}
		UnityEngine.GameObject gameObject = UnityEngine.GameObject.Find(id);
		if (gameObject != null)
		{
			findCache.Add(id, gameObject);
		}
		_ = gameObject == null;
		return gameObject;
	}

	private void SetPlayerCell(int x, int y)
	{
		TargetCameraLocation = new Vector3(40 * -tileWidth + x * tileWidth + 8, 12.5f * (float)tileHeight - (float)(y * tileHeight) - 12f, -10f);
	}

	public bool ListsDiffer(QudItemList l1, QudItemList l2)
	{
		if (l1 == null || l2 == null)
		{
			return true;
		}
		if (l1.objects.Count != l2.objects.Count)
		{
			return true;
		}
		for (int i = 0; i < l1.objects.Count; i++)
		{
			if (l1.objects[i].go != l2.objects[i].go)
			{
				return true;
			}
		}
		return false;
	}

	public void ProcessBufferExtra(IScreenBufferExtra extra)
	{
		QudScreenBufferExtra qudScreenBufferExtra = (QudScreenBufferExtra)extra;
		if (qudScreenBufferExtra.playerPosition != Point2D.invalid)
		{
			SetPlayerCell(qudScreenBufferExtra.playerPosition);
			RefreshLayout();
		}
		extra.Free();
	}

	public void LoadCommandLine()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (commandLineArgs == null || commandLineArgs.Length == 0)
		{
			return;
		}
		for (int i = 0; i < commandLineArgs.Length - 1; i++)
		{
			if (commandLineArgs[i].ToUpper().Contains("NOMETRICS"))
			{
				Globals.ForceMetricsOff = true;
				break;
			}
		}
	}

	public static void InitializeServicesForStandalone()
	{
		UnityEngine.Object.Instantiate(Instance.serviceLocatorPrefabForStandalone).TryGetComponent<ServiceLocator>(out var component);
		InitializeServices.AddRuntimeServices(component);
		IEnumerator enumerator = component.Initialize(sync: true);
		while (enumerator.MoveNext())
		{
		}
		ServiceLocator.TryGetService<PlatformService>(out var service);
		if (service.SetupLaunchUserAndStorage().Result)
		{
			UnityEngine.Debug.Log("Laundry Bear's Platform Layer is ready for use.");
		}
		else
		{
			UnityEngine.Debug.LogError("An error occurred while setting up the user or storage. See logs for details. Halting load.");
		}
	}

	private void Awake()
	{
		StartCoroutine(AwakeRoutine());
	}

	private IEnumerator AwakeRoutine()
	{
		Instance = this;
		InitializeServicesForStandalone();
		ServiceLocator.TryGetService<PlatformService>(out platformService);
		platformService.BeginShutdownNoticeSection();
		ServiceLocator.TryGetService<IPlatform>(out platform);
		platform.ShutdownEvent += OnShutdownRequest;
		CPUBoost.InitOnMainThread();
		using (CPUBoost.AutoScope())
		{
			Time.Start();
			Playing = Application.isPlaying;
			Application.quitting += Quitting;
			resolutions = new List<Resolution>(Screen.resolutions);
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			uiQueue.threadContext = Thread.CurrentThread;
			uiSynchronizationContext = SynchronizationContext.Current;
			LoadCommandLine();
			XRLCore.InitializePaths();
			Logger.gameLog.Info("Starting up logger...");
			Logger.gameLog.Info("Getting player 0...");
			if (player == null)
			{
				player = gameObject.AddComponent<PlayerInputManager>();
			}
			Logger.gameLog.Info("Got player 0...");
			originalBrightness = float.MinValue;
			try
			{
				UnityEngine.Debug.Log("Startup time: " + DateTime.Now.ToLongDateString() + " at " + DateTime.Now.ToLongTimeString());
				UnityEngine.Debug.Log("Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
				UnityEngine.Debug.Log("Platform: " + Application.platform);
				UnityEngine.Debug.Log("System Language: " + Application.systemLanguage);
				UnityEngine.Debug.Log("Unity Version: " + Application.unityVersion);
				UnityEngine.Debug.Log("Unity Reported Version: " + Application.version);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.Log("Error with log header: " + ex.ToString());
			}
			Application.targetFrameRate = 60;
			for (int i = 0; i < 50; i++)
			{
				Regions.Add(new ClickRegion());
			}
			SplashCanvas.SetActive(value: true);
			ConsoleLib.Console.ColorUtility.LoadBaseColors();
			PlatformManager.Awake();
			GetComponent<CapabilityManager>().Init();
			Options.LoadOptions();
			Options.UpdateFlags();
			GetComponent<ControlManager>().Init();
			AchievementManager.Awake();
			FastSerialization.CacheFieldSaveVersions();
			uiManager.Init();
			if (Environment.GetEnvironmentVariable("HARMONY_LOG_FILE").IsNullOrEmpty())
			{
				Environment.SetEnvironmentVariable("HARMONY_LOG_FILE", DataManager.SavePath("harmony.log.txt"));
			}
			ModManager.Init();
			ModManager.LogRunningMods();
			ConsoleLib.Console.ColorUtility.LoadModColors();
			Options.LoadModOptions();
			yield return null;
			Options.UpdateFlags(null, Refresh: true);
			CommandBindingManager.LoadCommands();
			Screens.Awake();
			ComponentReflection.Initialize();
			yield return null;
			Mutations.GetAllMutationEquipment();
			StatusScreensScreen.prewarm();
			yield return null;
			minimapTexture = new Texture2D(80, 50, TextureFormat.ARGB32, mipChain: false);
			minimapTexture.filterMode = UnityEngine.FilterMode.Point;
			Minimap.GetComponent<Image>().sprite = Sprite.Create(minimapTexture, new Rect(0f, 0f, 80f, 50f), new Vector2(0f, 0f));
			for (int j = 0; j < 80; j++)
			{
				for (int k = 0; k < 50; k++)
				{
					minimapTexture.SetPixel(j, k, new Color(0f, 0f, 0f, 0f));
				}
			}
			minimapTexture.Apply();
			_overlayOptionsUpdated = false;
			mouseDisable = 0f;
			ClearBuffer = ScreenBuffer.create(80, 25);
			AwakeComplete = true;
		}
	}

	public void DirtyControlPanel()
	{
		_overlayOptionsUpdated = true;
	}

	public void UpdateMinimap()
	{
		lock (minimapColors)
		{
			minimapTexture.SetPixels32(minimapColors);
			minimapTexture.Apply();
		}
	}

	private void Quitting()
	{
		Playing = false;
	}

	public void Quit()
	{
		if (platform.SupportsQuit)
		{
			platform.Quit();
		}
	}

	private void OnShutdownRequest()
	{
		HandleOnDestroy();
	}

	private void OnDestroy()
	{
		HandleOnDestroy();
	}

	private void HandleOnDestroy()
	{
		if (_destroyed)
		{
			return;
		}
		try
		{
			Task task = The.Game?.SaveTask;
			if (task != null && !task.IsCompleted)
			{
				task.Wait();
			}
			XRLCore.Stop();
			if (XRLCore.CoreThread != null)
			{
				XRLCore.CoreThread.Interrupt();
				XRLCore.CoreThread.Abort();
			}
		}
		catch
		{
		}
		try
		{
			PlatformManager.Shutdown();
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Platform Shutdown", x);
		}
		try
		{
			DataManager.Shutdown();
		}
		catch (Exception x2)
		{
			MetricsManager.LogException("ZoneManager Shutdown", x2);
		}
		try
		{
			ServiceLocator.TryGetService<PlatformService>(out var service);
			service.EndShutdownNoticeSection();
		}
		catch (Exception x3)
		{
			MetricsManager.LogException("PlatformService Shutdown", x3);
		}
	}

	public bool HasViewData(string V)
	{
		if (V.StartsWith("ModernPopup"))
		{
			return true;
		}
		return _ViewData.ContainsKey(V);
	}

	public bool AnyViewInStackIsStage()
	{
		if (CurrentGameView == "Stage")
		{
			return true;
		}
		lock (GameViewStack)
		{
			foreach (string item in GameViewStack)
			{
				if (item == "Stage")
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AnyViewInStackForcefullscreenInModernOrLegacy()
	{
		ViewInfo viewData = GetViewData(CurrentGameView);
		if (viewData == null || !viewData.ForceFullscreen)
		{
			if (!Options.ModernUI)
			{
				ViewInfo viewData2 = GetViewData(CurrentGameView);
				if (viewData2 != null && viewData2.ForceFullscreenInLegacy)
				{
					goto IL_003b;
				}
			}
			lock (GameViewStack)
			{
				foreach (string item in GameViewStack)
				{
					ViewInfo viewData3 = GetViewData(item);
					if (viewData3 == null || !viewData3.ForceFullscreen)
					{
						if (Options.ModernUI)
						{
							continue;
						}
						ViewInfo viewData4 = GetViewData(item);
						if (viewData4 == null || !viewData4.ForceFullscreenInLegacy)
						{
							continue;
						}
					}
					return true;
				}
			}
			return false;
		}
		goto IL_003b;
		IL_003b:
		return true;
	}

	public bool AnyViewInStackForcefullscreen()
	{
		try
		{
			ViewInfo viewData = GetViewData(CurrentGameView);
			if (viewData != null && viewData.ForceFullscreen)
			{
				return true;
			}
			lock (GameViewStack)
			{
				foreach (string item in GameViewStack)
				{
					ViewInfo viewData2 = GetViewData(item);
					if (viewData2 != null && viewData2.ForceFullscreen)
					{
						return true;
					}
				}
			}
		}
		catch
		{
			return false;
		}
		return false;
	}

	public bool AnyViewInStack(Predicate<string> test)
	{
		if (test(CurrentGameView))
		{
			return true;
		}
		lock (GameViewStack)
		{
			foreach (string item in GameViewStack)
			{
				if (test(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AnyViewInStack(Predicate<ViewInfo> test)
	{
		return AnyViewInStack((string s) => test(GetViewData(s)));
	}

	public ViewInfo GetViewData(string V)
	{
		if (_ViewData.TryGetValue(V, out var value))
		{
			return value;
		}
		if (V.StartsWith("ModernPopup"))
		{
			return _ViewData["ModernPopup*"];
		}
		return _ViewData["*Default"];
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		TargetCameraLocation = MainCameraLetterbox.DesiredPan;
	}

	public void OnDrag(Vector2 Delta)
	{
		if (XRLCore.bThreadFocus)
		{
			TargetCameraLocation -= new Vector3(Delta.x * MainCameraLetterbox.UnitsPerPixel, Delta.y * MainCameraLetterbox.UnitsPerPixel, 0f);
			if (MainCameraLetterbox.Pannable)
			{
				MainCameraLetterbox.OnUpdate();
			}
		}
	}

	public void OnTileOver(int x, int y)
	{
		if (mouseDisable > 0f || (!GetViewData(CurrentGameView).WantsTileOver && Options.ModernUI) || (LastTileOver.x == x && LastTileOver.y == y))
		{
			return;
		}
		LastTileOver = new Point2D(x, y);
		lock (Regions)
		{
			for (int i = 0; i < nRegions; i++)
			{
				if (Regions[i].OverCommand != null && Regions[i].Contains(x, y))
				{
					Keyboard.PushMouseEvent(Regions[i].OverCommand, x, y);
					return;
				}
			}
		}
		Keyboard.PushMouseEvent("PointerOver", x, y);
	}

	public void OnTileClicked(string Event, int x, int y)
	{
		if (mouseDisable > 0f)
		{
			return;
		}
		lock (Regions)
		{
			for (int i = 0; i < nRegions; i++)
			{
				if (Event == "LeftClick" && Regions[i].LeftClickCommand != null && Regions[i].Contains(x, y))
				{
					Keyboard.PushMouseEvent(Regions[i].LeftClickCommand, x, y);
					return;
				}
				if (Event == "RightClick" && Regions[i].RightClickCommand != null && Regions[i].Contains(x, y))
				{
					Keyboard.PushMouseEvent(Regions[i].RightClickCommand, x, y);
					return;
				}
			}
		}
		if (!(Event == "LeftClick") && !(Event == "RightClick"))
		{
			Keyboard.PushMouseEvent(Event, x, y);
		}
	}

	public void ZoomIn()
	{
		if (TargetZoomFactor < 1f)
		{
			TargetZoomFactor = 1f;
		}
		else
		{
			TargetZoomFactor = RoundToNearestQuarter(Mathf.Max(1f, Camera.main.GetComponent<LetterboxCamera>().DesiredZoomFactor + 0.25f));
		}
	}

	public void ZoomOut()
	{
		if (TargetZoomFactor == 1f && NeedsToPan)
		{
			TargetZoomFactor = -1f;
		}
		else if (TargetZoomFactor <= 1f)
		{
			if (Options.GetOption("OptionAllowFramelessZoomOut") == "Yes")
			{
				TargetZoomFactor = -2f;
			}
		}
		else
		{
			TargetZoomFactor = RoundToNearestQuarter(Mathf.Max(1f, Camera.main.GetComponent<LetterboxCamera>().DesiredZoomFactor - 0.25f));
		}
	}

	public static float RoundToNearestQuarter(float value)
	{
		return (float)Math.Round(value * 4f, MidpointRounding.AwayFromZero) / 4f;
	}

	public void OnScroll(Vector2 Amount)
	{
		if (!Options.MouseScrollWheel || !XRLCore.bThreadFocus || (CurrentGameView != "MapEditor" && currentUIView != null && CurrentGameView != "Popup:Item" && currentUIView.UICanvas != "Looker" && currentUIView.UICanvas != "Keybinds" && currentUIView.UICanvas != Options.StageViewID))
		{
			return;
		}
		ViewInfo viewData = GetViewData(CurrentGameView);
		if ((!viewData.ForceFullscreenInLegacy && !viewData.ForceFullscreen && !viewData.TakesScroll) || _ActiveGameView == Options.StageViewID || CurrentGameView == "MapEditor")
		{
			if (Amount.y < 0f && TargetZoomFactor <= 1f)
			{
				ZoomOut();
				return;
			}
			if (Amount.y > 0f && TargetZoomFactor < 1f)
			{
				ZoomIn();
				return;
			}
			float result = 1f;
			float.TryParse(Options.GetOption("OptionZoomSensitivity"), out result);
			TargetZoomFactor = Mathf.Max(1f, Camera.main.GetComponent<LetterboxCamera>().DesiredZoomFactor + Mathf.Sign(Amount.y) * (0.25f / result));
		}
		else if (Amount.y > 0f)
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad8, '8'));
		}
		else
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad2, '2'));
		}
	}

	public void OnControlPanelButton(string ID)
	{
		if (ID == "CmdZoomIn")
		{
			ZoomIn();
		}
		if (ID == "CmdZoomOut")
		{
			ZoomOut();
		}
		if (ID == "CmdAlt")
		{
			bAlt = !bAlt;
			return;
		}
		if (CurrentGameView != Options.StageViewID)
		{
			switch (ID)
			{
			case "CmdMoveU":
				Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Space, 'y'));
				return;
			case "CmdMoveD":
				Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.N, 'n'));
				return;
			case "CmdWait":
				Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Space, ' '));
				return;
			case "CmdMoveN":
				Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad8));
				return;
			case "CmdMoveS":
				Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad2));
				return;
			case "CmdMoveE":
				Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad6));
				return;
			case "CmdMoveW":
				Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad4));
				return;
			case "CmdMoveNW":
				Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad7));
				return;
			case "CmdMoveNE":
				Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad9));
				return;
			case "CmdMoveSW":
				Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad1));
				return;
			case "CmdMoveSE":
				Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad3));
				return;
			}
		}
		else
		{
			if (ID.StartsWith("CmdMove") && bAlt)
			{
				Keyboard.PushMouseEvent(ID.Replace("CmdMove", "CmdAttack"), -1, -1);
				return;
			}
			if (ID == "CmdWaitUntilHealed" && bAlt)
			{
				Keyboard.PushMouseEvent(ID.Replace("CmdAutoExplore", "CmdAttack"), -1, -1);
				return;
			}
		}
		if (ID == "CmdEscape")
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Escape));
		}
		else if (ID == "CmdReturn")
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Return, '\r'));
		}
		else
		{
			Keyboard.PushMouseEvent(ID, -1, -1);
		}
	}

	public void ClearRegions()
	{
		lock (Regions)
		{
			nRegions = 0;
		}
	}

	public void AddRegion(int x1, int y1, int x2, int y2, string LeftCommand = null, string RightCommand = null, string OverCommand = null)
	{
		lock (Regions)
		{
			if (nRegions >= Regions.Count)
			{
				UnityEngine.Debug.Log("Add region");
				Regions.Add(new ClickRegion());
			}
			Regions[nRegions].x1 = x1;
			Regions[nRegions].y1 = y1;
			Regions[nRegions].x2 = x2;
			Regions[nRegions].y2 = y2;
			Regions[nRegions].LeftClickCommand = LeftCommand;
			Regions[nRegions].RightClickCommand = RightCommand;
			Regions[nRegions].OverCommand = OverCommand;
			nRegions++;
		}
	}

	public void ShowTooltipForTile(int x, int y, string mode = "rightclick")
	{
		if (ModernUI && UIManager.instance.currentWindow != null && UIManager.instance.currentWindow.name == "Stage")
		{
			tileTooltip.ForceHideTooltip();
			gameQueue.queueSingletonTask("TileHover", () =>
			{
				Look.QueueLookerTooltip(x, y, mode);
			});
		}
	}

	public void TileHover(int x, int y)
	{
	}

	public bool InGameView()
	{
		lock (GameViewStack)
		{
			return GameViewStack.Count > 0;
		}
	}

	public void ClearViewStack()
	{
		GameViewStack.Clear();
	}

	public static void EnsureGameView(string view)
	{
		Instance.bViewUpdated = true;
	}

	public string[] GetGameViewStackCopy()
	{
		lock (GameViewStack)
		{
			RingDeque<string> gameViewStack = GameViewStack;
			bool flag = !CurrentGameView.IsNullOrEmpty();
			int num = gameViewStack.Count;
			if (flag)
			{
				num++;
			}
			string[] array = new string[num];
			if (flag)
			{
				array[--num] = CurrentGameView;
			}
			for (int i = 0; i < num; i++)
			{
				array[i] = gameViewStack[i];
			}
			return array;
		}
	}

	public void SetGameViewStack(string[] Views)
	{
		ControlManager.ResetInput();
		lock (GameViewStack)
		{
			ClearViewStack();
			for (int num = Views.Length - 2; num >= 0; num--)
			{
				GameViewStack.Push(Views[num]);
			}
			CurrentGameView = Views[^1];
			bViewUpdated = true;
			TextConsole.BufferUpdated = true;
		}
	}

	public void SetGameViewStack(string NewView)
	{
		ControlManager.ResetInput();
		lock (GameViewStack)
		{
			ClearViewStack();
			CurrentGameView = NewView;
			bViewUpdated = true;
			TextConsole.BufferUpdated = true;
		}
	}

	public void PushGameView(string NewView, bool bHard = true)
	{
		ControlManager.ResetInput();
		lock (GameViewStack)
		{
			if (LeftGameViews.Contains(NewView))
			{
				LeftGameViews.Remove(NewView);
			}
			GameViewStack.Enqueue(CurrentGameView);
			CurrentGameView = NewView;
			bViewUpdated = true;
			if (bHard)
			{
				TextConsole.BufferUpdated = true;
			}
		}
	}

	public void ForceGameView()
	{
		lock (GameViewStack)
		{
			GameViewStack.Clear();
			CurrentGameView = Options.StageViewID;
			TextConsole.BufferUpdated = true;
			bViewUpdated = true;
		}
	}

	public void PopGameView(bool bHard = false)
	{
		ControlManager.ResetInput();
		lock (GameViewStack)
		{
			if (bHard)
			{
				TextConsole.BufferUpdated = true;
			}
			if (GameViewStack.Count > 0)
			{
				if (CurrentGameView != null && !LeftGameViews.Contains(CurrentGameView))
				{
					LeftGameViews.Add(CurrentGameView);
				}
				CurrentGameView = GameViewStack.Eject();
			}
			bViewUpdated = true;
		}
	}

	public void RemoveGameView(string Name, bool Hard = false)
	{
		lock (GameViewStack)
		{
			if (GameViewStack.Count <= 0)
			{
				return;
			}
			if (Name == CurrentGameView)
			{
				ControlManager.ResetInput();
				if (CurrentGameView != null && !LeftGameViews.Contains(CurrentGameView))
				{
					LeftGameViews.Add(CurrentGameView);
				}
				CurrentGameView = GameViewStack.Eject();
				bViewUpdated = true;
				if (Hard)
				{
					TextConsole.BufferUpdated = true;
				}
			}
			else
			{
				GameViewStack.Remove(Name);
			}
		}
	}

	private void UpdateView()
	{
		ShowNongameViews();
		LeftGameViews.Clear();
		if (_ActiveGameView != _CurrentGameView)
		{
			ControlManager.ResetInput();
			Keyboard.ClearInput();
		}
		_ActiveGameView = _CurrentGameView;
		if (_ActiveGameView == Options.StageViewID)
		{
			currentUIView = null;
			HideNongameViews(forShowingLater: false);
			currentUIView = GetViewData(_ActiveGameView);
			if (Options.ModernUI && currentUIView.UICanvasHost != 0 && currentUIView.UICanvasHost == 1 && ModernUI && currentUIView.UICanvas != "_manuallyshown")
			{
				UIManager.showWindow(currentUIView.UICanvas, aggressive: true);
			}
			return;
		}
		currentUIView = GetViewData(_ActiveGameView);
		if (_ActiveGameView == null || currentUIView == null)
		{
			UIManager.showWindow("PassthroughDefault", aggressive: true);
		}
		else
		{
			if (currentUIView.UICanvasHost == 0)
			{
				return;
			}
			if (currentUIView.UICanvasHost == 1)
			{
				if (currentUIView.UICanvas != "_manuallyshown" && ModernUI)
				{
					UIManager.showWindow(currentUIView.UICanvas);
				}
				else
				{
					UIManager.instance.currentWindow = null;
				}
			}
			else
			{
				MetricsManager.LogError("Bad view canvas host: " + currentUIView.UICanvasHost);
			}
		}
	}

	private void HideNongameViews(bool forShowingLater = true)
	{
		if (!forShowingLater)
		{
			lock (GameViewStack)
			{
				GameViewStack.Clear();
			}
		}
	}

	private void ShowNongameViews()
	{
		HiddenNongameViews.Clear();
	}

	public ex3DSprite2 getTile(int x, int y)
	{
		return ConsoleCharacter[x, y];
	}

	public ex3DSprite2 GetOverlayTile(int X, int Y)
	{
		if (X < 0 || X > 79 || Y < 0 || Y > 24)
		{
			return null;
		}
		return OverlayCharacter[X + 1, Y + 1];
	}

	public Vector3 getTileCenter(int x, int y, int z = 0)
	{
		return new Vector3(40 * -tileWidth + x * tileWidth + 8, 12.5f * (float)tileHeight - (float)(y * tileHeight) - 12f, 100 - z);
	}

	public Vector3 getScreenTileCenter(int x, int y, int z = 0)
	{
		return MainCamera.GetComponent<Camera>().WorldToScreenPoint(getTileCenter(x, y, z));
	}

	public Vector3 getTileCenter(float x, float y, float z = 0f)
	{
		return new Vector3((float)(40 * -tileWidth) + x * (float)tileWidth + 8f, 12.5f * (float)tileHeight - y * (float)tileHeight - 12f, 100f - z);
	}

	private void StartGameThread()
	{
		UnityEngine.Debug.Log("Starting game thread...");
		useGUILayout = false;
		MainCamera = GameObjectFind("Main Camera");
		cameraMainCamera = MainCamera.GetComponent<Camera>();
		MainCameraLetterbox = MainCamera.GetComponent<LetterboxCamera>();
		WaveCollapseTools.LoadTemplates();
		SoundManager.Init();
		Thread threadContext = XRLCore.Start();
		gameQueue.threadContext = threadContext;
		TV = MainCamera.GetComponent<CC_AnalogTV>();
		TargetIntensity = TV.noiseIntensity;
		TV.noiseIntensity = 1f;
		TileRoot = new UnityEngine.GameObject();
		TileRoot.name = "_tileroot";
		TileRoot.AddComponent<CameraShake>();
		playerTracker = UnityEngine.Object.Instantiate(playerTrackerPrefab).GetComponent<PlayerTracker>();
		playerTracker.transform.SetParent(TileRoot.transform, worldPositionStays: false);
		OverlayRoot = new UnityEngine.GameObject();
		OverlayRoot.name = "_overlayroot";
		for (int i = 0; i < 80; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				CurrentTile[i, j] = null;
			}
		}
		for (int k = 0; k < 80; k++)
		{
			for (int l = 0; l < 25; l++)
			{
				CurrentShadermode[k, l] = 0;
				ConsoleCharacter[k, l] = SpriteManager.CreateSprite("assets_content_textures_text_1.bmp").GetComponent<ex3DSprite2>();
				ConsoleCharacter[k, l].gameObject.transform.SetParent(TileRoot.transform);
				ConsoleCharacter[k, l].gameObject.transform.position = getTileCenter(k, l);
				ConsoleCharacter[k, l].gameObject.AddComponent<TileBehavior>().SetPosition(k, l);
				ConsoleCharacterTransform[k, l] = ConsoleCharacter[k, l]?.transform;
			}
		}
		Backdrop = SpriteManager.CreateSprite("assets_content_textures_text_0.bmp");
		Backdrop.name = "Backdrop";
		Backdrop.transform.SetParent(TileRoot.transform);
		Backdrop.transform.position = new Vector3(0f, 0f, 1000f);
		Backdrop.GetComponent<ex3DSprite2>().width = 1280f;
		Backdrop.GetComponent<ex3DSprite2>().height = 600f;
		Backdrop.GetComponent<ex3DSprite2>().color = The.Color.DarkBlack;
		Backdrop.GetComponent<ex3DSprite2>().detailcolor = The.Color.DarkBlack;
		Backdrop.GetComponent<ex3DSprite2>().backcolor = The.Color.DarkBlack;
		for (int m = 0; m < 82; m++)
		{
			for (int n = 0; n < 27; n++)
			{
				if (m != 0 && m != 81 && n != 0 && n != 26)
				{
					continue;
				}
				if (m == 0 && n == 0)
				{
					OverlayCharacter[m, n] = SpriteManager.CreateSprite(Options.UseTiles ? "Assets_Content_Textures_UI_ui_upleft.png" : "assets_content_textures_text_27.bmp").GetComponent<ex3DSprite2>();
				}
				else if (m == 81 && n == 0)
				{
					OverlayCharacter[m, n] = SpriteManager.CreateSprite(Options.UseTiles ? "Assets_Content_Textures_UI_ui_upright.png" : "assets_content_textures_text_26.bmp").GetComponent<ex3DSprite2>();
				}
				else if (m == 0 && n == 26)
				{
					OverlayCharacter[m, n] = SpriteManager.CreateSprite(Options.UseTiles ? "Assets_Content_Textures_UI_ui_downleft.png" : "assets_content_textures_text_24.bmp").GetComponent<ex3DSprite2>();
				}
				else if (m == 81 && n == 26)
				{
					OverlayCharacter[m, n] = SpriteManager.CreateSprite(Options.UseTiles ? "Assets_Content_Textures_UI_ui_downright.png" : "assets_content_textures_text_25.bmp").GetComponent<ex3DSprite2>();
				}
				else
				{
					switch (m)
					{
					case 0:
						OverlayCharacter[m, n] = SpriteManager.CreateSprite(Options.UseTiles ? "Assets_Content_Textures_UI_ui_left.png" : "assets_content_textures_text_27.bmp").GetComponent<ex3DSprite2>();
						break;
					case 81:
						OverlayCharacter[m, n] = SpriteManager.CreateSprite(Options.UseTiles ? "Assets_Content_Textures_UI_ui_right.png" : "assets_content_textures_text_26.bmp").GetComponent<ex3DSprite2>();
						break;
					default:
						switch (n)
						{
						case 0:
							OverlayCharacter[m, n] = SpriteManager.CreateSprite(Options.UseTiles ? "Assets_Content_Textures_UI_ui_up.png" : "assets_content_textures_text_24.bmp").GetComponent<ex3DSprite2>();
							break;
						case 26:
							OverlayCharacter[m, n] = SpriteManager.CreateSprite(Options.UseTiles ? "Assets_Content_Textures_UI_ui_down.png" : "assets_content_textures_text_25.bmp").GetComponent<ex3DSprite2>();
							break;
						}
						break;
					}
				}
				switch (m)
				{
				case 0:
					OverlayCharacter[m, n].gameObject.AddComponent<BorderTileBehavior>().SetDirection("CmdMoveW", m - 1, n - 1);
					break;
				case 81:
					OverlayCharacter[m, n].gameObject.AddComponent<BorderTileBehavior>().SetDirection("CmdMoveE", m - 1, n - 1);
					break;
				default:
					switch (n)
					{
					case 0:
						OverlayCharacter[m, n].gameObject.AddComponent<BorderTileBehavior>().SetDirection("CmdMoveN", m - 1, n - 1);
						break;
					case 26:
						OverlayCharacter[m, n].gameObject.AddComponent<BorderTileBehavior>().SetDirection("CmdMoveS", m - 1, n - 1);
						break;
					}
					break;
				}
				if (m == 0 && n == 0)
				{
					OverlayCharacter[m, n].GetComponent<BorderTileBehavior>().SetDirection("CmdMoveNW", m - 1, n - 1);
				}
				if (m == 81 && n == 0)
				{
					OverlayCharacter[m, n].GetComponent<BorderTileBehavior>().SetDirection("CmdMoveNE", m - 1, n - 1);
				}
				if (m == 0 && n == 26)
				{
					OverlayCharacter[m, n].GetComponent<BorderTileBehavior>().SetDirection("CmdMoveSW", m - 1, n - 1);
				}
				if (m == 81 && n == 26)
				{
					OverlayCharacter[m, n].GetComponent<BorderTileBehavior>().SetDirection("CmdMoveSE", m - 1, n - 1);
				}
				OverlayCharacter[m, n].gameObject.transform.parent = OverlayRoot.transform;
				OverlayCharacter[m, n].gameObject.transform.position = getTileCenter(m - 1, n - 1) + new Vector3(0f, 0f, -10f);
				OverlayCharacter[m, n].shader = SpriteManager.GetShaderMode(0);
			}
		}
		for (int num = 0; num < 256; num++)
		{
			CharInfos[num] = SpriteManager.GetTextureInfo("assets_content_textures_text_" + num + ".bmp");
		}
		UnityEngine.Debug.Log("Initilization complete...");
		bInitComplete = true;
	}

	public void OnGUI()
	{
		if (!XRLCore.bThreadFocus)
		{
			return;
		}
		capslock = UnityEngine.Event.current.capsLock;
		Keyboard._bAlt = Input.GetKey(UnityEngine.KeyCode.RightAlt) || Input.GetKey(UnityEngine.KeyCode.LeftAlt) || Instance.player.GetButton("Highlight");
		Keyboard._bCtrl = Input.GetKey(UnityEngine.KeyCode.RightControl) || Input.GetKey(UnityEngine.KeyCode.LeftControl) || Input.GetKey(UnityEngine.KeyCode.RightMeta) || Input.GetKey(UnityEngine.KeyCode.LeftMeta);
		Keyboard._bShift = Input.GetKey(UnityEngine.KeyCode.RightShift) || Input.GetKey(UnityEngine.KeyCode.LeftShift);
		if (!ModernUI && (CurrentGameView == "Popup:Text" || CurrentGameView == "Popup:AskString"))
		{
			pushKeyEvents(always: true);
			return;
		}
		if (ModernUI && !Options.ModernCharacterSheet)
		{
			bool flag = false;
			if (Instance._ActiveGameView == "Equipment")
			{
				flag = true;
			}
			if (Instance._ActiveGameView == "Inventory")
			{
				flag = true;
			}
			if (Instance._ActiveGameView == "Status")
			{
				flag = true;
			}
			if (Instance._ActiveGameView == "Factions")
			{
				flag = true;
			}
			if (Instance._ActiveGameView == "Quests")
			{
				flag = true;
			}
			if (Instance._ActiveGameView == "Journal")
			{
				flag = true;
			}
			if (Instance._ActiveGameView == "Tinkering")
			{
				flag = true;
			}
			if (Instance._ActiveGameView == "SkillsAndPowers")
			{
				flag = true;
			}
			if (flag)
			{
				pushKeyEvents();
				return;
			}
		}
		if (UIManager.instance == null || !UIManager.instance.AllowPassthroughInput())
		{
			return;
		}
		if (ModernUI && currentUIView != null)
		{
			if (currentUIView.UICanvas != null && currentUIView?.NavCategory?.Contains("*AllowLegacyInputPassthrough") != true)
			{
				return;
			}
			string currentGameView = CurrentGameView;
			if (currentGameView != null && currentGameView.StartsWith("ModernPopup"))
			{
				return;
			}
		}
		pushKeyEvents();
	}

	public static void pushKeyEvents(bool always = false)
	{
		capslock = UnityEngine.Event.current.capsLock;
		Keyboard._bAlt = Input.GetKey(UnityEngine.KeyCode.RightAlt) || Input.GetKey(UnityEngine.KeyCode.LeftAlt) || Instance.player.GetButton("Highlight");
		Keyboard._bCtrl = Input.GetKey(UnityEngine.KeyCode.RightControl) || Input.GetKey(UnityEngine.KeyCode.LeftControl) || Input.GetKey(UnityEngine.KeyCode.RightMeta) || Input.GetKey(UnityEngine.KeyCode.LeftMeta);
		Keyboard._bShift = Input.GetKey(UnityEngine.KeyCode.RightShift) || Input.GetKey(UnityEngine.KeyCode.LeftShift);
		if (!UnityEngine.Event.current.isKey || UnityEngine.Event.current.type != EventType.KeyDown || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.RightMeta || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.LeftMeta || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.LeftControl || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.RightControl || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.LeftAlt || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.RightAlt || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.LeftShift || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.RightShift || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.None)
		{
			return;
		}
		if (!Options.ModernUI && UnityEngine.Event.current.keyCode >= UnityEngine.KeyCode.A && UnityEngine.Event.current.keyCode <= UnityEngine.KeyCode.Z && Options.GetOption("OptionPushAZHotkeys") == "Yes")
		{
			always = true;
		}
		if (!ControlManager.isKeyMapped(UnityEngine.Event.current.keyCode, Keyboard._bAlt, Keyboard._bCtrl, Keyboard._bShift) | always)
		{
			if (UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.KeypadEnter)
			{
				UnityEngine.Event.current.keyCode = UnityEngine.KeyCode.Return;
			}
			if (UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.Return)
			{
				ControlManager.WaitForKeyup("Accept");
			}
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.Event.current), bAllowMap: true);
		}
	}

	private void EditorInit()
	{
	}

	private void OnApplicationFocus(bool focus)
	{
		XRLCore.bThreadFocus = focus;
		focused = focus;
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		focused = !pauseStatus;
	}

	public bool ColorMatch(Color C1, Color C2)
	{
		if (C1.r == C2.r && C1.g == C2.g)
		{
			return C1.b == C2.b;
		}
		return false;
	}

	public override void RegisterViews()
	{
		foreach (Type item in ModManager.GetTypesWithAttribute(typeof(UIView)))
		{
			string text = null;
			object[] customAttributes = item.GetCustomAttributes(typeof(UIView), inherit: false);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				UIView uIView = (UIView)customAttributes[i];
				text = text ?? uIView.UICanvas ?? uIView.ID;
				if (_ViewData.TryGetValue(uIView.ID, out var value))
				{
					if (!value.IsSame(uIView.AsGameManagerViewInfo()))
					{
						UnityEngine.Debug.LogError("Found a second UIView with ID " + uIView.ID + " in " + item.ToString() + " with different parameters from the last one.");
					}
				}
				else
				{
					_ViewData[uIView.ID] = uIView.AsGameManagerViewInfo();
				}
			}
		}
	}

	public void SetActiveView(string id)
	{
	}

	public override void OnStart()
	{
		RegisterViews();
		UIManager.showWindow("PassthroughDefault");
		gameObject.AddComponent<CombatJuiceManager>().gameManager = this;
	}

	public Vector3 GetCellCenter(int x, int y, int z = -10)
	{
		return new Vector3(40 * -tileWidth + x * tileWidth + 8, 12.5f * (float)tileHeight - (float)(y * tileHeight) - 12f, z);
	}

	public Vector3 CenterOnCell(int x, int y)
	{
		if (!NeedsToPan)
		{
			return GetCellCenter(x, y);
		}
		MainCameraLetterbox.SetPositionImmediately(new Vector3(40 * -tileWidth + x * tileWidth + 8, 12.5f * (float)tileHeight - (float)(y * tileHeight) - 12f, -10f));
		TargetCameraLocation = MainCameraLetterbox.DesiredPan;
		return new Vector3(TargetCameraLocation.x, TargetCameraLocation.y, 0f);
	}

	public void RefreshLayout(bool updateForceFullscreenIfSwapped = false)
	{
		if (MainCameraLetterbox == null || CurrentGameView == "MapEditor")
		{
			return;
		}
		ShouldForceFullscreen = TargetZoomFactor == -2f || AnyViewInStackForcefullscreenInModernOrLegacy();
		bool flag = MainCameraLetterbox.Pannable;
		if (CurrentGameView == null || !GetViewData(CurrentGameView).KeepCurrentFullscreen)
		{
			if (!ShouldForceFullscreen)
			{
				if (MainCameraLetterbox.DesiredZoomFactor != TargetZoomFactor || DockMovable == 3)
				{
					UpdatePreferredSidebarPosition();
				}
				if (!NeedsToPan)
				{
					flag = false;
					stageDock.maxWidth = stageRect.rect.width * 0.4f;
					Vector3 letterboxTotalArea = MainCameraLetterbox.GetLetterboxTotalArea();
					float num = stageDock.safeArea.rect.width - letterboxTotalArea.x;
					stageDock.maxWidth = Mathf.Max(stageDock.maxWidth, stageRect.rect.width - num);
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				ShouldForceFullscreen = true;
			}
		}
		if (!Options.ModernUI)
		{
			string currentGameView = CurrentGameView;
			if (currentGameView != null && currentGameView.StartsWith("PopupMessage"))
			{
				ShouldForceFullscreen = true;
				flag = false;
			}
		}
		if (!Options.ModernUI)
		{
			ShouldForceFullscreen = true;
			flag = false;
		}
		if (ShouldForceFullscreen != MainCameraLetterbox.ForcingFullscreen)
		{
			MainCameraLetterbox.UpdateDelay = 10;
		}
		if (!updateForceFullscreenIfSwapped && ShouldForceFullscreen != MainCameraLetterbox.ForcingFullscreen)
		{
			return;
		}
		if (ShouldForceFullscreen)
		{
			MainCameraLetterbox.SetTargetArea(stageRect);
			MainCameraLetterbox.ForcingFullscreen = true;
			MainCameraLetterbox.DesiredZoomFactor = 1f;
			return;
		}
		MainCameraLetterbox.Pannable = flag;
		if (flag)
		{
			MainCameraLetterbox.DesiredPan = TargetCameraLocation;
		}
		MainCameraLetterbox.SetTargetArea(stageDock.safeArea);
		MainCameraLetterbox.ForcingFullscreen = false;
		MainCameraLetterbox.DesiredZoomFactor = Math.Max(1f, TargetZoomFactor);
	}

	public void UpdateSelectedAbility()
	{
		if (_selectedAbility == -1)
		{
			return;
		}
		selectedAbilitybuilder.Length = 0;
		currentlySelectedAbility = AbilityAPI.GetAbility(selectedAbility);
		if (currentlySelectedAbility == null)
		{
			selectedAbilitybuilder.Append("<color=#666666>");
			selectedAbilitybuilder.Append(ControlManager.getCommandInputDescription("Previous Ability"));
			selectedAbilitybuilder.Append(ControlManager.getCommandInputDescription("Next Ability"));
			selectedAbilitybuilder.Append("</color>");
			selectedAbilitybuilder.Append(" <none>");
			selectedAbilityText.text = selectedAbilitybuilder.ToString();
			return;
		}
		selectedAbilitybuilder.Append("<color=#666666>");
		selectedAbilitybuilder.Append(ControlManager.getCommandInputDescription("Previous Ability"));
		selectedAbilitybuilder.Append(ControlManager.getCommandInputDescription("Next Ability"));
		selectedAbilitybuilder.Append("</color>");
		selectedAbilitybuilder.Append(" ");
		if (currentlySelectedAbility.Cooldown > 0 || !currentlySelectedAbility.Enabled)
		{
			selectedAbilitybuilder.Append("<color=#999999>");
		}
		else
		{
			selectedAbilitybuilder.Append("<color=#FFFFFF>");
		}
		selectedAbilitybuilder.Append("<color=#FFFF00>");
		selectedAbilitybuilder.Append(ControlManager.getCommandInputDescription("Use Ability"));
		selectedAbilitybuilder.Append("</color>");
		selectedAbilitybuilder.Append(" ");
		selectedAbilitybuilder.Append(currentlySelectedAbility.DisplayName);
		if (currentlySelectedAbility.Cooldown > 0)
		{
			selectedAbilitybuilder.Append(" [");
			selectedAbilitybuilder.Append(currentlySelectedAbility.Cooldown / 10 + 1);
			selectedAbilitybuilder.Append(" turns]");
		}
		selectedAbilitybuilder.Append("</color>");
		selectedAbilityText.text = selectedAbilitybuilder.ToString();
	}

	public string LongDirectionToShortDirection(string dir)
	{
		switch (dir)
		{
		case "North":
		case "N":
			return "N";
		case "South":
		case "S":
			return "S";
		case "East":
		case "E":
			return "E";
		case "West":
		case "W":
			return "W";
		case "Northeast":
		case "NE":
			return "NE";
		case "Northwest":
		case "NW":
			return "NW";
		case "Southeast":
		case "SE":
			return "SE";
		case "Southwest":
		case "SW":
			return "SW";
		default:
			return ".";
		}
	}

	public void SetActiveLayersForNavCategory(string navCategory, bool setCurrentCategory = true)
	{
		if (setCurrentCategory)
		{
			currentNavCategory = navCategory;
		}
		List<string> layers;
		if (CommandBindingManager.NavCategories.TryGetValue(navCategory, out var value))
		{
			layers = value.Layers;
		}
		else
		{
			MetricsManager.LogError("NavCategory '" + navCategory + "' not found, defaulting to Menu");
			layers = CommandBindingManager.NavCategories["Menu"].Layers;
		}
		ControlManager.EnableOnlyLayers(layers);
	}

	public void UpdateInput()
	{
		if (!XRLCore.bThreadFocus || _ActiveGameView == "MapEditor")
		{
			return;
		}
		if (currentNavDirectionDisplay != null && currentNavDirectionDisplay.text != "")
		{
			currentNavDirectionDisplay.text = "";
		}
		string text = GetViewData(CurrentGameView).NavCategory;
		if (CurrentGameView == "EmbarkBuilder")
		{
			text = "Chargen";
		}
		DebugNavCategory = text;
		if (text == null || text == "StringInput")
		{
			text = "Menu";
		}
		if (text == "Targeting" || text == "Looker")
		{
			if (ControlManager.isCommandDown("CmdFire"))
			{
				Keyboard.PushCommand("CmdFire");
			}
			if (ControlManager.isCommandDown("CmdThrow"))
			{
				Keyboard.PushCommand("CmdFire");
			}
			if (PickTargetWindow.currentMode == PickTargetWindow.TargetMode.PickDirection)
			{
				if (player.GetButtonDown("Accept"))
				{
					Keyboard.PushMouseEvent("Meta:Navigate" + currentNavDirection);
				}
				if (player.GetButtonDown("Fire"))
				{
					Keyboard.PushMouseEvent("Meta:Navigate" + currentNavDirection);
				}
				if (player.GetButtonDownRepeating("CmdMoveN", ignoreSkipframes: true))
				{
					Keyboard.PushMouseEvent("Meta:NavigateNorth");
				}
				if (player.GetButtonDownRepeating("CmdMoveS", ignoreSkipframes: true))
				{
					Keyboard.PushMouseEvent("Meta:NavigateSouth");
				}
				if (player.GetButtonDownRepeating("CmdMoveE", ignoreSkipframes: true))
				{
					Keyboard.PushMouseEvent("Meta:NavigateEast");
				}
				if (player.GetButtonDownRepeating("CmdMoveW", ignoreSkipframes: true))
				{
					Keyboard.PushMouseEvent("Meta:NavigateWest");
				}
				if (player.GetButtonDownRepeating("CmdMoveNW", ignoreSkipframes: true))
				{
					Keyboard.PushMouseEvent("Meta:NavigateNorthwest");
				}
				if (player.GetButtonDownRepeating("CmdMoveNE", ignoreSkipframes: true))
				{
					Keyboard.PushMouseEvent("Meta:NavigateNortheast");
				}
				if (player.GetButtonDownRepeating("CmdMoveSW", ignoreSkipframes: true))
				{
					Keyboard.PushMouseEvent("Meta:NavigateSouthwest");
				}
				if (player.GetButtonDownRepeating("CmdMoveSE", ignoreSkipframes: true))
				{
					Keyboard.PushMouseEvent("Meta:NavigateSoutheast");
				}
				currentNavDirection = ResolveMovementDirection("IndicateDirection");
				if (currentNavDirectionDisplay == null)
				{
					UnityEngine.GameObject gameObject = UnityEngine.GameObject.Find("CurrentNavDirection");
					if (gameObject != null)
					{
						currentNavDirectionDisplay = gameObject.GetComponent<UnityEngine.UI.Text>();
					}
				}
				playerTracker.setActiveDirection(currentNavDirection);
				if (currentNavDirectionDisplay != null)
				{
					if (currentNavDirection != null)
					{
						currentNavDirectionDisplay.text = currentNavDirection;
					}
					else
					{
						currentNavDirectionDisplay.text = "";
					}
				}
				if (player.GetButtonDownRepeating("Take A Step") || player.GetButtonDownRepeating("Wait"))
				{
					Keyboard.PushMouseEvent("Meta:Navigate" + currentNavDirection);
				}
			}
			else
			{
				if (text != "Looker")
				{
					string text2 = ResolveMovementDirection("IndicateDirection");
					if (text2 != null)
					{
						if (lookDelay == 0f && lookRepeat == 0f)
						{
							Keyboard.PushMouseEvent("Meta:Navigate" + text2);
						}
						lookRepeat -= UnityEngine.Time.deltaTime;
						if (lookDelay <= ControlManager.delaytime)
						{
							lookDelay += UnityEngine.Time.deltaTime;
						}
						if (lookRepeat <= 0f && lookDelay >= ControlManager.delaytime)
						{
							Keyboard.PushMouseEvent("Meta:Navigate" + text2);
							lookRepeat = ControlManager.repeattime;
						}
					}
					else
					{
						lookRepeat = 0f;
						lookDelay = 0f;
					}
				}
				currentNavDirection = null;
				playerTracker.setActiveDirection(currentNavDirection);
				if (currentNavDirectionDisplay != null)
				{
					if (currentNavDirection != null)
					{
						currentNavDirectionDisplay.text = currentNavDirection;
					}
					else
					{
						currentNavDirectionDisplay.text = "";
					}
				}
				if (player.GetButtonDownRepeating("Navigate Vertical") || player.GetNegativeButtonDownRepeating("Navigate Vertical"))
				{
					if (player.GetAxis("Navigate Vertical") > 0f)
					{
						Keyboard.PushMouseEvent("Meta:NavigateNorth");
					}
					if (player.GetAxis("Navigate Vertical") < 0f)
					{
						Keyboard.PushMouseEvent("Meta:NavigateSouth");
					}
				}
				else if (player.GetButtonDownRepeating("Navigate Horizontal") || player.GetNegativeButtonDownRepeating("Navigate Horizontal"))
				{
					if (player.GetAxis("Navigate Horizontal") > 0f)
					{
						Keyboard.PushMouseEvent("Meta:NavigateEast");
					}
					if (player.GetAxis("Navigate Horizontal") < 0f)
					{
						Keyboard.PushMouseEvent("Meta:NavigateWest");
					}
				}
				else
				{
					if (player.GetButtonDownRepeating("CmdMoveN", ignoreSkipframes: true))
					{
						Keyboard.PushMouseEvent("Meta:NavigateNorth");
					}
					if (player.GetButtonDownRepeating("CmdMoveS", ignoreSkipframes: true))
					{
						Keyboard.PushMouseEvent("Meta:NavigateSouth");
					}
					if (player.GetButtonDownRepeating("CmdMoveE", ignoreSkipframes: true))
					{
						Keyboard.PushMouseEvent("Meta:NavigateEast");
					}
					if (player.GetButtonDownRepeating("CmdMoveW", ignoreSkipframes: true))
					{
						Keyboard.PushMouseEvent("Meta:NavigateWest");
					}
					if (player.GetButtonDownRepeating("CmdMoveNW", ignoreSkipframes: true))
					{
						Keyboard.PushMouseEvent("Meta:NavigateNorthwest");
					}
					if (player.GetButtonDownRepeating("CmdMoveNE", ignoreSkipframes: true))
					{
						Keyboard.PushMouseEvent("Meta:NavigateNortheast");
					}
					if (player.GetButtonDownRepeating("CmdMoveSW", ignoreSkipframes: true))
					{
						Keyboard.PushMouseEvent("Meta:NavigateSouthwest");
					}
					if (player.GetButtonDownRepeating("CmdMoveSE", ignoreSkipframes: true))
					{
						Keyboard.PushMouseEvent("Meta:NavigateSoutheast");
					}
				}
				if (player.GetButtonDown("Take A Step"))
				{
					Keyboard.PushMouseEvent("Command:CmdFire");
				}
			}
		}
		if (ControlManager.SkipFrames > 0 || Instance.player.GetButton("Highlight"))
		{
			return;
		}
		if (The.Player?.ActivatedAbilities?.AbilityByGuid != null && CurrentGameView == "Stage" && CapabilityManager.AllowKeyboardHotkeys)
		{
			if (AbilityManager.PlayerAbilityLock.TryEnterReadLock(0))
			{
				try
				{
					foreach (ActivatedAbilityEntry playerAbility in AbilityManager.PlayerAbilities)
					{
						if (ControlManager.isCommandDown(playerAbility.Command))
						{
							playerAbility.TrySendCommandEventOnPlayer();
							return;
						}
					}
				}
				finally
				{
					AbilityManager.PlayerAbilityLock.ExitReadLock();
				}
			}
			for (int i = 0; i < AbilityBar.ABILITY_COMMANDS.Count; i++)
			{
				if (ControlManager.isCommandDown(AbilityBar.ABILITY_COMMANDS[i]) && AbilityBar.hotkeyAbilityCommands.TryGetValue(AbilityBar.ABILITY_COMMANDS[i], out var value))
				{
					value.TrySendCommandEventOnPlayer();
					return;
				}
			}
		}
		if (CurrentGameView == "Stage")
		{
			if (ControlManager.isCommandDown("CmdAbilityBar1"))
			{
				SingletonWindowBase<AbilityBar>.instance.MoveToPage(0);
			}
			if (ControlManager.isCommandDown("CmdAbilityBar2"))
			{
				SingletonWindowBase<AbilityBar>.instance.MoveToPage(1);
			}
			if (ControlManager.isCommandDown("CmdAbilityBar3"))
			{
				SingletonWindowBase<AbilityBar>.instance.MoveToPage(2);
			}
			if (ControlManager.isCommandDown("CmdAbilityBar4"))
			{
				SingletonWindowBase<AbilityBar>.instance.MoveToPage(3);
			}
			if (ControlManager.isCommandDown("CmdAbilityBar5"))
			{
				SingletonWindowBase<AbilityBar>.instance.MoveToPage(4);
			}
			if (ControlManager.isCommandDown("CmdAbilityNextPage"))
			{
				SingletonWindowBase<AbilityBar>.instance.MoveToNextPage();
			}
			if (ControlManager.isCommandDown("CmdAbilityPrevPage"))
			{
				SingletonWindowBase<AbilityBar>.instance.MoveToPreviousPage();
			}
		}
		for (int j = 0; j < CommandBindingManager.AutoRepeatInputActions.Count; j++)
		{
			if (ControlManager.isCommandDown(CommandBindingManager.AutoRepeatInputActions[j]))
			{
				Keyboard.PushCommand(CommandBindingManager.AutoRepeatInputActions[j]);
				return;
			}
		}
		if (text == "Adventure")
		{
			for (int k = 0; k < CommandBindingManager.AutoDownAdventureInputActions.Count; k++)
			{
				CommandBinding commandBinding = CommandBindingManager.AutoDownAdventureInputActions[k];
				if (ControlManager.isCommandDown(commandBinding.name))
				{
					Keyboard.PushCommand(commandBinding.name);
					return;
				}
			}
		}
		for (int l = 0; l < CommandBindingManager.AutoDownPassInputActions.Count; l++)
		{
			CommandBinding commandBinding2 = CommandBindingManager.AutoDownPassInputActions[l];
			if (ControlManager.isCommandDown(commandBinding2.name))
			{
				Keyboard.PushCommand(commandBinding2.name);
			}
		}
		for (int m = 0; m < CommandBindingManager.AutoDownInputActions.Count; m++)
		{
			CommandBinding commandBinding3 = CommandBindingManager.AutoDownInputActions[m];
			if (ControlManager.isCommandDown(commandBinding3.name))
			{
				Keyboard.PushCommand(commandBinding3.name);
				return;
			}
		}
		for (int n = 0; n < CommandBindingManager.AutoDownUIInputActions.Count; n++)
		{
			CommandBinding commandBinding4 = CommandBindingManager.AutoDownUIInputActions[n];
			if (ControlManager.isCommandDown(commandBinding4.name))
			{
				Keyboard.PushCommand(commandBinding4.name);
				return;
			}
		}
		if (ControlManager.IsLayerEnabled("Menus") && UIManager.instance.PassthroughOnTop())
		{
			if (player.GetButtonDown("Accept"))
			{
				Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Space));
			}
			if (player.GetButtonDown("Cancel"))
			{
				Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Escape));
			}
			if (Input.GetMouseButtonDown(1))
			{
				Keyboard.PushKey(UnityEngine.KeyCode.Escape);
			}
		}
		if (currentNavCategory != text)
		{
			SetActiveLayersForNavCategory(text);
		}
		else
		{
			if (!UIManager.instance.AllowPassthroughInput())
			{
				return;
			}
			if (currentNavCategory != null && Options.ModernUI && ControlManager.IsLayerEnabled("Conversation"))
			{
				if (player.GetButtonDown("Cancel"))
				{
					Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Escape));
				}
				if (player.GetButtonDown("Trade"))
				{
					Keyboard.PushKey(UnityEngine.KeyCode.Tab);
				}
				return;
			}
			if (currentUIView != null && !(currentUIView.NavCategory == "Menu") && !(currentUIView.UICanvas == "Stage") && !(currentUIView.UICanvas == "Popup:Text") && !(currentUIView.UICanvas == "Looker") && text == "StringInput")
			{
				if (player.GetButtonDown("Accept"))
				{
					Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Return));
				}
				if (player.GetButtonDown("Cancel"))
				{
					Keyboard.PushMouseEvent("Meta:Cancel");
				}
				return;
			}
			if (player.GetButtonDown("Accept") && CurrentGameView == "AbilityManager")
			{
				Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Space, ' '));
			}
			if (ControlManager.IsLayerEnabled("Menus"))
			{
				if (ControlManager.GetButtonDownRepeating("Page Up"))
				{
					Keyboard.PushKey(UnityEngine.KeyCode.PageUp);
				}
				if (ControlManager.GetButtonDownRepeating("Page Down"))
				{
					Keyboard.PushKey(UnityEngine.KeyCode.PageDown);
				}
				if (player.GetButtonDown("V Positive"))
				{
					Keyboard.PushKey(UnityEngine.KeyCode.KeypadPlus);
				}
				if (player.GetButtonDown("V Negative"))
				{
					Keyboard.PushKey(UnityEngine.KeyCode.KeypadMinus);
				}
			}
			if (ControlManager.IsLayerEnabled("Menus") && !ControlManager.IsLayerEnabled("Targeting") && !ControlManager.IsLayerEnabled("Looker"))
			{
				if (player.GetButtonDownRepeating("Navigate Vertical") || player.GetNegativeButtonDownRepeating("Navigate Vertical"))
				{
					if (player.GetAxis("Navigate Vertical") > 0f)
					{
						Keyboard.PushMouseEvent("Meta:NavigateNorth");
					}
					if (player.GetAxis("Navigate Vertical") < 0f)
					{
						Keyboard.PushMouseEvent("Meta:NavigateSouth");
					}
				}
				else if (player.GetButtonDownRepeating("Navigate Horizontal") || player.GetNegativeButtonDownRepeating("Navigate Horizontal"))
				{
					if (player.GetAxis("Navigate Horizontal") > 0f)
					{
						Keyboard.PushMouseEvent("Meta:NavigateEast");
					}
					if (player.GetAxis("Navigate Horizontal") < 0f)
					{
						Keyboard.PushMouseEvent("Meta:NavigateWest");
					}
				}
				if (ControlManager.isCommandDown("Navigate Up"))
				{
					Keyboard.PushKey(UnityEngine.KeyCode.Keypad8);
				}
				if (ControlManager.isCommandDown("Navigate Down"))
				{
					Keyboard.PushKey(UnityEngine.KeyCode.Keypad2);
				}
				if (ControlManager.isCommandDown("Navigate Left"))
				{
					Keyboard.PushKey(UnityEngine.KeyCode.Keypad4);
				}
				if (ControlManager.isCommandDown("Navigate Right"))
				{
					Keyboard.PushKey(UnityEngine.KeyCode.Keypad6);
				}
				if (ControlManager.GetButtonDownRepeating("Page Left"))
				{
					Keyboard.PushKey(UnityEngine.KeyCode.Keypad7);
				}
				if (ControlManager.GetButtonDownRepeating("Page Right"))
				{
					Keyboard.PushKey(UnityEngine.KeyCode.Keypad9);
				}
				if (player.GetButtonDown("New Map Pin"))
				{
					Keyboard.PushCommand("New Map Pin");
				}
			}
			if (ControlManager.IsLayerEnabled("Conversation") && player.GetButtonDown("Trade"))
			{
				Keyboard.PushKey(UnityEngine.KeyCode.Tab);
			}
			if (text == "Adventure")
			{
				DoNavigate("CmdMoveN", "CmdMoveN", "CmdAutoMoveN");
				DoNavigate("CmdMoveS", "CmdMoveS", "CmdAutoMoveS");
				DoNavigate("CmdMoveE", "CmdMoveE", "CmdAutoMoveE");
				DoNavigate("CmdMoveW", "CmdMoveW", "CmdAutoMoveW");
				DoNavigate("CmdMoveNE", "CmdMoveNE", "CmdAutoMoveNE");
				DoNavigate("CmdMoveNW", "CmdMoveNW", "CmdAutoMoveNW");
				DoNavigate("CmdMoveSE", "CmdMoveSE", "CmdAutoMoveSE");
				DoNavigate("CmdMoveSW", "CmdMoveSW", "CmdAutoMoveSW");
				if (player.GetButtonDown("CmdSystemMenu"))
				{
					Keyboard.PushCommand("CmdSystemMenu");
				}
				if (player.GetButtonDown("System"))
				{
					Keyboard.PushMouseEvent("Meta:System");
					ControlManager.ResetInput();
				}
				if (!Options.ModernUI)
				{
					if (player.GetButtonDown("Next Ability"))
					{
						selectedAbility++;
						if (selectedAbility >= AbilityAPI.abilityCount)
						{
							selectedAbility = 0;
						}
						UpdateSelectedAbility();
					}
					if (player.GetButtonDown("Previous Ability"))
					{
						selectedAbility--;
						if (selectedAbility < 0)
						{
							selectedAbility = AbilityAPI.abilityCount - 1;
						}
						UpdateSelectedAbility();
					}
					if (player.GetButtonDown("Use Ability"))
					{
						currentlySelectedAbility?.TrySendCommandEventOnPlayer();
					}
				}
				if (player.GetButtonDown("CmdMoveU"))
				{
					Keyboard.PushMouseEvent("Command:CmdMoveU");
				}
				if (player.GetButtonDown("CmdMoveD"))
				{
					Keyboard.PushMouseEvent("Command:CmdMoveD");
				}
				if (player.GetButtonDownRepeating("CmdAttackN"))
				{
					Keyboard.PushMouseEvent("Command:CmdAttackN");
				}
				if (player.GetButtonDownRepeating("CmdAttackS"))
				{
					Keyboard.PushMouseEvent("Command:CmdAttackS");
				}
				if (player.GetButtonDownRepeating("CmdAttackE"))
				{
					Keyboard.PushMouseEvent("Command:CmdAttackE");
				}
				if (player.GetButtonDownRepeating("CmdAttackW"))
				{
					Keyboard.PushMouseEvent("Command:CmdAttackW");
				}
				if (player.GetButtonDownRepeating("CmdAttackNW"))
				{
					Keyboard.PushMouseEvent("Command:CmdAttackNW");
				}
				if (player.GetButtonDownRepeating("CmdAttackNE"))
				{
					Keyboard.PushMouseEvent("Command:CmdAttackNE");
				}
				if (player.GetButtonDownRepeating("CmdAttackSW"))
				{
					Keyboard.PushMouseEvent("Command:CmdAttackSW");
				}
				if (player.GetButtonDownRepeating("CmdAttackSE"))
				{
					Keyboard.PushMouseEvent("Command:CmdAttackSE");
				}
				if (player.GetButtonDownRepeating("CmdAttackU"))
				{
					Keyboard.PushMouseEvent("Command:CmdAttackU");
				}
				if (player.GetButtonDownRepeating("CmdAttackD"))
				{
					Keyboard.PushMouseEvent("Command:CmdAttackD");
				}
				if (player.GetButtonDown("CmdMoveToPointOfInterest"))
				{
					Keyboard.PushMouseEvent("Command:CmdMoveToPointOfInterest");
				}
				if (player.GetButtonDown("Character Sheet"))
				{
					Keyboard.PushMouseEvent("Meta:CharacterSheet");
				}
				if (player.GetButtonDown("CmdZoomIn"))
				{
					ZoomIn();
				}
				if (player.GetButtonDown("CmdZoomOut"))
				{
					ZoomOut();
				}
				if (player.GetButtonDown("CmdWish"))
				{
					Keyboard.PushMouseEvent("Command:CmdWish");
				}
				if (player.GetButtonDown("CmdQuests"))
				{
					Keyboard.PushMouseEvent("Command:CmdQuests");
				}
				if (player.GetButtonDown("CmdSave"))
				{
					Keyboard.PushMouseEvent("Command:CmdSave");
				}
				if (player.GetButtonDown("CmdLoad"))
				{
					Keyboard.PushMouseEvent("Command:CmdLoad");
				}
				currentNavDirection = ResolveMovementDirection("IndicateDirection");
				playerTracker.setActiveDirection(currentNavDirection);
				if (player.GetButtonDownRepeating("ForceAttack") && !(currentNavDirection == ".") && currentNavDirection != null)
				{
					Keyboard.PushMouseEvent("Command:CmdAttack" + LongDirectionToShortDirection(currentNavDirection));
				}
				int num = ControlManager.isCommandDownValue("Take A Step");
				if (num > 0)
				{
					if (currentNavDirection == "." || currentNavDirection == null)
					{
						if (TakeAStepFilterMode <= 1)
						{
							TakeAStepFilterMode = 1;
							Keyboard.PushCommand("CmdWait");
						}
					}
					else
					{
						TakeAStepFilterMode = 2;
						if (player.GetButton("GamepadAlt"))
						{
							Keyboard.PushMouseEvent("Command:CmdAttack" + LongDirectionToShortDirection(currentNavDirection));
						}
						else
						{
							if (num == 1)
							{
								Keyboard.PushCommand("CmdMove" + currentNavDirection);
							}
							if (num == 2)
							{
								Keyboard.PushCommand("CmdAutoMove" + currentNavDirection);
							}
						}
					}
				}
				if (TakeAStepFilterMode > 0 && !ControlManager.isCommandPressed("Take A Step"))
				{
					TakeAStepFilterMode = 0;
					ControlManager.ConsumeAllInput("Take A Step");
				}
				if (player.GetButtonDown("CmdGetFrom"))
				{
					if (currentNavDirection == "." || currentNavDirection == null)
					{
						Keyboard.PushCommand("CmdGetFrom");
					}
					else
					{
						Keyboard.PushCommand("CmdGetFrom" + LongDirectionToShortDirection(currentNavDirection));
					}
				}
				if (player.GetButtonDown("CmdUse"))
				{
					if (currentNavDirection == "." || currentNavDirection == null)
					{
						Keyboard.PushCommand("CmdUse");
					}
					else
					{
						Keyboard.PushCommand("CmdUse" + LongDirectionToShortDirection(currentNavDirection));
					}
				}
				if (ResolveMovementDirection("LookDirection") != null)
				{
					Look.bLocked = false;
					Keyboard.PushMouseEvent("Command:CmdLook");
				}
			}
			else if (!ControlManager.IsLayerEnabled("Targeting"))
			{
				playerTracker.setActiveDirection(null);
			}
			if (!(text == "Looker"))
			{
				return;
			}
			if (player.GetButtonDown("Accept"))
			{
				Keyboard.PushCommand("Interact");
			}
			if (player.GetButtonDown("GamepadAlt"))
			{
				Look.bLocked = !Look.bLocked;
			}
			if (ControlManager.GetButtonDown("CmdWalk"))
			{
				Keyboard.PushCommand("CmdWalk");
			}
			string text3 = ResolveMovementDirection("LookDirection");
			if (text3 != null)
			{
				if (lookDelay == 0f && lookRepeat == 0f)
				{
					Keyboard.PushMouseEvent("Meta:Navigate" + text3);
				}
				lookRepeat -= UnityEngine.Time.deltaTime;
				lookDelay += UnityEngine.Time.deltaTime;
				if (lookRepeat <= 0f && lookDelay > ControlManager.delaytime)
				{
					Keyboard.PushMouseEvent("Meta:Navigate" + text3);
					lookRepeat = ControlManager.repeattime;
				}
			}
			else
			{
				lookRepeat = 0f;
				lookDelay = 0f;
			}
		}
		static void DoNavigate(string command, string pressCommand, string repeatCommand)
		{
			switch (ControlManager.isCommandDownValue(command))
			{
			case 1:
				Keyboard.PushCommand(pressCommand);
				break;
			case 2:
				Keyboard.PushCommand(repeatCommand);
				break;
			}
		}
	}

	public static string ResolveMovementDirection(string Axis)
	{
		return ControlManager.ResolveAxisDirection(Axis);
	}

	public static bool IsOnUIContext()
	{
		return SynchronizationContext.Current == The.UiContext;
	}

	public static bool IsOnGameContext()
	{
		return SynchronizationContext.Current == The.GameContext;
	}

	public void ShowStage()
	{
		if (Options.ModernUI)
		{
			if (!UIManager.getWindow("Stage").canvas.enabled || UIManager.getWindow("Stage").canvasGroup.alpha == 0f)
			{
				UIManager.getWindow("Stage").Show();
				UIManager.getWindow("PlayerStatusBar").Show();
				UIManager.getWindow("AbilityBar").Show();
				UIManager.getWindow("MessageLog").Show();
				UIManager.getWindow<MessageLogWindow>("MessageLog").Show();
				UIManager.getWindow<NearbyItemsWindow>("NearbyItems").ShowIfEnabled();
				UIManager.getWindow<MinimapWindow>("Minimap").ShowIfEnabled();
			}
		}
		else
		{
			HideStage();
		}
	}

	public void HideStage()
	{
		UIManager.getWindow("Stage").Hide();
		UIManager.getWindow("PlayerStatusBar").Hide();
		UIManager.getWindow("AbilityBar").Hide();
		UIManager.getWindow("MessageLog").Hide();
		UIManager.getWindow("NearbyItems").Hide();
		UIManager.getWindow("Minimap").Hide();
	}

	public override void OnUpdate()
	{
		dirtyFocused = Application.isFocused;
		if (The.Game != null && Application.isFocused)
		{
			The.Game.realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
			The.Game.frameDelta = UnityEngine.Time.deltaTime;
		}
		ControlManager.OnUpdate();
		UpdateCountFromChangedToActiveView++;
		frame++;
		if (consoleTileAreaHandler.activeSelf != !Options.ModernUI)
		{
			consoleTileAreaHandler.SetActive(!Options.ModernUI);
		}
		if (fadeToBlack)
		{
			if (originalBrightness == float.MinValue)
			{
				originalBrightness = UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().brightness;
			}
			if (fadeToBlackStage == 0)
			{
				fadingToBlackTimer += UnityEngine.Time.deltaTime;
				if (fadingToBlackTimer < 2f)
				{
					UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().brightness = Mathf.Lerp(originalBrightness, -100f, fadingToBlackTimer / 2f);
				}
				else if (fadingToBlackTimer < 3f)
				{
					UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().brightness = -100f;
					fadeToBlackStage = 1;
				}
			}
			if (fadeToBlackStage == 3)
			{
				fadingToBlackTimer += UnityEngine.Time.deltaTime;
				if (fadingToBlackTimer < 5f)
				{
					UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().brightness = Mathf.Lerp(-100f, originalBrightness, (fadingToBlackTimer - 3f) / 2f);
				}
				else
				{
					UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().brightness = originalBrightness;
					fadeToBlack = false;
					fadingToBlackTimer = 0f;
					originalBrightness = float.MinValue;
					gameQueue.queueTask(() =>
					{
						Thread.Sleep(2000);
						XRLCore.Core.Game.FinishQuestStep("Tomb of the Eaters", "Ascend the Tomb and Cross into Brightsheol");
					});
				}
			}
		}
		if (mouseDisable > 0f)
		{
			mouseDisable -= UnityEngine.Time.deltaTime;
		}
		UpdateInput();
		if (Input.GetKeyDown(UnityEngine.KeyCode.F8) && (Input.GetKey(UnityEngine.KeyCode.LeftControl) || Input.GetKey(UnityEngine.KeyCode.RightControl)))
		{
			TutorialManager.ToggleHide();
		}
		uiQueue.executeTasks();
		if (pauseUpdates)
		{
			if (ControlManager.GetButtonDown("Cancel"))
			{
				pauseUpdates = false;
			}
			return;
		}
		if (XRLCore.bStarted && lastFullscreen != Screen.fullScreen)
		{
			if (Screen.fullScreen != Options.DisplayFullscreen)
			{
				if (Options.HasOption("OptionDisplayFullscreen"))
				{
					Options.SetOption("OptionDisplayFullscreen", Screen.fullScreen ? "Yes" : "No");
				}
				else
				{
					Options.SetOption("OptionDisplayFullscreen", "Yes");
					Screen.fullScreen = true;
				}
			}
			lastFullscreen = Screen.fullScreen;
		}
		if (Spacefolding)
		{
			MainCamera.gameObject.GetComponent<CC_RadialBlur>().enabled = true;
			_spacefoldingT += UnityEngine.Time.deltaTime;
			MainCamera.gameObject.GetComponent<CC_RadialBlur>().amount = 1f - Easing.BounceEaseInOut(_spacefoldingT / 2f);
			if (_spacefoldingT > 2f)
			{
				MainCamera.gameObject.GetComponent<CC_RadialBlur>().enabled = false;
				_spacefolding = false;
			}
		}
		if (_Spin > 0f)
		{
			_CurrentSpin += UnityEngine.Time.deltaTime;
			MainCamera.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 360f * (Math.Min(_Spin, _CurrentSpin) - _Spin) / _Spin);
			if (_CurrentSpin >= _Spin)
			{
				MainCamera.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
				_Spin = 0f;
			}
		}
		if (Fuzzing)
		{
			CC_AnalogTV component = MainCamera.gameObject.GetComponent<CC_AnalogTV>();
			component.enabled = true;
			component.noiseIntensity = (0.5f - _fuzzingT) * 2f;
			_fuzzingT += UnityEngine.Time.deltaTime;
			if ((double)_fuzzingT > 0.5)
			{
				component.enabled = Options.DisplayScanlines;
				component.noiseIntensity = 0.045f;
				Fuzzing = false;
			}
		}
		if (_ThinWorldDistoring > 0f && !Options.DisableFullscreenWarpEffects)
		{
			CC_AnalogTV component2 = MainCamera.gameObject.GetComponent<CC_AnalogTV>();
			_ThinWorldDistoring -= UnityEngine.Time.deltaTime;
			if (_ThinWorldDistoring <= 0f)
			{
				component2.distortion = 0f;
				component2.cubicDistortion = 0f;
				component2.scale = 1f;
				component2.noiseIntensity = 0.045f;
			}
			else
			{
				component2.distortion = Mathf.Lerp(-2f, 0f, 1f - _ThinWorldDistoring / MAX_THIN_WORLD_DISTORTING);
				component2.cubicDistortion = Mathf.Lerp(-2f, 0f, 1f - _ThinWorldDistoring / MAX_THIN_WORLD_DISTORTING);
				component2.scale = 1f - _ThinWorldDistoring / MAX_THIN_WORLD_DISTORTING;
				component2.noiseIntensity = Mathf.Lerp(1f, 0.045f, 1f - _ThinWorldDistoring / MAX_THIN_WORLD_DISTORTING);
			}
		}
		if (_StaticEffecting > 0 && !Options.DisableFullscreenWarpEffects)
		{
			CC_Pixelate component3 = MainCamera.gameObject.GetComponent<CC_Pixelate>();
			if ((double)_StaticEffectTime > (double)_StaticEffecting * 0.25)
			{
				_StaticEffecting = 0;
				_StaticEffectTime = 0f;
				component3.enabled = false;
			}
			else
			{
				component3.enabled = true;
				_StaticEffectTime += UnityEngine.Time.deltaTime;
				_NextStaticEffectGlitch -= UnityEngine.Time.deltaTime;
				if (_NextStaticEffectGlitch <= 0f)
				{
					_NextStaticEffectGlitch = (float)Stat.RandomCosmetic(50, 200) / 1000f;
					component3.scale = (float)Stat.RandomCosmetic(140, 200) + _StaticEffectTime * 200f;
				}
			}
		}
		if (!Greyscaling && GreyscaleLevel > 0)
		{
			Greyscaling = true;
			MainCamera.gameObject.GetComponent<CC_Grayscale>().amount = 0f;
			MainCamera.gameObject.GetComponent<CC_Grayscale>().enabled = true;
		}
		else if (Greyscaling && GreyscaleLevel <= 0)
		{
			if (MainCamera.gameObject.GetComponent<CC_Grayscale>().amount > 0f)
			{
				MainCamera.gameObject.GetComponent<CC_Grayscale>().amount -= UnityEngine.Time.deltaTime * 0.5f;
			}
			else
			{
				Greyscaling = false;
				MainCamera.gameObject.GetComponent<CC_Grayscale>().amount = 0f;
				MainCamera.gameObject.GetComponent<CC_Grayscale>().enabled = false;
			}
		}
		if (Greyscaling && MainCamera.gameObject.GetComponent<CC_Grayscale>().amount < 1f)
		{
			MainCamera.gameObject.GetComponent<CC_Grayscale>().amount += UnityEngine.Time.deltaTime * 0.2f;
		}
		if (Options.DisableFullscreenWarpEffects)
		{
			if (Hallucinating)
			{
				Hallucinating = false;
				MainCamera.gameObject.GetComponent<CC_Wiggle>().enabled = false;
			}
		}
		else if (!Hallucinating && FungalVisionary.VisionLevel > 0)
		{
			Hallucinating = true;
			MainCamera.gameObject.GetComponent<CC_Wiggle>().enabled = true;
		}
		else if (Hallucinating && FungalVisionary.VisionLevel <= 0)
		{
			Hallucinating = false;
			MainCamera.gameObject.GetComponent<CC_Wiggle>().enabled = false;
		}
		if (Globals.RenderMode != RenderModeType.Text)
		{
			_ = Options.DisableImposters;
		}
		_ = Hallucinating;
		if (!XRLCore.bThreadFocus)
		{
			SoundManager.Update();
			Thread.Sleep(250);
			return;
		}
		if (bFirstUpdate)
		{
			Instance = this;
			MainCamera = GameObjectFind("Main Camera");
			MainCanvas = GameObjectFind("UI Manager").GetComponent<Canvas>();
			bFirstUpdate = false;
		}
		ClipboardHelper.UpdateFromMainThread();
		SoundManager.Update();
		if (Screen.width != LastWidth || Screen.height != LastHeight)
		{
			Sidebar.bOverlayUpdated = true;
			LastWidth = Screen.width;
			LastHeight = Screen.height;
			UpdatePreferredSidebarPosition();
			if (QualitySettings.vSyncCount > 0)
			{
				int vSyncCount = QualitySettings.vSyncCount;
				QualitySettings.vSyncCount = 0;
				QualitySettings.vSyncCount = vSyncCount;
			}
		}
		if (_overlayOptionsUpdated)
		{
			lock (GameViewStack)
			{
				_overlayOptionsUpdated = false;
				UpdateView();
			}
		}
		RefreshLayout(updateForceFullscreenIfSwapped: true);
		if (!string.IsNullOrEmpty(Social.TweetThis))
		{
			string tweetThis = Social.TweetThis;
			Social.TweetThis = null;
			Application.OpenURL("http://www.twitter.com/intent/tweet?text=" + Uri.EscapeDataString(tweetThis) + "&via=cavesofqud&hashtags=cavesofqud");
		}
		_ = bEditorMode;
		if (!bInitStarted)
		{
			StartGameThread();
			bInitStarted = true;
		}
		if (TV.noiseIntensity != TargetIntensity)
		{
			TV.noiseIntensity = Mathf.MoveTowards(TV.noiseIntensity, TargetIntensity, UnityEngine.Time.deltaTime * 1f);
		}
		if (!bInitComplete)
		{
			return;
		}
		if ((XRLCore.bStarted && XRLCore.CoreThread.ThreadState == System.Threading.ThreadState.Stopped) || !XRLCore.bStarted)
		{
			UnityEngine.Debug.Log("Exiting main thread due to stopped game thread." + XRLCore.CoreThread.ThreadState);
			if (platform.SupportsQuit)
			{
				platform.Quit();
			}
		}
		if (MetricsManager.ManagerObject == null && Globals.EnableMetrics)
		{
			MetricsManager.Init();
		}
		MetricsManager.Update();
		if (TextConsole.BufferUpdated)
		{
			lock (GameViewStack)
			{
				lock (TextConsole.BufferCS)
				{
					if (_ActiveGameView != CurrentGameView || bViewUpdated)
					{
						ControlManager.ResetInput();
						bViewUpdated = false;
						UpdateView();
					}
					if (!FadeSplash)
					{
						UnityEngine.GameObject gameObject = UnityEngine.GameObject.Find("Splash");
						if (gameObject != null)
						{
							FadeSplash = true;
							LeanTween.alpha(gameObject, 0f, 2f);
							gameObject.AddComponent<Temporary>().Duration = 2f;
							gameObject.GetComponent<Temporary>().BeforeDestroy = () =>
							{
								GameObjectFind("Splash").Destroy();
							};
						}
					}
					while (TextConsole.bufferExtras.Count > 0)
					{
						ProcessBufferExtra(TextConsole.bufferExtras.Dequeue());
					}
					if (TextConsole.CurrentBuffer.focusPosition != Point2D.invalid)
					{
						SetPlayerCell(TextConsole.CurrentBuffer.focusPosition);
					}
					RequestedBackdrops.Clear();
					int num = 0;
					for (int num2 = 0; num2 < 80; num2++)
					{
						for (int num3 = 0; num3 < 25; num3++)
						{
							ConsoleChar consoleChar = TextConsole.CurrentBuffer.Buffer[num2, num3];
							ex3DSprite2 ex3DSprite3 = ConsoleCharacter[num2, num3];
							Transform transform = ConsoleCharacterTransform[num2, num3];
							consoleChar.BeforeRender(num2, num3, ex3DSprite3, TextConsole.CurrentBuffer);
							if (!(consoleChar.WantsBackdrop == "backdropBlack") && !RequestedBackdrops.Contains(consoleChar.WantsBackdrop) && !string.IsNullOrEmpty(consoleChar.WantsBackdrop))
							{
								RequestedBackdrops.Add(consoleChar.WantsBackdrop);
								if (!BackdropsByName.TryGetValue(consoleChar.WantsBackdrop, out var value))
								{
									value = UnityEngine.Object.Instantiate(Resources.Load(consoleChar.WantsBackdrop) as UnityEngine.GameObject);
									value.SetActive(value: true);
									value.name = consoleChar.WantsBackdrop;
									value.transform.position = new Vector3(0f, 0f, value.transform.position.z);
									BackdropsByName.Add(consoleChar.WantsBackdrop, value);
									Backdrops.Add(value);
								}
								if (!value.activeSelf)
								{
									value.SetActive(value: true);
								}
							}
							if (!consoleChar.HFlip && !consoleChar.VFlip)
							{
								if (transform.localScale != Vector3.one)
								{
									transform.localScale = Vector3.one;
									BoxCollider component4 = ex3DSprite3.GetComponent<BoxCollider>();
									component4.size = new Vector3(Math.Abs(component4.size.x), Math.Abs(component4.size.y), Math.Abs(component4.size.z));
								}
							}
							else if (consoleChar.HFlip && consoleChar.VFlip)
							{
								if (transform.localScale != -Vector3.one)
								{
									transform.localScale = -Vector3.one;
									BoxCollider component5 = ex3DSprite3.GetComponent<BoxCollider>();
									component5.size = new Vector3(0f - Math.Abs(component5.size.x), 0f - Math.Abs(component5.size.y), 0f - Math.Abs(component5.size.z));
								}
							}
							else if (consoleChar.HFlip)
							{
								if (transform.localScale != new Vector3(-1f, 1f, 1f))
								{
									transform.localScale = new Vector3(-1f, 1f, 1f);
									BoxCollider component6 = ex3DSprite3.GetComponent<BoxCollider>();
									component6.size = new Vector3(0f - Math.Abs(component6.size.x), Math.Abs(component6.size.y), Math.Abs(component6.size.z));
								}
							}
							else if (consoleChar.VFlip && transform.localScale != new Vector3(1f, -1f, 1f))
							{
								transform.localScale = new Vector3(1f, -1f, 1f);
								BoxCollider component7 = ex3DSprite3.GetComponent<BoxCollider>();
								component7.size = new Vector3(Math.Abs(component7.size.x), 0f - Math.Abs(component7.size.y), Math.Abs(component7.size.z));
							}
							if (consoleChar.Char == '\0')
							{
								string tile = consoleChar.Tile;
								Color foreground = consoleChar.Foreground;
								Color color = consoleChar.Background;
								Color detail = consoleChar.Detail;
								if (consoleChar.BackdropBleedthrough)
								{
									color = Color.clear;
								}
								if (consoleChar.WantsBackdrop == "backdropBlack")
								{
									color = backdropBlackColor;
								}
								bool flag = CurrentTile[num2, num3] != tile;
								if (flag || !ColorMatch(ex3DSprite3.color, foreground) || !ColorMatch(ex3DSprite3.backcolor, color) || !ColorMatch(ex3DSprite3.detailcolor, detail))
								{
									if (flag)
									{
										exTextureInfo textureInfo = SpriteManager.GetTextureInfo(tile);
										if (textureInfo == null)
										{
											UnityEngine.Debug.LogError("Invalid TextureID: " + tile);
											CurrentTile[num2, num3] = tile;
											ConsoleCharacter[num2, num3].textureInfo = null;
										}
										else
										{
											CurrentTile[num2, num3] = tile;
											ex3DSprite3.textureInfo = textureInfo;
											if (textureInfo.ShaderMode != CurrentShadermode[num2, num3])
											{
												ex3DSprite3.shader = SpriteManager.GetShaderMode(textureInfo.ShaderMode);
												CurrentShadermode[num2, num3] = textureInfo.ShaderMode;
											}
										}
									}
									num++;
									ex3DSprite3.color = foreground;
									ex3DSprite3.detailcolor = detail;
									ex3DSprite3.backcolor = color;
								}
							}
							else
							{
								char c = consoleChar.Char;
								if (c < '\0' || c > 'ÿ')
								{
									c = ' ';
								}
								exTextureInfo exTextureInfo2 = CharInfos[(uint)c];
								Color foreground2 = consoleChar.Foreground;
								Color background = consoleChar.Background;
								if (exTextureInfo2 != null && exTextureInfo2.ShaderMode != CurrentShadermode[num2, num3])
								{
									ex3DSprite3.shader = SpriteManager.GetShaderMode(exTextureInfo2.ShaderMode);
									CurrentShadermode[num2, num3] = exTextureInfo2.ShaderMode;
								}
								if (ex3DSprite3.textureInfo != exTextureInfo2 || !ColorMatch(ex3DSprite3.backcolor, foreground2) || !ColorMatch(ex3DSprite3.detailcolor, foreground2) || !ColorMatch(ex3DSprite3.color, background))
								{
									num++;
									CurrentTile[num2, num3] = null;
									ex3DSprite3.textureInfo = exTextureInfo2;
									ex3DSprite3.backcolor = foreground2;
									ex3DSprite3.detailcolor = foreground2;
									ex3DSprite3.color = background;
								}
							}
							consoleChar.AfterRender(num2, num3, ex3DSprite3, TextConsole.CurrentBuffer);
						}
					}
					for (int num4 = 0; num4 < Backdrops.Count; num4++)
					{
						if (!RequestedBackdrops.Contains(Backdrops[num4].name))
						{
							Backdrops[num4].SetActive(value: false);
						}
					}
					UpdateSelectedAbility();
					TextConsole.BufferUpdated = false;
					if (LastViewTag != TextConsole.CurrentBuffer.ViewTag)
					{
						MainCameraLetterbox.UpdateDelay = 0;
					}
					LastViewTag = TextConsole.CurrentBuffer.ViewTag;
					for (int num5 = 0; num5 < 82; num5++)
					{
						for (int num6 = 0; num6 < 27; num6++)
						{
							if (num5 == 0 || num5 == 81 || num6 == 0 || num6 == 26)
							{
								OverlayCharacter[num5, num6].color = new Color(0.05f, 0.2f, 0.2f);
								OverlayCharacter[num5, num6].detailcolor = new Color(0.05f, 0.2f, 0.2f);
								OverlayCharacter[num5, num6].backcolor = new Color(0.05f, 0.23f, 0.23f, 1f);
							}
						}
					}
				}
			}
		}
		if (ShouldForceFullscreen)
		{
			HideStage();
		}
		else if (AnyViewInStackIsStage())
		{
			ShowStage();
		}
		else
		{
			HideStage();
		}
		if (AnyViewInStackForcefullscreen() && !CombatJuiceManager.NoPause)
		{
			MainCameraLetterbox.tilesWide = 80f;
			MainCameraLetterbox.tilesHigh = 25f;
			if (OverlayRoot.activeInHierarchy)
			{
				OverlayRoot.SetActive(value: false);
			}
			CombatJuiceManager.pause();
		}
		else
		{
			if (OverlayRootForceoff)
			{
				if (OverlayRoot.activeInHierarchy)
				{
					OverlayRoot.SetActive(value: false);
				}
			}
			else if (!NeedsArrowBorder || (!Options.DisplayMousableZoneTransitionBorder && !TutorialManager.IsActive))
			{
				MainCameraLetterbox.tilesWide = 80f;
				MainCameraLetterbox.tilesHigh = 25f;
				if (OverlayRoot.activeInHierarchy)
				{
					OverlayRoot.SetActive(value: false);
				}
			}
			else
			{
				MainCameraLetterbox.tilesWide = 82f;
				MainCameraLetterbox.tilesHigh = 27f;
				if (!OverlayRoot.activeInHierarchy)
				{
					OverlayRoot.SetActive(value: true);
				}
			}
			CombatJuiceManager.update();
		}
		PlatformManager.Update();
		AchievementManager.Update();
		if (MainCamera != null && MainCameraLetterbox.CurrentPosition != lastCameraPosition)
		{
			lastCameraPosition = MainCameraLetterbox.CurrentPosition;
			UpdatePreferredSidebarPosition();
		}
		if (Instance.DockMovable == 1 && MessageLogWindow.Shown)
		{
			MainCameraLetterbox.TileAreaAlignment = LetterboxCamera.TileAreaAlignmentType.Left;
		}
		else if (Instance.DockMovable == 2 && MessageLogWindow.Shown)
		{
			MainCameraLetterbox.TileAreaAlignment = LetterboxCamera.TileAreaAlignmentType.Right;
		}
		else
		{
			MainCameraLetterbox.TileAreaAlignment = LetterboxCamera.TileAreaAlignmentType.Center;
		}
		MainCameraLetterbox?.OnUpdate();
	}

	public bool StringBuilderContentsEquals(StringBuilder sb1, StringBuilder sb2, int maxcheck = 800)
	{
		if (sb1.Length != sb2.Length)
		{
			return false;
		}
		for (int i = 0; i < sb1.Length && i < maxcheck; i++)
		{
			if (sb1[i] != sb2[i])
			{
				return false;
			}
		}
		return true;
	}

	public static void Restart()
	{
		if (!IsOnUIContext())
		{
			Instance.uiQueue.queueTask(Restart);
			return;
		}
		using (Loading.StartTask("Restarting application..."))
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
			for (int i = 1; i < commandLineArgs.Length - 1; i++)
			{
				utf16ValueStringBuilder.Append('"');
				utf16ValueStringBuilder.Append(commandLineArgs[i]);
				utf16ValueStringBuilder.Append("\" ");
			}
			if (commandLineArgs.Length > 1)
			{
				utf16ValueStringBuilder.Append('"');
				utf16ValueStringBuilder.Append(commandLineArgs[^1]);
				utf16ValueStringBuilder.Append('"');
			}
			ProcessStartInfo startInfo = Process.GetCurrentProcess().StartInfo;
			startInfo.FileName = commandLineArgs[0];
			if (utf16ValueStringBuilder.Length > 0)
			{
				startInfo.Arguments = utf16ValueStringBuilder.ToString();
			}
			Process.Start(startInfo);
			Instance.Quit();
		}
	}
}
