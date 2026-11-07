using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

public class InventoryLine : BaseLineWithTooltip, IFrameworkControl, IFrameworkControlSubcontexts, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IScrollHandler
{
	public class Context : NavigationContext
	{
		public InventoryLineData data;
	}

	public GameObject categoryMode;

	public GameObject itemMode;

	public UITextSkin categoryLabel;

	public UITextSkin categoryExpandLabel;

	public HotkeySpread spread;

	private Context context = new Context();

	public Image dotImage;

	public UITextSkin categoryWeightText;

	public UITextSkin itemWeightText;

	public UITextSkin text;

	public UIThreeColorProperties icon;

	public StringBuilder SB = new StringBuilder();

	public UITextSkin hotkeyText;

	public GameObject hotkeySpacer;

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

	public Image background;

	public Image highlight;

	public bool wasSelected;

	public bool wasHighlighted;

	public bool isHighlighted;

	private ControlManager.InputDeviceType lastDeviceType = ControlManager.InputDeviceType.Unknown;

	private List<AbilityManagerSpacer> spacers = new List<AbilityManagerSpacer>();

	public static bool dragging = false;

	public static GameObject dragObject = null;

	public static InventoryLineData itemBeingDragged;

	public static float startAlpha = 1f;

	public static EquipmentLineData draggingOverEquipment;

	public static NavigationController.DragContext itemDragContext;

	private int scrollIndex = -1;

	private KeyCode hotkey;

	public bool selected => context?.IsActive() ?? false;

	public NavigationContext GetNavigationContext()
	{
		return context;
	}

	public void SetupContexts(ScrollChildContext scrollContext)
	{
		scrollContext.proxyTo = this.context;
		Context context = this.context;
		if (context.commandHandlers == null)
		{
			context.commandHandlers = new Dictionary<string, Action>
			{
				{ "InventoryQuickDrop", HandleQuickDrop },
				{ "InventoryQuickEat", HandleQuickEat },
				{ "InventoryQuickDrink", HandleQuickDrink },
				{ "InventoryQuickApply", HandleQuickApply }
			};
		}
	}

	public void LateUpdate()
	{
		if ((selected != wasSelected || isHighlighted != wasHighlighted) && highlight != null)
		{
			if (!highlight.enabled)
			{
				highlight.enabled = true;
			}
			highlight.color = new Color(highlight.color.r, highlight.color.g, highlight.color.b, (selected || isHighlighted) ? (64f / 255f) : 0f);
		}
		wasSelected = selected;
		wasHighlighted = isHighlighted;
		if (selected || isHighlighted)
		{
			InventoryAndEquipmentStatusScreen.equipmentPaneHighlighted = false;
		}
	}

	public void HandleQuickDrop()
	{
		context.data.screen.HandleQuickDrop(context.data);
	}

	public void HandleQuickEat()
	{
		context.data.screen.HandleQuickEat(context.data);
	}

	public void HandleQuickDrink()
	{
		context.data.screen.HandleQuickDrink(context.data);
	}

	public void HandleQuickApply()
	{
		context.data.screen.HandleQuickApply(context.data);
	}

	public override void Update()
	{
		tooltipContextActive = context?.IsActive() ?? false;
		InventoryLineData data = context.data;
		if (data == null || !data.screen.equipmentListController.scrollContext.IsActive())
		{
			InventoryLineData data2 = context.data;
			if ((data2 == null || !data2.screen.equipmentPaperdollController.scrollContext.IsActive()) && (context.data?.screen.inventoryController.scrollContext?.IsActive() ?? true) && ControlManager.isHotkeyDown(hotkey))
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
		}
		if (lastDeviceType != ControlManager.activeControllerType)
		{
			setData(context.data);
			lastDeviceType = ControlManager.activeControllerType;
		}
		base.Update();
	}

	public void setData(FrameworkDataElement data)
	{
		if (scrollChild == null)
		{
			scrollChild = GetComponent<FrameworkUnityScrollChild>();
		}
		if (!(data is InventoryLineData inventoryLineData))
		{
			return;
		}
		if (data != this.context.data)
		{
			hotkeyText.SetText("");
		}
		this.context.data = inventoryLineData;
		hotkey = KeyCode.None;
		if (dotImage.enabled != inventoryLineData.category)
		{
			dotImage.enabled = inventoryLineData.category;
		}
		ControlId.Assign(gameObject, null);
		if (inventoryLineData.category)
		{
			if (hotkeySpacer.activeSelf)
			{
				hotkeySpacer.SetActive(value: false);
			}
			if (!categoryMode.activeSelf)
			{
				categoryMode.SetActive(value: true);
				itemMode.SetActive(value: false);
			}
			categoryLabel.SetText(inventoryLineData.categoryName);
			categoryExpandLabel.SetText(inventoryLineData.categoryExpanded ? "[-]" : "[+]");
			tooltipGo = null;
			tooltipCompareGo = null;
			if (Options.ShowNumberOfItems)
			{
				categoryWeightText.SetText($"|{inventoryLineData.categoryAmount} items|{inventoryLineData.categoryWeight} lbs.|");
			}
			else
			{
				categoryWeightText.SetText($"|{inventoryLineData.categoryWeight} lbs.|");
			}
			itemWeightText.SetText("");
			ControlId.Assign(gameObject, "InventoryLine:Category:" + inventoryLineData.categoryName);
		}
		else
		{
			if (!hotkeySpacer.activeSelf)
			{
				hotkeySpacer.SetActive(value: true);
			}
			if (categoryMode.activeSelf)
			{
				categoryMode.SetActive(value: false);
				itemMode.SetActive(value: true);
			}
			tooltipGo = inventoryLineData.go;
			tooltipCompareGo = The.Player.getComparisonBodypart(inventoryLineData.go)?.Equipped ?? The.Player.getComparisonBodypart(inventoryLineData.go)?.DefaultBehavior;
			dotImage.enabled = false;
			categoryWeightText.SetText("");
			itemWeightText.SetText($"[{inventoryLineData.go.Weight} lbs.]");
			text.SetText(inventoryLineData.displayName);
			icon.FromRenderable(inventoryLineData.go.RenderForUI("Inventory"));
			if (inventoryLineData != null && inventoryLineData.go?.IDIfAssigned != null)
			{
				ControlId.Assign(gameObject, "InventoryLine:Item:" + inventoryLineData?.go?.IDIfAssigned);
			}
		}
		Context context = this.context;
		if (context.axisHandlers == null)
		{
			context.axisHandlers = new Dictionary<InputAxisTypes, Action>();
		}
		this.context.axisHandlers[InputAxisTypes.NavigationXAxis] = (InventoryAndEquipmentStatusScreen.paginationMode ? new Action(XAxis) : null);
		if (this.context.axisHandlers[InputAxisTypes.NavigationXAxis] == null)
		{
			this.context.axisHandlers.Remove(InputAxisTypes.NavigationXAxis);
		}
		UpdateHotkey();
	}

