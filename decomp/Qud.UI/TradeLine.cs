using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

public class TradeLine : BaseLineWithTooltip, IFrameworkControl, IFrameworkControlSubcontexts, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IScrollHandler
{
	public class Context : NavigationContext
	{
		public TradeLineData data;
	}

	public Context context = new Context();

	public UnityEngine.GameObject categoryMode;

	public UnityEngine.GameObject itemMode;

	public UITextSkin categoryText;

	public UITextSkin categoryExpanderText;

	public UITextSkin text;

	public UITextSkin check;

	public UITextSkin hotkey;

	public UnityEngine.GameObject rightFloatSpacer;

	public UITextSkin rightFloatText;

	public UIThreeColorProperties icon;

	public UnityEngine.GameObject iconSpacer;

	public TradeScreen screen;

	public StringBuilder SB = new StringBuilder();

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
			Description = "Select"
		}
	};

	public static List<MenuOption> itemOptions = new List<MenuOption>
	{
		new MenuOption
		{
			Id = "Accept",
			InputCommand = "Accept",
			Description = "Select"
		}
	};

	public Image background;

	public bool wasSelected;

	public bool wasHighlighted;

	public bool isHighlighted;

	public Dictionary<KeyCode, int> intKeys = new Dictionary<KeyCode, int>
	{
		{
			KeyCode.Alpha0,
			0
		},
		{
			KeyCode.Alpha1,
			1
		},
		{
			KeyCode.Alpha2,
			2
		},
		{
			KeyCode.Alpha3,
			3
		},
		{
			KeyCode.Alpha4,
			4
		},
		{
			KeyCode.Alpha5,
			5
		},
		{
			KeyCode.Alpha6,
			6
		},
		{
			KeyCode.Alpha7,
			7
		},
		{
			KeyCode.Alpha8,
			8
		},
		{
			KeyCode.Alpha9,
			9
		}
	};

	private bool wasActive;

	private string TradeTypeAmount = "";

	private List<AbilityManagerSpacer> spacers = new List<AbilityManagerSpacer>();

	public static bool dragging = false;

	public static TradeLineData itemBeingDragged;

	public static float startAlpha = 1f;

	public static Vector2 dragStartPosition;

	public static int dragStartQuantity;

	public static NavigationController.DragContext itemDragContext;

	public bool selected => context?.IsActive() ?? false;

	public static UnityEngine.GameObject dragObject => SingletonWindowBase<TradeScreen>.instance.dragIndicator;

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
			context.axisHandlers = new Dictionary<InputAxisTypes, Action> { 
			{
				InputAxisTypes.NavigationVAxis,
				HandleNavigationVAxis
			} };
		}
		context = this.context;
		if (context.commandHandlers == null)
		{
			context.commandHandlers = new Dictionary<string, Action>
			{
				{ "V Positive", HandleVPositive },
				{ "V Negative", HandleVNegative },
				{ "CmdTradeAdd", HandleTradeAdd },
				{ "CmdTradeAllItems", HandleTradeAddAll },
				{ "CmdTradeRemove", HandleTradeRemove },
				{ "CmdVendorActions", HandleVendorActions },
				{ "CmdVendorLook", HandleVendorLook },
				{ "CmdVendorRepair", HandleVendorRepair },
				{ "CmdVendorRead", HandleVendorRead },
				{ "CmdVendorRecharge", HandleVendorRecharge },
				{ "CmdVendorExamine", HandleVendorExamine }
			};
		}
		if (this.context.data.type == TradeLineDataType.Category)
		{
			this.context.menuOptionDescriptions = (this.context.data.collapsed ? categoryExpandOptions : categoryCollapseOptions);
		}
		if (this.context.data.type == TradeLineDataType.Item)
		{
			this.context.menuOptionDescriptions = itemOptions;
		}
	}

	public void HandleTradeSome()
	{
		if (context.data.go != null)
		{
			screen.HandleTradeSome(this);
		}
	}

	public void HandleVPositive()
	{
		if (context.data.go == null)
		{
			screen.HandleVPositive();
		}
	}

	public void HandleVNegative()
	{
		if (context.data.go == null)
		{
			screen.HandleVNegative();
		}
	}

	public async void HandleTradeAddAll()
	{
		if (context.data.go != null)
		{
			if (context.data.numberSelected > 0)
			{
				TradeLineData data = context.data;
				data.numberSelected = await screen.setHowManySelected(context.data.go, 0);
				setData(context.data);
				screen.UpdateTotals();
			}
			else
			{
				TradeLineData data = context.data;
				data.numberSelected = await screen.setHowManySelected(context.data.go, context.data.go.Count);
				setData(context.data);
				screen.UpdateTotals();
			}
		}
	}

	public async void HandleVendorActions()
	{
		if (context.data.go != null)
		{
			string selected = null;
			await APIDispatch.RunAndWaitAsync(() =>
			{
				selected = TradeUI.ShowVendorActions(context.data.go, TradeScreen.Trader, IncludeModernTradeOptions: true);
			});
			if (selected == "Recharge")
			{
				screen.ClearAndSetupTradeUI();
			}
			if (selected == "Identify")
			{
				screen.ClearAndSetupTradeUI();
			}
			screen.UpdateViewFromData();
			if (selected == "Add to trade")
			{
				HandleTradeSome();
			}
		}
	}

	public async void HandleVendorLook()
	{
		if (context.data.go != null)
		{
			await APIDispatch.RunAndWaitAsync(() =>
			{
				TradeUI.DoVendorLook(context.data.go, TradeScreen.Trader);
			});
			screen.UpdateViewFromData();
		}
	}

	public async void HandleVendorExamine()
	{
		if (context.data.go != null)
		{
			await APIDispatch.RunAndWaitAsync(() =>
			{
				TradeUI.DoVendorExamine(context.data.go, TradeScreen.Trader);
			});
			screen.UpdateViewFromData();
		}
	}

	public async void HandleVendorRecharge()
	{
		if (context.data.go != null)
		{
			await APIDispatch.RunAndWaitAsync(() => TradeUI.DoVendorRecharge(context.data.go, TradeScreen.Trader));
			screen.ClearAndSetupTradeUI();
			screen.UpdateViewFromData();
		}
	}

	public async void HandleVendorRead()
	{
		if (context.data.go != null && TradeScreen.Trader != null && TradeScreen.Trader.GetIntProperty("Librarian") != 0)
		{
			await APIDispatch.RunAndWaitAsync(() =>
			{
				TradeUI.DoVendorRead(context.data.go, TradeScreen.Trader);
			});
			screen.UpdateViewFromData();
		}
	}

	public async void HandleVendorRepair()
	{
		if (context.data.go != null)
		{
			await APIDispatch.RunAndWaitAsync(() =>
			{
				TradeUI.DoVendorRepair(context.data.go, TradeScreen.Trader);
			});
			screen.UpdateViewFromData();
		}
	}

	public async void HandleTradeAdd()
	{
		if (context.data.go != null)
		{
			TradeLineData data = context.data;
			data.numberSelected = await screen.incrementHowManySelected(context.data.go, 1);
			setData(context.data);
			screen.UpdateTotals();
		}
	}

	public async void HandleTradeRemove()
	{
		if (context.data.go != null)
		{
			TradeLineData data = context.data;
			data.numberSelected = await screen.incrementHowManySelected(context.data.go, -1);
			setData(context.data);
			screen.UpdateTotals();
		}
	}

	public void HandleNavigationVAxis()
	{
		screen.HandleVAxis(NavigationController.currentEvent.axisValue);
		NavigationController.currentEvent.Handle();
	}

	public void HandleNavigationXAxis()
	{
		screen.HandleXAxis(NavigationController.currentEvent.axisValue);
		NavigationController.currentEvent.Handle();
	}

	public override void Update()
	{
		tooltipContextActive = context?.IsActive() ?? false;
		if (context.IsActive() && !dragging && context.data.go != null)
		{
			bool flag = false;
			foreach (KeyValuePair<KeyCode, int> item in intKeys.Where((KeyValuePair<KeyCode, int> kv) => Input.GetKeyDown(kv.Key) && !ControlManager.isKeyMapped(kv.Key)))
			{
				TradeTypeAmount += item.Value;
				flag = true;
			}
			if (Input.GetKeyDown(KeyCode.Backspace) && !ControlManager.isKeyMapped(KeyCode.Backspace) && !string.IsNullOrEmpty(TradeTypeAmount))
			{
				TradeTypeAmount = TradeTypeAmount.Substring(0, TradeTypeAmount.Length - 1);
				flag = true;
			}
			if (flag)
			{
				int num = Convert.ToInt32(TradeTypeAmount.IsNullOrEmpty() ? ((object)0) : TradeTypeAmount);
				if (num > context.data.go.Count)
				{
					num = ((TradeTypeAmount.Length <= 0 || TradeTypeAmount.Last() != '0') ? context.data.go.Count : 0);
				}
				if (num < 0)
				{
					num = 0;
				}
				SingletonWindowBase<TradeScreen>.instance.dragIndicatorText.SetText($"{{{{W|{num}}}}}");
				screen.setHowManySelected(context.data.go, num).ContinueWith((Task<int> i) =>
				{
					context.data.numberSelected = i.Result;
					setData(context.data);
					screen.UpdateTotals();
				});
			}
		}
		base.Update();
	}

	public void LateUpdate()
	{
		if (context.IsActive() && !dragging && !wasActive)
		{
			wasActive = true;
			if (!wasHighlighted && isHighlighted)
			{
				TradeTypeAmount = "";
			}
			if (!wasSelected && selected)
			{
				TradeTypeAmount = "";
			}
		}
		else if (wasActive && !context.IsActive())
		{
			wasActive = false;
		}
		if ((selected != wasSelected || isHighlighted != wasHighlighted) && background != null)
		{
			background.enabled = selected || isHighlighted;
			if (isHighlighted)
			{
				background.color = new Color(background.color.r, background.color.g, background.color.b, 0.5f);
			}
			if (selected)
			{
				background.color = new Color(background.color.r, background.color.g, background.color.b, 1f);
			}
		}
		wasSelected = selected;
		wasHighlighted = isHighlighted;
	}

	public void setData(FrameworkDataElement data)
	{
		if (!(data is TradeLineData tradeLineData))
		{
			return;
		}
		tooltipGo = tradeLineData.go;
		tooltipCompareGo = null;
		itemMode.SetActive(tradeLineData.type == TradeLineDataType.Item);
		categoryMode.SetActive(tradeLineData.type == TradeLineDataType.Category);
		context.data = tradeLineData;
		if (tradeLineData.type == TradeLineDataType.Category)
		{
			string text = ((!tradeLineData.collapsed) ? "-" : "+");
			categoryText.SetText("[" + text + "] " + tradeLineData.category);
			check.SetText("");
			isHighlighted = false;
			if (TutorialManager.currentStep != null)
			{
				ControlId.Assign(gameObject, "TradeLine:category:" + tradeLineData.category);
			}
			return;
		}
		if (TutorialManager.currentStep != null)
		{
			ControlId.Assign(gameObject, "TradeLine:item:" + tradeLineData.go?.ID);
		}
		icon.FromRenderable(tradeLineData.go.RenderForUI());
		check.SetText("");
		if (tradeLineData.numberSelected > 0)
		{
			check.SetText($"{{{{W|{tradeLineData.numberSelected}}}}}");
		}
		isHighlighted = tradeLineData.numberSelected > 0;
		SB.Clear();
		XRL.World.GameObject go = tradeLineData.go;
		SB.Append(go.DisplayName);
		this.text.SetText(SB.ToString());
		_ = tradeLineData.go.IsCurrency;
		string text2 = $"{TradeUI.GetValue(tradeLineData.go, tradeLineData.traderInventory):0.00}";
		rightFloatText.gameObject.SetActive(value: true);
		rightFloatText.color = new Color(0.2674735f, 0.6836081f, 0.9245283f);
		if (tradeLineData.go.IsCurrency)
		{
			rightFloatText.SetText("[{{W|$" + text2 + "}}]");
		}
		else
		{
			rightFloatText.SetText("[$" + text2 + "]");
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

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (context.data.go != null)
		{
			CursorManager.instance.cursorHidden = true;
			CursorManager.instance.forceCursorHidden = true;
			itemDragContext = new NavigationController.DragContext(() =>
			{
				OnEndDrag(null);
			});
			dragObject.SetActive(value: true);
			dragObject.transform.SetParent(UIManager.mainCanvas.transform, worldPositionStays: true);
			dragObject.GetComponent<CanvasGroup>().interactable = false;
			dragObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
			SingletonWindowBase<TradeScreen>.instance.dragIndicatorText.SetText($"{{{{W|{screen.howManySelected(context.data.go)}}}}}");
			RectTransform component = dragObject.GetComponent<RectTransform>();
			component.pivot = new Vector2(0f, 0f);
			component.anchoredPosition = Input.mousePosition / UIManager.mainCanvas.scaleFactor;
			dragStartPosition = Input.mousePosition;
			itemBeingDragged = context.data;
			startAlpha = GetComponent<CanvasGroup>().alpha;
			dragging = true;
			NavigationController.BeginDragWithContext(itemDragContext, () =>
			{
				OnEndDrag(null);
			});
		}
	}

	public async void OnDrag(PointerEventData eventData)
	{
		if (dragging)
		{
			RectTransform component = dragObject.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0.5f, 0.5f);
			component.anchorMax = new Vector2(0.5f, 0.5f);
			component.anchoredPosition = ScreenToRectPos(eventData.position, UIManager.mainCanvas, UIManager.mainCanvas.transform as RectTransform);
			float num = Input.mousePosition.x - dragStartPosition.x;
			int num2 = (int)Math.Max(0f, Math.Min((float)dragStartQuantity + num / 200f * (float)context.data.entry.GO.Count, context.data.entry.GO.Count));
			SingletonWindowBase<TradeScreen>.instance.dragIndicatorText.SetText($"{{{{W|{num2}}}}}");
			if (screen.IsGoingToWarn(context.data.go))
			{
				OnEndDrag(null);
			}
			TradeLineData data = context.data;
			data.numberSelected = await screen.setHowManySelected(context.data.go, num2);
			dragStartQuantity = context.data.numberSelected;
			setData(context.data);
			screen.UpdateTotals();
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (dragging)
		{
			CursorManager.instance.cursorHidden = false;
			CursorManager.instance.forceCursorHidden = false;
			dragObject.SetActive(value: false);
			dragging = false;
			itemBeingDragged = null;
			NavigationController.EndDragWithContext(itemDragContext);
			itemDragContext = null;
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (context.IsActive() && !dragging)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				{
					HandleTradeSome();
				}
				else
				{
					HandleTradeAddAll();
				}
			}
			else if (eventData.button == PointerEventData.InputButton.Right)
			{
				HandleVendorActions();
			}
		}
		base.OnPointerClick(eventData);
	}

	public async void OnScroll(PointerEventData eventData)
	{
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			eventData.Use();
			int num = ((Input.mouseScrollDelta.y > 0f) ? 1 : (-1));
			int num2 = Math.Max(0, Math.Min(context.data.numberSelected + num, context.data.entry.GO.Count));
			SingletonWindowBase<TradeScreen>.instance.dragIndicatorText.SetText($"{{{{W|{num2}}}}}");
			TradeLineData data = context.data;
			data.numberSelected = await screen.setHowManySelected(context.data.go, num2);
			setData(context.data);
			screen.UpdateTotals();
		}
		else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			eventData.Use();
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
