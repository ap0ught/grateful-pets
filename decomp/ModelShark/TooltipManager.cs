using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModelShark;

public class TooltipManager : MonoBehaviour
{
	[Tooltip("If you have multiple cameras in your scene, this is the one that will be used by ProTips.")]
	public Camera guiCamera;

	[Tooltip("The RectTransform to match if you have an angled UI and you want tooltips to match the RectTransform's angle.")]
	public RectTransform matchRotationTo;

	[Tooltip("When enabled, tooltips will be triggered by pressing-and-holding, not hovering over. They will be dismissed by releasing the hold, instead of hover off.")]
	public bool touchSupport;

	[Tooltip("How long to wait before beginning to display a tooltip.")]
	public float tooltipDelay = 0.33f;

	[Tooltip("How long the tooltip fade-in transition will last. Set to 0 for increased performance.")]
	public float fadeDuration = 0.2f;

	[Tooltip("For tooltip prefabs, the start/end character that signifies a dynamic field.")]
	public string textFieldDelimiter = "%";

	[Tooltip("Determines whether tooltips are repositioned when they would flow off the canvas. Disable for increased performance.")]
	public bool overflowProtection = true;

	public GameObject tooltipContainerPosition;

	public Canvas GuiCanvas;

	private static TooltipManager instance;

	private bool isInitialized;

	public Vector3 positionOverride;

	public List<TooltipTrigger> activeTooltipTriggers = new List<TooltipTrigger>();

