using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Cysharp.Text;
using Genkit;
using Qud.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.CharacterBuilds;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

public class TutorialManager : MonoBehaviour
{
	[Serializable]
	public class TutorialManagerGameSystem : IGameSystem
	{
		public string StepClass;

		public int Step;

		public override void BeforeSave()
		{
			if (currentStep != null)
			{
				StepClass = currentStep?.GetType().ToString();
				Step = currentStep?.step ?? 0;
			}
			else
			{
				StepClass = null;
				Step = 0;
			}
		}

		public override void AfterLoad(XRLGame game)
		{
			if (!string.IsNullOrEmpty(StepClass))
			{
				try
				{
					TutorialStep tutorialStep = ModManager.CreateInstance(StepClass) as TutorialStep;
					if (tutorialStep != null)
					{
						tutorialStep.step = Step;
					}
					StartTutorial(tutorialStep);
					return;
				}
				catch (Exception x)
				{
					MetricsManager.LogException("TutorialManagerGameSystem::AfterLoad", x);
					return;
				}
			}
			currentStep = null;
		}
	}

	private static TutorialManager instance;

	private float lastPaddingX = 64f;

	private float lastPaddingY = 64f;

	private string lastStyle = "big";

	public UnityEngine.GameObject bigFrame;

	public UnityEngine.GameObject horizFrame;

	public UnityEngine.GameObject cellFrame;

	private int popupPaddingX;

	private int popupPaddingY;

	private float popupBottomMargin;

	private bool ShowingPopup;

	private string PopupText;

	private Location2D _PopupCellLocation;

	public string PopupControlID;

	public string PopupDirectionHint;

	private static TaskCompletionSource<bool> PopupCompletion = null;

	public Action afterPopup;

	public UITextSkin buttonText;

	private static string lastSoundText = null;

	public RectTransform highlightRect;

	public string highlightStyle = "big";

	public static TutorialStep currentStep;

	public UnityEngine.GameObject PopupAcceptButton;

	private static List<string> keysByLength = null;

	public UnityEngine.GameObject buttonArrow;

	public RectTransform windowRect;

	private RectTransform lastHighlight;

	public UITextSkin highlightText;

	public string lastText;

	public string lastDirectionHint;

	public Image[] imagesToHideWhenNoFrame;

	public TextMeshProUGUI[] textToHideWhenNoFrame;

	public UnityEngine.GameObject[] goToHideWhenNoFrame;

	public static int TWIDDLE_MENU_YPADDING = 6;

	public bool popupAcceptPressed;

	public Location2D PopupCellLocation
	{
		get
		{
			return _PopupCellLocation;
		}
		set
		{
			_PopupCellLocation = value;
		}
	}

	public static bool IsActive => currentStep != null;

	private void Start()
	{
		if (instance != null)
		{
			Debug.LogError("Two instance sof tutorialmanager!!");
		}
		instance = this;
		ShowingPopup = false;
		highlightRect.gameObject.SetActive(value: false);
	}

	public static void BeforeRenderEvent()
	{
		currentStep?.BeforeRenderEvent();
	}

	public static int AdjustExaminationRoll(int result)
	{
		int? num = currentStep?.AdjustExaminationRoll(result);
		if (!num.HasValue)
		{
			return result;
		}
		return num.Value;
	}

	public static bool StartingLineTooltip(XRL.World.GameObject go, XRL.World.GameObject compareGo)
	{
		return currentStep?.StartingLineTooltip(go, compareGo) ?? true;
	}

	public static bool AllowTooltipUpdate()
	{
		return currentStep?.AllowTooltipUpdate() ?? true;
	}

	public static async Task ShowCellPopup(Location2D cell, string text, string directionHint = "ne", int paddingX = 6, int paddingY = 6, Action after = null)
	{
		if (PopupCompletion != null)
		{
			try
			{
				PopupCompletion.TrySetResult(result: true);
				PopupCompletion = null;
			}
			catch (Exception x)
			{
				MetricsManager.LogException("ShowCellPopup", x);
			}
		}
		PopupCompletion = new TaskCompletionSource<bool>();
		await The.UiContext;
		SoundManager.PlayUISound("Sounds/UI/ui_notification_question", 1f, Combat: false, Interface: true);
		instance.popupPaddingX = paddingX;
		instance.popupPaddingY = paddingY;
		instance.PopupDirectionHint = directionHint;
		if (instance.afterPopup != null)
		{
			Action action = instance.afterPopup;
			instance.afterPopup = null;
			action();
		}
		instance.afterPopup = after;
		GameManager.Instance.SetActiveLayersForNavCategory("Menu", setCurrentCategory: false);
		instance.PopupText = text;
		instance.PopupControlID = null;
		instance.PopupCellLocation = cell;
		SoundManager.PlayUISound("Sounds/UI/ui_notification_question", 1f, Combat: false, Interface: true);
		instance.ShowingPopup = true;
		ControlManager.ResetInput();
		await PopupCompletion.Task;
	}

