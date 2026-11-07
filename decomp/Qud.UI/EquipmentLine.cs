using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Anatomy;

namespace Qud.UI;

public class EquipmentLine : BaseLineWithTooltip, IFrameworkControl, IFrameworkControlSubcontexts, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
	public class Context : NavigationContext
	{
		public EquipmentLineData data;

		public Context()
		{
		}

		public Context(string debugname)
			: base(debugname)
		{
		}
	}

	private Context context = new Context("EquipmentLineContext");

	public UITextSkin text;

	public UITextSkin itemText;

	public UIThreeColorProperties icon;

	public bool isDefaultBehavior;

	public StringBuilder SB = new StringBuilder();

	public Image dragOverIndicator;

	public static List<MenuOption> categoryExpandOptions = new List<MenuOption>
	{
		new MenuOption
		{
			Id = "Accept",
			InputCommand = "Accept",
			Description = "Expand"
		}
	};

	public static List<MenuOption> categoryCollapseOptions = new List<MenuOption>
	{
		new MenuOption
		{
			Id = "Accept",
			InputCommand = "Accept",
			Description = "Collapse"
		}
	};

	public FrameworkUnityScrollChild scrollChild;

	public Image[] frames;

	public bool wasSelected;

	public bool wasHighlighted;

	public bool isHighlighted;

	public Image highlightImage;

	public static bool dragging = false;

	public static UnityEngine.GameObject dragObject = null;

	public static EquipmentLineData itemBeingDragged;

	public static float startAlpha = 1f;

	private static List<BodyPart> partsScrap = new List<BodyPart>();

	private static Color DEFAULT_FRAME_COLOR = ConsoleLib.Console.ColorUtility.FromWebColor("3A5A66");

	public UITextSkin hotkeyText;

	private int scrollIndex = -1;

	private UnityEngine.KeyCode hotkey;

	private List<AbilityManagerSpacer> spacers = new List<AbilityManagerSpacer>();

	public bool selected => context?.IsActive() ?? false;

	public NavigationContext GetNavigationContext()
	{
		return context;
	}

	public void SetupContexts(ScrollChildContext scrollContext)
	{
		scrollContext.proxyTo = this.context;
		Context context = this.context;
		if (context.axisHandlers == null)
		{
			context.axisHandlers = new Dictionary<InputAxisTypes, Action>();
		}
		context = this.context;
		if (context.commandHandlers == null)
		{
			context.commandHandlers = new Dictionary<string, Action>();
		}
		if (scrollContext.commandHandlers == null)
		{
			scrollContext.commandHandlers = new Dictionary<string, Action>();
		}
	}

	public override void Update()
	{
		tooltipContextActive = context?.IsActive() ?? false;
		EquipmentLineData data = context.data;
		if (data == null || !data.screen.equipmentListController.scrollContext.IsActive())
		{
			EquipmentLineData data2 = context.data;
			if (data2 == null || !data2.screen.equipmentPaperdollController.scrollContext.IsActive())
			{
				goto IL_0141;
			}
		}
		bool? flag = context.data?.screen.inventoryController.scrollContext?.IsActive();
		if (flag.HasValue && flag != true && ControlManager.isHotkeyDown(hotkey))
		{
			if (scrollChild?.scrollContext?.IsActive() == true)
			{
				context.data.screen.HandleSelectItem(context.data);
			}
			else
			{
				context.Activate();
			}
		}
		goto IL_0141;
		IL_0141:
		bool flag2 = false;
		if ((InventoryLine.dragging || dragging) && InventoryLine.draggingOverEquipment == context.data && dragOverIndicator != null)
		{
			flag2 = true;
		}
		if (dragOverIndicator != null)
		{
			if (flag2 && !dragOverIndicator.enabled)
			{
				dragOverIndicator.enabled = true;
			}
			if (!flag2 && dragOverIndicator.enabled)
			{
				dragOverIndicator.enabled = false;
			}
		}
		if (selected || isHighlighted)
		{
			InventoryAndEquipmentStatusScreen.equipmentPaneHighlighted = true;
		}
		base.Update();
	}

	public void LateUpdate()
	{
		if ((selected != wasSelected || isHighlighted != wasHighlighted) && highlightImage != null)
		{
			highlightImage.enabled = selected || isHighlighted;
		}
		wasSelected = selected;
		wasHighlighted = isHighlighted;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (context.data.bodyPart.Equipped != null)
		{
			DraggableInventoryArea.inside = null;
			dragObject = UnityEngine.Object.Instantiate(gameObject);
			dragObject.transform.position = gameObject.transform.position;
			dragObject.transform.SetParent(UIManager.mainCanvas.transform, worldPositionStays: true);
			dragObject.GetComponent<CanvasGroup>().interactable = false;
			dragObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
			RectTransform component = dragObject.GetComponent<RectTransform>();
			component.pivot = new Vector2(0f, 0f);
			component.anchoredPosition = Input.mousePosition / UIManager.mainCanvas.scaleFactor;
			itemBeingDragged = context.data;
			startAlpha = GetComponent<CanvasGroup>().alpha;
			GetComponent<CanvasGroup>().alpha = 0.3f;
			dragging = true;
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (dragging)
		{
			RectTransform component = dragObject.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0.5f, 0.5f);
			component.anchorMax = new Vector2(0.5f, 0.5f);
			component.anchoredPosition = ScreenToRectPos(eventData.position, UIManager.mainCanvas, UIManager.mainCanvas.transform as RectTransform);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		UnityEngine.Object.Destroy(dragObject);
		GetComponent<CanvasGroup>().alpha = startAlpha;
		dragging = false;
		dragObject = null;
		itemBeingDragged = null;
		if (InventoryLine.draggingOverEquipment != context.data)
		{
			context.data.screen.HandleDragDrop(context.data, InventoryLine.draggingOverEquipment);
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		base.OnPointerClick(eventData);
		if (context.IsActive() && context.Activate() && !dragging)
		{
			if (isDefaultBehavior && eventData.button == PointerEventData.InputButton.Right)
			{
				context.data.screen.HandleLookAtDefaultEquipmnent(context.data);
				return;
			}
			NavigationController.instance.FireInputButtonEvent(InputButtonTypes.AcceptButton, new Dictionary<string, object> { { "PointerEventData", eventData } });
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		InventoryLine.draggingOverEquipment = context.data;
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		InventoryLine.draggingOverEquipment = null;
	}

	private async void SetPrimaryLimb()
	{
		_ = context.data.bodyPart;
		await APIDispatch.RunAndWaitAsync(() =>
		{
			context.data.bodyPart.SetAsPreferredDefault();
		});
		context.data.screen.UpdateViewFromData();
		if (context.data.screen.IsPaperdollMode)
		{
			context.data.screen.equipmentPaperdollController.scrollContext.ActivateAndEnable();
		}
		else
		{
			context.data.screen.equipmentListController.scrollContext.ActivateAndEnable();
		}
	}

	public void setData(FrameworkDataElement data)
	{
		if (scrollChild == null)
		{
			scrollChild = GetComponent<FrameworkUnityScrollChild>();
		}
		tooltipCompareGo = null;
		if (!(data is EquipmentLineData equipmentLineData))
		{
			return;
		}
		equipmentLineData.line = this;
		if (equipmentLineData.showCybernetics)
		{
			tooltipGo = equipmentLineData.bodyPart.Cybernetics ?? null;
		}
		else
		{
			tooltipGo = equipmentLineData.bodyPart.Equipped ?? equipmentLineData.bodyPart.DefaultBehavior ?? null;
		}
		ControlId.Assign(base.gameObject, "EquipmentLine:Slot:" + equipmentLineData?.bodyPart?.Name, findNestedComponent: true);
		this.context.data = equipmentLineData;
		Context context = this.context;
		if (context.commandHandlers == null)
		{
			context.commandHandlers = new Dictionary<string, Action>();
		}
		this.context.commandHandlers["CmdInsert"] = SetPrimaryLimb;
		int num = 0;
		if (Options.IndentBodyParts)
		{
			num = (equipmentLineData?.bodyPart?.ParentBody.GetPartDepth(equipmentLineData?.bodyPart)).GetValueOrDefault();
		}
		string cardinalDescription = equipmentLineData.bodyPart.GetCardinalDescription();
		this.text.SetText((equipmentLineData.bodyPart.Primary ? "{{G|*}}" : "") + ((num == 0) ? cardinalDescription : cardinalDescription.PadLeft(num + cardinalDescription.Length, ' ')));
		XRL.World.GameObject gameObject = ((!equipmentLineData.showCybernetics) ? (equipmentLineData?.bodyPart?.Equipped ?? equipmentLineData?.bodyPart?.DefaultBehavior ?? null) : equipmentLineData?.bodyPart?.Cybernetics);
		isDefaultBehavior = false;
		if (equipmentLineData.showCybernetics)
		{
			icon.FromRenderable(equipmentLineData.bodyPart?.Cybernetics?.RenderForUI("Equipment") ?? null);
		}
		else
		{
			XRL.World.GameObject gameObject2 = equipmentLineData.bodyPart?.Equipped ?? equipmentLineData.bodyPart?.DefaultBehavior ?? null;
			if (gameObject2 != null)
			{
				if (gameObject2 == equipmentLineData.bodyPart?.DefaultBehavior)
				{
					icon.FromRenderable(gameObject2.RenderForUI("Equipment")?.GreyOutForUI());
					isDefaultBehavior = true;
				}
				else
				{
					partsScrap.Clear();
					equipmentLineData.bodyPart.ParentBody.GetPartsEquippedOn(gameObject2, partsScrap);
					bool flag = false;
					if (partsScrap.Count == 0 || equipmentLineData.bodyPart != partsScrap[0])
					{
						flag = true;
					}
					icon.FromRenderable((!flag) ? gameObject2?.RenderForUI("Equipment") : gameObject2?.RenderForUI("Equipment")?.GreyOutForUI());
				}
			}
			else
			{
				icon.FromRenderable(gameObject2?.RenderForUI("Equipment"));
			}
		}
		if (itemText != null)
		{
			itemText.SetText(gameObject?.DisplayName ?? "{{K|-}}");
		}
		if (frames != null && frames.Length != 0)
		{
			string text = gameObject?.GetTagOrStringProperty("EquipmentFrameColors", "----") ?? "----";
			for (int i = 0; i < frames.Length; i++)
			{
				if (text[i] == '-')
				{
					frames[i].color = DEFAULT_FRAME_COLOR;
				}
				else
				{
					frames[i].color = ConsoleLib.Console.ColorUtility.colorFromChar(text[i]);
				}
			}
		}
		context = this.context;
		if (context.axisHandlers == null)
		{
			context.axisHandlers = new Dictionary<InputAxisTypes, Action>();
		}
		this.context.axisHandlers[InputAxisTypes.NavigationXAxis] = (InventoryAndEquipmentStatusScreen.paginationMode ? new Action(XAxis) : null);
		if (this.context.axisHandlers[InputAxisTypes.NavigationXAxis] == null)
		{
			this.context.axisHandlers.Remove(InputAxisTypes.NavigationXAxis);
		}
	}

	public void UpdateHotkey()
	{
		if (hotkeyText != null)
		{
			if (context?.data?.spread == null || ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
			{
				hotkey = UnityEngine.KeyCode.None;
				hotkeyText.SetText("");
				return;
			}
			hotkey = context.data.spread.codeAt(scrollIndex);
			if (hotkey == UnityEngine.KeyCode.None)
			{
				hotkeyText.SetText("");
			}
			else
			{
				hotkeyText.SetText($"{context.data.spread.charAt(scrollIndex)})");
			}
		}
		else
		{
			hotkey = UnityEngine.KeyCode.None;
		}
	}

	public void ScrollIndexChanged(int index)
	{
		scrollIndex = index;
		UpdateHotkey();
	}

	public void XAxis()
	{
		if (NavigationController.currentEvent.axisValue < 0 && Options.GetOption("OptionPressingRightInInventoryEquips") == "Yes")
		{
			context.data.screen.HandleDragDrop(context.data, null);
		}
	}

	public int HighlightClosestSpacer(Vector2 screenPosition)
	{
		AbilityManagerSpacer abilityManagerSpacer = ((spacers.Count > 0) ? spacers[0] : null);
		float num = float.MaxValue;
		int result = -1;
		Vector3[] array = new Vector3[4];
		int num2 = 0;
		foreach (AbilityManagerSpacer spacer in spacers)
		{
			(spacer.transform as RectTransform).GetWorldCorners(array);
			float magnitude = (new Vector2((array[0].x + array[2].x) / 2f, (array[0].y + array[2].y) / 2f) - screenPosition).magnitude;
			if (magnitude < num)
			{
				abilityManagerSpacer = spacer;
				num = magnitude;
				result = num2;
			}
			spacer.image.enabled = false;
			num2++;
		}
		if (abilityManagerSpacer != null)
		{
			abilityManagerSpacer.image.enabled = true;
		}
		return result;
	}
}
