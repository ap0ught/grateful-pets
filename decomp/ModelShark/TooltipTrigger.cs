using System;
using System.Collections.Generic;
using Qud.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ModelShark;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerUpHandler
{
	public TooltipStyle tooltipStyle;

	public List<ParameterizedTextField> parameterizedTextFields;

	public List<DynamicImageField> dynamicImageFields;

	public List<DynamicSectionField> dynamicSectionFields;

	public object AdditionalData;

	public bool isRemotelyActivated;

	[HideInInspector]
	public GameObject remoteTrigger;

	public bool isManuallyTriggered;

	[Tooltip("Controls the color and fade amount of the tooltip background.")]
	public Color backgroundTint = Color.white;

	public TipPosition tipPosition;

	public int minTextWidth = 100;

	public int maxTextWidth = 200;

	[HideInInspector]
	[Tooltip("Once open, this tooltip will stay open until something manually closes it. No other tooltips that use this style will open, since there is only one instance of a tooltip style at a time.")]
	public bool staysOpen;

	[HideInInspector]
	[Tooltip("If true, this tooltip will not be angled/rotated along with other tooltips (see MatchRotationTo on TooltipManager).")]
	public bool neverRotate;

	[HideInInspector]
	[Tooltip("While open, this tooltip will prevent any other tooltips from triggering.")]
	public bool isBlocking;

	private float hoverTimer;

	private float popupTimer;

	public bool CustomDelay;

	public float CustomDelayTime = 1f;

	private float popupTime = 2f;

	private bool isInitialized;

	private bool isMouseOver;

	private bool isMouseDown;

	public bool dismissOnMouseMove;

	public Vector3 lastMousePosition = new Vector2(0f, 0f);

	public Action onHideAction;

	public Tooltip Tooltip { get; set; }

	private float tooltipDelay
	{
		get
		{
			if (!CustomDelay)
			{
				return TooltipManager.Instance.tooltipDelay;
			}
			return CustomDelayTime;
		}
	}

	public void Start()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (isInitialized || TooltipManager.Instance == null)
		{
			return;
		}
		if (this.tooltipStyle != null)
		{
			if (!TooltipManager.Instance.Tooltips.ContainsKey(this.tooltipStyle))
			{
				TooltipStyle tooltipStyle = UnityEngine.Object.Instantiate(this.tooltipStyle);
				tooltipStyle.name = this.tooltipStyle.name;
				tooltipStyle.transform.SetParent(TooltipManager.Instance.TooltipContainer.transform, worldPositionStays: false);
				Tooltip tooltip = new Tooltip
				{
					GameObject = tooltipStyle.gameObject
				};
				tooltip.Initialize();
				tooltip.Deactivate();
				TooltipManager.Instance.Tooltips.Add(this.tooltipStyle, tooltip);
			}
			Tooltip = TooltipManager.Instance.Tooltips[this.tooltipStyle];
		}
		isInitialized = true;
	}

	private void Update()
	{
		if (CursorManager.instance.cursorHidden)
		{
			return;
		}
		if (hoverTimer > 0f)
		{
			hoverTimer += Time.unscaledDeltaTime;
		}
		if (hoverTimer > tooltipDelay)
		{
			hoverTimer = 0f;
			if (!isManuallyTriggered)
			{
				StartHover();
			}
		}
		if (popupTimer > 0f)
		{
			popupTimer += Time.unscaledDeltaTime;
		}
		if (IsDisplayed())
		{
			if (popupTimer > popupTime && Tooltip != null && !Tooltip.StaysOpen)
			{
				HidePopup();
			}
			if (Input.mousePresent && dismissOnMouseMove && Vector3.Distance(Input.mousePosition, lastMousePosition) > 10f)
			{
				HidePopup();
				lastMousePosition = Input.mousePosition;
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!TooltipManager.Instance.touchSupport && !isRemotelyActivated && TooltipManager.Instance.BlockingTooltip == null && !Tooltip.GameObject.activeInHierarchy)
		{
			hoverTimer = 0.001f;
		}
	}

	public void OnMouseOver()
	{
		if (!isMouseOver && !TooltipManager.Instance.touchSupport && !isRemotelyActivated && TooltipManager.Instance.BlockingTooltip == null && !Tooltip.GameObject.activeInHierarchy)
		{
			hoverTimer = 0.001f;
			isMouseOver = true;
		}
	}

	public void OnMouseDown()
	{
		if (!isMouseDown && TooltipManager.Instance.touchSupport && !isRemotelyActivated && TooltipManager.Instance.BlockingTooltip == null && !Tooltip.GameObject.activeInHierarchy)
		{
			hoverTimer = 0.001f;
			isMouseDown = true;
		}
	}

	public void OnMouseExit()
	{
		if (!TooltipManager.Instance.touchSupport)
		{
			isMouseOver = false;
			StopHover();
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (TooltipManager.Instance.touchSupport && !isRemotelyActivated && TooltipManager.Instance.BlockingTooltip == null && !Tooltip.GameObject.activeInHierarchy)
		{
			hoverTimer = 0.001f;
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (!TooltipManager.Instance.touchSupport && !isRemotelyActivated && TooltipManager.Instance.BlockingTooltip == null && Tooltip != null && !(Tooltip.GameObject == null) && !Tooltip.GameObject.activeInHierarchy)
		{
			hoverTimer = 0.001f;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!TooltipManager.Instance.touchSupport)
		{
			StopHover();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (TooltipManager.Instance.touchSupport)
		{
			StopHover();
		}
	}

	public void OnMouseUp()
	{
		if (TooltipManager.Instance.touchSupport)
		{
			isMouseDown = false;
			StopHover();
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (!(TooltipManager.Instance != null) || !TooltipManager.Instance.touchSupport)
		{
			StopHover();
		}
	}

	public virtual void StartHover()
	{
		if (!isManuallyTriggered || !Tooltip.GameObject.activeInHierarchy)
		{
			if (minTextWidth > maxTextWidth)
			{
				maxTextWidth = minTextWidth;
			}
			Tooltip.WarmUp();
			Tooltip.StaysOpen = staysOpen;
			Tooltip.NeverRotate = neverRotate;
			Tooltip.IsBlocking = isBlocking;
			TooltipManager.Instance.SetTextAndSize(this);
			StartCoroutine(TooltipManager.Instance.Show(this));
		}
	}

	public void Deactivate()
	{
		if (onHideAction != null)
		{
			onHideAction();
			onHideAction = null;
		}
		if (TooltipManager.Instance.activeTooltipTriggers.Contains(this))
		{
			TooltipManager.Instance.activeTooltipTriggers.Remove(this);
		}
		if (Tooltip != null && Tooltip.GameObject != null)
		{
			Tooltip.Deactivate();
		}
	}

	public void ForceHideTooltip()
	{
		hoverTimer = (popupTimer = 0f);
		Deactivate();
	}

	public void StopHover()
	{
		if (Tooltip != null && !(Tooltip.GameObject == null) && !isRemotelyActivated && !Tooltip.StaysOpen)
		{
			hoverTimer = 0f;
			Deactivate();
		}
	}

	public void HidePopup()
	{
		if (Tooltip != null && !(Tooltip.GameObject == null) && Tooltip.GameObject.activeInHierarchy)
		{
			popupTimer = 0f;
			remoteTrigger = null;
			Deactivate();
		}
	}

	public bool IsDisplayed()
	{
		if (Tooltip != null && Tooltip.GameObject != null && Tooltip.GameObject.activeInHierarchy)
		{
			return TooltipManager.Instance.activeTooltipTriggers.Contains(this);
		}
		return false;
	}

	public void ShowManually(bool bForceDisplay = false)
	{
		ShowManually(bForceDisplay, Vector2.zero);
	}

	public void ShowManually(bool bForceDisplay, Vector3 pos, bool usePosOverride = false, bool alwaysStayOpen = false)
	{
		if (bForceDisplay || !IsDisplayed())
		{
			if (IsDisplayed())
			{
				Deactivate();
			}
			Initialize();
			if (minTextWidth > maxTextWidth)
			{
				maxTextWidth = minTextWidth;
			}
			Tooltip.WarmUp();
			Tooltip.StaysOpen = staysOpen;
			if (alwaysStayOpen)
			{
				Tooltip.StaysOpen = true;
			}
			Tooltip.NeverRotate = neverRotate;
			Tooltip.IsBlocking = isBlocking;
			dismissOnMouseMove = !usePosOverride;
			if (staysOpen && isManuallyTriggered)
			{
				dismissOnMouseMove = false;
			}
			TooltipManager.Instance.SetTextAndSize(this);
			lastMousePosition = Input.mousePosition;
			if (usePosOverride)
			{
				TooltipManager.Instance.positionOverride = pos;
			}
			StartCoroutine(TooltipManager.Instance.Show(this, usePosOverride));
		}
	}

	public void Popup(float duration, GameObject triggeredBy)
	{
		if (!(popupTimer > 0f) && TooltipManager.Instance.BlockingTooltip == null)
		{
			Initialize();
			remoteTrigger = triggeredBy;
			if (minTextWidth > maxTextWidth)
			{
				maxTextWidth = minTextWidth;
			}
			Tooltip.WarmUp();
			Tooltip.StaysOpen = staysOpen;
			Tooltip.NeverRotate = neverRotate;
			Tooltip.IsBlocking = isBlocking;
			TooltipManager.Instance.SetTextAndSize(this);
			StartCoroutine(TooltipManager.Instance.Show(this));
			popupTimer = 0.001f;
			popupTime = duration;
		}
	}

	public void SetText(string parameterName, string text)
	{
		if (parameterizedTextFields == null)
		{
			parameterizedTextFields = new List<ParameterizedTextField>();
		}
		bool flag = false;
		for (int i = 0; i < parameterizedTextFields.Count; i++)
		{
			if (parameterizedTextFields[i].name == parameterName)
			{
				parameterizedTextFields[i].value = text;
				flag = true;
			}
		}
		if (!flag)
		{
			string textFieldDelimiter = TooltipManager.Instance.textFieldDelimiter;
			parameterizedTextFields.Add(new ParameterizedTextField
			{
				name = parameterName,
				placeholder = string.Format("{0}{1}{0}", textFieldDelimiter, parameterName),
				value = text
			});
		}
	}

	public void SetImage(string parameterName, Sprite sprite)
	{
		if (dynamicImageFields == null)
		{
			dynamicImageFields = new List<DynamicImageField>();
		}
		bool flag = false;
		for (int i = 0; i < dynamicImageFields.Count; i++)
		{
			if (dynamicImageFields[i].name == parameterName)
			{
				dynamicImageFields[i].replacementSprite = sprite;
				flag = true;
			}
		}
		if (!flag)
		{
			dynamicImageFields.Add(new DynamicImageField
			{
				name = parameterName,
				placeholderSprite = null,
				replacementSprite = sprite
			});
		}
	}

	public void TurnSectionOn(string parameterName)
	{
		ToggleSection(parameterName, isOn: true);
	}

	public void TurnSectionOff(string parameterName)
	{
		ToggleSection(parameterName, isOn: false);
	}

	public void ToggleSection(string parameterName, bool isOn)
	{
		if (dynamicSectionFields == null)
		{
			dynamicSectionFields = new List<DynamicSectionField>();
		}
		bool flag = false;
		for (int i = 0; i < dynamicSectionFields.Count; i++)
		{
			if (dynamicSectionFields[i].name == parameterName)
			{
				dynamicSectionFields[i].isOn = isOn;
				flag = true;
			}
		}
		if (!flag)
		{
			dynamicSectionFields.Add(new DynamicSectionField
			{
				name = parameterName,
				isOn = isOn
			});
		}
	}
}
