using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

namespace Qud.UI;

public class MovableSceneFrameWindowBase<T> : SingletonWindowBase<T> where T : class, new()
{
	public GameObject content;

	public Canvas contentCanvas;

	public RectTransform windowFrame;

	public GameObject toggle;

	public MoveableSceneFrameHeader sceneFrameHeader;

	public MovableSceneFrameCornerTab cornerTab;

	public bool Docked;

	public bool Masked;

	private bool _initcomplete;

	private GameManager.PreferredSidebarPosition preferredSide = GameManager.PreferredSidebarPosition.Left;

	private GameManager.PreferredSidebarPosition currentSide = GameManager.PreferredSidebarPosition.Left;

	public float preferredXOffset = float.MinValue;

	public float preferredYOffset = float.MinValue;

	public float preferredWidth = float.MinValue;

	public float preferredHeight = float.MinValue;

	public string preferredPinnedSide;

	public float LeftSideTransform()
	{
		return (currentSide == GameManager.PreferredSidebarPosition.Left) ? 1 : (-1);
	}

	public void Awake()
	{
		_initcomplete = false;
	}

	public virtual void Update()
	{
		if (!_initcomplete)
		{
			Init();
		}
		if (!_initcomplete || !gameObject.activeInHierarchy)
		{
			return;
		}
		bool flag = false;
		if (Options.ShiftHidesSidebar && Keyboard.bShift && GameManager.Instance?.CurrentGameView == "Stage")
		{
			flag = true;
		}
		if (flag)
		{
			canvas.enabled = false;
		}
		else
		{
			canvas.enabled = raycaster.enabled;
		}
		if (Docked)
		{
			if (sceneFrameHeader.gameObject.activeSelf)
			{
				sceneFrameHeader.ToggleBackgroundOpacity(2);
				sceneFrameHeader.gameObject.SetActive(value: false);
				cornerTab.gameObject.SetActive(value: false);
			}
			return;
		}
		if (UIManager._WindowFramePin == 0 != sceneFrameHeader.gameObject.activeSelf)
		{
			sceneFrameHeader?.gameObject.SetActive(UIManager._WindowFramePin == 0);
			cornerTab?.gameObject.SetActive(UIManager._WindowFramePin == 0);
		}
		if (preferredXOffset != windowFrame.anchoredPosition.x * LeftSideTransform() || preferredYOffset != windowFrame.anchoredPosition.y || preferredWidth != windowFrame.sizeDelta.x || preferredHeight != windowFrame.sizeDelta.y)
		{
			EnsureOnscreen();
			preferredXOffset = windowFrame.anchoredPosition.x * LeftSideTransform();
			preferredYOffset = windowFrame.anchoredPosition.y;
			preferredWidth = windowFrame.sizeDelta.x;
			preferredHeight = windowFrame.sizeDelta.y;
			SavePreferences();
		}
		if (preferredSide != currentSide && !sceneFrameHeader.sideMovePin)
		{
			OnPreferredSidebarPositionChanged(preferredSide);
		}
		if (Keyboard.bAlt != Masked && !Docked)
		{
			if (Masked)
			{
				toggle.GetComponent<Mask>().Destroy();
			}
			else
			{
				toggle.AddComponent<Mask>();
			}
			Masked = !Masked;
		}
	}

