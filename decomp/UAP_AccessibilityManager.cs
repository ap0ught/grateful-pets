using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[AddComponentMenu("Accessibility/Core/UAP Manager")]
public class UAP_AccessibilityManager : MonoBehaviour
{
	public delegate void OnPauseToggleCallbackFunc();

	public delegate void OnTapEvent();

	public delegate void OnSwipeEvent();

	public delegate void OnAccessibilityModeChanged(bool enabled);

	public enum ESDirection
	{
		EUp,
		EDown,
		ELeft,
		ERight
	}

	public static string PluginVersion = "1.1.1";

	public static float PluginVersionAsFloat = 1.11f;

	public bool m_DefaultState;

	public bool m_AutoTurnOnIfScreenReaderDetected = true;

	public bool m_RecheckAutoEnablingOnResume;

	public bool m_SaveEnabledState;

	public bool m_HandleUI = true;

	private bool m_BlockInput;

	public bool m_HandleMagicGestures = true;

	public bool m_ExploreByTouch = true;

	private float m_ExploreByTouchDelay = 0.75f;

	public bool m_ReadDisabledInteractables = true;

	public bool m_CyclicMenus;

	public bool m_AllowBuiltInVirtualKeyboard = true;

	public bool m_DebugOutput;

	private float m_HintDelay = 0.6f;

	private float m_DisabledDelay = 0.2f;

	private float m_ValueDelay = 0.25f;

	private float m_TypeDelay = 1f;

	public bool m_WindowsUseMouseSwipes;

	private Canvas m_CanvasRoot;

	public bool m_EditorOverride;

	public bool m_EditorEnabledState;

	[Header("WebGL")]
	public bool m_WebGLTTS = true;

	public string m_GoogleTTSAPIKey = "";

	[Header("Windows")]
	public bool m_WindowsTTS = true;

	public int m_WindowsTTSVolume = 100;

	public bool m_WindowsUseKeys = true;

	private KeyCode m_NextElementKey = KeyCode.DownArrow;

	private KeyCode m_PreviousElementKey = KeyCode.UpArrow;

	private KeyCode m_NextContainerKey = KeyCode.RightArrow;

	private KeyCode m_PreviousContainerKey = KeyCode.LeftArrow;

	private bool m_UseTabAndShiftTabForContainerJumping = true;

	private KeyCode m_InteractKey = KeyCode.Return;

	private KeyCode m_SliderIncrementKey = KeyCode.UpArrow;

	private KeyCode m_SliderDecrementKey = KeyCode.DownArrow;

	private KeyCode m_DropDownPreviousKey = KeyCode.UpArrow;

	private KeyCode m_DropDownNextKey = KeyCode.DownArrow;

	private KeyCode m_AbortKey = KeyCode.Escape;

	private KeyCode m_DownKey = KeyCode.DownArrow;

	private KeyCode m_UpKey = KeyCode.UpArrow;

	private KeyCode m_RightKey = KeyCode.RightArrow;

	private KeyCode m_LeftKey = KeyCode.LeftArrow;

	[Header("Android")]
	public bool m_AndroidTTS = true;

	public bool m_AndroidUseUpAndDownForElements;

	[Header("iOS")]
	public bool m_iOSTTS = true;

	[Header("Mac OS")]
	public bool m_MacOSTTS = true;

	[Header("Sound Effects")]
	public AudioClip m_UINavigationClick;

	public AudioClip m_UIInteract;

	public AudioClip m_UIFocusEnter;

	public AudioClip m_UIFocusLeave;

	public AudioClip m_UIBoundsReached;

	public AudioClip m_UIPopUpOpen;

	public AudioClip m_UIPopUpClose;

	[Header("Types")]
	private AudioClip m_DisabledAsAudio;

	private AudioClip m_ButtonAsAudio;

	private AudioClip m_ToggleAsAudio;

	private AudioClip m_SliderAsAudio;

	private AudioClip m_TextEditAsAudio;

	private AudioClip m_DropDownAsAudio;

	[Header("Localization")]
	private static string m_CurrentLanguage = "English";

	private static Dictionary<string, string> m_CurrentLocalizationTable = null;

	[Header("Other Resources")]
	public AudioSource m_AudioPlayer;

	public AudioSource m_SFXPlayer;

	public RectTransform m_Frame;

	public GameObject m_FrameTemplate;

	public GameObject m_TouchBlocker;

	public Text m_DebugOutputLabel;

	public bool m_AllowVoiceOverGlobal = true;

	public bool m_DetectVoiceOverAtRuntime = true;

	private UAP_AudioQueue m_AudioQueue;

	private static UAP_AccessibilityManager instance = null;

	private static bool isDestroyed = false;

	private static bool m_IsInitialized = false;

	private static bool m_IsEnabled = false;

	private static bool m_Paused = false;

	private List<AccessibleUIGroupRoot> m_ActiveContainers = new List<AccessibleUIGroupRoot>();

	private List<List<AccessibleUIGroupRoot>> m_SuspendedContainers = new List<List<AccessibleUIGroupRoot>>();

	private List<int> m_SuspendedActiveContainerIndex = new List<int>();

	private AccessibleUIGroupRoot.Accessible_UIElement m_CurrentItem;

	private AccessibleUIGroupRoot.Accessible_UIElement m_PreviousItem;

	private static List<AccessibleUIGroupRoot> m_ContainersToActivate = new List<AccessibleUIGroupRoot>();

	private int m_ActiveContainerIndex = -1;

	private bool m_ReadItemNextUpdate;

	private bool m_CurrentElementHasSoleFocus;

	private int m_LastUpdateTouchCount;

	private OnPauseToggleCallbackFunc m_OnPauseToggleCallbacks;

	private OnTapEvent m_OnTwoFingerSingleTapCallbacks;

	private OnTapEvent m_OnThreeFingerSingleTapCallbacks;

	private OnTapEvent m_OnThreeFingerDoubleTapCallbacks;

	private OnTapEvent m_OnTwoFingerSwipeUpHandler;

	private OnTapEvent m_OnTwoFingerSwipeDownHandler;

	private OnTapEvent m_OnTwoFingerSwipeLeftHandler;

	private OnTapEvent m_OnTwoFingerSwipeRightHandler;

	private OnAccessibilityModeChanged m_OnAccessibilityModeChanged;

	private OnTapEvent m_OnBackCallbacks;

	private bool m_SwipeActive;

	private int m_SwipeTouchCount;

	private bool m_SwipeWaitForLift;

	private Vector2 m_SwipeStartPos;

	private Vector2 m_SwipeCurrPos;

	private float m_SwipeDeltaTime;

	private float m_DoubleTap_LastTapTime = -1f;

	private const float m_DoubleTapTime = 0.2f;

	private bool m_DoubleTapFoundThisFrame;

	private float m_MagicTap_LastTapTime = -1f;

	private int m_MagicTap_TouchCountHelper;

	private bool m_WaitingForMagicTap;

	private int m_TripleTap_Count;

	private float m_TripleTap_LastTapTime = -1f;

	private int m_TripleTap_TouchCountHelper;

	private bool m_WaitingForThreeFingerTap;

	private bool m_ExploreByTouch_IsActive;

	private float m_ExploreByTouch_WaitTimer = -1f;

	private Vector3 m_ExploreByTouch_StartPosition = new Vector3(0f, 0f, 0f);

	private float m_ExploreByTouch_SingleTapWaitTimer = -1f;

	private Vector3 m_ExploreByTouch_SingleTapStartPosition = new Vector3(0f, 0f, 0f);

	private bool m_ContinuousReading;

	private bool m_ContinuousReading_WaitInputClear;

	private static bool sIsEuropeanLanguage = false;

	private bool m_TouchExplore_Active;

	private float m_TouchExplore_CheckVelocityTimer = -1f;

	private float m_TouchExplore_WaitForDoubleTapToExpireTimer = -1f;

	private Vector3 m_TouchExplore_CheckStartPosition = Vector3.one;

	private const float m_TouchExplore_CheckDuration = 0.15f;

	private float m_ScrubPhaseMaxDuration = 1f;

	private float m_ScrubScreenFractionMinswipeWidth = 0.1f;

	private float m_ScrubPhaseTimeoutTimer = -1f;

	private bool m_ScrubWaitForLift;

	private int m_ScrubPhaseIndex;

	private Vector3 m_ScrubPhaseStartPoint;

	private Vector3 m_ScrubPhaseLastPoint;

	public bool WindowsUseExploreByTouch => m_WindowsUseMouseSwipes;

	public static string GoogleTTSAPIKey
	{
		get
		{
			if (instance == null)
			{
				return "";
			}
			return instance.m_GoogleTTSAPIKey;
		}
	}

	private UAP_AccessibilityManager()
	{
		m_ActiveContainers.Clear();
	}

	private void Awake()
	{
		if (instance != this && instance != null)
		{
			Log("Found another UAP Accessibility Manager in the scene. Destroying this one.");
			UnityEngine.Object.DestroyImmediate(gameObject);
		}
		else
		{
			instance = this;
		}
	}

