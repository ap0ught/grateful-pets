using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

[UIView("PickGameObject", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "PickGameObject", UICanvasHost = 1)]
public class PickGameObjectScreen : SingletonWindowBase<PickGameObjectScreen>, ControlManager.IControllerChangedEvent
{
	public class Context : NavigationContext
	{
	}

	public struct Result
	{
		public XRL.World.GameObject item;

		public bool wantToStore;

		public bool requestInterfaceExit;
	}

	public enum SortMode
	{
		AZ,
		Category
	}

	protected TaskCompletionSource<Result> menucomplete = new TaskCompletionSource<Result>();

	public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

	public Context navigationContext = new Context
	{
		buttonHandlers = null
	};

	public PickItem.PickItemDialogStyle style;

	public List<XRL.World.GameObject> rawGameObjects = new List<XRL.World.GameObject>();

	public List<PickGameObjectLineData> listItems = new List<PickGameObjectLineData>();

	public FrameworkScroller hotkeyBar;

	public FrameworkScroller itemScrollerController;

	public List<string> usedCategories = new List<string>();

	public Dictionary<string, List<XRL.World.GameObject>> objectCategories = new Dictionary<string, List<XRL.World.GameObject>>();

	public Dictionary<string, bool> categoryCollapsed = new Dictionary<string, bool>();

	public Dictionary<XRL.World.GameObject, string> nameCache = new Dictionary<XRL.World.GameObject, string>();

	public Dictionary<XRL.World.GameObject, string> categoryCache = new Dictionary<XRL.World.GameObject, string>();

	public UITextSkin titleText;

	public static bool RequestInterfaceExit;

	public static XRL.World.GameObject Actor;

	public static XRL.World.GameObject Container;

	public static Cell Cell;

	public static bool PreserveOrder;

	public static bool ShowContext;

	public static bool NotePlayerOwned;

	public static List<string> CategoryPriority;

	private List<UnityEngine.KeyCode> hotkeySpread = new List<UnityEngine.KeyCode>();

	public bool DoUpdate;

	public LayoutGroup targetLayoutGroup;

	public SortMode sortMode = SortMode.Category;

	public static readonly MenuOption TOGGLE_SORT = new MenuOption
	{
		Id = "TOGGLE_SORT",
		KeyDescription = "Toggle Sort",
		InputCommand = "Page Left",
		Description = "toggle sort"
	};

	public static List<MenuOption> defaultMenuOptions = new List<MenuOption>
	{
		new MenuOption
		{
			Id = "Cancel",
			InputCommand = "Cancel",
			Description = "Close Menu"
		},
		new MenuOption
		{
			InputCommand = "NavigationXYAxis",
			Description = "navigate",
			disabled = true
		},
		TOGGLE_SORT
	};

	public static List<MenuOption> getItemMenuOptions = new List<MenuOption>
	{
		new MenuOption
		{
			Id = "Cancel",
			InputCommand = "Cancel",
			Description = "Close Menu"
		},
		new MenuOption
		{
			InputCommand = "NavigationXYAxis",
			Description = "navigate",
			disabled = true
		},
		TOGGLE_SORT
	};

	public static readonly MenuOption TAKE_ALL = new MenuOption
	{
		Id = "TAKE_ALL",
		KeyDescription = "Take All",
		InputCommand = "Take All",
		Description = "take all"
	};

	public static readonly MenuOption STORE_ITEM = new MenuOption
	{
		Id = "STORE_ITEMS",
		KeyDescription = "Store Items",
		InputCommand = "Store Items",
		Description = "store an item"
	};

	private NavigationContext lastContext;

	private bool hiddenFromTargetPicker;

	public static string sortModeDescription => "sort: " + Markup.Color((SingletonWindowBase<PickGameObjectScreen>.instance.sortMode == SortMode.AZ) ? "w" : "y", PreserveOrder ? "list" : "a-z") + "/" + Markup.Color((SingletonWindowBase<PickGameObjectScreen>.instance.sortMode == SortMode.Category) ? "w" : "y", "by class");

