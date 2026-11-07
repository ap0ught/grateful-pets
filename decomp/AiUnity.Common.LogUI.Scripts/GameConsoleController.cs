using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AiUnity.Common.Extensions;
using AiUnity.Common.Log;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AiUnity.Common.LogUI.Scripts;

public class GameConsoleController : MonoBehaviour, IGameConsoleController, IButtonStateChange, IEventSystemHandler
{
	public Toggle AutoScrollToggle;

	public Dropdown FilterDropDown;

	public InputField FontInputField;

	public GameObject GameConsoleIcon;

	public GameObject GameConsolePanel;

	public GameObject TextBoxPanel;

	public GameObject MoveButton;

	private Text messageText;

	private bool firstUpdate = true;

	private Image gameConsoleIconImage;

	private RectTransform gameConsoleIconTransform;

	private RectTransform gameConsolePanelTransform;

	private ScrollRect gameConsoleScrollRect;

	private bool iconEnable = true;

	private StringBuilder messageBuffer;

	private RectTransformStorage windowStorage;

	private GameObject titlePanel;

	private LogLevels LogLevelsFilter { get; set; }

	private List<LogMessage> LogMessages { get; set; }

	private int MessageCharacters { get; set; }

	private void Awake()
	{
		LogMessages = new List<LogMessage>();
		messageBuffer = new StringBuilder();
		if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
		{
			new GameObject("EventSystem", typeof(EventSystem)).AddComponent<StandaloneInputModule>();
		}
		messageText = TextBoxPanel.GetComponentInChildren<Text>() ?? TextBoxPanel.AddComponent<Text>();
		messageText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
	}

	private void Start()
	{
		FontInputField.onEndEdit.AddListener((string i) =>
		{
			SetFontSize(i, updateControl: false);
		});
		FilterDropDown.onValueChanged.AddListener((int i) =>
		{
			SetMaxLogLevelFilter(i, updateControl: false);
		});
		gameConsoleIconTransform = (RectTransform)GameConsoleIcon.transform;
		gameConsoleIconImage = GameConsoleIcon.GetComponent<Image>();
		gameConsolePanelTransform = (RectTransform)GameConsolePanel.transform;
		gameConsoleScrollRect = GameConsolePanel.GetComponentsInChildren<ScrollRect>().FirstOrDefault();
		gameConsoleScrollRect.verticalNormalizedPosition = 0f;
		titlePanel = GameConsolePanel.transform.Find("TitlePanel").gameObject;
	}

	private void Update()
	{
		if (firstUpdate)
		{
			UpdateGameConsole();
			firstUpdate = false;
		}
		if (gameConsoleScrollRect != null && AutoScrollToggle.isOn)
		{
			gameConsoleScrollRect.verticalNormalizedPosition = 0f;
		}
	}

	public void ButtonStateChange(ButtonState buttonState)
	{
		EnableDrag(buttonState == ButtonState.Pressed);
	}