	public static bool AllowOverlandEncounters()
	{
		return currentStep?.AllowOverlandEncounters() ?? true;
	}

	public static bool OnTradeOffer()
	{
		return currentStep?.OnTradeOffer() ?? true;
	}

	public static bool OnTradeComplete()
	{
		return currentStep?.OnTradeComplete() ?? true;
	}

	public static async Task ShowIntermissionPopupAsync(string message, Action after = null, int yPadding = 0, float bottomMargin = 0f, string hint = "sw")
	{
		await ShowCIDPopupAsync("RootCanvas", message, "s", "[~Accept] Continue", 0, yPadding, bottomMargin, after);
	}

	public static async Task ShowCIDPopupAsync(string cid, string text, string directionHint = "ne", string buttonText = "[~Accept] Continue", int paddingX = 16, int paddingY = 16, float bottomMargin = 0f, Action after = null)
	{
		if (PopupCompletion != null)
		{
			try
			{
				PopupCompletion.TrySetResult(result: true);
				PopupCompletion = null;
			}
			catch (Exception x)
			{
				MetricsManager.LogException("ShowCellPopup", x);
			}
		}
		PopupCompletion = new TaskCompletionSource<bool>();
		await The.UiContext;
		instance.buttonText.SetText(instance.hotkeyReplace(buttonText));
		if (instance.afterPopup != null)
		{
			Action action = instance.afterPopup;
			instance.afterPopup = null;
			action();
		}
		instance.afterPopup = after;
		instance.popupPaddingX = paddingX;
		instance.popupPaddingY = paddingY;
		instance.popupBottomMargin = bottomMargin;
		instance.PopupDirectionHint = directionHint;
		GameManager.Instance.SetActiveLayersForNavCategory("Menu", setCurrentCategory: false);
		instance.PopupText = text;
		instance.PopupControlID = cid;
		instance.PopupCellLocation = null;
		SoundManager.PlayUISound("Sounds/UI/ui_notification_question", 1f, Combat: false, Interface: true);
		instance.ShowingPopup = true;
		ControlManager.ResetInput();
		await PopupCompletion.Task;
	}

	public static void RefreshHighlightPosition()
	{
		instance.Highlight(instance.lastHighlight, instance.lastText, instance.lastDirectionHint, instance.lastPaddingX, instance.lastPaddingY, 0f, instance.lastStyle);
		Canvas.ForceUpdateCanvases();
	}

	public static void StartTutorial(TutorialStep firstStep)
	{
		The.Game.RequireSystem<TutorialManagerGameSystem>();
		currentStep = firstStep;
		if (currentStep != null)
		{
			currentStep.manager = instance;
		}
	}

	public static bool AllowTargetPick(XRL.World.GameObject go, Type ability, List<Cell> target)
	{
		return currentStep?.AllowTargetPick(go, ability, target) ?? true;
	}

	public static bool BeforePlayerEnterCell(Cell cell)
	{
		return currentStep?.BeforePlayerEnterCell(cell) ?? true;
	}

	public static bool IsEditableChargenPanel(string id)
	{
		return currentStep?.IsEditableChargenPanel(id) ?? true;
	}

	public static bool AllowMouseEvent(string ev, int x, int y, object data)
	{
		TutorialManager tutorialManager = instance;
		if ((object)tutorialManager != null && tutorialManager.ShowingPopup)
		{
			return false;
		}
		return true;
	}

	public static bool AllowSelectPregen(string id)
	{
		return currentStep?.AllowSelectPregen(id) ?? true;
	}

	public static bool AllowSelectGenotype(string id)
	{
		return currentStep?.AllowSelectGenotype(id) ?? true;
	}

	public static bool AllowInventoryInteract(XRL.World.GameObject go)
	{
		TutorialManager tutorialManager = instance;
		if ((object)tutorialManager != null && tutorialManager.ShowingPopup)
		{
			return false;
		}
		return currentStep?.AllowInventoryInteract(go) ?? true;
	}