	public static TooltipManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Object.FindFirstObjectByType<TooltipManager>();
			}
			if (instance == null)
			{
				return null;
			}
			if (!instance.isInitialized)
			{
				instance.Initialize();
			}
			return instance;
		}
	}

	public Canvas RootCanvas { get; set; }

	public GameObject TooltipContainer { get; set; }

	public GameObject TooltipContainerNoAngle { get; set; }

	public Dictionary<TooltipStyle, Tooltip> Tooltips { get; set; }

	public Tooltip BlockingTooltip { get; set; }

	private void Awake()
	{
		instance = this;
		if (!isInitialized)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		if (!isInitialized)
		{
			RootCanvas = CanvasHelper.GetRootCanvas();
			if (GuiCanvas == null)
			{
				GuiCanvas = RootCanvas;
			}
			if (guiCamera == null)
			{
				guiCamera = Camera.main;
			}
			Tooltips = new Dictionary<TooltipStyle, Tooltip>();
			TooltipContainer = CreateTooltipContainer("Tooltip Container");
			TooltipContainerNoAngle = CreateTooltipContainer("Tooltip Container (No Angle)");
			ResetTooltipRotation();
			isInitialized = true;
		}
	}

	private GameObject CreateTooltipContainer(string containerName)
	{
		GameObject gameObject = GameObject.Find(containerName);
		if (gameObject == null)
		{
			gameObject = new GameObject(containerName);
			gameObject.transform.SetParent(GuiCanvas.transform, worldPositionStays: false);
			RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
			Vector2 vector = (rectTransform.anchoredPosition = Vector2.zero);
			Vector2 vector2 = (rectTransform.offsetMax = vector);
			Vector2 anchorMin = (rectTransform.offsetMin = vector2);
			rectTransform.anchorMin = anchorMin;
			Vector3 vector5 = (rectTransform.localScale = Vector3.one);
			rectTransform.anchorMax = vector5;
			if (tooltipContainerPosition == null)
			{
				gameObject.transform.SetAsLastSibling();
			}
			else
			{
				gameObject.transform.SetSiblingIndex(tooltipContainerPosition.transform.GetSiblingIndex());
			}
		}
		return gameObject;
	}

	public void ResetTooltipRotation()
	{
		TooltipContainer.transform.rotation = ((matchRotationTo != null) ? matchRotationTo.transform.rotation : GuiCanvas.transform.rotation);
	}

	public void SetTextAndSize(TooltipTrigger trigger)
	{
		Tooltip tooltip = trigger.Tooltip;
		if (tooltip == null || trigger.parameterizedTextFields == null || ((tooltip.TextFields == null || tooltip.TextFields.Count == 0) && (tooltip.TMPFields == null || tooltip.TMPFields.Count == 0)))
		{
			return;
		}
		LayoutElement mainTextContainer = tooltip.TooltipStyle.mainTextContainer;
		if (mainTextContainer == null)
		{
			Debug.LogWarning($"No main text container defined on tooltip style \"{trigger.Tooltip.GameObject.name}\". Note: This LayoutElement is needed in order to resize text appropriately.");
		}
		else
		{
			mainTextContainer.preferredWidth = trigger.minTextWidth;
		}
		for (int i = 0; i < tooltip.TextFields.Count; i++)
		{
			Text text = tooltip.TextFields[i].Text;
			if (text.text.Length < 3)
			{
				continue;
			}
			for (int j = 0; j < trigger.parameterizedTextFields.Count; j++)
			{
				if (!string.IsNullOrEmpty(trigger.parameterizedTextFields[j].value))
				{
					text.text = text.text.Replace(trigger.parameterizedTextFields[j].placeholder, trigger.parameterizedTextFields[j].value);
					text.gameObject.SetActive(value: true);
				}
				else
				{
					text.gameObject.SetActive(value: false);
				}
			}
			if (mainTextContainer != null)
			{
				if (text.preferredWidth > (float)trigger.maxTextWidth)
				{
					mainTextContainer.preferredWidth = trigger.maxTextWidth;
				}
				else if (text.preferredWidth > (float)trigger.minTextWidth && text.preferredWidth > mainTextContainer.preferredWidth)
				{
					mainTextContainer.preferredWidth = text.preferredWidth;
				}
			}
		}
		for (int k = 0; k < tooltip.TMPFields.Count; k++)
		{
			TextMeshProUGUI text2 = tooltip.TMPFields[k].Text;
			if (text2.text.Length < 3)
			{
				continue;
			}
			for (int l = 0; l < trigger.parameterizedTextFields.Count; l++)
			{
				text2.text = text2.text.Replace(trigger.parameterizedTextFields[l].placeholder, trigger.parameterizedTextFields[l].value);
			}
			if (string.IsNullOrWhiteSpace(text2.text))
			{
				text2.gameObject.SetActive(value: false);
			}
			else
			{
				text2.gameObject.SetActive(value: true);
			}
			if (mainTextContainer != null)
			{
				if (text2.preferredWidth > (float)trigger.maxTextWidth)
				{
					mainTextContainer.preferredWidth = trigger.maxTextWidth;
				}
				else if (text2.preferredWidth > (float)trigger.minTextWidth && text2.preferredWidth > mainTextContainer.preferredWidth)
				{
					mainTextContainer.preferredWidth = text2.preferredWidth;
				}
			}
		}
	}

	public IEnumerator Show(TooltipTrigger trigger, bool bUsePositionOverride = false)
	{
		if (trigger.tooltipStyle == null)
		{
			Debug.LogWarning("TooltipTrigger \"" + trigger.name + "\" has no associated TooltipStyle. Cannot show tooltip.");
			yield break;
		}
		if (!activeTooltipTriggers.Contains(trigger))
		{
			activeTooltipTriggers.Add(trigger);
		}
		Tooltip tooltip = trigger.Tooltip;
		Image backgroundImage = tooltip.BackgroundImage;
		if (tooltip.NeverRotate)
		{
			tooltip.GameObject.transform.SetParent(TooltipContainerNoAngle.transform, worldPositionStays: false);
		}
		if (trigger.dynamicImageFields != null)
		{
			for (int i = 0; i < trigger.dynamicImageFields.Count; i++)
			{
				for (int j = 0; j < tooltip.ImageFields.Count; j++)
				{
					if (tooltip.ImageFields[j].Name == trigger.dynamicImageFields[i].name)
					{
						if (trigger.dynamicImageFields[i].replacementSprite == null)
						{
							tooltip.ImageFields[j].Image.sprite = tooltip.ImageFields[j].Original;
						}
						else
						{
							tooltip.ImageFields[j].Image.sprite = trigger.dynamicImageFields[i].replacementSprite;
						}
					}
				}
			}
		}
		if (trigger.dynamicSectionFields != null)
		{
			for (int k = 0; k < trigger.dynamicSectionFields.Count; k++)
			{
				for (int l = 0; l < tooltip.SectionFields.Count; l++)
				{
					if (tooltip.SectionFields[l].Name == trigger.dynamicSectionFields[k].name)
					{
						tooltip.SectionFields[l].GameObject.SetActive(trigger.dynamicSectionFields[k].isOn);
					}
				}
			}
		}
		if (tooltip.SetupHelpers != null)
		{
			for (int m = 0; m < tooltip.SetupHelpers.Count; m++)
			{
				tooltip.SetupHelpers[m].BeforeShow(trigger, tooltip);
			}
		}
		Vector2 vector = positionOverride;
		Canvas.ForceUpdateCanvases();
		TooltipContainer.transform.SetParent(GuiCanvas.transform, worldPositionStays: false);
		if (bUsePositionOverride)
		{
			tooltip.SetPositionManual(vector, TipPosition.TopRightCorner, trigger.tooltipStyle, tooltip.BackgroundImage, tooltip.RectTransform, GuiCanvas, guiCamera);
		}
		else
		{
			tooltip.SetPosition(trigger, GuiCanvas, guiCamera);
		}
		backgroundImage.color = trigger.backgroundTint;
		if (tooltip.IsBlocking)
		{
			BlockingTooltip = tooltip;
		}
		tooltip.Display(fadeDuration);
		if (Input.mousePresent)
		{
			trigger.lastMousePosition = Input.mousePosition;
		}
	}

	public void Update()
	{
		for (int i = 0; i < activeTooltipTriggers.Count; i++)
		{
			if (!activeTooltipTriggers[i].gameObject.activeInHierarchy || ((!activeTooltipTriggers[i].isManuallyTriggered || !activeTooltipTriggers[i].staysOpen) && !ControlManager.LastInputFromMouse))
			{
				activeTooltipTriggers[i].ForceHideTooltip();
			}
		}
	}

	public void CloseAll()
	{
		TooltipTrigger[] array = Object.FindObjectsByType<TooltipTrigger>(FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ForceHideTooltip();
		}
	}
}