	public void EnableDrag(bool enable)
	{
		DragPanel[] componentsInChildren = GetComponentsInChildren<DragPanel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = enable;
		}
	}

	public void EnableConfigurationScreen()
	{
		titlePanel.SetActive(!titlePanel.activeSelf);
	}

	public void AddMessage(int logLevel, string message, string loggerName = null, DateTime dateTime = default(DateTime))
	{
		LogMessage item = new LogMessage(message, logLevel, loggerName, dateTime);
		LogMessages.Add(item);
		if (((uint)LogLevelsFilter & (uint)logLevel) == (uint)logLevel)
		{
			AppendMessage(message);
		}
	}

	public void ClearMessages()
	{
		ReplaceBuffer(string.Empty);
	}

	public IEnumerator MinHandler(bool minimize, bool animate = true)
	{
		if (minimize)
		{
			windowStorage.Store(gameConsolePanelTransform);
			GameConsoleIcon.SetActive(value: true);
			Vector3 targetConsolePosition = gameConsoleIconTransform.position + (Vector3)gameConsoleIconTransform.rect.center;
			yield return StartCoroutine(AnimateGameConsole(targetConsolePosition, Vector3.zero, 1f, animate));
			GameConsolePanel.SetActive(value: false);
		}
		else
		{
			GameConsolePanel.SetActive(value: true);
			yield return StartCoroutine(AnimateGameConsole(windowStorage.position, Vector3.one, 0f, animate));
			GameConsoleIcon.SetActive(value: false);
		}
	}

	public void MinHandlerAnimate(bool minimize)
	{
		StartCoroutine(MinHandler(minimize));
	}

	public void SetConsoleActive(bool consoleActive)
	{
		GameConsolePanel.SetActive(consoleActive);
	}

	public void SetFontSize(int fontSize, bool updateControl = true, bool updateMessage = true)
	{
		if (updateControl)
		{
			FontInputField.text = fontSize.ToString();
		}
		if (updateMessage)
		{
			messageText.fontSize = fontSize;
		}
	}

	public void SetIconActive(bool iconActive)
	{
		if (iconEnable)
		{
			GameConsoleIcon.SetActive(iconActive);
		}
	}

	public void SetIconEnable(bool iconEnable)
	{
		if (!iconEnable)
		{
			GameConsoleIcon.SetActive(value: false);
		}
		this.iconEnable = iconEnable;
	}

	public void SetLogLevelFilter(LogLevels logLevels, bool updateControl = true, bool updateMessage = true)
	{
		LogLevelsFilter = logLevels;
		if (updateControl)
		{
			double num = (double)logLevels.GetFlags().Max();
			FilterDropDown.value = ((num > 0.0) ? ((int)Math.Log(num, 2.0) + 1) : 0);
		}
		if (updateMessage)
		{
			FilterMessages();
		}
	}

	public void SetMaxLogLevelFilter(int maxLevel, bool updateControl = true, bool updateMessage = true)
	{
		SetLogLevelFilter((int)(Math.Pow(2.0, maxLevel) - 1.0), updateControl, updateMessage);
	}

	private IEnumerator AnimateGameConsole(Vector3 targetConsolePosition, Vector3 targetConsoleScale, float targetIconAlpha, bool animate)
	{
		float timeToLerp = 0.8f;
		float timeLerped = (animate ? 0f : timeToLerp);
		Vector3 startingPosition = gameConsolePanelTransform.position;
		Vector3 startingScale = gameConsolePanelTransform.localScale;
		float startAlpha = gameConsoleIconImage.color.a;
		Color sourceColor = gameConsoleIconImage.color;
		while (timeLerped <= timeToLerp)
		{
			timeLerped += Time.deltaTime;
			gameConsolePanelTransform.position = Vector3.Lerp(startingPosition, targetConsolePosition, timeLerped / timeToLerp);
			gameConsolePanelTransform.localScale = Vector3.Lerp(startingScale, targetConsoleScale, timeLerped / timeToLerp);
			sourceColor.a = Mathf.Lerp(startAlpha, targetIconAlpha, timeLerped / timeToLerp * (timeLerped / timeToLerp));
			gameConsoleIconImage.color = sourceColor;
			if (animate)
			{
				yield return null;
			}
		}
	}

	private void AppendMessage(string message)
	{
		messageText.text = messageBuffer.Append(message).ToString();
		LimitMessageLength();
	}

	private void FilterMessages()
	{
		ReplaceBuffer(string.Concat((from m in LogMessages
			where LogLevelsFilter.Has(m.LogLevels)
			select m.Message).ToArray()));
	}

	private void LimitMessageLength()
	{
		if (messageText.text.Length > 12000)
		{
			int startIndex = messageText.text.IndexOf(Environment.NewLine, 3000);
			messageBuffer = new StringBuilder(messageText.text.Substring(startIndex));
			messageText.text = messageBuffer.ToString();
		}
	}

	private void ReplaceBuffer(string buffer)
	{
		messageBuffer = new StringBuilder(buffer);
		messageText.text = messageBuffer.ToString();
		LimitMessageLength();
	}

	private void SetFontSize(string fontSizeText, bool updateControl = true, bool updateMessage = true)
	{
		int result = 0;
		if (int.TryParse(fontSizeText, out result))
		{
			SetFontSize(result, updateControl, updateMessage);
		}
	}

	private void SetLogLevelFilter(int level, bool updateControl = true, bool updateMessage = true)
	{
		int i = ((level <= 0) ? level : ((int)Math.Pow(2.0, (int)Math.Log(level, 2.0) + 1) - 1));
		SetLogLevelFilter(i.ToEnum<LogLevels>(), updateControl, updateMessage);
	}

	private void UpdateGameConsole()
	{
		windowStorage.Store(gameConsolePanelTransform);
		StartCoroutine(MinHandler(!GameConsolePanel.activeInHierarchy, animate: false));
	}
}