	public bool isCollapsed(string category)
	{
		if (!categoryCollapsed.TryGetValue(category, out var value))
		{
			return CategoryPriority != null;
		}
		return value;
	}

	public void SetupContext()
	{
		vertNav.parentContext = navigationContext;
		vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		vertNav.contexts.Add(itemScrollerController.scrollContext);
		vertNav.contexts.Add(hotkeyBar.scrollContext);
		Context context = navigationContext;
		if (context.buttonHandlers == null)
		{
			context.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
			{
				InputButtonTypes.CancelButton,
				XRL.UI.Framework.Event.Helpers.Handle(Cancel)
			} };
		}
		context = navigationContext;
		if (context.commandHandlers == null)
		{
			context.commandHandlers = new Dictionary<string, Action>
			{
				{
					TOGGLE_SORT.InputCommand,
					XRL.UI.Framework.Event.Helpers.Handle(() =>
					{
						HandleMenuOption(TOGGLE_SORT);
					})
				},
				{
					TAKE_ALL.InputCommand,
					XRL.UI.Framework.Event.Helpers.Handle(() =>
					{
						HandleMenuOption(TAKE_ALL);
					})
				},
				{
					STORE_ITEM.InputCommand,
					XRL.UI.Framework.Event.Helpers.Handle(() =>
					{
						HandleMenuOption(STORE_ITEM);
					})
				}
			};
		}
		TOGGLE_SORT.Description = sortModeDescription;
		vertNav.wraps = true;
		vertNav.Setup();
		itemScrollerController.scrollContext.parentContext = vertNav;
	}

	protected async Task<Result> showScreen(IList<XRL.World.GameObject> Items, string Title, PickItem.PickItemDialogStyle Style, XRL.World.GameObject Actor, XRL.World.GameObject Container, Cell Cell, bool PreserveOrder, string CategoryPriority = null, bool ShowContext = false, bool NotePlayerOwned = false, bool Reentry = false)
	{
		The.Player.IsRealityDistortionUsable();
		RequestInterfaceExit = false;
		style = Style;
		PickGameObjectScreen.Actor = Actor;
		PickGameObjectScreen.Container = Container;
		PickGameObjectScreen.Cell = Cell;
		PickGameObjectScreen.PreserveOrder = PreserveOrder;
		PickGameObjectScreen.CategoryPriority = CategoryPriority?.CachedCommaExpansion();
		PickGameObjectScreen.ShowContext = ShowContext;
		PickGameObjectScreen.NotePlayerOwned = NotePlayerOwned;
		menucomplete.TrySetCanceled();
		menucomplete = new TaskCompletionSource<Result>();
		rawGameObjects.Clear();
		rawGameObjects.AddRange(Items);
		try
		{
			MinEvent.UIHold = true;
			await The.UiContext;
			GameManager.Instance.PushGameView("PickGameObject");
			if (!Reentry)
			{
				if (categoryCollapsed == null)
				{
					categoryCollapsed = new Dictionary<string, bool>();
				}
				categoryCollapsed.Clear();
				if (CategoryPriority != null)
				{
					foreach (string item in PickGameObjectScreen.CategoryPriority)
					{
						categoryCollapsed.Add(item, value: false);
					}
				}
			}
			titleText?.SetText(Title);
			Show();
			BeforeShow(Reentry);
			itemScrollerController.scrollContext.ActivateAndEnable();
			Result result = await menucomplete.Task;
			result.requestInterfaceExit = RequestInterfaceExit;
			return result;
		}
		finally
		{
			Hide();
			GameManager.Instance.PopGameView(bHard: true);
			MinEvent.UIHold = false;
		}
	}

	public static Task<Result> show(IList<XRL.World.GameObject> Items, string Title, PickItem.PickItemDialogStyle style, XRL.World.GameObject Actor, XRL.World.GameObject Container, Cell Cell, bool PreserveOrder, string CategoryPriority, bool ShowContext, bool NotePlayerOwned, bool Reentry)
	{
		return NavigationController.instance.SuspendContextWhile(() => SingletonWindowBase<PickGameObjectScreen>.instance.showScreen(Items, Title, style, Actor, Container, Cell, PreserveOrder, CategoryPriority, ShowContext, NotePlayerOwned, Reentry));
	}

	public void UpdateViewFromData(bool Reentry)
	{
		int selectedPosition = itemScrollerController.selectedPosition;
		if (listItems == null)
		{
			listItems = new List<PickGameObjectLineData>();
		}
		listItems.ForEach((PickGameObjectLineData i) =>
		{
			i.free();
		});
		listItems.Clear();
		int num = 0;
		nameCache.Clear();
		categoryCache.Clear();
		if (sortMode == SortMode.Category)
		{
			usedCategories.Clear();
			foreach (XRL.World.GameObject rawGameObject in rawGameObjects)
			{
				if (!categoryCache.ContainsKey(rawGameObject))
				{
					categoryCache.Add(rawGameObject, rawGameObject.GetInventoryCategory());
				}
			}
			foreach (KeyValuePair<string, List<XRL.World.GameObject>> objectCategory in objectCategories)
			{
				objectCategory.Value.Clear();
			}
			foreach (XRL.World.GameObject rawGameObject2 in rawGameObjects)
			{
				string inventoryCategory = rawGameObject2.GetInventoryCategory();
				if (!objectCategories.ContainsKey(inventoryCategory))
				{
					objectCategories.Add(inventoryCategory, new List<XRL.World.GameObject>());
				}
				objectCategories[inventoryCategory].Add(rawGameObject2);
				if (!usedCategories.Contains(inventoryCategory))
				{
					usedCategories.Add(inventoryCategory);
				}
			}
			if (!PreserveOrder)
			{
				usedCategories.Sort();
			}
		}
		if (sortMode == SortMode.Category)
		{
			num = 0;
			foreach (string usedCategory in usedCategories)
			{
				listItems.Add(PooledFrameworkDataElement<PickGameObjectLineData>.next().set(PickGameObjectLineDataType.Category, PickGameObjectLineDataStyle.Interact, null, usedCategory, isCollapsed(usedCategory), indent: false, getHotkeyChar(num), getHotkeyDescription(num)));
				num++;
				if (isCollapsed(usedCategory))
				{
					continue;
				}
				if (!PreserveOrder)
				{
					objectCategories[usedCategory].Sort((XRL.World.GameObject a, XRL.World.GameObject b) => categoryCache[a].CompareTo(categoryCache[b]));
				}
				foreach (XRL.World.GameObject item in objectCategories[usedCategory])
				{
					listItems.Add(PooledFrameworkDataElement<PickGameObjectLineData>.next().set(PickGameObjectLineDataType.Item, PickGameObjectLineDataStyle.Interact, item, usedCategory, collapsed: false, indent: true, getHotkeyChar(num), getHotkeyDescription(num)));
					num++;
				}
			}
		}
		else if (sortMode == SortMode.AZ)
		{
			foreach (XRL.World.GameObject rawGameObject3 in rawGameObjects)
			{
				nameCache.Add(rawGameObject3, rawGameObject3.GetCachedDisplayNameForSort());
			}
			num = 0;
			if (!PreserveOrder)
			{
				rawGameObjects.Sort((XRL.World.GameObject a, XRL.World.GameObject b) => nameCache[a].CompareTo(nameCache[b]));
			}
			foreach (XRL.World.GameObject rawGameObject4 in rawGameObjects)
			{
				listItems.Add(PooledFrameworkDataElement<PickGameObjectLineData>.next().set(PickGameObjectLineDataType.Item, PickGameObjectLineDataStyle.Interact, rawGameObject4, null, collapsed: false, indent: false, getHotkeyChar(num), getHotkeyDescription(num)));
				num++;
			}
		}
		itemScrollerController.BeforeShow(listItems);
		if (Reentry)
		{
			int num2 = Math.Max(0, Math.Min(selectedPosition, listItems.Count - 1));
			itemScrollerController.selectedPosition = num2;
			itemScrollerController.scrollContext.SelectIndex(num2);
			itemScrollerController.ScrollSelectedIntoView();
		}
		else if (sortMode == SortMode.Category && listItems.Count > 0)
		{
			int val = listItems.FindIndex((PickGameObjectLineData i) => i.go != null);
			val = Math.Max(0, Math.Min(val, listItems.Count - 1));
			itemScrollerController.selectedPosition = val;
			itemScrollerController.scrollContext.SelectIndex(val);
			itemScrollerController.ScrollSelectedIntoView();
		}
		foreach (FrameworkUnityScrollChild selectionClone in itemScrollerController.selectionClones)
		{
			selectionClone.GetComponent<PickGameObjectLine>().screen = this;
		}
	}

	private char getHotkeyChar(int n)
	{
		if (hotkeySpread.Count <= n)
		{
			return '\0';
		}
		return Keyboard.ConvertKeycodeToLowercaseChar(hotkeySpread[n]);
	}

	private string getHotkeyDescription(int n)
	{
		if (hotkeySpread.Count <= n)
		{
			return "";
		}
		return Keyboard.ConvertKeycodeToLowercaseChar(hotkeySpread[n]).ToString();
	}

	public void BeforeShow(bool Reentry)
	{
		XRL.World.Event.PinCurrentPool();
		SetupContext();
		hotkeySpread.Clear();
		hotkeySpread.AddRange(ControlManager.GetHotkeySpread(new List<string> { "Menus", "UINav" }));
		itemScrollerController.onSelected.RemoveAllListeners();
		itemScrollerController.onSelected.AddListener(HandleSelectItem);
		itemScrollerController.onHighlight.RemoveAllListeners();
		itemScrollerController.onHighlight.AddListener(HandleHighlightObject);
		itemScrollerController.scrollContext.wraps = false;
		UpdateMenuBars();
		UpdateViewFromData(Reentry);
		FastLayoutRebuilder.RefreshLayoutGroupsImmediateAndRecursiveCached(gameObject);
		XRL.World.Event.ResetToPin();
	}

	public IEnumerable<FrameworkDataElement> yieldMenuOptions()
	{
		foreach (MenuOption defaultMenuOption in defaultMenuOptions)
		{
			yield return defaultMenuOption;
		}
		if (style == PickItem.PickItemDialogStyle.GetItemDialog)
		{
			yield return TAKE_ALL;
		}
		if (style == PickItem.PickItemDialogStyle.GetItemDialog && Actor != null && Container != null)
		{
			yield return STORE_ITEM;
		}
		if (!(NavigationController.instance.activeContext is PickGameObjectLine.Context))
		{
			yield break;
		}
		foreach (MenuOption menuOptionDescription in NavigationController.instance.activeContext.menuOptionDescriptions)
		{
			yield return menuOptionDescription;
		}
	}

	public void UpdateMenuBars()
	{
		hotkeyBar.BeforeShow(null, yieldMenuOptions());
		hotkeyBar.GetNavigationContext().disabled = false;
		hotkeyBar.onSelected.RemoveAllListeners();
		hotkeyBar.onSelected.AddListener(HandleMenuOption);
	}

	public async void HandleMenuOption(FrameworkDataElement element)
	{
		if (FrameworkUnityController.instance.DebugContextActivity)
		{
			Debug.LogError("UI:DEBUG TAKE_ALL Initialized");
		}
		if (element == TAKE_ALL)
		{
			if (!TutorialManager.AllowCommand("Take All"))
			{
				return;
			}
			bool doClose = false;
			await APIDispatch.RunAndWaitAsync(() =>
			{
				if (PickItem.TakeAll(Actor, Container, Cell, rawGameObjects, ref RequestInterfaceExit))
				{
					doClose = true;
				}
			});
			if (FrameworkUnityController.instance.DebugContextActivity)
			{
				Debug.LogError("UI:DEBUG TAKE_ALL Complete");
			}
			if (doClose)
			{
				menucomplete.TrySetResult(default);
			}
			return;
		}
		if (element == STORE_ITEM)
		{
			menucomplete.TrySetResult(new Result
			{
				wantToStore = true
			});
			return;
		}
		if (element.Id == "Cancel")
		{
			Cancel();
		}
		if (element == TOGGLE_SORT)
		{
			if (sortMode == SortMode.AZ)
			{
				sortMode = SortMode.Category;
			}
			else
			{
				sortMode = SortMode.AZ;
			}
			BeforeShow(Reentry: false);
		}
	}

	public override void Hide()
	{
		navigationContext.disabled = true;
		listItems.ForEach((PickGameObjectLineData i) =>
		{
			i.free();
		});
		listItems.Clear();
		base.Hide();
	}

	public void Cancel()
	{
		ControlManager.ResetInput();
		Hide();
		menucomplete.TrySetResult(default);
	}

	void ControlManager.IControllerChangedEvent.ControllerChanged()
	{
	}

	public void HandleVAxis(int? val)
	{
		if (val == 0)
		{
			return;
		}
		PickGameObjectLineData item = listItems[itemScrollerController.selectedPosition];
		foreach (string usedCategory in usedCategories)
		{
			categoryCollapsed[usedCategory] = val < 0;
		}
		BeforeShow(Reentry: false);
		ReselectAfterCategoryStateChange(item);
	}

	public void HandleXAxis(int? val)
	{
		if (val != 0)
		{
			PickGameObjectLineData pickGameObjectLineData = listItems[itemScrollerController.selectedPosition];
			if (pickGameObjectLineData.category != null)
			{
				categoryCollapsed[pickGameObjectLineData.category] = val < 0;
			}
			BeforeShow(Reentry: false);
			ReselectAfterCategoryStateChange(pickGameObjectLineData);
		}
	}

	public void ReselectAfterCategoryStateChange(PickGameObjectLineData item)
	{
		int num = -1;
		if (item.go != null)
		{
			num = listItems.FindIndex((PickGameObjectLineData i) => i.go == item.go);
		}
		if (num == -1)
		{
			num = listItems.FindIndex((PickGameObjectLineData i) => i.category == item.category);
		}
		num = Math.Max(0, Math.Min(num, listItems.Count - 1));
		itemScrollerController.selectedPosition = num;
		itemScrollerController.ScrollSelectedIntoView();
		itemScrollerController.scrollContext.GetContextAt(num).Activate();
	}

	public void BackgroundClicked()
	{
		if (navigationContext.IsActive())
		{
			Cancel();
		}
	}

	public void HandleSelectItem(FrameworkDataElement element)
	{
		if (element is PickGameObjectLineData pickGameObjectLineData)
		{
			if (pickGameObjectLineData.type == PickGameObjectLineDataType.Category)
			{
				categoryCollapsed[pickGameObjectLineData.category] = !isCollapsed(pickGameObjectLineData.category);
				BeforeShow(Reentry: false);
			}
			else
			{
				menucomplete.TrySetResult(new Result
				{
					item = pickGameObjectLineData.go
				});
			}
		}
	}

	public void HandleHighlightObject(FrameworkDataElement element)
	{
		_ = element is PickGameObjectLineData;
	}

	public void Update()
	{
		CanvasGroup canvasGroup = base.canvasGroup;
		if ((object)canvasGroup != null && canvasGroup.alpha == 1f && GameManager.Instance.CurrentGameView == "PickTarget")
		{
			hiddenFromTargetPicker = true;
			base.canvasGroup.alpha = 0f;
		}
		if (hiddenFromTargetPicker && GameManager.Instance.CurrentGameView != "PickTarget")
		{
			base.canvasGroup.alpha = 1f;
		}
		if (!isCurrentWindow() || !navigationContext.IsActive())
		{
			return;
		}
		if (listItems != null)
		{
			foreach (PickGameObjectLineData listItem in listItems)
			{
				if (ControlManager.isCharDown(listItem.quickKey))
				{
					HandleSelectItem(listItem);
					return;
				}
			}
		}
		if (lastContext != NavigationController.instance.activeContext)
		{
			UpdateMenuBars();
			lastContext = NavigationController.instance.activeContext;
		}
	}

	public void MoveItem(PickGameObjectLineData data, int direction)
	{
	}
}