	public static bool AllowSelectedPopupCommand(string id, QudMenuItem command)
	{
		TutorialManager tutorialManager = instance;
		if ((object)tutorialManager != null && tutorialManager.ShowingPopup)
		{
			return false;
		}
		return currentStep?.AllowSelectedPopupCommand(id, command) ?? true;
	}

	public static bool AllowOnSelected(FrameworkDataElement element)
	{
		TutorialManager tutorialManager = instance;
		if ((object)tutorialManager != null && tutorialManager.ShowingPopup)
		{
			return false;
		}
		return currentStep?.AllowOnSelected(element) ?? true;
	}

	public static bool AllowPushKey(Keyboard.XRLKeyEvent ev, bool bAllowMap = false)
	{
		TutorialManager tutorialManager = instance;
		if ((object)tutorialManager != null && tutorialManager.ShowingPopup)
		{
			return false;
		}
		return true;
	}

	public static bool AllowCommand(string Command)
	{
		TutorialManager tutorialManager = instance;
		if ((object)tutorialManager != null && tutorialManager.ShowingPopup)
		{
			return false;
		}
		return currentStep?.AllowCommand(Command) ?? true;
	}

	public static void EndTutorial()
	{
		currentStep = null;
	}

	public static void OnTrigger(string trigger)
	{
		currentStep?.OnTrigger(trigger);
	}

	public static async void ToggleHide()
	{
		await The.UiContext;
		instance.gameObject.SetActive(!instance.gameObject.activeSelf);
	}

	public static void OnBootGame(XRLGame game, EmbarkInfo info)
	{
		if (currentStep != null)
		{
			currentStep?.OnBootGame(game, info);
		}
	}

	public static async void AdvanceStep(TutorialStep nextStep)
	{
		await The.UiContext;
		currentStep = nextStep;
		if (currentStep != null)
		{
			currentStep.manager = instance;
			currentStep.LateUpdate();
		}
	}

	public void After(int milliseconds, Action after)
	{
		Task.Run(async () =>
		{
			await Task.Delay(milliseconds);
			after();
		});
	}

	public void HighlightObject(XRL.World.GameObject go, string text, string directionHint, float paddingX = 3f, float paddingY = 3f)
	{
		HighlightCell((go?.CurrentCell?.X).GetValueOrDefault(), (go?.CurrentCell?.Y).GetValueOrDefault(), text, directionHint, paddingX, paddingY);
	}

	public string hotkeyReplace(string source)
	{
		if (source == null || (!source.Contains("~") && !source.Contains("%")))
		{
			return source;
		}
		using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
		utf16ValueStringBuilder.Append(source);
		if (keysByLength == null)
		{
			keysByLength = CommandBindingManager.CommandBindings.Keys.ToList();
			keysByLength.Sort((string a, string b) => b.Length - a.Length);
		}
		if (utf16ValueStringBuilder.ToString().Contains("~Highlight") && ControlManager.activeControllerType != ControlManager.InputDeviceType.Gamepad)
		{
			utf16ValueStringBuilder.Replace("~Highlight", "{{W|Alt}}");
		}
		for (int num = 0; num < keysByLength.Count; num++)
		{
			string text = keysByLength[num];
			if (utf16ValueStringBuilder.ToString().Contains("~Highlight"))
			{
				utf16ValueStringBuilder.Replace("~" + text, ControlManager.getCommandInputFormatted(text));
				if (!utf16ValueStringBuilder.ToString().Contains("~") && !utf16ValueStringBuilder.ToString().Contains("%"))
				{
					break;
				}
			}
			else if (utf16ValueStringBuilder.ToString().Contains("~" + text))
			{
				string text2 = ControlManager.getCommandInputFormatted(text);
				if (string.IsNullOrEmpty(text2))
				{
					text2 = ((!CommandBindingManager.CommandsByID.TryGetValue(text, out var value)) ? ("{{hotkey|" + text + " [Unbound]}}") : ("{{hotkey|" + value.DisplayText + " [Unbound]}}"));
				}
				utf16ValueStringBuilder.Replace("~" + text, text2);
				if (!utf16ValueStringBuilder.ToString().Contains("~") && !utf16ValueStringBuilder.ToString().Contains("%"))
				{
					break;
				}
			}
			else if (utf16ValueStringBuilder.ToString().Contains("%Highlight"))
			{
				utf16ValueStringBuilder.Replace("%" + text, ControlManager.getCommandInputFormatted(text, mapGlyphs: true, allBinds: true));
				if (!utf16ValueStringBuilder.ToString().Contains("~") && !utf16ValueStringBuilder.ToString().Contains("%"))
				{
					break;
				}
			}
			else if (utf16ValueStringBuilder.ToString().Contains("%" + text))
			{
				utf16ValueStringBuilder.Replace("%" + text, ControlManager.getCommandInputFormatted(text, mapGlyphs: true, allBinds: true));
				if (!utf16ValueStringBuilder.ToString().Contains("~") && !utf16ValueStringBuilder.ToString().Contains("%"))
				{
					break;
				}
			}
		}
		return utf16ValueStringBuilder.ToString();
	}