	private void Start()
	{
		Initialize();
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			isDestroyed = true;
		}
	}

	private static void Initialize()
	{
		if (m_IsInitialized)
		{
			return;
		}
		m_IsInitialized = true;
		if (instance == null)
		{
			instance = (UnityEngine.Object.Instantiate(Resources.Load("Accessibility Manager")) as GameObject).GetComponent<UAP_AccessibilityManager>();
		}
		if (instance.m_CanvasRoot == null)
		{
			instance.m_CanvasRoot = instance.transform.GetChild(0).GetComponent<Canvas>();
		}
		if (instance.m_AudioPlayer == null)
		{
			instance.m_AudioPlayer = instance.m_CanvasRoot.gameObject.GetComponent<AudioSource>();
			if (instance.m_AudioPlayer == null || instance.m_AudioPlayer == instance.m_SFXPlayer)
			{
				instance.m_AudioPlayer = instance.m_CanvasRoot.gameObject.AddComponent<AudioSource>();
			}
		}
		if (instance.m_SFXPlayer == null)
		{
			instance.m_SFXPlayer = instance.m_CanvasRoot.gameObject.AddComponent<AudioSource>();
		}
		if (instance.m_AudioQueue == null)
		{
			GameObject gameObject = new GameObject("Audio Queue");
			instance.m_AudioQueue = gameObject.AddComponent<UAP_AudioQueue>();
			instance.m_AudioQueue.Initialize();
			gameObject.transform.SetParent(instance.transform, worldPositionStays: false);
		}
		if (PlayerPrefs.HasKey("UAP_Language"))
		{
			m_CurrentLanguage = PlayerPrefs.GetString("UAP_Language", Application.systemLanguage.ToString());
		}
		else
		{
			m_CurrentLanguage = Application.systemLanguage.ToString();
		}
		instance.LoadLocalization();
		InitNGUI();
		HideElementFrame();
		m_IsEnabled = ShouldAutoEnable();
		instance.EnableTouchBlocker(m_IsEnabled);
		SavePluginEnabledState();
		_ = m_IsEnabled;
		Log("Initializing UAP Accessibility Manager");
	}

	private void LoadLocalization()
	{
		if (m_CurrentLocalizationTable == null)
		{
			m_CurrentLocalizationTable = new Dictionary<string, string>();
		}
		else
		{
			m_CurrentLocalizationTable.Clear();
		}
		TextAsset textAsset = Resources.Load("UAP_InternalLocalization") as TextAsset;
		if (textAsset == null)
		{
			Debug.LogError("[Accessibility] Localization table 'UAP_InternalLocalization.txt' not found.");
			return;
		}
		string[] array = textAsset.text.Split('\r');
		if (array.Length < 2)
		{
			array = textAsset.text.Split('\n');
			if (array.Length < 2)
			{
				Debug.LogError("[Accessibility] Localization table empty or invalid");
				return;
			}
		}
		string[] array2 = array[0].Split('\t');
		if (array2.Length < 2)
		{
			Debug.LogError("[Accessibility] Localization table invalid");
			return;
		}
		int num = -1;
		for (int i = 1; i < array2.Length; i++)
		{
			if (m_CurrentLanguage.ToLower().CompareTo(array2[i].ToLower()) == 0)
			{
				num = i;
			}
		}
		if (num < 0)
		{
			num = 1;
			Debug.LogWarning("[Accessibility] Current language '" + m_CurrentLanguage + "' is not supported. Defaulting to language " + array2[num]);
		}
		for (int j = 1; j < array.Length; j++)
		{
			string[] array3 = array[j].Split('\t');
			string text = array3[0].Trim();
			if (!string.IsNullOrEmpty(text) && array3.Length > num)
			{
				m_CurrentLocalizationTable.Add(text, array3[num]);
			}
		}
		Log("Localization table loaded successfully. Entries: " + m_CurrentLocalizationTable.Count.ToString("N0") + " Current Language:" + m_CurrentLanguage);
	}

	public static void SetLanguage(string language)
	{
		if (language.ToLower().CompareTo(m_CurrentLanguage.ToLower()) != 0)
		{
			m_CurrentLanguage = language;
			PlayerPrefs.SetString("UAP_Language", language);
			PlayerPrefs.Save();
			if (instance != null)
			{
				instance.LoadLocalization();
			}
		}
		DetectEuropeanLanguage();
	}

	private static void InitNGUI()
	{
		HideElementFrame();
		instance.EnableTouchBlocker(m_IsEnabled);
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode sceneLoadMode)
	{
		Log("Level loaded, clearing everything.");
		InitNGUI();
		m_ActiveContainers.Clear();
		m_SuspendedContainers.Clear();
		m_SuspendedActiveContainerIndex.Clear();
		m_AudioQueue.Stop();
		m_CurrentItem = null;
	}

	private static void CreateNGUIItemFrame()
	{
	}

	public static bool ShouldUseBuiltInKeyboard()
	{
		if (instance == null)
		{
			return false;
		}
		if (!instance.m_AllowBuiltInVirtualKeyboard)
		{
			return false;
		}
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return true;
		}
		if (!Application.isMobilePlatform)
		{
			return false;
		}
		return UAP_VirtualKeyboard.SupportsSystemLanguage();
	}

	private static bool ShouldAutoEnable()
	{
		bool result = instance.m_DefaultState;
		int num;
		if (!instance.m_SaveEnabledState)
		{
			num = 0;
		}
		else
		{
			num = (PlayerPrefs.HasKey("UAP_Enabled_State") ? 1 : 0);
			if (num != 0)
			{
				result = PlayerPrefs.GetInt("UAP_Enabled_State", 0) == 1;
			}
		}
		if (num == 0)
		{
			_ = instance.m_AutoTurnOnIfScreenReaderDetected;
		}
		return result;
	}

	private static void SavePluginEnabledState()
	{
		PlayerPrefs.SetInt("UAP_Enabled_State", m_IsEnabled ? 1 : 0);
		PlayerPrefs.Save();
	}

	private static void ReadItem(AccessibleUIGroupRoot.Accessible_UIElement element, bool quickOnly = false)
	{
		Initialize();
		if (!m_IsEnabled || m_Paused)
		{
			return;
		}
		if (instance.m_AudioPlayer.isPlaying)
		{
			instance.m_AudioPlayer.Stop();
		}
		if (element == null)
		{
			return;
		}
		UpdateElementFrame(ref element);
		SpeakElement_Text(ref element);
		instance.ReadDisabledState();
		instance.ReadValue();
		if (element.ReadType())
		{
			if (instance.m_ContinuousReading)
			{
				instance.SayPause(instance.m_TypeDelay * 0.5f);
			}
			else
			{
				instance.SayPause(instance.m_TypeDelay);
			}
			instance.ReadType();
		}
		if (!quickOnly)
		{
			instance.SayPause(instance.m_HintDelay);
			instance.ReadHint();
		}
	}

	private static void UpdateElementFrame(ref AccessibleUIGroupRoot.Accessible_UIElement element)
	{
		if (instance.m_Frame == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(instance.m_FrameTemplate, instance.m_TouchBlocker.transform);
			instance.m_Frame = gameObject.transform as RectTransform;
		}
		HideElementFrame();
		if (element.m_Object.Is3DElement())
		{
			UAP_BaseElement_3D uAP_BaseElement_3D = element.m_Object as UAP_BaseElement_3D;
			Camera camera = ((uAP_BaseElement_3D.m_CameraRenderingThisObject != null) ? uAP_BaseElement_3D.m_CameraRenderingThisObject : Camera.main);
			GameObject gameObject2 = (element.m_Object.m_UseTargetForOutline ? element.m_Object.GetTargetGameObject() : element.m_Object.gameObject);
			Vector2 vector = camera.WorldToScreenPoint(gameObject2.transform.position);
			instance.m_Frame.gameObject.SetActive(value: true);
			instance.m_Frame.transform.SetParent(instance.m_TouchBlocker.transform);
			instance.m_Frame.position = vector;
			instance.m_Frame.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, uAP_BaseElement_3D.GetPixelHeight());
			instance.m_Frame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, uAP_BaseElement_3D.GetPixelWidth());
			return;
		}
		RectTransform elementRect = GetElementRect(ref element);
		if (!(elementRect != null))
		{
			return;
		}
		instance.m_Frame.gameObject.SetActive(value: true);
		if (!(GetOwningCanvas(ref element) == null))
		{
			if (instance.m_Frame.transform.parent != elementRect.transform)
			{
				instance.m_Frame.transform.SetParent(elementRect.transform, worldPositionStays: false);
				(instance.m_Frame.transform as RectTransform).anchoredPosition3D = Vector3.zero;
				instance.m_Frame.transform.localScale = Vector3.one;
			}
			instance.m_Frame.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, elementRect.rect.height);
			instance.m_Frame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, elementRect.rect.width);
		}
	}

	private static void HideElementFrame()
	{
		if (instance.m_Frame != null && instance.m_Frame.gameObject != null)
		{
			instance.m_Frame.gameObject.SetActive(value: false);
		}
	}

	private static Canvas GetOwningCanvas(ref AccessibleUIGroupRoot.Accessible_UIElement element)
	{
		return element.m_Object.gameObject.GetComponentInParent<Canvas>();
	}

	private static RectTransform GetElementRect(ref AccessibleUIGroupRoot.Accessible_UIElement element)
	{
		GameObject gameObject = (element.m_Object.m_UseTargetForOutline ? element.m_Object.GetTargetGameObject() : element.m_Object.gameObject);
		RectTransform component = gameObject.GetComponent<RectTransform>();
		if (component != null)
		{
			bool flag = !gameObject.GetComponentInParent<UAP_ScrollControl>();
			if (element.m_Object.m_IsInsideScrollView & flag)
			{
				ScrollRect componentInParent = gameObject.GetComponentInParent<ScrollRect>();
				if (componentInParent != null)
				{
					bool flag2 = true;
					if (componentInParent.viewport == null)
					{
						Transform transform = componentInParent.transform.Find("Viewport");
						componentInParent.viewport = ((transform != null) ? ((RectTransform)transform) : null);
						if (componentInParent.viewport == null)
						{
							componentInParent.viewport = (RectTransform)componentInParent.transform.GetChild(0);
						}
						if (componentInParent.viewport == componentInParent.content)
						{
							componentInParent.viewport = (RectTransform)componentInParent.transform;
						}
					}
					RectTransform rectTransform = (RectTransform)componentInParent.viewport.transform;
					Vector3[] array = new Vector3[4];
					rectTransform.GetWorldCorners(array);
					Vector3[] array2 = new Vector3[4];
					component.GetWorldCorners(array2);
					bool flag3 = array2[0].x < array[0].x || array2[2].x > array[2].x;
					bool flag4 = array2[0].y < array[0].y || array2[2].y > array[2].y;
					if (!componentInParent.vertical)
					{
						flag4 = false;
					}
					if (!componentInParent.horizontal)
					{
						flag3 = false;
					}
					if (flag3 | flag4)
					{
						flag2 = false;
					}
					if (!flag2)
					{
						Vector3[] array3 = new Vector3[4];
						componentInParent.content.GetWorldCorners(array3);
						if (componentInParent.horizontal & flag3)
						{
							if (array3[2].x == 0f)
							{
								array3[2].x = 0.01f;
							}
							float num = array[2].x - array[0].x;
							float num2 = array3[2].x - array3[0].x;
							float num3 = array2[0].x - array3[0].x;
							float num4 = num2 - num;
							if (num3 >= num4)
							{
								componentInParent.horizontalNormalizedPosition = 1f;
							}
							else
							{
								float horizontalNormalizedPosition = num3 / num4;
								componentInParent.horizontalNormalizedPosition = horizontalNormalizedPosition;
							}
						}
						if (componentInParent.vertical & flag4)
						{
							if (array3[2].y == 0f)
							{
								array3[2].y = 0.01f;
							}
							float num5 = array[2].y - array[0].y;
							float num6 = array3[2].y - array3[0].y;
							float num7 = array2[0].y - array3[0].y;
							float num8 = num6 - num5;
							if (num7 >= num8)
							{
								componentInParent.verticalNormalizedPosition = 1f;
							}
							else
							{
								float verticalNormalizedPosition = num7 / num8;
								componentInParent.verticalNormalizedPosition = verticalNormalizedPosition;
							}
						}
					}
				}
			}
		}
		return component;
	}

	private bool IsPositionOverElement(Vector2 fingerPos, AccessibleUIGroupRoot.Accessible_UIElement element)
	{
		if (element.m_Object.Is3DElement())
		{
			UAP_BaseElement_3D uAP_BaseElement_3D = element.m_Object as UAP_BaseElement_3D;
			Camera camera = ((uAP_BaseElement_3D.m_CameraRenderingThisObject != null) ? uAP_BaseElement_3D.m_CameraRenderingThisObject : Camera.main);
			GameObject gameObject = (element.m_Object.m_UseTargetForOutline ? element.m_Object.GetTargetGameObject() : element.m_Object.gameObject);
			Vector2 vector = camera.WorldToScreenPoint(gameObject.transform.position);
			float pixelWidth = uAP_BaseElement_3D.GetPixelWidth();
			float pixelHeight = uAP_BaseElement_3D.GetPixelHeight();
			bool flag = true;
			Vector2 vector2 = vector - fingerPos;
			Vector2 vector3 = new Vector2(0f, 0f);
			vector3.x = pixelWidth * 0.5f;
			vector3.y = pixelHeight * 0.5f;
			if (vector3.x < Mathf.Abs(vector2.x))
			{
				flag = false;
			}
			if (vector3.y < Mathf.Abs(vector2.y))
			{
				flag = false;
			}
			if (flag)
			{
				return true;
			}
		}
		RectTransform elementRect = GetElementRect(ref element);
		if (elementRect != null)
		{
			Canvas componentInParent = element.m_Object.GetComponentInParent<Canvas>();
			if (componentInParent.renderMode != RenderMode.ScreenSpaceOverlay)
			{
				Vector3[] array = new Vector3[4];
				elementRect.GetWorldCorners(array);
				Camera worldCamera = componentInParent.worldCamera;
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = worldCamera.WorldToScreenPoint(array[i]);
				}
				if (fingerPos.x >= array[0].x && fingerPos.x <= array[2].x && fingerPos.y >= array[3].y && fingerPos.y <= array[1].y)
				{
					return true;
				}
			}
			else
			{
				CanvasScaler componentInParent2 = element.m_Object.GetComponentInParent<CanvasScaler>();
				float num = Screen.width;
				float num2 = Screen.height;
				if (componentInParent2 != null && componentInParent2.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
				{
					num2 = componentInParent2.referenceResolution.y;
					num = componentInParent2.referenceResolution.x;
				}
				float num3 = (float)Screen.width / num;
				float num4 = (float)Screen.height / num2;
				Vector2 vector4 = elementRect.position;
				float width = elementRect.rect.width;
				float height = elementRect.rect.height;
				width *= num3;
				height *= num4;
				vector4.x -= (elementRect.pivot.x - 0.5f) * width;
				vector4.y -= (elementRect.pivot.y - 0.5f) * height;
				bool flag2 = true;
				Vector2 vector5 = vector4 - fingerPos;
				Vector2 vector6 = new Vector2(0f, 0f);
				vector6.x = width * 0.5f;
				vector6.y = height * 0.5f;
				if (vector6.x < Mathf.Abs(vector5.x))
				{
					flag2 = false;
				}
				if (vector6.y < Mathf.Abs(vector5.y))
				{
					flag2 = false;
				}
				if (flag2)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void SayAudio(AudioClip clip, string altText, UAP_AudioQueue.EAudioType type, bool allowVoiceOver, UAP_AudioQueue.EInterrupt interrupts)
	{
		SayAudio(clip, altText, type, allowVoiceOver, null, interrupts);
	}

	private void SayAudio(AudioClip clip, string altText, UAP_AudioQueue.EAudioType type, bool allowVoiceOver, UAP_AudioQueue.UAP_GenericCallback callbackOnDone = null, UAP_AudioQueue.EInterrupt interrupts = UAP_AudioQueue.EInterrupt.None)
	{
		if (!(clip == null) || (altText.Length >= 1 && altText != null))
		{
			if (clip != null)
			{
				m_AudioQueue.QueueAudio(clip, type, callbackOnDone, interrupts);
			}
			else
			{
				m_AudioQueue.QueueAudio(altText, type, allowVoiceOver, callbackOnDone, interrupts);
			}
		}
	}

	private void SayPause(float durationInSec)
	{
		m_AudioQueue.QueuePause(durationInSec);
	}

	private void ReadDisabledState()
	{
		if (m_CurrentItem != null && (m_CurrentItem.m_Type == AccessibleUIGroupRoot.EUIElement.EButton || m_CurrentItem.m_Type == AccessibleUIGroupRoot.EUIElement.ESlider || m_CurrentItem.m_Type == AccessibleUIGroupRoot.EUIElement.EToggle || m_CurrentItem.m_Type == AccessibleUIGroupRoot.EUIElement.ETextEdit || m_CurrentItem.m_Type == AccessibleUIGroupRoot.EUIElement.EDropDown) && !m_CurrentItem.m_Object.IsInteractable())
		{
			SayPause(m_DisabledDelay);
			SayAudio(m_DisabledAsAudio, Localize_Internal("ElementDisabled"), UAP_AudioQueue.EAudioType.Element_Hint, m_CurrentItem.m_Object.m_AllowVoiceOver);
		}
	}

	private void ReadType()
	{
		if (m_CurrentItem != null)
		{
			switch (m_CurrentItem.m_Type)
			{
			case AccessibleUIGroupRoot.EUIElement.EButton:
				SayAudio(m_ButtonAsAudio, Localize_Internal("Element_Button"), UAP_AudioQueue.EAudioType.Element_Type, m_CurrentItem.m_Object.m_AllowVoiceOver);
				break;
			case AccessibleUIGroupRoot.EUIElement.ESlider:
				SayAudio(m_SliderAsAudio, Localize_Internal("Element_Slider"), UAP_AudioQueue.EAudioType.Element_Type, m_CurrentItem.m_Object.m_AllowVoiceOver);
				break;
			case AccessibleUIGroupRoot.EUIElement.EToggle:
				SayAudio(m_ToggleAsAudio, Localize_Internal("Element_Toggle"), UAP_AudioQueue.EAudioType.Element_Type, m_CurrentItem.m_Object.m_AllowVoiceOver);
				break;
			case AccessibleUIGroupRoot.EUIElement.ETextEdit:
				SayAudio(m_TextEditAsAudio, Localize_Internal("Element_TextEdit"), UAP_AudioQueue.EAudioType.Element_Type, m_CurrentItem.m_Object.m_AllowVoiceOver);
				break;
			case AccessibleUIGroupRoot.EUIElement.EDropDown:
				SayAudio(m_DropDownAsAudio, Localize_Internal("Element_Dropdown"), UAP_AudioQueue.EAudioType.Element_Type, m_CurrentItem.m_Object.m_AllowVoiceOver);
				break;
			case AccessibleUIGroupRoot.EUIElement.ELabel:
				break;
			}
		}
	}

	private void ReadValue(bool allowPause = true, bool interrupt = false)
	{
		if (m_CurrentItem == null)
		{
			return;
		}
		AudioClip currentValueAsAudio = m_CurrentItem.m_Object.GetCurrentValueAsAudio();
		string currentValueAsText = m_CurrentItem.m_Object.GetCurrentValueAsText();
		if (currentValueAsAudio != null || (currentValueAsText != null && currentValueAsText.Length > 0))
		{
			if (allowPause)
			{
				SayPause(instance.m_ValueDelay);
			}
			SayAudio(currentValueAsAudio, currentValueAsText, UAP_AudioQueue.EAudioType.Element_Text, m_CurrentItem.m_Object.m_AllowVoiceOver, interrupt ? UAP_AudioQueue.EInterrupt.Elements : UAP_AudioQueue.EInterrupt.None);
		}
	}

	private void ReadHint()
	{
		if (m_CurrentItem == null)
		{
			return;
		}
		if (m_CurrentItem.m_Object.m_CustomHint)
		{
			SayAudio(m_CurrentItem.m_Object.m_HintAsAudio, m_CurrentItem.m_Object.GetCustomHint(), UAP_AudioQueue.EAudioType.Element_Hint, m_CurrentItem.m_Object.m_AllowVoiceOver);
			return;
		}
		switch (m_CurrentItem.m_Type)
		{
		case AccessibleUIGroupRoot.EUIElement.EButton:
			if (Application.isMobilePlatform)
			{
				SayAudio(null, Localize_Internal("Mobile_HintButton"), UAP_AudioQueue.EAudioType.Element_Hint, m_CurrentItem.m_Object.m_AllowVoiceOver);
			}
			else
			{
				SayAudio(null, Localize_Internal("Desktop_HintButton"), UAP_AudioQueue.EAudioType.Element_Hint, m_CurrentItem.m_Object.m_AllowVoiceOver);
			}
			break;
		case AccessibleUIGroupRoot.EUIElement.ETextEdit:
			if (Application.isMobilePlatform)
			{
				SayAudio(null, Localize_Internal("Mobile_HintTextEdit"), UAP_AudioQueue.EAudioType.Element_Hint, m_CurrentItem.m_Object.m_AllowVoiceOver);
			}
			else
			{
				SayAudio(null, Localize_Internal("Desktop_HintTextEdit"), UAP_AudioQueue.EAudioType.Element_Hint, m_CurrentItem.m_Object.m_AllowVoiceOver);
			}
			break;
		case AccessibleUIGroupRoot.EUIElement.EToggle:
			if (Application.isMobilePlatform)
			{
				SayAudio(null, Localize_Internal("Mobile_HintToggle"), UAP_AudioQueue.EAudioType.Element_Hint, m_CurrentItem.m_Object.m_AllowVoiceOver);
			}
			else
			{
				SayAudio(null, Localize_Internal("Desktop_HintToggle"), UAP_AudioQueue.EAudioType.Element_Hint, m_CurrentItem.m_Object.m_AllowVoiceOver);
			}
			break;
		case AccessibleUIGroupRoot.EUIElement.EDropDown:
			if (Application.isMobilePlatform)
			{
				SayAudio(null, Localize_Internal("Mobile_HintDropdown"), UAP_AudioQueue.EAudioType.Element_Hint, m_CurrentItem.m_Object.m_AllowVoiceOver);
			}
			else
			{
				SayAudio(null, Localize_Internal("Desktop_HintDropdown"), UAP_AudioQueue.EAudioType.Element_Hint, m_CurrentItem.m_Object.m_AllowVoiceOver);
			}
			break;
		case AccessibleUIGroupRoot.EUIElement.ESlider:
			if (Application.isMobilePlatform)
			{
				SayAudio(null, Localize_Internal("Mobile_HintSlider"), UAP_AudioQueue.EAudioType.Element_Hint, m_CurrentItem.m_Object.m_AllowVoiceOver);
			}
			else
			{
				SayAudio(null, Localize_Internal("Desktop_HintSlider"), UAP_AudioQueue.EAudioType.Element_Hint, m_CurrentItem.m_Object.m_AllowVoiceOver);
			}
			break;
		case AccessibleUIGroupRoot.EUIElement.ELabel:
			break;
		}
	}

	private static void SpeakElement_Text(ref AccessibleUIGroupRoot.Accessible_UIElement element)
	{
		instance.SayAudio(element.m_Object.m_TextAsAudio, element.m_Object.GetTextToRead(), UAP_AudioQueue.EAudioType.Element_Hint, element.AllowsVoiceOver(), UAP_AudioQueue.EInterrupt.Elements);
	}

	public static void PauseAccessibility(bool pause, bool forceRepeatCurrentItem = false)
	{
		if (isDestroyed)
		{
			return;
		}
		Initialize();
		StopContinuousReading();
		if (m_Paused != pause)
		{
			m_Paused = pause;
			instance.m_AudioQueue.Stop();
			instance.m_AudioPlayer.Stop();
			instance.EnableTouchBlocker(!pause);
			if (pause)
			{
				HideElementFrame();
			}
			if (!pause & forceRepeatCurrentItem)
			{
				instance.m_PreviousItem = null;
				instance.m_CurrentItem = null;
				instance.UpdateCurrentItem(UAP_BaseElement.EHighlightSource.Internal);
				ReadItem(instance.m_CurrentItem);
			}
			else
			{
				instance.m_PreviousItem = instance.m_CurrentItem;
			}
		}
	}

	private static void StopContinuousReading()
	{
		Initialize();
		instance.m_ContinuousReading = false;
		instance.m_ContinuousReading_WaitInputClear = false;
	}

	private void EnableTouchBlocker(bool enable)
	{
		if (!enable || !m_IsEnabled)
		{
			m_TouchBlocker.SetActive(value: false);
		}
		else if (!m_Paused)
		{
			m_TouchBlocker.SetActive(m_HandleUI);
		}
	}

	public static void BlockInput(bool block, bool stopSpeakingOnBlock = true)
	{
		Initialize();
		StopContinuousReading();
		instance.m_BlockInput = block;
		if (block & stopSpeakingOnBlock)
		{
			StopSpeaking();
		}
	}

	public static void ElementRemoved(AccessibleUIGroupRoot.Accessible_UIElement element)
	{
		if (!(instance == null) && !isDestroyed && element == instance.m_CurrentItem && element != null)
		{
			if (instance.m_PreviousItem == element)
			{
				instance.m_PreviousItem = null;
			}
			instance.UpdateCurrentItem(UAP_BaseElement.EHighlightSource.Internal, makeSureItemIsSelected: true);
			ReadItem(instance.m_CurrentItem);
		}
	}

	public static void ActivateContainer(AccessibleUIGroupRoot container, bool activate)
	{
		if (isDestroyed)
		{
			return;
		}
		Initialize();
		if (activate)
		{
			Log("Activating UI Container " + container.name);
		}
		else
		{
			Log("Deactivating UI Container " + container.name);
		}
		if (activate)
		{
			if (m_ContainersToActivate.Contains(container))
			{
				return;
			}
			bool flag = false;
			for (int i = 0; i < m_ContainersToActivate.Count; i++)
			{
				if (container.m_Priority > m_ContainersToActivate[i].m_Priority)
				{
					m_ContainersToActivate.Insert(i, container);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				m_ContainersToActivate.Add(container);
			}
		}
		else
		{
			if (m_ContainersToActivate.Contains(container))
			{
				m_ContainersToActivate.Remove(container);
			}
			instance.ActivateContainer_Internal(container, activate: false, readCurrentItem: false);
			instance.m_ReadItemNextUpdate = !instance.m_BlockInput;
		}
	}

	private AccessibleUIGroupRoot GetActivePopup()
	{
		foreach (AccessibleUIGroupRoot activeContainer in m_ActiveContainers)
		{
			if (activeContainer.m_PopUp)
			{
				return activeContainer;
			}
		}
		return null;
	}

	private void ActivateContainer_Internal(AccessibleUIGroupRoot container, bool activate, bool readCurrentItem = true)
	{
		if (container == null)
		{
			return;
		}
		if (activate)
		{
			if (m_ActiveContainers.Contains(container))
			{
				return;
			}
			if (container.m_PopUp)
			{
				if (m_IsEnabled)
				{
					PlaySFX(m_UIPopUpOpen);
				}
				List<AccessibleUIGroupRoot> list = new List<AccessibleUIGroupRoot>();
				foreach (AccessibleUIGroupRoot activeContainer in m_ActiveContainers)
				{
					list.Add(activeContainer);
				}
				m_SuspendedContainers.Add(list);
				m_SuspendedActiveContainerIndex.Add(m_ActiveContainerIndex);
				m_ActiveContainers.Clear();
				SelectNothing(UAP_BaseElement.EHighlightSource.Internal);
			}
			else
			{
				AccessibleUIGroupRoot activePopup = GetActivePopup();
				if (activePopup != null && !activePopup.m_AllowExternalJoining && !container.transform.IsChildOf(activePopup.transform))
				{
					if (m_SuspendedContainers.Count > 0)
					{
						List<AccessibleUIGroupRoot> list2 = m_SuspendedContainers[m_SuspendedContainers.Count - 1];
						bool flag = false;
						for (int i = 0; i < list2.Count; i++)
						{
							if (container.m_Priority > list2[i].m_Priority)
							{
								list2.Insert(i, container);
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							list2.Add(container);
						}
					}
					else
					{
						List<AccessibleUIGroupRoot> list3 = new List<AccessibleUIGroupRoot>();
						list3.Add(container);
						m_SuspendedContainers.Add(list3);
						m_SuspendedActiveContainerIndex.Add(0);
					}
					return;
				}
			}
			bool flag2 = false;
			for (int j = 0; j < m_ActiveContainers.Count; j++)
			{
				if (container.m_Priority > m_ActiveContainers[j].m_Priority)
				{
					m_ActiveContainers.Insert(j, container);
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				m_ActiveContainers.Add(container);
			}
			if (m_ActiveContainers.Count == 1)
			{
				m_ActiveContainerIndex = 0;
				ReadContainerName();
				m_CurrentItem = m_ActiveContainers[0].GetCurrentElement(m_CyclicMenus);
				UpdateCurrentItem(UAP_BaseElement.EHighlightSource.Internal);
				if (m_ActiveContainers[0] == container && container.m_AutoRead)
				{
					ReadFromTop();
				}
				else if (readCurrentItem)
				{
					ReadItem(m_CurrentItem);
				}
			}
			else if (m_CurrentItem == null)
			{
				UpdateCurrentItem(UAP_BaseElement.EHighlightSource.Internal);
				if (m_ActiveContainers[m_ActiveContainerIndex] == container && container.m_AutoRead)
				{
					ReadFromTop();
				}
				else if (readCurrentItem)
				{
					ReadItem(m_CurrentItem);
				}
			}
			return;
		}
		if (!m_ActiveContainers.Contains(container))
		{
			for (int k = 0; k < m_SuspendedContainers.Count; k++)
			{
				List<AccessibleUIGroupRoot> list4 = m_SuspendedContainers[k];
				for (int l = 0; l < list4.Count; l++)
				{
					if (list4[l] == container)
					{
						list4.Remove(container);
						if (m_SuspendedActiveContainerIndex[k] == l)
						{
							m_SuspendedActiveContainerIndex[k] = 0;
						}
						if (list4.Count == 0)
						{
							m_SuspendedContainers.Remove(list4);
							m_SuspendedActiveContainerIndex.RemoveAt(k);
						}
						break;
					}
				}
			}
			return;
		}
		if (container.m_PopUp)
		{
			if (m_IsEnabled)
			{
				PlaySFX(m_UIPopUpClose);
			}
			m_ActiveContainers.Clear();
			m_ActiveContainerIndex = -1;
			if (m_SuspendedContainers.Count > 0)
			{
				foreach (AccessibleUIGroupRoot item in m_SuspendedContainers[m_SuspendedContainers.Count - 1])
				{
					m_ActiveContainers.Add(item);
				}
				m_SuspendedContainers.RemoveAt(m_SuspendedContainers.Count - 1);
				m_ActiveContainerIndex = m_SuspendedActiveContainerIndex[m_SuspendedActiveContainerIndex.Count - 1];
				m_SuspendedActiveContainerIndex.RemoveAt(m_SuspendedActiveContainerIndex.Count - 1);
			}
			if (m_ActiveContainers.Count == 0)
			{
				m_ActiveContainerIndex = -1;
			}
		}
		else
		{
			int num = m_ActiveContainers.IndexOf(container);
			if (num == m_ActiveContainerIndex)
			{
				m_ActiveContainerIndex = m_ActiveContainers.Count - (num + 2);
			}
			else if (num < m_ActiveContainerIndex)
			{
				m_ActiveContainerIndex--;
			}
			m_ActiveContainers.Remove(container);
		}
		if (m_ActiveContainerIndex < 0)
		{
			m_CurrentItem = null;
			m_PreviousItem = null;
			return;
		}
		UpdateCurrentItem(UAP_BaseElement.EHighlightSource.Internal);
		if (readCurrentItem)
		{
			ReadItem(m_CurrentItem);
		}
	}

	public static void ResetCurrentContainerFocus()
	{
		if (!(instance == null) && IsActive() && instance.m_ActiveContainerIndex >= 0 && instance.m_ActiveContainers.Count > instance.m_ActiveContainerIndex)
		{
			instance.m_ActiveContainers[instance.m_ActiveContainerIndex].ResetToStart();
		}
	}

	private void SelectNothing(UAP_BaseElement.EHighlightSource selectionSource)
	{
		m_ActiveContainerIndex = 0;
		if (m_PreviousItem != null && m_PreviousItem.m_Object != null)
		{
			m_PreviousItem.m_Object.HoverHighlight(enable: false, selectionSource);
		}
		if (m_CurrentItem != null && m_CurrentItem != m_PreviousItem && m_CurrentItem.m_Object != null)
		{
			m_CurrentItem.m_Object.HoverHighlight(enable: false, selectionSource);
		}
		m_CurrentItem = null;
		m_PreviousItem = null;
		HideElementFrame();
	}

	private void UpdateCurrentItem(UAP_BaseElement.EHighlightSource selectionSource, bool makeSureItemIsSelected = false)
	{
		m_CurrentItem = null;
		bool flag = IsEnabled();
		if (m_ActiveContainers.Count == 0)
		{
			Log("Nothing on screen to select");
			m_ActiveContainerIndex = -1;
			if (flag && m_PreviousItem != null && m_PreviousItem.m_Object != null)
			{
				m_PreviousItem.m_Object.HoverHighlight(enable: false, selectionSource);
			}
			return;
		}
		int num = m_ActiveContainerIndex;
		if (num < 0)
		{
			num = 0;
		}
		for (int i = 0; i < m_ActiveContainers.Count; i++)
		{
			int num2;
			for (num2 = i + num; num2 >= m_ActiveContainers.Count; num2 -= m_ActiveContainers.Count)
			{
			}
			if (m_ActiveContainers.Count > 0)
			{
				m_CurrentItem = m_ActiveContainers[num2].GetCurrentElement(m_CyclicMenus);
			}
			else
			{
				m_CurrentItem = null;
			}
			if (m_CurrentItem == null)
			{
				Log("Nothing selected");
				if (makeSureItemIsSelected)
				{
					DecrementUIElement();
					UpdateCurrentItem(selectionSource);
					break;
				}
			}
			if (m_CurrentItem == null)
			{
				continue;
			}
			m_ActiveContainerIndex = num2;
			if (m_PreviousItem != m_CurrentItem && (m_PreviousItem == null || m_CurrentItem == null || m_PreviousItem.m_Object != m_CurrentItem.m_Object))
			{
				Log("Selected item was updated" + ((m_CurrentItem != null) ? (" to " + m_CurrentItem.m_Object.GetTextToRead()) : " NONE") + " (Frame: " + Time.frameCount + ") + previous item: " + ((m_PreviousItem != null) ? m_PreviousItem.m_Object.GetTextToRead() : "null"));
				if (flag)
				{
					if (m_PreviousItem != null && m_PreviousItem.m_Object != null)
					{
						m_PreviousItem.m_Object.HoverHighlight(enable: false, selectionSource);
					}
					if (m_CurrentItem != null && m_CurrentItem.m_Object != null)
					{
						m_CurrentItem.m_Object.HoverHighlight(enable: true, selectionSource);
					}
				}
			}
			m_PreviousItem = m_CurrentItem;
			break;
		}
	}

	public static bool IsEnabled()
	{
		if (isDestroyed)
		{
			return false;
		}
		Initialize();
		return m_IsEnabled;
	}

	public new static string GetInstanceID()
	{
		if (instance == null)
		{
			return "Not initialized";
		}
		return instance.gameObject.GetInstanceID().ToString("0");
	}

	public static bool IsActive()
	{
		if (isDestroyed)
		{
			return false;
		}
		Initialize();
		if (m_IsEnabled)
		{
			return !m_Paused;
		}
		return false;
	}

	public static bool IsCurrentPlatformSupported()
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
		{
			return true;
		}
		return false;
	}

	public static void EnableMagicGestures(bool enable)
	{
		Initialize();
		instance.m_HandleMagicGestures = enable;
	}

	public static bool IsMagicGesturesEnabled()
	{
		if (instance == null)
		{
			return false;
		}
		return instance.m_HandleMagicGestures;
	}

	public static void EnableAccessibility(bool enable, bool readNotification = false)
	{
		if (isDestroyed || enable == m_IsEnabled)
		{
			return;
		}
		Initialize();
		StopContinuousReading();
		if (enable)
		{
			if (PlayerPrefs.GetInt("Accessibility_FirstTimeStartup", 1) == 1)
			{
				if (Application.isMobilePlatform)
				{
					instance.SayAudio(null, Localize_Internal("Mobile_General"), UAP_AudioQueue.EAudioType.App, allowVoiceOver: true, UAP_AudioQueue.EInterrupt.All);
				}
				else
				{
					instance.SayAudio(null, Localize_Internal("Desktop_General"), UAP_AudioQueue.EAudioType.App, allowVoiceOver: true, UAP_AudioQueue.EInterrupt.All);
				}
				PlayerPrefs.SetInt("Accessibility_FirstTimeStartup", 0);
				PlayerPrefs.Save();
				readNotification = false;
			}
			bool isEnabled = m_IsEnabled;
			m_IsEnabled = true;
			m_Paused = false;
			instance.OnEnable();
			if (readNotification)
			{
				instance.Say_Internal(Localize_Internal("EnabledAccessibility"), canBeInterrupted: false, allowVoiceOver: true, UAP_AudioQueue.EInterrupt.All);
			}
			if (!isEnabled)
			{
				instance.StartScreenOver();
			}
		}
		else
		{
			StopSpeaking();
			if (readNotification)
			{
				instance.Say_Internal(Localize_Internal("DisabledAccessibility"), canBeInterrupted: false, allowVoiceOver: true, UAP_AudioQueue.EInterrupt.All);
			}
			HideElementFrame();
			m_IsEnabled = false;
			instance.OnDisable();
		}
		SavePluginEnabledState();
		if (instance.m_OnAccessibilityModeChanged != null)
		{
			instance.m_OnAccessibilityModeChanged(enable);
		}
	}

	private void OnEnable()
	{
		EnableTouchBlocker(enable: true);
	}

	private void OnDisable()
	{
		if (m_AudioQueue != null)
		{
			m_AudioQueue.Stop();
		}
		if (m_TouchBlocker != null)
		{
			EnableTouchBlocker(enable: false);
		}
	}

	private void UpdateMagicTapDetection()
	{
		bool flag = false;
		if (m_WaitingForMagicTap && Time.unscaledTime > m_MagicTap_LastTapTime + 0.2f)
		{
			m_WaitingForMagicTap = false;
			if (m_OnTwoFingerSingleTapCallbacks != null)
			{
				m_OnTwoFingerSingleTapCallbacks();
			}
		}
		int touchCount = GetTouchCount();
		if (m_MagicTap_TouchCountHelper < 2 && touchCount == 2)
		{
			if (Time.unscaledTime < m_MagicTap_LastTapTime + 0.2f)
			{
				flag = true;
				m_WaitingForMagicTap = false;
			}
			else
			{
				m_WaitingForMagicTap = true;
			}
			m_MagicTap_LastTapTime = Time.unscaledTime;
		}
		m_MagicTap_TouchCountHelper = touchCount;
		if (flag)
		{
			m_MagicTap_LastTapTime = -1f;
			if (m_HandleMagicGestures && m_OnPauseToggleCallbacks != null)
			{
				m_OnPauseToggleCallbacks();
			}
		}
	}

	private void UpdateThreeFingerTapDetection()
	{
		if (m_WaitingForThreeFingerTap && Time.unscaledTime > m_TripleTap_LastTapTime + 0.2f)
		{
			m_WaitingForThreeFingerTap = false;
			if (m_TripleTap_Count == 1)
			{
				if (m_IsEnabled && m_OnThreeFingerSingleTapCallbacks != null)
				{
					m_OnThreeFingerSingleTapCallbacks();
				}
			}
			else if (m_TripleTap_Count == 2 && m_IsEnabled && m_OnThreeFingerDoubleTapCallbacks != null)
			{
				m_OnThreeFingerDoubleTapCallbacks();
			}
			m_TripleTap_Count = 0;
		}
		int touchCount = GetTouchCount();
		bool flag = false;
		if (m_TripleTap_TouchCountHelper < 3 && touchCount == 3)
		{
			if (Time.unscaledTime < m_TripleTap_LastTapTime + 0.2f)
			{
				m_TripleTap_Count++;
				if (m_TripleTap_Count == 3)
				{
					m_WaitingForThreeFingerTap = false;
					flag = true;
				}
			}
			else
			{
				m_WaitingForThreeFingerTap = true;
				m_TripleTap_Count = 1;
			}
			m_TripleTap_LastTapTime = Time.unscaledTime;
			if (m_DebugOutputLabel != null && instance.m_DebugOutput)
			{
				m_DebugOutputLabel.text = "Three finger touch - Count: " + m_TripleTap_Count;
			}
		}
		m_TripleTap_TouchCountHelper = touchCount;
		if (flag)
		{
			if (m_DebugOutputLabel != null && instance.m_DebugOutput)
			{
				m_DebugOutputLabel.text = "Three finger triple tap detected.";
			}
			m_TripleTap_LastTapTime = -1f;
			m_TripleTap_Count = 0;
			if (m_HandleMagicGestures)
			{
				ToggleAccessibility();
			}
		}
	}

	public static void ToggleAccessibility()
	{
		Initialize();
		instance.ToggleAccessibility_Internal();
	}

	private void ToggleAccessibility_Internal()
	{
		bool flag = IsEnabled();
		if (!flag)
		{
			instance.Say_Internal(Localize_Internal("EnabledAccessibility"), canBeInterrupted: false, allowVoiceOver: true, UAP_AudioQueue.EInterrupt.All);
		}
		EnableAccessibility(!flag);
		if (flag)
		{
			Say_Internal(Localize_Internal("DisabledAccessibility"), canBeInterrupted: false, allowVoiceOver: true, UAP_AudioQueue.EInterrupt.All);
		}
	}

	private void Update()
	{
		UpdateContainerActivations();
		UpdateThreeFingerTapDetection();
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.A) && (Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.LeftMeta)))
		{
			ToggleAccessibility();
		}
		if (!m_IsEnabled)
		{
			return;
		}
		if (m_LastUpdateTouchCount == 0)
		{
			int touchCount = GetTouchCount();
			if (touchCount > 1)
			{
				m_AudioQueue.StopAllInterruptibles();
			}
			else if (touchCount == 1)
			{
				m_AudioQueue.InterruptAppAnnouncement();
			}
		}
		if (m_HandleMagicGestures)
		{
			HandlePauseGestures();
		}
		if (m_Paused)
		{
			return;
		}
		if (m_WindowsUseMouseSwipes)
		{
			UpdateScrubDetection();
			UpdateSwipeDetection();
		}
		if (m_ContinuousReading && !m_ContinuousReading_WaitInputClear && Input.anyKey)
		{
			StopContinuousReading();
		}
		if (m_ContinuousReading_WaitInputClear && !Input.anyKey)
		{
			m_ContinuousReading_WaitInputClear = false;
		}
		if (HandleUI())
		{
			if (m_ContinuousReading)
			{
				UpdateContinuousReading();
			}
			if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer && !m_CurrentElementHasSoleFocus && EventSystem.current != null)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			if (m_CurrentElementHasSoleFocus)
			{
				_ = m_CurrentItem.m_Type;
				_ = 5;
			}
			if (m_CurrentItem != null)
			{
				UpdateElementFrame(ref m_CurrentItem);
			}
			else
			{
				HideElementFrame();
			}
			if (m_WindowsUseMouseSwipes)
			{
				UpdateDoubleTapDetection();
			}
			if ((m_ExploreByTouch && !m_CurrentElementHasSoleFocus) || UAP_VirtualKeyboard.IsOpen())
			{
				UpdateExploreByTouch2();
			}
			if (m_WindowsUseKeys)
			{
				UpdateKeyboardInput();
			}
		}
		m_LastUpdateTouchCount = GetTouchCount();
	}

	private void UpdateContinuousReading()
	{
		if (!m_AudioQueue.IsCompletelyEmpty())
		{
			return;
		}
		int num = m_ActiveContainerIndex;
		int activeContainerIndex = m_ActiveContainerIndex;
		if (!m_ActiveContainers[num].IncrementCurrentItem(rollOverAllowed: false))
		{
			if (num == m_ActiveContainers.Count - 1)
			{
				StopContinuousReading();
			}
			else
			{
				bool flag = true;
				do
				{
					num++;
					if (num < m_ActiveContainers.Count)
					{
						m_ActiveContainers[num].JumpToFirst();
						flag = m_ActiveContainers[num].GetCurrentElementIndex() < 0;
					}
					else
					{
						flag = false;
					}
				}
				while (flag);
				if (num == m_ActiveContainers.Count - 1 && m_ActiveContainers[num].GetCurrentElementIndex() < 0)
				{
					StopContinuousReading();
				}
				else
				{
					m_ActiveContainerIndex = num;
				}
			}
		}
		if (m_ContinuousReading)
		{
			m_CurrentItem = m_ActiveContainers[m_ActiveContainerIndex].GetCurrentElement(rollOverAllowed: false);
			if (activeContainerIndex != m_ActiveContainerIndex)
			{
				ReadContainerName();
			}
			UpdateCurrentItem(UAP_BaseElement.EHighlightSource.Internal);
			ReadItem(m_CurrentItem, quickOnly: true);
		}
	}

	private void HandlePauseGestures()
	{
		if (m_WindowsUseMouseSwipes)
		{
			UpdateMagicTapDetection();
		}
	}

	private bool TouchExploreMinDistanceReach(float movedDistance, float moveTime)
	{
		float num = Screen.dpi * 0.75f * (moveTime / 0.25f);
		if (num == 0f)
		{
			num = (float)Mathf.Min(Screen.width, Screen.height) * 0.1f;
		}
		if (movedDistance <= num)
		{
			return true;
		}
		return false;
	}

	private void UpdateExploreByTouch2()
	{
		if (!HandleUI())
		{
			return;
		}
		if (GetTouchCount() > 1)
		{
			m_TouchExplore_Active = false;
			m_TouchExplore_CheckVelocityTimer = -1f;
			m_TouchExplore_WaitForDoubleTapToExpireTimer = -1f;
			return;
		}
		if (m_TouchExplore_Active)
		{
			if (GetTouchCount() != 1)
			{
				m_TouchExplore_Active = false;
				m_TouchExplore_CheckVelocityTimer = -1f;
				m_TouchExplore_WaitForDoubleTapToExpireTimer = -1f;
			}
			else
			{
				ExploreByTouch_SelectElementUnderFinger(GetTouchPosition());
			}
			return;
		}
		if (m_TouchExplore_WaitForDoubleTapToExpireTimer > 0f)
		{
			if (GetTouchCount() != 0 || m_DoubleTapFoundThisFrame)
			{
				m_TouchExplore_Active = false;
				m_TouchExplore_CheckVelocityTimer = -1f;
				m_TouchExplore_WaitForDoubleTapToExpireTimer = -1f;
				return;
			}
			m_TouchExplore_WaitForDoubleTapToExpireTimer -= Time.unscaledDeltaTime;
			if (m_TouchExplore_WaitForDoubleTapToExpireTimer <= 0f)
			{
				ExploreByTouch_SelectElementUnderFinger(GetTouchPosition());
				m_SwipeActive = false;
				m_SwipeWaitForLift = true;
			}
		}
		else if (m_TouchExplore_CheckVelocityTimer > 0f)
		{
			m_TouchExplore_CheckVelocityTimer -= Time.unscaledDeltaTime;
			if (m_TouchExplore_CheckVelocityTimer <= 0f || GetTouchCount() == 0)
			{
				float num = 0.15f - m_TouchExplore_CheckVelocityTimer;
				m_TouchExplore_CheckVelocityTimer = -1f;
				Vector3 vector = GetTouchPosition() - m_TouchExplore_CheckStartPosition;
				_ = vector.magnitude;
				if (TouchExploreMinDistanceReach(vector.magnitude, num))
				{
					m_TouchExplore_WaitForDoubleTapToExpireTimer = 0.2f - num;
					if (GetTouchCount() == 0 && m_TouchExplore_WaitForDoubleTapToExpireTimer > 0f)
					{
						m_TouchExplore_CheckVelocityTimer = -1f;
						return;
					}
					m_TouchExplore_Active = true;
					ExploreByTouch_SelectElementUnderFinger(GetTouchPosition());
					m_SwipeActive = false;
					m_SwipeWaitForLift = true;
				}
			}
		}
		if (m_LastUpdateTouchCount == 0 && GetTouchCount() == 1)
		{
			m_TouchExplore_CheckVelocityTimer = 0.15f;
			m_TouchExplore_CheckStartPosition = GetTouchPosition();
			m_TouchExplore_Active = false;
		}
	}

	private void UpdateExploreByTouch()
	{
		if (!HandleUI())
		{
			CancelExploreByTouch();
			m_ExploreByTouch_SingleTapWaitTimer = -1f;
			return;
		}
		bool flag = true;
		if (!WindowsUseExploreByTouch)
		{
			flag = false;
		}
		if (!flag)
		{
			CancelExploreByTouch();
			m_ExploreByTouch_SingleTapWaitTimer = -1f;
			return;
		}
		bool flag2 = GetTouchCount() == 1;
		if (m_ExploreByTouch_IsActive)
		{
			if (!flag2)
			{
				CancelExploreByTouch();
				m_ExploreByTouch_SingleTapWaitTimer = -1f;
				return;
			}
			Vector3 touchPosition = GetTouchPosition();
			Vector3 fingerPos = new Vector2(touchPosition.x, touchPosition.y);
			if (m_DebugOutputLabel != null && instance.m_DebugOutput)
			{
				m_DebugOutputLabel.text = "Explore by Touch - Drag active";
			}
			ExploreByTouch_SelectElementUnderFinger(fingerPos);
			return;
		}
		if (flag2)
		{
			if (m_ExploreByTouch_SingleTapWaitTimer > 0f)
			{
				m_ExploreByTouch_SingleTapWaitTimer -= Time.unscaledDeltaTime;
			}
			else if (Input.GetMouseButtonDown(0))
			{
				m_ExploreByTouch_SingleTapWaitTimer = 0.2f;
				m_ExploreByTouch_SingleTapStartPosition = GetTouchPosition();
			}
		}
		else if (m_ExploreByTouch_SingleTapWaitTimer > 0f)
		{
			m_ExploreByTouch_SingleTapWaitTimer -= Time.unscaledDeltaTime;
			if (m_ExploreByTouch_SingleTapWaitTimer < 0f)
			{
				Vector3 vector = GetTouchPosition() - m_ExploreByTouch_SingleTapStartPosition;
				float num = Screen.dpi * 0.2f;
				float num2 = 0.5f * (float)(Screen.width + Screen.height) * 0.1f;
				if (num == 0f || num > num2)
				{
					num = num2;
				}
				if (vector.sqrMagnitude < num * num)
				{
					Vector3 touchPosition2 = GetTouchPosition();
					if (m_DebugOutputLabel != null && instance.m_DebugOutput)
					{
						Text debugOutputLabel = m_DebugOutputLabel;
						string[] array = new string[9]
						{
							"Explore by Touch Single Tap \nFinger Delta: ",
							vector.magnitude.ToString("000.00000"),
							" on a screen with ",
							Screen.dpi.ToString(),
							" dpi. (Start: ",
							null,
							null,
							null,
							null
						};
						Vector3 exploreByTouch_StartPosition = m_ExploreByTouch_StartPosition;
						array[5] = exploreByTouch_StartPosition.ToString();
						array[6] = " End: ";
						exploreByTouch_StartPosition = touchPosition2;
						array[7] = exploreByTouch_StartPosition.ToString();
						array[8] = ")";
						debugOutputLabel.text = string.Concat(array);
					}
					Vector3 fingerPos2 = new Vector2(touchPosition2.x, touchPosition2.y);
					m_CurrentItem = null;
					ExploreByTouch_SelectElementUnderFinger(fingerPos2);
				}
			}
		}
		if (m_ExploreByTouch_WaitTimer < 0f)
		{
			m_ExploreByTouch_WaitTimer = m_ExploreByTouchDelay;
			m_ExploreByTouch_StartPosition = GetTouchPosition();
			return;
		}
		if (!flag2)
		{
			CancelExploreByTouch();
			return;
		}
		m_ExploreByTouch_WaitTimer -= Time.unscaledDeltaTime;
		if (!(m_ExploreByTouch_WaitTimer < 0f))
		{
			return;
		}
		m_ExploreByTouch_SingleTapWaitTimer = -1f;
		Vector3 touchPosition3 = GetTouchPosition();
		float magnitude = ((Vector2)(touchPosition3 - m_ExploreByTouch_StartPosition)).magnitude;
		float num3 = magnitude / 0.2f;
		if (num3 < 600f)
		{
			if (m_DebugOutputLabel != null && instance.m_DebugOutput)
			{
				m_DebugOutputLabel.text = "Explore by Touch. Ratio: " + num3.ToString("0.#####") + " distance: " + magnitude.ToString("0.####");
			}
			m_ExploreByTouch_IsActive = true;
			m_CurrentItem = null;
			Vector3 fingerPos3 = new Vector2(touchPosition3.x, touchPosition3.y);
			ExploreByTouch_SelectElementUnderFinger(fingerPos3);
		}
		else
		{
			CancelExploreByTouch();
		}
	}

	private void ExploreByTouch_SelectElementUnderFinger(Vector3 fingerPos)
	{
		bool flag = false;
		if (m_CurrentItem != null && IsPositionOverElement(fingerPos, m_CurrentItem))
		{
			return;
		}
		for (int i = 0; i < m_ActiveContainers.Count; i++)
		{
			List<AccessibleUIGroupRoot.Accessible_UIElement> elements = m_ActiveContainers[i].GetElements();
			for (int j = 0; j < elements.Count; j++)
			{
				if (elements[j].m_Object.IsElementActive() && IsPositionOverElement(fingerPos, elements[j]))
				{
					flag = true;
					if (m_ActiveContainers[i].m_AllowTouchExplore)
					{
						m_CurrentItem = elements[j];
						m_ActiveContainerIndex = i;
						m_ActiveContainers[i].SetActiveElementIndex(j, m_CyclicMenus);
						PlaySFX(m_UINavigationClick);
						UpdateCurrentItem(UAP_BaseElement.EHighlightSource.TouchExplore);
						ReadItem(m_CurrentItem);
					}
					else if (m_ActiveContainerIndex != i)
					{
						m_ActiveContainerIndex = i;
						ReadContainerName();
						m_CurrentItem = m_ActiveContainers[i].GetCurrentElement(m_CyclicMenus);
						PlaySFX(m_UINavigationClick);
						UpdateCurrentItem(UAP_BaseElement.EHighlightSource.TouchExplore);
						ReadItem(m_CurrentItem);
					}
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
	}

	private void ReadContainerName()
	{
		if (m_IsEnabled)
		{
			string containerName = m_ActiveContainers[m_ActiveContainerIndex].GetContainerName();
			if (containerName.Length > 0)
			{
				SayAudio(null, containerName, UAP_AudioQueue.EAudioType.Container_Name, allowVoiceOver: true);
			}
		}
	}

	private void CancelExploreByTouch()
	{
		m_ExploreByTouch_IsActive = false;
		m_ExploreByTouch_WaitTimer = -1f;
	}

	private void UpdateDoubleTapDetection()
	{
		m_DoubleTapFoundThisFrame = false;
		int touchCount = GetTouchCount();
		if (Input.GetMouseButtonDown(0) && touchCount == 1)
		{
			if (Time.unscaledTime < m_DoubleTap_LastTapTime + 0.2f)
			{
				m_DoubleTapFoundThisFrame = true;
			}
			m_DoubleTap_LastTapTime = Time.unscaledTime;
		}
		if (m_DoubleTapFoundThisFrame && m_CurrentItem != null)
		{
			CancelExploreByTouch();
			m_ExploreByTouch_SingleTapWaitTimer = -1f;
			if (m_CurrentElementHasSoleFocus && !UAP_VirtualKeyboard.IsOpen())
			{
				m_CurrentItem.m_Object.InteractEnd();
				LeaveFocussedItem();
			}
			else if (m_CurrentItem.m_Object.IsInteractable())
			{
				InteractWithElement(m_CurrentItem);
			}
		}
	}

	public static void FinishCurrentInteraction()
	{
		if (m_IsInitialized)
		{
			instance.m_CurrentItem.m_Object.InteractEnd();
			instance.LeaveFocussedItem();
		}
	}

	public static bool GetSpeakDisabledInteractables()
	{
		Initialize();
		return instance.m_ReadDisabledInteractables;
	}

	private void LeaveFocussedItem()
	{
		PlaySFX(m_UIFocusLeave);
		m_CurrentElementHasSoleFocus = false;
		Say_Internal("");
		if (m_CurrentItem.m_Type == AccessibleUIGroupRoot.EUIElement.ETextEdit)
		{
			ReadValue(allowPause: false, interrupt: true);
		}
		else if (m_CurrentItem.m_Type == AccessibleUIGroupRoot.EUIElement.EDropDown)
		{
			string text = Localize_Internal("DropdownItemSelected");
			string currentValueAsText = m_CurrentItem.m_Object.GetCurrentValueAsText();
			text = text.Replace("TT", currentValueAsText);
			SayAudio(null, text, UAP_AudioQueue.EAudioType.Element_Text, m_CurrentItem.m_Object.m_AllowVoiceOver, UAP_AudioQueue.EInterrupt.Elements);
		}
	}

	private void CancelFocussedItem()
	{
		PlaySFX(m_UIFocusLeave);
		m_CurrentItem.m_Object.InteractAbort();
		Say_Internal("");
		m_CurrentElementHasSoleFocus = false;
	}

	private bool IsActiveContainer2DNavigation()
	{
		if (m_ActiveContainerIndex >= 0)
		{
			return m_ActiveContainers[m_ActiveContainerIndex].m_2DNavigation;
		}
		return false;
	}

	private void UpdateKeyboardInput()
	{
		if (m_CurrentElementHasSoleFocus && !UAP_VirtualKeyboard.IsOpen())
		{
			if (m_CurrentItem.m_Type == AccessibleUIGroupRoot.EUIElement.ESlider)
			{
				bool flag = false;
				if (Input.GetKeyDown(m_SliderIncrementKey))
				{
					m_CurrentItem.m_Object.Increment();
					flag = true;
				}
				else if (Input.GetKeyDown(m_SliderDecrementKey))
				{
					m_CurrentItem.m_Object.Decrement();
					flag = true;
				}
				if (flag)
				{
					ReadValue(allowPause: false, interrupt: true);
				}
			}
			else if (m_CurrentItem.m_Type == AccessibleUIGroupRoot.EUIElement.EDropDown)
			{
				bool flag2 = false;
				bool flag3 = false;
				if (Input.GetKeyDown(m_DropDownNextKey))
				{
					if (!m_CurrentItem.m_Object.Increment())
					{
						flag3 = true;
					}
					flag2 = true;
				}
				else if (Input.GetKeyDown(m_DropDownPreviousKey))
				{
					if (!m_CurrentItem.m_Object.Decrement())
					{
						flag3 = true;
					}
					flag2 = true;
				}
				if (flag2)
				{
					ReadValue(allowPause: false, interrupt: true);
					ReadDropdownItemIndex(ref m_CurrentItem);
				}
				if (flag3)
				{
					PlaySFX(m_UIBoundsReached);
				}
			}
			if (Input.GetKeyDown(m_InteractKey))
			{
				m_CurrentItem.m_Object.InteractEnd();
				LeaveFocussedItem();
			}
			if (Input.GetKeyDown(m_AbortKey))
			{
				CancelFocussedItem();
			}
		}
		else
		{
			if (!HandleUI())
			{
				return;
			}
			if (IsActiveContainer2DNavigation())
			{
				if (Input.GetKeyDown(m_DownKey))
				{
					Navigate2DUIElement(ESDirection.EDown);
				}
				if (Input.GetKeyDown(m_UpKey))
				{
					Navigate2DUIElement(ESDirection.EUp);
				}
				if (Input.GetKeyDown(m_RightKey))
				{
					Navigate2DUIElement(ESDirection.ERight);
				}
				if (Input.GetKeyDown(m_LeftKey))
				{
					Navigate2DUIElement(ESDirection.ELeft);
				}
				if (Input.GetKeyDown(m_DownKey) || Input.GetKeyDown(m_UpKey) || Input.GetKeyDown(m_RightKey) || Input.GetKeyDown(m_LeftKey))
				{
					return;
				}
			}
			else
			{
				if (Input.GetKeyDown(m_NextElementKey))
				{
					IncrementUIElement();
					return;
				}
				if (Input.GetKeyDown(m_PreviousElementKey))
				{
					DecrementUIElement();
					return;
				}
				if (Input.GetKeyDown(m_PreviousContainerKey) || (m_UseTabAndShiftTabForContainerJumping && Input.GetKeyDown(KeyCode.Tab) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))))
				{
					int activeContainerIndex = m_ActiveContainerIndex;
					if (DecrementContainer(resetToStartItem: true))
					{
						UpdateCurrentItem(UAP_BaseElement.EHighlightSource.UserInput);
						PlaySFX(m_UINavigationClick);
					}
					else
					{
						PlaySFX(m_UIBoundsReached);
					}
					if (activeContainerIndex != m_ActiveContainerIndex)
					{
						ReadContainerName();
					}
					ReadItem(m_CurrentItem);
					return;
				}
				if (Input.GetKeyDown(m_NextContainerKey) || (m_UseTabAndShiftTabForContainerJumping && Input.GetKeyDown(KeyCode.Tab)))
				{
					int activeContainerIndex2 = m_ActiveContainerIndex;
					if (IncrementContainer(resetToStartItem: true))
					{
						UpdateCurrentItem(UAP_BaseElement.EHighlightSource.UserInput);
						PlaySFX(m_UINavigationClick);
					}
					else
					{
						PlaySFX(m_UIBoundsReached);
					}
					if (activeContainerIndex2 != m_ActiveContainerIndex)
					{
						ReadContainerName();
					}
					ReadItem(m_CurrentItem);
					return;
				}
			}
			if (Input.GetKeyDown(m_InteractKey))
			{
				InteractWithElement(m_CurrentItem);
			}
		}
	}

	private void InteractWithElement(AccessibleUIGroupRoot.Accessible_UIElement item)
	{
		if (item != null && item.m_Object.IsInteractable())
		{
			Say_Internal("");
			switch (item.m_Type)
			{
			case AccessibleUIGroupRoot.EUIElement.EButton:
				PlaySFX(m_UIInteract);
				item.m_Object.Interact();
				break;
			case AccessibleUIGroupRoot.EUIElement.EToggle:
				PlaySFX(m_UIInteract);
				item.m_Object.Interact();
				ReadItem(m_CurrentItem);
				break;
			case AccessibleUIGroupRoot.EUIElement.ESlider:
				PlaySFX(m_UIFocusEnter);
				item.m_Object.Interact();
				m_CurrentElementHasSoleFocus = true;
				break;
			case AccessibleUIGroupRoot.EUIElement.ETextEdit:
				PlaySFX(m_UIFocusEnter);
				item.m_Object.Interact();
				m_CurrentElementHasSoleFocus = true;
				break;
			case AccessibleUIGroupRoot.EUIElement.EDropDown:
				PlaySFX(m_UIFocusEnter);
				item.m_Object.Interact();
				m_CurrentElementHasSoleFocus = true;
				ReadValue(allowPause: false, interrupt: true);
				ReadDropdownItemIndex(ref item);
				break;
			case AccessibleUIGroupRoot.EUIElement.ELabel:
				break;
			}
		}
	}

	private void ReadDropdownItemIndex(ref AccessibleUIGroupRoot.Accessible_UIElement item)
	{
		int itemCount = ((AccessibleDropdown)item.m_Object).GetItemCount();
		int selectedItemIndex = ((AccessibleDropdown)item.m_Object).GetSelectedItemIndex();
		string text = Localize_Internal("DropdownItemIndex");
		text = text.Replace("XX", selectedItemIndex.ToString("0"));
		text = text.Replace("YY", itemCount.ToString("0"));
		SayPause(m_TypeDelay);
		SayAudio(null, text, UAP_AudioQueue.EAudioType.Element_Text, m_CurrentItem.m_Object.m_AllowVoiceOver);
	}

	private void UpdateContainerActivations()
	{
		bool flag = m_ReadItemNextUpdate;
		m_ReadItemNextUpdate = false;
		if (m_ContainersToActivate.Count > 0)
		{
			flag = true;
			foreach (AccessibleUIGroupRoot item in m_ContainersToActivate)
			{
				ActivateContainer_Internal(item, activate: true, readCurrentItem: false);
			}
			m_ContainersToActivate.Clear();
		}
		UpdateCurrentItem(UAP_BaseElement.EHighlightSource.Internal);
		if (flag)
		{
			ReadItem(m_CurrentItem);
		}
	}

	private void CancelTaps()
	{
		m_TripleTap_TouchCountHelper = 0;
		m_TripleTap_LastTapTime = -1f;
		m_WaitingForThreeFingerTap = false;
		m_TripleTap_Count = 0;
		m_ExploreByTouch_SingleTapWaitTimer = -1f;
		m_DoubleTap_LastTapTime = -1f;
		m_MagicTap_TouchCountHelper = 0;
		m_MagicTap_LastTapTime = -1f;
		m_WaitingForMagicTap = false;
	}

	public void OnSwipe(ESDirection dir, int fingerCount)
	{
		CancelExploreByTouch();
		CancelTaps();
		if (m_CurrentElementHasSoleFocus && !UAP_VirtualKeyboard.IsOpen())
		{
			if (fingerCount != 1)
			{
				return;
			}
			if (m_CurrentItem.m_Type == AccessibleUIGroupRoot.EUIElement.ESlider)
			{
				bool flag = false;
				switch (dir)
				{
				case ESDirection.EUp:
					((AccessibleSlider)m_CurrentItem.m_Object).Increment();
					flag = true;
					break;
				case ESDirection.EDown:
					((AccessibleSlider)m_CurrentItem.m_Object).Decrement();
					flag = true;
					break;
				}
				if (flag)
				{
					ReadValue(allowPause: false, interrupt: true);
				}
			}
			else
			{
				if (m_CurrentItem.m_Type != AccessibleUIGroupRoot.EUIElement.EDropDown)
				{
					return;
				}
				bool flag2 = false;
				bool flag3 = false;
				switch (dir)
				{
				case ESDirection.EDown:
				case ESDirection.ERight:
					if (!m_CurrentItem.m_Object.Increment())
					{
						flag3 = true;
					}
					flag2 = true;
					break;
				case ESDirection.EUp:
				case ESDirection.ELeft:
					if (!m_CurrentItem.m_Object.Decrement())
					{
						flag3 = true;
					}
					flag2 = true;
					break;
				}
				if (flag2)
				{
					ReadValue(allowPause: false, interrupt: true);
					ReadDropdownItemIndex(ref m_CurrentItem);
				}
				if (flag3)
				{
					PlaySFX(m_UIBoundsReached);
				}
			}
			return;
		}
		switch (fingerCount)
		{
		case 1:
			if (!HandleUI())
			{
				break;
			}
			if (IsActiveContainer2DNavigation())
			{
				Navigate2DUIElement(dir);
				break;
			}
			switch (dir)
			{
			case ESDirection.ERight:
				IncrementUIElement();
				break;
			case ESDirection.ELeft:
				DecrementUIElement();
				break;
			case ESDirection.EDown:
			{
				int activeContainerIndex2 = m_ActiveContainerIndex;
				if (IncrementContainer(resetToStartItem: true))
				{
					UpdateCurrentItem(UAP_BaseElement.EHighlightSource.UserInput);
					PlaySFX(m_UINavigationClick);
				}
				else
				{
					PlaySFX(m_UIBoundsReached);
				}
				if (activeContainerIndex2 != m_ActiveContainerIndex)
				{
					ReadContainerName();
				}
				ReadItem(m_CurrentItem);
				break;
			}
			case ESDirection.EUp:
			{
				int activeContainerIndex = m_ActiveContainerIndex;
				if (DecrementContainer(resetToStartItem: true))
				{
					UpdateCurrentItem(UAP_BaseElement.EHighlightSource.UserInput);
					PlaySFX(m_UINavigationClick);
				}
				else
				{
					PlaySFX(m_UIBoundsReached);
				}
				if (activeContainerIndex != m_ActiveContainerIndex)
				{
					ReadContainerName();
				}
				ReadItem(m_CurrentItem);
				break;
			}
			}
			break;
		case 2:
			switch (dir)
			{
			case ESDirection.EUp:
				if (m_OnTwoFingerSwipeUpHandler != null)
				{
					m_OnTwoFingerSwipeUpHandler();
				}
				else
				{
					StartReadingfromTop();
				}
				break;
			case ESDirection.EDown:
				if (m_OnTwoFingerSwipeDownHandler != null)
				{
					m_OnTwoFingerSwipeDownHandler();
				}
				else
				{
					StartReadingFromCurrentElement();
				}
				break;
			case ESDirection.ELeft:
				if (m_OnTwoFingerSwipeLeftHandler != null)
				{
					m_OnTwoFingerSwipeLeftHandler();
				}
				break;
			case ESDirection.ERight:
				if (m_OnTwoFingerSwipeRightHandler != null)
				{
					m_OnTwoFingerSwipeRightHandler();
				}
				break;
			}
			break;
		case 3:
			if (dir == ESDirection.EUp)
			{
				HandleUI();
			}
			break;
		}
	}

	public static void SetFocusToTopOfPage()
	{
		if (!(instance == null) && IsActive())
		{
			instance.SetFocusToTopOfPage_Internal();
		}
	}

	private void SetFocusToTopOfPage_Internal()
	{
		if (!HandleUI() || m_ActiveContainers.Count <= 0)
		{
			return;
		}
		int activeContainerIndex = m_ActiveContainerIndex;
		m_ActiveContainerIndex = 0;
		m_ActiveContainers[m_ActiveContainerIndex].JumpToFirst();
		UpdateCurrentItem(UAP_BaseElement.EHighlightSource.Internal);
		if (m_CurrentItem != null)
		{
			if (activeContainerIndex != 0)
			{
				ReadContainerName();
			}
			ReadItem(m_CurrentItem, quickOnly: true);
		}
	}

	private void StartReadingfromTop()
	{
		if (HandleUI() && m_ActiveContainers.Count > 0)
		{
			SetFocusToTopOfPage_Internal();
			m_ContinuousReading = true;
			m_ContinuousReading_WaitInputClear = true;
		}
	}

	public static void ReadFromCurrent()
	{
		if (IsEnabled())
		{
			instance.StartReadingFromCurrentElement();
		}
	}

	private void StartReadingFromCurrentElement()
	{
		if (HandleUI() && m_ActiveContainers.Count > 0)
		{
			UpdateCurrentItem(UAP_BaseElement.EHighlightSource.Internal);
			if (m_CurrentItem != null)
			{
				ReadItem(m_CurrentItem, quickOnly: true);
			}
			m_ContinuousReading = true;
			m_ContinuousReading_WaitInputClear = true;
		}
	}

	public static void ReadFromTop()
	{
		if (IsEnabled())
		{
			instance.StartReadingfromTop();
		}
	}

	private void Navigate2DUIElement(ESDirection direction)
	{
		if (!HandleUI())
		{
			return;
		}
		UpdateCurrentItem(UAP_BaseElement.EHighlightSource.UserInput);
		if (m_ActiveContainerIndex < 0)
		{
			return;
		}
		bool flag = m_ActiveContainers[m_ActiveContainerIndex].MoveFocus2D(direction);
		int activeContainerIndex = m_ActiveContainerIndex;
		bool flag2 = true;
		if (!flag)
		{
			if (!m_ActiveContainers[m_ActiveContainerIndex].IsConstrainedToContainer(direction) && m_ActiveContainers.Count > 1)
			{
				if (IncrementContainer())
				{
					m_ActiveContainers[m_ActiveContainerIndex].JumpToFirst();
				}
				else
				{
					PlaySFX(m_UIBoundsReached);
				}
			}
			else
			{
				PlaySFX(m_UIBoundsReached);
				flag2 = false;
			}
		}
		UpdateCurrentItem(UAP_BaseElement.EHighlightSource.UserInput);
		if (flag2)
		{
			PlaySFX(m_UINavigationClick);
		}
		if (activeContainerIndex != m_ActiveContainerIndex)
		{
			ReadContainerName();
		}
		ReadItem(m_CurrentItem);
	}

	private void DecrementUIElement()
	{
		if (!HandleUI())
		{
			return;
		}
		AccessibleUIGroupRoot.Accessible_UIElement currentItem = m_CurrentItem;
		int activeContainerIndex = m_ActiveContainerIndex;
		UpdateCurrentItem(UAP_BaseElement.EHighlightSource.UserInput);
		if (m_ActiveContainerIndex >= 0)
		{
			if (!m_ActiveContainers[m_ActiveContainerIndex].DecrementCurrentItem(m_CyclicMenus) && m_ActiveContainers.Count > 1 && DecrementContainer())
			{
				m_ActiveContainers[m_ActiveContainerIndex].JumpToLast();
			}
			UpdateCurrentItem(UAP_BaseElement.EHighlightSource.UserInput);
			if (currentItem != m_CurrentItem)
			{
				PlaySFX(m_UINavigationClick);
			}
			else
			{
				PlaySFX(m_UIBoundsReached);
			}
			if (activeContainerIndex != m_ActiveContainerIndex)
			{
				ReadContainerName();
			}
			ReadItem(m_CurrentItem);
		}
	}

	private bool DecrementContainer(bool resetToStartItem = false)
	{
		if (!HandleUI())
		{
			return false;
		}
		if (m_ActiveContainerIndex > 0 || m_CyclicMenus)
		{
			m_ActiveContainerIndex--;
			if (m_ActiveContainerIndex < 0)
			{
				m_ActiveContainerIndex = m_ActiveContainers.Count - 1;
			}
			if (resetToStartItem && m_ActiveContainers.Count > 0)
			{
				m_ActiveContainers[m_ActiveContainerIndex].ResetToStart();
			}
			return true;
		}
		return false;
	}

	private void IncrementUIElement()
	{
		if (!HandleUI())
		{
			return;
		}
		AccessibleUIGroupRoot.Accessible_UIElement currentItem = m_CurrentItem;
		int activeContainerIndex = m_ActiveContainerIndex;
		UpdateCurrentItem(UAP_BaseElement.EHighlightSource.UserInput);
		if (m_ActiveContainerIndex >= 0)
		{
			if (!m_ActiveContainers[m_ActiveContainerIndex].IncrementCurrentItem(m_CyclicMenus) && m_ActiveContainers.Count > 1 && IncrementContainer())
			{
				m_ActiveContainers[m_ActiveContainerIndex].JumpToFirst();
			}
			UpdateCurrentItem(UAP_BaseElement.EHighlightSource.UserInput);
			if (currentItem != m_CurrentItem)
			{
				PlaySFX(m_UINavigationClick);
			}
			else
			{
				PlaySFX(m_UIBoundsReached);
			}
			if (activeContainerIndex != m_ActiveContainerIndex)
			{
				ReadContainerName();
			}
			ReadItem(m_CurrentItem);
		}
	}

	private void PlaySFX(AudioClip clip)
	{
		if (!(clip == null) && !(m_SFXPlayer == null))
		{
			m_SFXPlayer.PlayOneShot(clip, 0.8f);
		}
	}

	private bool IncrementContainer(bool resetToStartItem = false)
	{
		if (!HandleUI())
		{
			return false;
		}
		if (m_ActiveContainerIndex < m_ActiveContainers.Count - 1 || m_CyclicMenus)
		{
			m_ActiveContainerIndex++;
			if (m_ActiveContainerIndex >= m_ActiveContainers.Count)
			{
				m_ActiveContainerIndex = 0;
			}
			if (resetToStartItem && m_ActiveContainers.Count > 0)
			{
				m_ActiveContainers[m_ActiveContainerIndex].ResetToStart();
			}
			return true;
		}
		return false;
	}

	private void UpdateScrubDetection()
	{
		if (m_ExploreByTouch_IsActive)
		{
			return;
		}
		int touchCount = GetTouchCount();
		if (m_ScrubWaitForLift)
		{
			if (touchCount == 0)
			{
				m_ScrubWaitForLift = false;
			}
		}
		else if (touchCount == 2)
		{
			if (m_ScrubPhaseIndex == 0)
			{
				m_ScrubPhaseIndex = 1;
				m_ScrubPhaseTimeoutTimer = m_ScrubPhaseMaxDuration;
				m_ScrubPhaseStartPoint = GetTouchPosition();
				m_ScrubPhaseLastPoint = m_ScrubPhaseStartPoint;
				return;
			}
			m_ScrubPhaseTimeoutTimer -= Time.unscaledDeltaTime;
			if (m_ScrubPhaseTimeoutTimer <= 0f)
			{
				AbortScrubDetection();
				return;
			}
			Vector3 touchPosition = GetTouchPosition();
			if (Mathf.Abs(touchPosition.x - m_ScrubPhaseLastPoint.x) < 5f)
			{
				return;
			}
			bool flag = false;
			if ((m_ScrubPhaseIndex != 1 && m_ScrubPhaseIndex != 3) ? (touchPosition.x < m_ScrubPhaseLastPoint.x) : (touchPosition.x > m_ScrubPhaseLastPoint.x))
			{
				m_ScrubPhaseLastPoint = touchPosition;
				return;
			}
			if (m_ScrubPhaseIndex == 3)
			{
				AbortScrubDetection();
				return;
			}
			float num = m_ScrubScreenFractionMinswipeWidth * (float)Mathf.Min(Screen.height, Screen.width);
			if (Mathf.Abs(m_ScrubPhaseLastPoint.x - m_ScrubPhaseStartPoint.x) < num)
			{
				AbortScrubDetection();
				return;
			}
			m_ScrubPhaseIndex++;
			m_ScrubPhaseTimeoutTimer = m_ScrubPhaseMaxDuration;
			m_ScrubPhaseStartPoint = m_ScrubPhaseLastPoint;
			m_ScrubPhaseLastPoint = touchPosition;
			if (m_ScrubPhaseIndex == 3)
			{
				m_SwipeActive = false;
				m_SwipeWaitForLift = true;
			}
		}
		else
		{
			if (m_ScrubPhaseIndex == 0)
			{
				return;
			}
			if (m_ScrubPhaseIndex == 3)
			{
				m_ScrubPhaseLastPoint = GetTouchPosition();
				float num2 = Mathf.Abs(m_ScrubPhaseLastPoint.x - m_ScrubPhaseStartPoint.x);
				float num3 = m_ScrubScreenFractionMinswipeWidth * (float)Mathf.Min(Screen.height, Screen.width);
				if (num2 < num3)
				{
					AbortScrubDetection();
					return;
				}
				if (m_HandleMagicGestures && m_OnBackCallbacks != null)
				{
					m_OnBackCallbacks();
				}
			}
			AbortScrubDetection();
		}
	}

	private void AbortScrubDetection()
	{
		m_ScrubWaitForLift = true;
		m_ScrubPhaseIndex = 0;
		m_ScrubPhaseTimeoutTimer = -1f;
	}

	private void UpdateSwipeDetection()
	{
		if (m_ExploreByTouch_IsActive)
		{
			return;
		}
		int touchCount = GetTouchCount();
		if (!m_SwipeActive)
		{
			if (m_SwipeWaitForLift)
			{
				if (touchCount == 0)
				{
					m_SwipeWaitForLift = false;
				}
			}
			else if (touchCount > 0)
			{
				m_SwipeActive = true;
				m_SwipeStartPos = new Vector2(0f, 0f);
				m_SwipeDeltaTime = 0f;
				m_SwipeTouchCount = touchCount;
				m_SwipeStartPos = GetTouchPosition();
			}
		}
		else if (touchCount < m_SwipeTouchCount)
		{
			Vector2 vector = m_SwipeCurrPos - m_SwipeStartPos;
			float magnitude = vector.magnitude;
			bool flag = true;
			if (m_SwipeDeltaTime < 0.08f)
			{
				flag = false;
			}
			float num = (float)Screen.height * (1f / 17f);
			if (vector.magnitude < num)
			{
				if (m_DebugOutputLabel != null && instance.m_DebugOutput)
				{
					m_DebugOutputLabel.text = "No swipe, minimum distance not reached";
				}
				flag = false;
			}
			if (magnitude / m_SwipeDeltaTime < 600f)
			{
				if (m_DebugOutputLabel != null && instance.m_DebugOutput)
				{
					m_DebugOutputLabel.text = "No swipe, distance/time ratio to small";
				}
				flag = false;
			}
			if (flag)
			{
				ESDirection eSDirection = ESDirection.EUp;
				float num2 = m_SwipeCurrPos.x - m_SwipeStartPos.x;
				float num3 = m_SwipeCurrPos.y - m_SwipeStartPos.y;
				if (Mathf.Abs(num2) > Mathf.Abs(num3))
				{
					eSDirection = ((!(num2 > 0f)) ? ESDirection.ELeft : ESDirection.ERight);
				}
				else
				{
					eSDirection = ((!(num3 > 0f)) ? ESDirection.EDown : ESDirection.EUp);
				}
				if (m_DebugOutputLabel != null && instance.m_DebugOutput)
				{
					m_DebugOutputLabel.text = "Swipe found. Direction " + eSDirection.ToString() + " - Finger Count: " + m_SwipeTouchCount;
				}
				if (!m_Paused && m_IsEnabled && HandleUI())
				{
					OnSwipe(eSDirection, m_SwipeTouchCount);
				}
			}
			m_SwipeActive = false;
			if (m_SwipeTouchCount > 0)
			{
				m_SwipeWaitForLift = true;
			}
			else
			{
				m_SwipeWaitForLift = false;
			}
		}
		else
		{
			m_SwipeCurrPos = GetTouchPosition();
			m_SwipeTouchCount = GetTouchCount();
			m_SwipeDeltaTime += Time.unscaledDeltaTime;
		}
	}

	private Vector3 GetTouchPosition()
	{
		return Input.mousePosition;
	}

	private int GetTouchCount()
	{
		if (Input.GetMouseButton(0) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftMeta)))
		{
			return 2;
		}
		if (Input.GetMouseButton(1) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftMeta)))
		{
			return 3;
		}
		if (Input.GetMouseButton(0))
		{
			return 1;
		}
		return 0;
	}

	public static void Say(string textToSay, bool canBeInterrupted = true, bool allowVoiceOver = true, UAP_AudioQueue.EInterrupt interrupts = UAP_AudioQueue.EInterrupt.Elements)
	{
		if (IsEnabled())
		{
			instance.Say_Internal(textToSay, canBeInterrupted, allowVoiceOver, interrupts);
		}
	}

	public static void SaySkippable(string textToSay)
	{
		if (IsEnabled() && !instance.m_AudioQueue.IsPlayingExceptAppOrSkippable())
		{
			instance.m_AudioQueue.QueueAudio(textToSay, UAP_AudioQueue.EAudioType.Skippable, allowVoiceOver: true);
		}
	}

	public static void SayAs(string textToSay, UAP_AudioQueue.EAudioType sayAs, UAP_AudioQueue.UAP_GenericCallback callback = null)
	{
		if (IsEnabled())
		{
			instance.m_AudioQueue.QueueAudio(textToSay, sayAs, allowVoiceOver: true, callback, UAP_AudioQueue.EInterrupt.Elements);
		}
	}

	private void Say_Internal(string textToSay, bool canBeInterrupted = true, bool allowVoiceOver = true, UAP_AudioQueue.EInterrupt interrupts = UAP_AudioQueue.EInterrupt.Elements)
	{
		m_AudioQueue.QueueAudio(textToSay, UAP_AudioQueue.EAudioType.App, allowVoiceOver, null, interrupts, canBeInterrupted);
	}

	public static bool IsSpeaking()
	{
		Initialize();
		return instance.m_AudioQueue.IsPlaying();
	}

	private void OnApplicationPause(bool paused)
	{
		if (!m_IsInitialized)
		{
			return;
		}
		if (paused)
		{
			SavePluginEnabledState();
		}
		StopContinuousReading();
		if (!m_IsEnabled && !paused && m_RecheckAutoEnablingOnResume && ShouldAutoEnable())
		{
			EnableAccessibility(enable: true);
			if (m_OnAccessibilityModeChanged != null)
			{
				m_OnAccessibilityModeChanged(enabled: true);
			}
		}
		if (m_IsEnabled && !m_Paused && !paused)
		{
			StartScreenOver();
		}
	}

	private void StartScreenOver()
	{
		if (m_ActiveContainers.Count > 0)
		{
			m_ActiveContainers[0].ResetToStart();
			UpdateCurrentItem(UAP_BaseElement.EHighlightSource.Internal);
			ReadItem(m_CurrentItem);
		}
	}

	public static bool IsTalkBackEnabledAndTouchExploreActive()
	{
		return false;
	}

	public static bool IsTalkBackEnabled()
	{
		return false;
	}

	public static void RegisterOnPauseToggledCallback(OnPauseToggleCallbackFunc func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnPauseToggleCallbacks = (OnPauseToggleCallbackFunc)Delegate.Combine(uAP_AccessibilityManager.m_OnPauseToggleCallbacks, func);
	}

	public static void UnregisterOnPauseToggledCallback(OnPauseToggleCallbackFunc func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnPauseToggleCallbacks = (OnPauseToggleCallbackFunc)Delegate.Remove(uAP_AccessibilityManager.m_OnPauseToggleCallbacks, func);
	}

	public static void RegisterOnBackCallback(OnTapEvent func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnBackCallbacks = (OnTapEvent)Delegate.Combine(uAP_AccessibilityManager.m_OnBackCallbacks, func);
	}

	public static void UnregisterOnBackCallback(OnTapEvent func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnBackCallbacks = (OnTapEvent)Delegate.Remove(uAP_AccessibilityManager.m_OnBackCallbacks, func);
	}

	public static void RegisterOnTwoFingerSingleTapCallback(OnTapEvent func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnTwoFingerSingleTapCallbacks = (OnTapEvent)Delegate.Combine(uAP_AccessibilityManager.m_OnTwoFingerSingleTapCallbacks, func);
	}

	public static void UnregisterOnTwoFingerSingleTapCallback(OnTapEvent func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnTwoFingerSingleTapCallbacks = (OnTapEvent)Delegate.Remove(uAP_AccessibilityManager.m_OnTwoFingerSingleTapCallbacks, func);
	}

	public static void RegisterOnThreeFingerSingleTapCallback(OnTapEvent func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnThreeFingerSingleTapCallbacks = (OnTapEvent)Delegate.Combine(uAP_AccessibilityManager.m_OnThreeFingerSingleTapCallbacks, func);
	}

	public static void UnregisterOnThreeFingerSingleTapCallback(OnTapEvent func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnThreeFingerSingleTapCallbacks = (OnTapEvent)Delegate.Remove(uAP_AccessibilityManager.m_OnThreeFingerSingleTapCallbacks, func);
	}

	public static void RegisterOnThreeFingerDoubleTapCallback(OnTapEvent func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnThreeFingerDoubleTapCallbacks = (OnTapEvent)Delegate.Combine(uAP_AccessibilityManager.m_OnThreeFingerDoubleTapCallbacks, func);
	}

	public static void UnregisterOnThreeFingerDoubleTapCallback(OnTapEvent func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnThreeFingerDoubleTapCallbacks = (OnTapEvent)Delegate.Remove(uAP_AccessibilityManager.m_OnThreeFingerDoubleTapCallbacks, func);
	}

	public static void RegisterAccessibilityModeChangeCallback(OnAccessibilityModeChanged func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnAccessibilityModeChanged = (OnAccessibilityModeChanged)Delegate.Combine(uAP_AccessibilityManager.m_OnAccessibilityModeChanged, func);
	}

	public static void UnregisterAccessibilityModeChangeCallback(OnAccessibilityModeChanged func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnAccessibilityModeChanged = (OnAccessibilityModeChanged)Delegate.Remove(uAP_AccessibilityManager.m_OnAccessibilityModeChanged, func);
	}

	public static void SetTwoFingerSwipeUpHandler(OnTapEvent func)
	{
		Initialize();
		instance.m_OnTwoFingerSwipeUpHandler = func;
	}

	public static void ResetTwoFingerSwipeUpHandler()
	{
		Initialize();
		instance.m_OnTwoFingerSwipeUpHandler = null;
	}

	public static void RegisterOnTwoFingerSwipeLeftCallback(OnTapEvent func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnTwoFingerSwipeLeftHandler = (OnTapEvent)Delegate.Combine(uAP_AccessibilityManager.m_OnTwoFingerSwipeLeftHandler, func);
	}

	public static void UnregisterOnTwoFingerSwipeLeftCallback(OnTapEvent func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnTwoFingerSwipeLeftHandler = (OnTapEvent)Delegate.Remove(uAP_AccessibilityManager.m_OnTwoFingerSwipeLeftHandler, func);
	}

	public static void RegisterOnTwoFingerSwipeRightCallback(OnTapEvent func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnTwoFingerSwipeRightHandler = (OnTapEvent)Delegate.Combine(uAP_AccessibilityManager.m_OnTwoFingerSwipeRightHandler, func);
	}

	public static void UnregisterOnTwoFingerSwipeRightCallback(OnTapEvent func)
	{
		Initialize();
		UAP_AccessibilityManager uAP_AccessibilityManager = instance;
		uAP_AccessibilityManager.m_OnTwoFingerSwipeRightHandler = (OnTapEvent)Delegate.Remove(uAP_AccessibilityManager.m_OnTwoFingerSwipeRightHandler, func);
	}

	public static void SetTwoFingerSwipeDownHandler(OnTapEvent func)
	{
		Initialize();
		instance.m_OnTwoFingerSwipeDownHandler = func;
	}

	public static void ResetTwoFingerSwipeDownHandler()
	{
		Initialize();
		instance.m_OnTwoFingerSwipeDownHandler = null;
	}

	public static bool SelectElement(GameObject element, bool forceRepeatItem = false)
	{
		if (element == null)
		{
			return false;
		}
		if (instance.m_DebugOutput)
		{
			Debug.Log("[Accessibility] Direct Select of GameObject: " + element.name);
		}
		UAP_BaseElement component = element.GetComponent<UAP_BaseElement>();
		if (component == null)
		{
			Debug.LogWarning("[Accessibility] SelectElement: No Accessibility component found on GameObject " + element.name);
			return false;
		}
		return component.SelectItem(forceRepeatItem);
	}

	public static bool MakeActiveContainer(AccessibleUIGroupRoot container, bool forceRepeatActiveItem = false)
	{
		if (instance == null)
		{
			return false;
		}
		if (!instance.m_ActiveContainers.Contains(container))
		{
			if (instance.m_DebugOutput && m_IsEnabled)
			{
				Debug.LogWarning("[Accessibility] Trying to select an item in a container that is inactive. Ignoring call.");
			}
			return false;
		}
		StopContinuousReading();
		int i;
		for (i = 0; i < instance.m_ActiveContainers.Count && !(instance.m_ActiveContainers[i] == container); i++)
		{
		}
		AccessibleUIGroupRoot.Accessible_UIElement currentElement = instance.m_ActiveContainers[i].GetCurrentElement(instance.m_CyclicMenus);
		bool flag = instance.m_CurrentItem == currentElement;
		instance.m_CurrentElementHasSoleFocus = false;
		instance.m_CurrentItem = currentElement;
		instance.m_ActiveContainerIndex = i;
		instance.UpdateCurrentItem(UAP_BaseElement.EHighlightSource.Internal);
		if (!flag | forceRepeatActiveItem)
		{
			ReadItem(instance.m_CurrentItem);
		}
		return true;
	}

	public static GameObject GetCurrentFocusObject()
	{
		Initialize();
		if (instance.m_CurrentItem == null)
		{
			return null;
		}
		return instance.m_CurrentItem.m_Object.gameObject;
	}

	private bool HandleUI()
	{
		if (m_BlockInput)
		{
			return false;
		}
		return m_HandleUI;
	}

	public static bool UseAndroidTTS()
	{
		Initialize();
		return instance.m_AndroidTTS;
	}

	public static bool UseiOSTTS()
	{
		Initialize();
		return instance.m_iOSTTS;
	}

	public static bool UseWindowsTTS()
	{
		Initialize();
		return instance.m_WindowsTTS;
	}

	public static bool UseMacOSTTS()
	{
		Initialize();
		return instance.m_MacOSTTS;
	}

	public static bool UseWebGLTTS()
	{
		Initialize();
		return instance.m_WebGLTTS;
	}

	public static void RecalculateUIElementsOrder(GameObject parent = null)
	{
		if (parent != null)
		{
			AccessibleUIGroupRoot[] componentsInChildren = parent.GetComponentsInChildren<AccessibleUIGroupRoot>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].RefreshContainer();
			}
		}
		else
		{
			AccessibleUIGroupRoot[] componentsInChildren = UnityEngine.Object.FindObjectsByType(typeof(AccessibleUIGroupRoot), FindObjectsSortMode.InstanceID) as AccessibleUIGroupRoot[];
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].RefreshContainer();
			}
		}
	}

	public static int GetSpeechRate()
	{
		Initialize();
		return instance.m_AudioQueue.GetSpeechRate();
	}

	public static int SetSpeechRate(int speechRate)
	{
		Initialize();
		return instance.m_AudioQueue.SetSpeechRate(speechRate);
	}

	public static void StopSpeaking()
	{
		Initialize();
		instance.m_AudioQueue.Stop();
	}

	public static string Localize(string key)
	{
		return Localize_Internal(key);
	}

	public static string Localize_Internal(string key)
	{
		if (m_CurrentLocalizationTable == null)
		{
			return key;
		}
		if (m_CurrentLocalizationTable.ContainsKey(key))
		{
			return m_CurrentLocalizationTable[key];
		}
		Debug.LogWarning("[Accessibility] No localization available for key '" + key + "'");
		return key;
	}

	public static bool IsVoiceOverAllowed()
	{
		if (instance == null)
		{
			return true;
		}
		return instance.m_AllowVoiceOverGlobal;
	}

	public static string FormatNumberToCurrentLocale(ulong intNumber)
	{
		if (sIsEuropeanLanguage)
		{
			return string.Format(CultureInfo.GetCultureInfoByIetfLanguageTag("de-DE"), "{0:n0}", intNumber);
		}
		return string.Format(CultureInfo.CurrentUICulture, "{0:n0}", intNumber);
	}

	public static string FormatNumberToCurrentLocale(double floatNumber)
	{
		if (sIsEuropeanLanguage)
		{
			return string.Format(CultureInfo.GetCultureInfoByIetfLanguageTag("de-DE"), "{0:0.##}", floatNumber);
		}
		return string.Format(CultureInfo.CurrentUICulture, "{0:0.##}", floatNumber);
	}

	private static void DetectEuropeanLanguage()
	{
		string text = m_CurrentLanguage.ToLower();
		if (string.IsNullOrEmpty(text))
		{
			text = Application.systemLanguage.ToString().ToLower();
		}
		sIsEuropeanLanguage = false;
		switch (text)
		{
		case "german":
		case "portuguese":
		case "spanish":
		case "french":
		case "italian":
			sIsEuropeanLanguage = true;
			break;
		default:
			sIsEuropeanLanguage = false;
			break;
		}
	}

	private static void Log(string message)
	{
		if (!(instance == null) && instance.m_DebugOutput)
		{
			Debug.Log("[Accessibility] " + message);
		}
	}
}
