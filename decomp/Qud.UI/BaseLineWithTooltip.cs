using ConsoleLib.Console;
using ModelShark;
using UnityEngine;
using UnityEngine.EventSystems;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace Qud.UI;

public class BaseLineWithTooltip : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public bool tooltipContextActive;

	public XRL.World.GameObject tooltipGo;

	public XRL.World.GameObject tooltipCompareGo;

	public TooltipTrigger tooltip;

	public bool Inside;

	public float TooltipTimer;

	public bool ShiftShowing;

	public static float TOOLTIP_DELAY = 2f;

	private bool GamepadAltShowing;

	private Vector3[] corners = new Vector3[4];

	public Vector2 ScreenToRectPos(Vector2 screen_pos, Canvas canvas, RectTransform rectTransform)
	{
		if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera != null)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screen_pos, canvas.worldCamera, out var localPoint);
			return localPoint;
		}
		Vector2 vector = screen_pos - new Vector2(rectTransform.position.x, rectTransform.position.y);
		return new Vector2(vector.x / rectTransform.lossyScale.x, vector.y / rectTransform.lossyScale.y);
	}

	public virtual void Update()
	{
		if (!TutorialManager.AllowTooltipUpdate())
		{
			return;
		}
		if (tooltipGo != null)
		{
			if (tooltipContextActive)
			{
				if (!ShiftShowing && (Input.GetKey(UnityEngine.KeyCode.LeftAlt) || Input.GetKey(UnityEngine.KeyCode.RightAlt)))
				{
					ShiftShowing = true;
					StartTooltip(tooltipGo, tooltipCompareGo, mousePosition: false, gameObject.transform as RectTransform);
					return;
				}
				if (ControlManager.GetButton("Highlight", forceEnable: true) && !ShiftShowing)
				{
					GamepadAltShowing = true;
					StartTooltip(tooltipGo, tooltipCompareGo, mousePosition: false, gameObject.transform as RectTransform);
					return;
				}
			}
			if (ShiftShowing && !Input.GetKey(UnityEngine.KeyCode.LeftAlt) && !Input.GetKey(UnityEngine.KeyCode.RightAlt))
			{
				ShiftShowing = false;
				tooltip?.ForceHideTooltip();
				tooltip = null;
			}
			else if (GamepadAltShowing && !ControlManager.GetButton("Highlight", forceEnable: true))
			{
				GamepadAltShowing = false;
				tooltip?.ForceHideTooltip();
				tooltip = null;
			}
			else if (!tooltipContextActive)
			{
				Inside = false;
				GamepadAltShowing = false;
				ShiftShowing = false;
			}
		}
		if (!Inside && !ShiftShowing && !GamepadAltShowing && tooltip != null)
		{
			tooltip.ForceHideTooltip();
			tooltip = null;
		}
		if (Inside && tooltip == null && tooltipGo != null && !ShiftShowing && !GamepadAltShowing)
		{
			TooltipTimer += Time.deltaTime;
			if (TooltipTimer >= TOOLTIP_DELAY)
			{
				StartTooltip(tooltipGo, tooltipCompareGo);
			}
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		Inside = true;
		TooltipTimer = 0f;
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		Inside = false;
		tooltip?.ForceHideTooltip();
		tooltip = null;
	}

	public void StartTooltip(XRL.World.GameObject go, XRL.World.GameObject compareGo, bool mousePosition = true, RectTransform targetRect = null)
	{
		Look.TooltipInformation tooltipInformation = Look.GenerateTooltipInformation(go);
		Look.TooltipInformation tooltipInformation2 = default;
		if (compareGo != null && compareGo.IsValid() && compareGo.HasPart<Description>())
		{
			tooltipInformation2 = Look.GenerateTooltipInformation(compareGo);
		}
		string text = "looker";
		if (tooltip != null)
		{
			return;
		}
		tooltip = ((text == "looker") ? GameManager.Instance.lookerTooltip : GameManager.Instance.tileTooltip);
		if (compareGo != null)
		{
			text = "compareLooker";
			tooltip = GameManager.Instance.compareLookerTooltip;
		}
		foreach (ParameterizedTextField parameterizedTextField2 in tooltip.parameterizedTextFields)
		{
			ParameterizedTextField current;
			ParameterizedTextField parameterizedTextField = (current = parameterizedTextField2);
			current.value = RTF.FormatToRTF(Markup.Color("y", parameterizedTextField.name switch
			{
				"DisplayName" => tooltipInformation.DisplayName, 
				"ConText" => tooltipInformation.SubHeader, 
				"WoundLevel" => tooltipInformation.WoundLevel, 
				"LongDescription" => tooltipInformation.LongDescription.Trim(), 
				"DisplayName2" => tooltipInformation2.DisplayName, 
				"ConText2" => tooltipInformation2.SubHeader, 
				"WoundLevel2" => tooltipInformation2.WoundLevel, 
				"LongDescription2" => tooltipInformation2.LongDescription.Trim(), 
				_ => "", 
			}), "FF", 60) ?? "";
		}
		tooltip.AdditionalData = new DualImageRenderableGlue.DualImageRenderableGlueData(tooltipInformation.IconRenderable, tooltipInformation2.IconRenderable);
		if (!tooltip.transform.parent.gameObject.activeInHierarchy)
		{
			tooltip.transform.parent.gameObject.SetActive(value: true);
		}
		if (mousePosition)
		{
			tooltip.ShowManually(bForceDisplay: true, Input.mousePosition, usePosOverride: true);
		}
		else
		{
			targetRect.GetWorldCorners(corners);
			tooltip.ShowManually(bForceDisplay: true, corners[0], usePosOverride: true);
		}
		TutorialManager.StartingLineTooltip(go, compareGo);
		if (go == null)
		{
			return;
		}
		tooltip.onHideAction = () =>
		{
			GameManager.Instance.gameQueue.queueSingletonTask("LookedAt" + go.GetHashCode(), () =>
			{
				go.FireEvent(XRL.World.Event.New("AfterLookedAt", "Looker", The.Player));
				The.Player?.FireEvent(XRL.World.Event.New("LookedAt", "Object", go));
			});
		};
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		Inside = false;
	}
}