	public void HighlightCell(int x, int y, string text, string directionHint, float paddingX = 3f, float paddingY = 3f, float bottomMargin = 0f)
	{
		lastHighlight = null;
		lastPaddingX = paddingX;
		lastPaddingY = paddingY;
		lastStyle = "cell";
		if (text != null && (text.Contains("<noframe>") || text.Contains("<no message>")))
		{
			Image[] array = imagesToHideWhenNoFrame;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
			TextMeshProUGUI[] array2 = textToHideWhenNoFrame;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].enabled = false;
			}
		}
		else
		{
			Image[] array = imagesToHideWhenNoFrame;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = true;
			}
			TextMeshProUGUI[] array2 = textToHideWhenNoFrame;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].enabled = true;
			}
		}
		if (text != null)
		{
			text = "{{y|" + text + "}}";
		}
		text = hotkeyReplace(text);
		if (!string.IsNullOrEmpty(text) && lastSoundText != text)
		{
			lastSoundText = text;
			SoundManager.PlayUISound("Sounds/UI/ui_notification_question", 1f, Combat: false, Interface: true);
		}
		highlightText.SetText(text);
		highlightRect.gameObject.SetActive(text != null);
		Vector3 tileCenter = GameManager.Instance.getTileCenter(x, y);
		highlightRect.sizeDelta = new Vector2((16f + paddingX) / Camera.main.GetComponent<LetterboxCamera>().baseOrthographicStageScale, (24f + paddingY) / Camera.main.GetComponent<LetterboxCamera>().baseOrthographicStageScale);
		highlightRect.position = Camera.main.WorldToScreenPoint(tileCenter);
		highlightRect.pivot = new Vector2(0.5f, 0.5f);
		highlightRect.anchorMin = new Vector2(0f, 1f);
		highlightRect.anchorMax = new Vector2(0f, 1f);
		highlightRect.position += new Vector3(0f, bottomMargin, 0f);
		highlightStyle = "cell";
		if (bigFrame.activeSelf != (highlightStyle == "big" && text != "{{y|<nohighlight>}}"))
		{
			bigFrame.SetActive(highlightStyle == "big" && text != "{{y|<nohighlight>}}");
		}
		if (horizFrame.activeSelf != (highlightStyle == "horiz" && text != "{{y|<nohighlight>}}"))
		{
			horizFrame.SetActive(highlightStyle == "horiz" && text != "{{y|<nohighlight>}}");
		}
		if (cellFrame.activeSelf != (highlightStyle == "cell" && text != "{{y|<nohighlight>}}"))
		{
			cellFrame.SetActive(highlightStyle == "cell" && text != "{{y|<nohighlight>}}");
		}
		if (buttonArrow.activeSelf != (text != "{{y|<nohighlight>}}"))
		{
			buttonArrow.SetActive(text != "{{y|<nohighlight>}}");
		}
		if (text == null || GameManager.Instance.CurrentGameView == "PopupMessage")
		{
			SetWindowPosition(null);
		}
		else
		{
			SetWindowPosition(directionHint);
		}
	}

	public void SetWindowPosition(string directionHint)
	{
		if (directionHint == null)
		{
			if (windowRect.gameObject.activeSelf)
			{
				windowRect.gameObject.SetActive(value: false);
			}
			return;
		}
		if (!windowRect.gameObject.activeSelf)
		{
			windowRect.gameObject.SetActive(value: true);
		}
		directionHint = directionHint?.ToUpper();
		if (directionHint == "N" || directionHint == null)
		{
			RectTransform rectTransform = windowRect;
			Vector2 anchorMin = (windowRect.anchorMax = new Vector2(0.5f, 1f));
			rectTransform.anchorMin = anchorMin;
			windowRect.pivot = new Vector2(0.5f, 0f);
		}
		if (directionHint == "S" || directionHint == null)
		{
			RectTransform rectTransform2 = windowRect;
			Vector2 anchorMin = (windowRect.anchorMax = new Vector2(0.5f, 0f));
			rectTransform2.anchorMin = anchorMin;
			windowRect.pivot = new Vector2(0.5f, 1f);
		}
		if (directionHint == "E" || directionHint == null)
		{
			RectTransform rectTransform3 = windowRect;
			Vector2 anchorMin = (windowRect.anchorMax = new Vector2(1f, 0.5f));
			rectTransform3.anchorMin = anchorMin;
			windowRect.pivot = new Vector2(0f, 0.5f);
		}
		if (directionHint == "W" || directionHint == null)
		{
			RectTransform rectTransform4 = windowRect;
			Vector2 anchorMin = (windowRect.anchorMax = new Vector2(0f, 0.5f));
			rectTransform4.anchorMin = anchorMin;
			windowRect.pivot = new Vector2(1f, 0.5f);
		}
		if (directionHint == "NE" || directionHint == null)
		{
			RectTransform rectTransform5 = windowRect;
			Vector2 anchorMin = (windowRect.anchorMax = new Vector2(1f, 1f));
			rectTransform5.anchorMin = anchorMin;
			windowRect.pivot = new Vector2(0f, 0f);
		}
		if (directionHint == "SE" || directionHint == null)
		{
			RectTransform rectTransform6 = windowRect;
			Vector2 anchorMin = (windowRect.anchorMax = new Vector2(1f, 0f));
			rectTransform6.anchorMin = anchorMin;
			windowRect.pivot = new Vector2(0f, 1f);
		}
		if (directionHint == "SW" || directionHint == null)
		{
			RectTransform rectTransform7 = windowRect;
			Vector2 anchorMin = (windowRect.anchorMax = new Vector2(0f, 0f));
			rectTransform7.anchorMin = anchorMin;
			windowRect.pivot = new Vector2(1f, 1f);
		}
		if (directionHint == "NW" || directionHint == null)
		{
			RectTransform rectTransform8 = windowRect;
			Vector2 anchorMin = (windowRect.anchorMax = new Vector2(0f, 1f));
			rectTransform8.anchorMin = anchorMin;
			windowRect.pivot = new Vector2(1f, 0f);
		}
		windowRect.anchoredPosition = new Vector2(0f, 0f);
	}

	public bool HighlightByCID(string CID, string text, string directionHint, int paddingX = 64, int paddingY = 64, float bottomMargin = 0f, string style = "horiz")
	{
		if (text != null)
		{
			text = "{{y|" + text + "}}";
		}
		RectTransform rectTransform = ControlId.Get(CID)?.gameObject?.transform as RectTransform;
		Highlight(rectTransform, text, directionHint, paddingX, paddingY, bottomMargin, style);
		if (rectTransform == null)
		{
			return false;
		}
		return true;
	}

	public void ClearHighlight()
	{
		Highlight(null, null, null);
	}

	public void Highlight(RectTransform target, string text, string directionHint, float paddingX = 64f, float paddingY = 64f, float bottomMargin = 0f, string style = "big")
	{
		lastText = text;
		lastDirectionHint = directionHint;
		lastPaddingX = paddingX;
		lastPaddingY = paddingY;
		lastStyle = style;
		if (bigFrame.activeSelf != (style == "big" && text != "{{y|<nohighlight>}}"))
		{
			bigFrame.SetActive(style == "big" && text != "{{y|<nohighlight>}}");
		}
		if (horizFrame.activeSelf != (style == "horiz" && text != "{{y|<nohighlight>}}"))
		{
			horizFrame.SetActive(style == "horiz" && text != "{{y|<nohighlight>}}");
		}
		if (cellFrame.activeSelf != (style == "cell" && text != "{{y|<nohighlight>}}"))
		{
			cellFrame.SetActive(style == "cell" && text != "{{y|<nohighlight>}}");
		}
		if (buttonArrow.activeSelf != (text != "{{y|<nohighlight>}}"))
		{
			buttonArrow.SetActive(text != "{{y|<nohighlight>}}");
		}
		if (text != null && (text.Contains("<noframe>") || text.Contains("<no message>") || text.Contains("<nohighlight>")))
		{
			Image[] array = imagesToHideWhenNoFrame;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
			TextMeshProUGUI[] array2 = textToHideWhenNoFrame;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].enabled = false;
			}
			UnityEngine.GameObject[] array3 = goToHideWhenNoFrame;
			foreach (UnityEngine.GameObject gameObject in array3)
			{
				if (gameObject.activeSelf)
				{
					gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			Image[] array = imagesToHideWhenNoFrame;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = true;
			}
			TextMeshProUGUI[] array2 = textToHideWhenNoFrame;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].enabled = true;
			}
			UnityEngine.GameObject[] array3 = goToHideWhenNoFrame;
			foreach (UnityEngine.GameObject gameObject2 in array3)
			{
				if (!gameObject2.activeSelf)
				{
					gameObject2.SetActive(value: true);
				}
			}
		}
		if (text != null)
		{
			text = "{{y|" + text + "}}";
		}
		text = hotkeyReplace(text);
		if (!string.IsNullOrEmpty(text) && lastSoundText != text)
		{
			lastSoundText = text;
			SoundManager.PlayUISound("Sounds/UI/ui_notification_question", 1f, Combat: false, Interface: true);
		}
		highlightStyle = style;
		highlightText.SetText(text);
		if (highlightRect.gameObject.activeSelf != (target != null))
		{
			highlightRect.gameObject.SetActive(target != null);
		}
		if (target != null)
		{
			highlightRect.CopyFrom(target.gameObject.transform as RectTransform);
			highlightRect.position = target.gameObject.transform.position;
			Vector2 vector = new Vector2(highlightRect.sizeDelta.x + paddingX, highlightRect.sizeDelta.y + paddingY);
			highlightRect.transform.position += new Vector3(0f, bottomMargin);
			if (vector != highlightRect.sizeDelta)
			{
				highlightRect.sizeDelta = vector;
				Canvas.ForceUpdateCanvases();
			}
		}
		SetWindowPosition(directionHint);
		if (lastHighlight != target)
		{
			lastHighlight = target;
			Canvas.ForceUpdateCanvases();
		}
	}

	public void OnPopupAcceptPressed()
	{
		popupAcceptPressed = true;
	}

	private void Update()
	{
		if (ShowingPopup)
		{
			transform.SetAsLastSibling();
			if (!ControlManager.EnabledLayers.Contains("UINav"))
			{
				GameManager.Instance.SetActiveLayersForNavCategory("Menu", setCurrentCategory: false);
			}
			if (ControlManager.GetButtonDown("Accept", skipTutorialCheck: true) || popupAcceptPressed)
			{
				popupAcceptPressed = false;
				try
				{
					PopupCompletion?.SetResult(result: true);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Tutorial Popup Update", x);
				}
				ShowingPopup = false;
				ClearHighlight();
				GameManager.Instance.SetActiveLayersForNavCategory(GameManager.Instance.currentNavCategory, setCurrentCategory: false);
				try
				{
					if (afterPopup != null)
					{
						Action action = afterPopup;
						afterPopup = null;
						action();
					}
					return;
				}
				catch (Exception x2)
				{
					MetricsManager.LogException("After tutorial popup", x2);
					return;
				}
			}
			if (PopupCellLocation != null)
			{
				HighlightCell(PopupCellLocation.X, PopupCellLocation.Y, PopupText, PopupDirectionHint, popupPaddingX, popupPaddingY, popupBottomMargin);
				PopupAcceptButton.SetActive(value: true);
			}
			else if (PopupControlID != null)
			{
				if (!HighlightByCID(PopupControlID, PopupText, PopupDirectionHint, popupPaddingX, popupPaddingY, popupBottomMargin))
				{
					HighlightCell(0, 0, PopupText, PopupDirectionHint, popupPaddingX, popupPaddingY, popupBottomMargin);
				}
				PopupAcceptButton.SetActive(value: true);
			}
		}
		else if (currentStep == null)
		{
			if (highlightRect.gameObject.activeInHierarchy)
			{
				highlightRect.gameObject.SetActive(value: false);
			}
		}
		else
		{
			PopupAcceptButton.SetActive(value: false);
			try
			{
				currentStep?.LateUpdate();
			}
			catch (Exception x3)
			{
				MetricsManager.LogException("TutorialManager::LateUpdate", x3);
			}
			transform.SetAsLastSibling();
		}
	}

	protected void _GameSync()
	{
		currentStep?.GameSync();
	}

	public static void GameSync()
	{
		instance._GameSync();
	}
}
