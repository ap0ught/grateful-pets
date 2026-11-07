using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI;

namespace Qud.UI;

[HasModSensitiveStaticCache]
public class UIManager : MonoBehaviour
{
	public static Canvas mainCanvas;

	private Array allKeyCodes;

	public List<WindowBase> windows = new List<WindowBase>();

	public Sprite DefaultSquare;

	public static UIManager instance;

	public Dictionary<string, WindowBase> windowsByName = new Dictionary<string, WindowBase>();

	public WindowBase _currentWindow;

	private Stack<Stack<WindowBase>> saveStack = new Stack<Stack<WindowBase>>();

	private Stack<WindowBase> windowStack = new Stack<WindowBase>();

	private static uint copyNumber = 0u;

	private static Queue<WindowBase> popupMessages = new Queue<WindowBase>();

	private CanvasScaler _scaler;

	public static int _WindowFramePin = 0;

	public static bool initComplete = false;

	public WindowBase currentWindow
	{
		get
		{
			return _currentWindow;
		}
		set
		{
			_currentWindow = value;
		}
	}

	public static bool UseNewPopups => Options.ModernUI;

	public static float width
	{
		get
		{
			if (instance?.gameObject == null)
			{
				return 1920f;
			}
			return instance?.gameObject?.GetComponent<RectTransform>()?.sizeDelta.x ?? 1920f;
		}
	}

	public static float height
	{
		get
		{
			if (instance?.gameObject == null)
			{
				return 1080f;
			}
			return instance?.gameObject?.GetComponent<RectTransform>()?.sizeDelta.y ?? 1080f;
		}
	}

	public static float scale => instance?._scaler?.scaleFactor ?? 1f;

	public static int WindowFramePin
	{
		get
		{
			return _WindowFramePin;
		}
		set
		{
			_WindowFramePin = value;
			PlayerPrefs.SetInt("_WindowFramePin", value);
		}
	}

	public static async void FadeUIOut(float time)
	{
		await The.UiContext;
		instance.StartCoroutine(instance._FadeOut(time));
	}

	public static async void FadeUIIn(float time)
	{
		await The.UiContext;
		instance.StartCoroutine(instance._FadeIn(time));
	}

	private IEnumerator _FadeOut(float time)
	{
		CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
		float start = time;
		while (time > 0f)
		{
			time -= Time.deltaTime;
			canvasGroup.alpha = Mathf.Lerp(1f, 0f, 1f - time / start);
			yield return null;
		}
		canvasGroup.alpha = 0f;
	}

	public IEnumerator _FadeIn(float time)
	{
		CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
		float start = time;
		while (time > 0f)
		{
			time -= Time.deltaTime;
			canvasGroup.alpha = Mathf.Lerp(0f, 1f, 1f - time / start);
			yield return null;
		}
		canvasGroup.alpha = 1f;
	}

	public void DestroyWindow(WindowBase window)
	{
		if (!(window == null))
		{
			windowsByName.Where((KeyValuePair<string, WindowBase> kv) => kv.Value == window).ToList().ForEach((KeyValuePair<string, WindowBase> kv) =>
			{
				windowsByName.Remove(kv.Key);
			});
			windows.Remove(window);
			UnityEngine.Object.Destroy(window.gameObject);
		}
	}

	public static void pushSaveStack()
	{
		instance.saveStack.Push(new Stack<WindowBase>(instance.windowStack));
	}

	public static void popSaveStack()
	{
		instance.windowStack = instance.saveStack.Pop();
	}

	public Vector2 WorldspaceToCanvasSpace(Vector3 WorldSpace, Camera camera)
	{
		if (camera == null)
		{
			return Vector2.zero;
		}
		return camera.WorldToScreenPoint(WorldSpace);
	}

	public bool PassthroughOnTop()
	{
		if (GameManager.Instance.CurrentGameView != null && GameManager.Instance.CurrentGameView.StartsWith("ModernPopup"))
		{
			return false;
		}
		if (!Options.ModernCharacterSheet && Options.ModernUI)
		{
			if (GameManager.Instance._ActiveGameView == "Equipment")
			{
				return true;
			}
			if (GameManager.Instance._ActiveGameView == "Inventory")
			{
				return true;
			}
			if (GameManager.Instance._ActiveGameView == "Status")
			{
				return true;
			}
			if (GameManager.Instance._ActiveGameView == "Factions")
			{
				return true;
			}
			if (GameManager.Instance._ActiveGameView == "Quests")
			{
				return true;
			}
			if (GameManager.Instance._ActiveGameView == "Journal")
			{
				return true;
			}
			if (GameManager.Instance._ActiveGameView == "Tinkering")
			{
				return true;
			}
			if (GameManager.Instance._ActiveGameView == "SkillsAndPowers")
			{
				return true;
			}
		}
		if (!(instance.currentWindow == null))
		{
			return instance.currentWindow.AllowPassthroughInput();
		}
		return true;
	}

	public bool AllowPassthroughInput()
	{
		if (!Options.ModernCharacterSheet && Options.ModernUI)
		{
			if (GameManager.Instance._ActiveGameView == "Equipment")
			{
				return true;
			}
			if (GameManager.Instance._ActiveGameView == "Inventory")
			{
				return true;
			}
			if (GameManager.Instance._ActiveGameView == "Status")
			{
				return true;
			}
			if (GameManager.Instance._ActiveGameView == "Factions")
			{
				return true;
			}
			if (GameManager.Instance._ActiveGameView == "Quests")
			{
				return true;
			}
			if (GameManager.Instance._ActiveGameView == "Journal")
			{
				return true;
			}
			if (GameManager.Instance._ActiveGameView == "Tinkering")
			{
				return true;
			}
			if (GameManager.Instance._ActiveGameView == "SkillsAndPowers")
			{
				return true;
			}
		}
		for (int i = 0; i < windows.Count; i++)
		{
			if (windows[i].Visible && !windows[i].AllowPassthroughInput())
			{
				return false;
			}
		}
		return true;
	}