	public void EnsureOnscreen()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (windowFrame.sizeDelta.x > UIManager.width)
		{
			windowFrame.sizeDelta = new Vector2(UIManager.width, windowFrame.sizeDelta.y);
		}
		if (windowFrame.sizeDelta.y > 0f)
		{
			windowFrame.sizeDelta = new Vector2(windowFrame.sizeDelta.x, 0f);
		}
		if (windowFrame.sizeDelta.x < 200f)
		{
			windowFrame.sizeDelta = new Vector2(200f, windowFrame.sizeDelta.y);
		}
		if (windowFrame.sizeDelta.y < 0f - (UIManager.height - 100f))
		{
			windowFrame.sizeDelta = new Vector2(windowFrame.sizeDelta.x, 0f - (UIManager.height - 100f));
		}
		if (windowFrame.anchorMin.x <= 0f)
		{
			if (windowFrame.anchoredPosition.x < 0f)
			{
				windowFrame.anchoredPosition = new Vector2(0f, windowFrame.anchoredPosition.y);
			}
		}
		else if (windowFrame.anchoredPosition.x > 0f)
		{
			windowFrame.anchoredPosition = new Vector2(0f, windowFrame.anchoredPosition.y);
		}
		if (Mathf.Abs(windowFrame.sizeDelta.x) + Mathf.Abs(windowFrame.anchoredPosition.x) > UIManager.width)
		{
			if (windowFrame.anchorMin.x <= 0f)
			{
				windowFrame.anchoredPosition = new Vector2(UIManager.width - windowFrame.sizeDelta.x, windowFrame.anchoredPosition.y);
			}
			else
			{
				windowFrame.anchoredPosition = new Vector2(windowFrame.sizeDelta.x - UIManager.width, windowFrame.anchoredPosition.y);
			}
		}
		if (windowFrame.anchoredPosition.y < 0f)
		{
			windowFrame.anchoredPosition = new Vector2(windowFrame.anchoredPosition.x, 0f);
		}
		if (windowFrame.anchoredPosition.y > 0f - windowFrame.sizeDelta.y)
		{
			windowFrame.anchoredPosition = new Vector2(windowFrame.anchoredPosition.x, 0f - windowFrame.sizeDelta.y);
		}
	}

	public void UpdateSideMovePin(bool pinned)
	{
		SavePreferences();
	}

	public override void Init()
	{
		if (UIManager.initComplete && !_initcomplete)
		{
			_initcomplete = true;
			sceneFrameHeader.Init();
			sceneFrameHeader.sideMovePinToggled.AddListener(UpdateSideMovePin);
			if (!GameManager.OnPreferredSidebarPositionUpdatedCallbacks.Contains(OnPreferredSidebarPositionChanged))
			{
				GameManager.OnPreferredSidebarPositionUpdatedCallbacks.Add(OnPreferredSidebarPositionChanged);
			}
			LoadPreferences();
			LoadPosition();
			EnsureOnscreen();
			base.Init();
		}
	}

	public virtual void LoadPosition()
	{
		if (preferredXOffset == float.MinValue || preferredYOffset == float.MinValue || preferredWidth == float.MinValue || preferredHeight == float.MinValue)
		{
			return;
		}
		if (sceneFrameHeader.sideMovePin)
		{
			bool sideMovePin = sceneFrameHeader.sideMovePin;
			sceneFrameHeader.sideMovePin = false;
			try
			{
				OnPreferredSidebarPositionChanged((preferredPinnedSide == "left") ? GameManager.PreferredSidebarPosition.Left : GameManager.PreferredSidebarPosition.Right);
			}
			finally
			{
				sceneFrameHeader.sideMovePin = sideMovePin;
			}
		}
		windowFrame.sizeDelta = new Vector2(preferredWidth, preferredHeight);
		windowFrame.anchoredPosition = new Vector2(preferredXOffset, preferredYOffset);
	}

	public virtual void OnPreferredSidebarPositionChanged(GameManager.PreferredSidebarPosition newSide)
	{
		preferredSide = newSide;
		if (sceneFrameHeader.sideMovePin || Docked)
		{
			return;
		}
		if (currentSide != newSide)
		{
			currentSide = newSide;
			switch (newSide)
			{
			case GameManager.PreferredSidebarPosition.Left:
				windowFrame.anchorMin = new Vector2(0f, 0f);
				windowFrame.anchorMax = new Vector2(0f, 1f);
				windowFrame.pivot = new Vector2(0f, 0f);
				windowFrame.anchoredPosition = new Vector2(0f - windowFrame.anchoredPosition.x, windowFrame.anchoredPosition.y);
				break;
			case GameManager.PreferredSidebarPosition.Right:
				windowFrame.anchorMin = new Vector2(1f, 0f);
				windowFrame.anchorMax = new Vector2(1f, 1f);
				windowFrame.pivot = new Vector2(1f, 0f);
				windowFrame.anchoredPosition = new Vector2(0f - windowFrame.anchoredPosition.x, windowFrame.anchoredPosition.y);
				break;
			}
		}
		EnsureOnscreen();
	}

	public void SavePreferences()
	{
		if (GameManager.Instance.DockMovable <= 0)
		{
			string id = gameObject.name + "_windpref_x";
			string id2 = gameObject.name + "_windpref_y";
			string id3 = gameObject.name + "_windpref_w";
			string id4 = gameObject.name + "_windpref_h";
			SetFloatPref(id, preferredXOffset);
			SetFloatPref(id2, preferredYOffset);
			SetFloatPref(id3, preferredWidth);
			SetFloatPref(id4, preferredHeight);
			PlayerPrefs.SetString(gameObject.name + "_windpref_side", (currentSide == GameManager.PreferredSidebarPosition.Left) ? "left" : "right");
		}
	}

	public void LoadPreferences()
	{
		string id = gameObject.name + "_windpref_x";
		string id2 = gameObject.name + "_windpref_y";
		string id3 = gameObject.name + "_windpref_w";
		string id4 = gameObject.name + "_windpref_h";
		preferredXOffset = GetFloatPref(id, float.MinValue);
		preferredYOffset = GetFloatPref(id2, float.MinValue);
		preferredWidth = GetFloatPref(id3, float.MinValue);
		preferredHeight = GetFloatPref(id4, float.MinValue);
		preferredPinnedSide = PlayerPrefs.GetString(gameObject.name + "_windpref_side", null);
	}

	public void SetFloatPref(string id, float value)
	{
		PlayerPrefs.SetFloat(id, value);
	}

	public float GetFloatPref(string id, float def)
	{
		return PlayerPrefs.GetFloat(id, def);
	}
}
