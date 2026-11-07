using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using XRL;
using XRL.CharacterBuilds.UI;
using XRL.Help;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[UIView("HelpScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "HelpScreen", UICanvasHost = 1)]
public class HelpScreen : SingletonWindowBase<HelpScreen>, ControlManager.IControllerChangedEvent
{
	public FrameworkScroller hotkeyBar;

	public FrameworkScroller helpScroller;

	public FrameworkScroller categoryScroller;

	public RectTransform safeArea;

	public EmbarkBuilderModuleBackButton backButton;

	public TaskCompletionSource<bool> completionSource;

	public NavigationContext globalContext = new NavigationContext();

	public ScrollContext<NavigationContext> midHorizNav = new ScrollContext<NavigationContext>();

	public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

	public List<HelpDataRow> menuItems = new List<HelpDataRow>();

	public Dictionary<string, bool> categoryExpanded = new Dictionary<string, bool>();

	public float lastWidth;

	public float breakpointBackButtonWidth;

	private ScrollViewCalcs _svc = new ScrollViewCalcs();

	public bool wasInScroller;

	private bool SelectFirst = true;

	public List<MenuOption> keyMenuOptions = new List<MenuOption>();

	public FrameworkDataElement lastSelectedElement;

	public bool breakBackButton => lastWidth <= breakpointBackButtonWidth;

	public static MenuOption BACK_BUTTON => EmbarkBuilderOverlayWindow.BackMenuOption;

	public async Task<bool> HelpMenu(string startPage = null)
	{
		menuItems = The.Manual.Pages.Select((KeyValuePair<string, XRLManualPage> kv) =>
		{
			categoryExpanded.TryAdd(kv.Key, value: true);
			return new HelpDataRow
			{
				CategoryId = kv.Key,
				Collapsed = false,
				Description = kv.Key,
				HelpText = kv.Value.GetData()
			};
		}).ToList();
		helpScroller.ScrollOnSelection = ShouldScrollToSelection;
		SelectFirst = true;
		completionSource?.TrySetCanceled();
		completionSource = new TaskCompletionSource<bool>();
		await The.UiContext;
		ControlManager.ResetInput();
		Show();
		if (!string.IsNullOrEmpty(startPage))
		{
			helpScroller.scrollContext.SelectIndex(menuItems.IndexOf(menuItems.Where((HelpDataRow i) => i.CategoryId == startPage).First()));
		}
		bool info = await completionSource.Task;
		DisableNavContext();
		await The.UiContext;
		Hide();
		return info;
	}

	public bool ShouldScrollToSelection()
	{
		return !ScrollViewCalcs.GetScrollViewCalcs(helpScroller.GetPrefabForIndex(helpScroller.selectedPosition).transform as RectTransform, _svc).isAnyInView;
	}

	public void Exit()
	{
		completionSource?.TrySetResult(result: false);
	}

	public void Update()
	{
		if (globalContext.IsActive())
		{
			bool flag = NavigationController.instance.activeContext?.IsInside(helpScroller.GetNavigationContext()) ?? false;
			float width = rectTransform.rect.width;
			if (flag != wasInScroller || lastWidth != width)
			{
				wasInScroller = flag;
				lastWidth = width;
				backButton.gameObject.SetActive(!breakBackButton);
				safeArea.offsetMin = new Vector2(breakBackButton ? 10 : 150, safeArea.offsetMin.y);
				safeArea.offsetMax = new Vector2(breakBackButton ? (-10) : (-150), safeArea.offsetMax.y);
				UpdateMenuBars();
			}
		}
	}

	public void ControllerChanged()
	{
		UpdateMenuBars();
	}

	public void SetupContext()
	{
		globalContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
		globalContext.buttonHandlers.Set(InputButtonTypes.CancelButton, XRL.UI.Framework.Event.Helpers.Handle(Exit));
		globalContext.commandHandlers = new Dictionary<string, Action> { 
		{
			BACK_BUTTON.InputCommand,
			XRL.UI.Framework.Event.Helpers.Handle(Exit)
		} };
		midHorizNav.SetAxis(InputAxisTypes.NavigationXAxis);
		midHorizNav.contexts.Clear();
		midHorizNav.contexts.Add(backButton.navigationContext);
		midHorizNav.contexts.Add(categoryScroller.GetNavigationContext());
		midHorizNav.contexts.Add(vertNav);
		vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		vertNav.contexts.Clear();
		vertNav.contexts.Add(helpScroller.GetNavigationContext());
		vertNav.contexts.Add(hotkeyBar.GetNavigationContext());
		midHorizNav.Setup();
		midHorizNav.parentContext = globalContext;
		helpScroller.scrollContext.wraps = false;
		vertNav.wraps = false;
	}

	public void UpdateMenuBars()
	{
		keyMenuOptions.Clear();
		keyMenuOptions.Add(new MenuOption
		{
			InputCommand = "NavigationXYAxis",
			Description = "navigate"
		});
		keyMenuOptions.Add(new MenuOption
		{
			InputCommand = "Accept",
			Description = "Toggle Visibility"
		});
		hotkeyBar.BeforeShow(null, keyMenuOptions);
		hotkeyBar.GetNavigationContext().disabled = false;
		hotkeyBar.onSelected.RemoveAllListeners();
		hotkeyBar.onSelected.AddListener(HandleMenuOption);
		foreach (NavigationContext item in hotkeyBar.scrollContext.contexts.GetRange(0, 2))
		{
			item.disabled = true;
		}
	}

	public void HandleMenuOption(FrameworkDataElement data)
	{
	}

	public IEnumerable<FrameworkDataElement> GetMenuItems()
	{
		foreach (HelpDataRow menuItem in menuItems)
		{
			menuItem.Collapsed = !categoryExpanded[menuItem.CategoryId];
			yield return menuItem;
		}
	}

	public IEnumerable<FrameworkDataElement> GetCategoryItems()
	{
		return GetMenuItems();
	}

	public override void Show()
	{
		base.Show();
		backButton?.gameObject.SetActive(value: true);
		if (backButton.navigationContext == null)
		{
			backButton.Awake();
		}
		backButton.navigationContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
		backButton.navigationContext.buttonHandlers.Set(InputButtonTypes.AcceptButton, XRL.UI.Framework.Event.Helpers.Handle(Exit));
		helpScroller.scrollContext.wraps = false;
		helpScroller.BeforeShow(null, GetMenuItems());
		categoryScroller.scrollContext.wraps = true;
		categoryScroller.BeforeShow(null, GetCategoryItems());
		foreach (FrameworkUnityScrollChild selectionClone in categoryScroller.selectionClones)
		{
			FrameworkHoverable component = selectionClone.GetComponent<FrameworkHoverable>();
			if ((object)component != null)
			{
				component.enabled = false;
			}
		}
		if (SelectFirst)
		{
			SelectFirst = false;
			helpScroller.scrollContext.selectedPosition = 0;
		}
		helpScroller.onSelected.RemoveAllListeners();
		helpScroller.onSelected.AddListener(HandleSelect);
		helpScroller.onHighlight.RemoveAllListeners();
		helpScroller.onHighlight.AddListener(HandleHighlight);
		categoryScroller.onSelected.RemoveAllListeners();
		categoryScroller.onSelected.AddListener(HandleSelectLeft);
		categoryScroller.onHighlight.RemoveAllListeners();
		categoryScroller.onHighlight.AddListener(HandleHighlightLeft);
		UpdateMenuBars();
		SetupContext();
		EnableNavContext();
	}

	public void HandleSelect(FrameworkDataElement element)
	{
		if (element is HelpDataRow helpDataRow)
		{
			categoryExpanded[helpDataRow.CategoryId] = !categoryExpanded[helpDataRow.CategoryId];
			Show();
			helpScroller.scrollContext.SelectIndex(GetMenuItems().ToList().FindIndex((FrameworkDataElement s) => s == element));
		}
	}

	public void HandleHighlight(FrameworkDataElement element)
	{
		lastSelectedElement = element;
		string catId = null;
		if (element is HelpDataRow helpDataRow)
		{
			catId = helpDataRow.CategoryId;
		}
		if (catId != null)
		{
			FrameworkScroller frameworkScroller = categoryScroller;
			int selectedPosition = (categoryScroller.scrollContext.selectedPosition = GetCategoryItems().ToList().FindIndex((FrameworkDataElement s) => (s as HelpDataRow)?.CategoryId == catId));
			frameworkScroller.selectedPosition = selectedPosition;
		}
	}

	public void HandleSelectLeft(FrameworkDataElement element)
	{
		HelpDataRow cat = element as HelpDataRow;
		if (cat != null)
		{
			categoryExpanded[cat.CategoryId] = true;
			Show();
			int num = GetMenuItems().ToList().FindIndex((FrameworkDataElement s) => (s as HelpDataRow)?.CategoryId == cat.CategoryId);
			helpScroller.scrollContext.GetContextAt(num).Activate();
			helpScroller.ScrollSelectedIntoView();
			ScrollViewCalcs.ScrollToTopOfRect(helpScroller.GetPrefabForIndex(num).GetComponent<RectTransform>());
		}
	}

	public void HandleHighlightLeft(FrameworkDataElement element)
	{
		HelpDataRow cat = element as HelpDataRow;
		if (cat != null && ((GetMenuItems().Skip(helpScroller.selectedPosition).FirstOrDefault() is HelpDataRow helpDataRow) ? helpDataRow.CategoryId : null) != cat.CategoryId)
		{
			int num = GetMenuItems().ToList().FindIndex((FrameworkDataElement s) => (s as HelpDataRow)?.CategoryId == cat.CategoryId);
			FrameworkScroller frameworkScroller = helpScroller;
			int selectedPosition = (helpScroller.scrollContext.selectedPosition = num);
			frameworkScroller.selectedPosition = selectedPosition;
			helpScroller.ScrollSelectedIntoView();
			ScrollViewCalcs.ScrollToTopOfRect(helpScroller.GetPrefabForIndex(num).GetComponent<RectTransform>());
		}
	}

	public override void Hide()
	{
		base.Hide();
		DisableNavContext();
		ControlManager.ResetInput();
		gameObject.SetActive(value: false);
	}

	public void EnableNavContext()
	{
		globalContext.disabled = false;
		helpScroller.GetNavigationContext().ActivateAndEnable();
	}

	public void DisableNavContext(bool deactivate = true)
	{
		if (deactivate)
		{
			NavigationContext activeContext = NavigationController.instance.activeContext;
			if (activeContext != null && activeContext.IsInside(globalContext))
			{
				NavigationController.instance.activeContext = null;
			}
		}
		globalContext.disabled = true;
	}
}