	public static void showWindow(string name, bool aggressive = false)
	{
		if (instance.currentWindow != null && instance.currentWindow.name == name)
		{
			return;
		}
		if (aggressive)
		{
			foreach (WindowBase window in instance.windows)
			{
				window._HideWithoutLeave();
			}
		}
		instance._showWindow(name);
	}

	public static T getWindow<T>(string name) where T : WindowBase
	{
		return instance.windowsByName[name] as T;
	}

	public static WindowBase getWindow(string name)
	{
		if (instance == null)
		{
			return null;
		}
		if (instance.windowsByName == null)
		{
			return null;
		}
		if (!instance.windowsByName.ContainsKey(name))
		{
			return null;
		}
		return instance.windowsByName[name];
	}

	public static WindowBase createWindow(string name, Type scriptType = null, Transform parent = null)
	{
		if (scriptType == null)
		{
			scriptType = typeof(WindowBase);
		}
		if (parent == null)
		{
			parent = instance.transform;
		}
		GameObject gameObject = new GameObject();
		gameObject.SetActive(value: false);
		gameObject.AddComponent(scriptType);
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		gameObject.transform.SetAsLastSibling();
		RectTransform component = gameObject.GetComponent<RectTransform>();
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.sizeDelta = Vector2.zero;
		WindowBase component2 = gameObject.GetComponent<WindowBase>();
		instance.windowsByName.Set(name, component2);
		return component2;
	}

	public static WindowBase copyWindow(string name)
	{
		WindowBase windowBase;
		lock (popupMessages)
		{
			if (popupMessages.Count > 0)
			{
				windowBase = popupMessages.Dequeue();
				windowBase.gameObject.SetActive(value: true);
			}
			else
			{
				windowBase = UnityEngine.Object.Instantiate(instance.windowsByName[name]);
				windowBase.gameObject.name = $"{name} #{copyNumber++}";
				instance.windowsByName[windowBase.gameObject.name] = windowBase;
				windowBase.transform.SetParent(instance.transform, worldPositionStays: false);
			}
		}
		return windowBase;
	}

	public static void releaseCopy(WindowBase window)
	{
		lock (popupMessages)
		{
			if (!popupMessages.Contains(window))
			{
				popupMessages.Enqueue(window);
				window.gameObject.SetActive(value: false);
			}
		}
	}

	public static void pushWindow(string name, bool hideOld = false)
	{
		GameManager.Instance.uiQueue.awaitTask(() =>
		{
			if (instance.currentWindow != null)
			{
				instance.windowStack.Push(instance.currentWindow);
				if (!hideOld)
				{
					instance.currentWindow = null;
				}
			}
			showWindow(name);
		});
	}

	public static void popWindow()
	{
		if (instance.currentWindow != null)
		{
			instance.currentWindow.Hide();
			instance.currentWindow = null;
		}
		if (instance.windowStack.Count > 0)
		{
			instance.currentWindow = instance.windowStack.Pop();
			instance.currentWindow.Show();
		}
	}

	private void _showWindow(string name)
	{
		DebugConsole.WriteLine("[UIManager] showing window " + name);
		if (name != null)
		{
			if (currentWindow != null && GameManager.Instance.GetViewData(name).OverlayMode == 0 && (!(currentWindow == SingletonWindowBase<StatusScreensScreen>.instance) || !(name == "PickGameObject")))
			{
				currentWindow.Hide();
			}
			currentWindow = windowsByName[name];
			currentWindow.Show();
		}
		else
		{
			currentWindow?.Hide();
			currentWindow = null;
		}
	}

	public void DiscoverWindows(string basePath, Transform root)
	{
		foreach (Transform item in root)
		{
			WindowBase component = item.gameObject.GetComponent<WindowBase>();
			if (component != null)
			{
				windows.Add(component);
				windowsByName.Add(basePath + component.name, component);
			}
			else if (basePath == "")
			{
				DiscoverWindows(item.name + "/", item.transform);
			}
		}
	}

	public void Init()
	{
		mainCanvas = GetComponent<Canvas>();
		_WindowFramePin = PlayerPrefs.GetInt("_WindowFramePin", 0);
		instance = this;
		_scaler = GetComponent<CanvasScaler>();
		allKeyCodes = Enum.GetValues(typeof(KeyCode));
		windows = new List<WindowBase>();
		windowsByName = new Dictionary<string, WindowBase>();
		DiscoverWindows("", gameObject.transform);
		foreach (WindowBase window in windows)
		{
			try
			{
				if (window.GetComponent<CanvasGroup>() == null)
				{
					window.gameObject.AddComponent<CanvasGroup>();
				}
				window.Init();
				window.Hide();
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Exception intializing window " + window.name, x);
			}
		}
		Update();
		initComplete = true;
	}

	[ModSensitiveCacheInit]
	private static void ModReload()
	{
		GameManager.Instance.uiQueue.queueTask(() =>
		{
			if (instance != null)
			{
				foreach (WindowBase window in instance.windows)
				{
					window.Init();
				}
			}
		});
	}

	private void Update()
	{
		if (!Mathf.Approximately((float)Options.StageScale, _scaler.scaleFactor))
		{
			_scaler.scaleFactor = (float)Options.StageScale;
			Canvas.ForceUpdateCanvases();
		}
	}
}