	public void XAxis()
	{
		XRL.UI.Framework.Event currentEvent = NavigationController.currentEvent;
		if (context.data.category)
		{
			if (currentEvent.axisValue < 0)
			{
				context.data.screen.inventoryController.scrollContext.SelectIndex(context.data.screen.inventoryController.selectedPosition - context.data.categoryOffset);
				context.data.screen.SetCategoryExpanded(context.data.categoryName, state: false);
			}
			else if (currentEvent.axisValue > 0)
			{
				context.data.screen.SetCategoryExpanded(context.data.categoryName, state: true);
			}
		}
		else if (currentEvent.axisValue < 0)
		{
			context.data.screen.inventoryController.scrollContext.SelectIndex(context.data.screen.inventoryController.selectedPosition - context.data.categoryOffset);
			context.data.screen.SetCategoryExpanded(context.data.categoryName, state: false);
		}
		else if (currentEvent.axisValue > 0)
		{
			if (Options.GetOption("OptionPressingRightInInventoryEquips") == "Yes")
			{
				context.data.screen.HandleDragDropAutoequip(context.data);
			}
			else
			{
				context.data.screen.SetCategoryExpanded(context.data.categoryName, state: true);
			}
		}
	}

	public void OnBeginDragObject()
	{
		hotkeyText.gameObject.SetActive(value: false);
		transform.localScale = new Vector3(1.5f, 1.5f);
		GetComponent<HorizontalLayoutGroup>().padding = new RectOffset(-52, 0, 8, 0);
		itemWeightText.SetText("");
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		itemDragContext = new NavigationController.DragContext(() =>
		{
			OnEndDrag(null);
		});
		if (!context.data.category && context.data.go != null)
		{
			DraggableInventoryArea.inside = null;
			dragObject = UnityEngine.Object.Instantiate(gameObject);
			dragObject.transform.SetParent(UIManager.mainCanvas.transform, worldPositionStays: true);
			dragObject.GetComponent<CanvasGroup>().interactable = false;
			dragObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
			dragObject.SendMessage("OnBeginDragObject", SendMessageOptions.DontRequireReceiver);
			RectTransform component = dragObject.GetComponent<RectTransform>();
			component.pivot = new Vector2(0f, 0f);
			component.anchoredPosition = Input.mousePosition / UIManager.mainCanvas.scaleFactor;
			itemBeingDragged = context.data;
			startAlpha = GetComponent<CanvasGroup>().alpha;
			GetComponent<CanvasGroup>().alpha = 0.3f;
			dragging = true;
			NavigationController.BeginDragWithContext(itemDragContext, () =>
			{
				OnEndDrag(null);
			});
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
		if (draggingOverEquipment != null)
		{
			context.data.screen.HandleDragDrop(context.data, draggingOverEquipment);
		}
		else if (DraggableInventoryArea.inside?.ID == "Equipment")
		{
			context.data.screen.HandleDragDropAutoequip(context.data);
		}
		NavigationController.EndDragWithContext(itemDragContext);
		itemDragContext = null;
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		base.OnPointerClick(eventData);
		if (context != null && context.Activate() && !dragging)
		{
			NavigationController.instance.FireInputButtonEvent(InputButtonTypes.AcceptButton, new Dictionary<string, object> { { "PointerEventData", eventData } });
		}
	}

	public void UpdateHotkey()
	{
		if (context?.data?.spread == null || ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
		{
			hotkey = KeyCode.None;
			hotkeyText.SetText("");
			return;
		}
		hotkey = context.data.spread.codeAt(scrollIndex);
		if (hotkey == KeyCode.None)
		{
			hotkeyText.SetText("");
		}
		else
		{
			hotkeyText.SetText($"{context.data.spread.charAt(scrollIndex)})");
		}
	}

	public void ScrollIndexChanged(int index)
	{
		scrollIndex = index;
		UpdateHotkey();
	}

	public async void OnScroll(PointerEventData eventData)
	{
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			eventData.Use();
			ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, eventData, ExecuteEvents.scrollHandler);
			ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, eventData, ExecuteEvents.scrollHandler);
			ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, eventData, ExecuteEvents.scrollHandler);
			ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, eventData, ExecuteEvents.scrollHandler);
			ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, eventData, ExecuteEvents.scrollHandler);
			ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, eventData, ExecuteEvents.scrollHandler);
		}
		else
		{
			ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, eventData, ExecuteEvents.scrollHandler);
		}
	}
}
