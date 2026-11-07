using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

[UIView("StatusScreensScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "StatusScreens", UICanvasHost = 1)]
public class StatusScreensScreen : SingletonWindowBase<StatusScreensScreen>, ControlManager.IControllerChangedEvent
{
	public enum STATUS_SCREEN_ORDINAL
	{
		TINKERING = 3
	}

	public NavigationContext navigationContext = new NavigationContext();

	public NavigationContext screenGlobalContext = new NavigationContext();

	public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

	public ScrollContext<NavigationContext> horizNav = new ScrollContext<NavigationContext>();

	public ScrollContext<NavigationContext> bottomHorizNav = new ScrollContext<NavigationContext>();

	public List<Transform> Screens;

	public ButtonBar screenTabs;

	public FilterBar filterBar;

	public UnityEngine.GameObject categoryBar;

	public NavigationContext categoryBarContext;

	public FrameworkScroller menuOptionScroller;

	public RememberMaxSize menuOptionSizeMinder;

	public Transform scaleTarget;

	public LayoutElement globalLayoutElement;

	protected TaskCompletionSource<ScreenReturn> menucomplete = new TaskCompletionSource<ScreenReturn>();

	public static XRL.World.GameObject GO;

	public int CurrentScreen;

	private IStatusScreen activeScreen;

	public int categoryBarLayoutFrames = 3;

	public VerticalLayoutGroup categoryBarLayoutGroup;

	private List<ButtonBar.ButtonBarButtonData> tabs = new List<ButtonBar.ButtonBarButtonData>(8);

	public static readonly MenuOption SET_FILTER = new MenuOption
	{
		Id = "SET_FILTER",
		KeyDescription = "Filter",
		InputCommand = "CmdFilter",
		Description = "Filter"
	};

	private string filterText = "";

	public List<MenuOption> defaultMenuOptionOrder = new List<MenuOption>
	{
		new MenuOption
		{
			InputCommand = "NavigationXYAxis",
			Description = "navigation",
			disabled = true
		},
		new MenuOption
		{
			InputCommand = "Accept",
			Description = "Accept",
			disabled = true
		}
	};

	public bool updateMenuBar;

	private ControlManager.InputDeviceType previousType;

	private NavigationContext previousContext;

	private static List<string> hideWhenShown = new List<string> { "PickDirection", "PickTarget" };

	private bool PickHide;

	public static bool prewarm()
	{
		SingletonWindowBase<StatusScreensScreen>.instance?._prewarm();
		return true;
	}

	public bool _prewarm()
	{
		GO = null;
		MinEvent.UIHold = true;
		try
		{
			for (int i = 0; i < Screens.Count; i++)
			{
				(Screens[i]?.GetComponent<IStatusScreen>())?.Prewarm();
			}
		}
		finally
		{
			MinEvent.UIHold = false;
			GameManager.Instance.PopGameView();
		}
		menuOptionScroller.DisableFirstLateUpdate();
		filterBar.DisableFirstLateUpdate();
		screenTabs.DisableFirstLateUpdate();
		return true;
	}

	public static Task<ScreenReturn> show(int StartingScreen, XRL.World.GameObject GO)
	{
		SingletonWindowBase<StatusScreensScreen>.instance.CurrentScreen = StartingScreen % SingletonWindowBase<StatusScreensScreen>.instance.Screens.Count;
		StatusScreensScreen.GO = GO;
		SoundManager.PlayUISound("Sounds/UI/ui_popup_open", 1f, Combat: false, Interface: true);
		return NavigationController.instance.SuspendContextWhile(() => SingletonWindowBase<StatusScreensScreen>.instance.showScreen(StartingScreen, GO));
	}

	protected async Task<ScreenReturn> showScreen(int StartingScreen, XRL.World.GameObject GO)
	{
		CurrentScreen = StartingScreen % Screens.Count;
		StatusScreensScreen.GO = GO;
		MinEvent.UIHold = true;
		GameManager.Instance.PushGameView("StatusScreensScreen");
		try
		{
			await The.UiContext;
			if (globalLayoutElement != null)
			{
				switch (Options.GetOption("OptionStatusScreenSize"))
				{
				case "Standard":
					globalLayoutElement.preferredHeight = 900f;
					globalLayoutElement.preferredWidth = 1615f;
					globalLayoutElement.flexibleHeight = 0f;
					globalLayoutElement.flexibleWidth = 0f;
					break;
				case "Full Height":
				{
					RectTransform rectTransform = UIManager.mainCanvas.transform as RectTransform;
					globalLayoutElement.preferredHeight = rectTransform.rect.height;
					globalLayoutElement.preferredWidth = Math.Min(rectTransform.rect.width, (int)((double)rectTransform.rect.height * 1.8));
					globalLayoutElement.flexibleHeight = 0f;
					globalLayoutElement.flexibleWidth = 0f;
					break;
				}
				case "Full":
					globalLayoutElement.preferredHeight = 0f;
					globalLayoutElement.preferredWidth = 0f;
					globalLayoutElement.flexibleHeight = 1f;
					globalLayoutElement.flexibleWidth = 1f;
					break;
				}
			}
			float num = 1f + (float)Convert.ToInt32(Options.GetOption("OptionCharacterSheetAdditionalScale")) * 0.01f;
			scaleTarget.localScale = new Vector3(num, num, 1f);
			categoryBarLayoutGroup.enabled = true;
			categoryBarLayoutFrames = 3;
			UpdateViewFromData();
			menucomplete = new TaskCompletionSource<ScreenReturn>();
			ScreenReturn result = await menucomplete.Task;
			Cleanup();
			return result;
		}
		finally
		{
			MinEvent.UIHold = false;
			GameManager.Instance.PopGameView();
		}
	}

	public void SetPage(int page)
	{
		CurrentScreen = page;
		UpdateActiveScreen();
	}

	public void NextPage()
	{
		InventoryAndEquipmentStatusScreen.EnteringFromEquipmentSide = true;
		CurrentScreen++;
		if (CurrentScreen >= Screens.Count)
		{
			CurrentScreen = 0;
		}
		UpdateActiveScreen();
	}

	public void PrevPage()
	{
		InventoryAndEquipmentStatusScreen.EnteringFromEquipmentSide = false;
		CurrentScreen--;
		if (CurrentScreen < 0)
		{
			CurrentScreen = Screens.Count - 1;
		}
		UpdateActiveScreen();
	}

	public void OnCloseButton()
	{
		if (navigationContext.IsActive())
		{
			Exit();
		}
	}

	public void Exit()
	{
		foreach (Transform screen in Screens)
		{
			if (screen != null)
			{
				screen.GetComponent<IStatusScreen>().Exit();
			}
		}
		SoundManager.PlayUISound("Sounds/UI/ui_popup_close", 1f, Combat: false, Interface: true);
		menucomplete.TrySetResult(ScreenReturn.Exit);
		freeTabs();
		Hide();
	}

	public void UpdateActiveScreen()
	{
		filterText = "";
		foreach (Transform screen in Screens)
		{
			if (screen != null)
			{
				screen.GetComponent<IStatusScreen>().HideScreen();
			}
		}
		int index = CurrentScreen % Screens.Count;
		activeScreen = Screens[index]?.GetComponent<IStatusScreen>();
		if (activeScreen != null)
		{
			GameManager.Instance.GetViewData("StatusScreensScreen").NavCategory = activeScreen.GetNavigationCategory();
		}
		bool flag = activeScreen?.WantsCategoryBar() ?? categoryBar.gameObject.activeInHierarchy;
		if (categoryBar.activeSelf != flag)
		{
			categoryBar.gameObject.SetActive(flag);
			categoryBarLayoutFrames = 3;
			categoryBarLayoutGroup.enabled = true;
		}
		if (activeScreen?.WantsCategoryBar() ?? categoryBar.gameObject.activeInHierarchy)
		{
			if (!vertNav.contexts.Contains(filterBar.scrollContext))
			{
				vertNav.contexts.Insert(1, filterBar.scrollContext);
				vertNav.Setup();
			}
		}
		else if (vertNav.contexts.Contains(filterBar.scrollContext))
		{
			vertNav.contexts.Remove(filterBar.scrollContext);
			vertNav.Setup();
		}
		menuOptionSizeMinder.forceReset = true;
		filterBar.SingleCategoryOnly = false;
		filterBar.ResetFilters();
		screenGlobalContext.menuOptionDescriptions?.Clear();
		screenGlobalContext.axisHandlers?.Clear();
		screenGlobalContext.buttonHandlers?.Clear();
		screenGlobalContext.commandHandlers?.Clear();
		NavigationContext navigationContext = activeScreen?.ShowScreen(GO, this);
		activeScreen?.CheckLayoutFrame();
		if (navigationContext == null)
		{
			MetricsManager.LogError("Selected active screen " + (activeScreen?.GetTabString() ?? "(null)") + " didn't return a navigation context! You probably need to override ShowScreen and return one.");
		}
		horizNav.contexts.Clear();
		horizNav.contexts.Add(navigationContext);
		horizNav.Setup();
		navigationContext.ActivateAndEnable();
		SetButtons();
	}

	private void SetButtons()
	{
		freeTabs();
		tabs.AddRange(from transform in Screens
			where transform != null
			select PooledFrameworkDataElement<ButtonBar.ButtonBarButtonData>.next().set(transform.GetComponent<IStatusScreen>().GetTabString().ToUpper(), transform.GetComponent<IStatusScreen>().GetTabIcon(), (CurrentScreen % Screens.Count == transform.GetSiblingIndex()) ? ButtonBar.ButtonBarButtonData.HighlightState.Highlighted : ButtonBar.ButtonBarButtonData.HighlightState.NotHighlighted));
		for (int num = 0; num < tabs.Count; num++)
		{
			int s = num;
			tabs[num].onSelect = () =>
			{
				CurrentScreen = s;
				UpdateActiveScreen();
			};
		}
		screenTabs.SetButtons(tabs);
		for (int num2 = 0; num2 < tabs.Count; num2++)
		{
			ControlId.Assign(tabs[num2]?.button.gameObject, $"StatusScreensScreenTab:{num2}");
		}
	}

	private void freeTabs()
	{
		for (int i = 0; i < tabs.Count; i++)
		{
			tabs[i].free();
		}
		tabs.Clear();
	}

	public void SelectTab(FrameworkDataElement el)
	{
		if (el is ButtonBar.ButtonBarButtonData buttonBarButtonData)
		{
			buttonBarButtonData.onSelect();
		}
	}

	public void OnSearchTextChange(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			filterText = "";
		}
		else
		{
			filterText = text;
		}
		OnFilterUpdated();
	}

	public void OnFilterUpdated()
	{
		activeScreen?.FilterUpdated(filterText);
	}

	public void SetupContexts()
	{
		filterBar.SetupContext();
		filterBar.OnSearchTextChange.RemoveAllListeners();
		filterBar.OnSearchTextChange.AddListener(OnSearchTextChange);
		filterBar.filtersUpdated = OnFilterUpdated;
		NavigationContext navigationContext = this.navigationContext;
		if (navigationContext.axisHandlers == null)
		{
			navigationContext.axisHandlers = new Dictionary<InputAxisTypes, Action> { 
			{
				InputAxisTypes.NavigationPageXAxis,
				XRL.UI.Framework.Event.Helpers.Handle(XRL.UI.Framework.Event.Helpers.Axis(NextPage, PrevPage))
			} };
		}
		navigationContext = this.navigationContext;
		if (navigationContext.buttonHandlers == null)
		{
			navigationContext.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
			{
				InputButtonTypes.CancelButton,
				XRL.UI.Framework.Event.Helpers.Handle(Exit)
			} };
		}
		navigationContext = this.navigationContext;
		if (navigationContext.commandHandlers == null)
		{
			navigationContext.commandHandlers = new Dictionary<string, Action>
			{
				{
					SET_FILTER.InputCommand,
					XRL.UI.Framework.Event.Helpers.Handle(() =>
					{
						filterBar.searchInput.EnterAndOpen();
					})
				},
				{
					"Category Left",
					XRL.UI.Framework.Event.Helpers.Handle(() =>
					{
						filterBar.CategoryLeft();
					})
				},
				{
					"Category Right",
					XRL.UI.Framework.Event.Helpers.Handle(() =>
					{
						filterBar.CategoryRight();
					})
				}
			};
		}
		screenTabs.onSelected.RemoveAllListeners();
		screenTabs.onSelected.AddListener(SelectTab);
		screenTabs.SetupContext();
		screenGlobalContext.parentContext = this.navigationContext;
		horizNav.SetAxis(InputAxisTypes.NavigationXAxis);
		vertNav.parentContext = screenGlobalContext;
		vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		vertNav.contexts.Clear();
		vertNav.contexts.Add(screenTabs.scrollContext);
		vertNav.contexts.Add(horizNav);
		bottomHorizNav.SetAxis(InputAxisTypes.NavigationXAxis);
		bottomHorizNav.contexts.Clear();
		bottomHorizNav.contexts.Add(filterBar.searchInput.context);
		bottomHorizNav.contexts.Add(menuOptionScroller.scrollContext);
		vertNav.contexts.Add(bottomHorizNav);
		screenGlobalContext.Setup();
		vertNav.Setup();
		Screens?.ForEach((Transform s) =>
		{
			s.GetComponent<IStatusScreen>().PrepareLayoutFrame();
		});
		UpdateActiveScreen();
	}

	public void UpdateViewFromData()
	{
		XRL.World.Event.PinCurrentPool();
		SetupContexts();
		SetButtons();
	}

	public void Cleanup()
	{
	}

	void ControlManager.IControllerChangedEvent.ControllerChanged()
	{
	}

	public void BackgroundClicked()
	{
		if (navigationContext.IsActive())
		{
			Exit();
		}
	}

	public virtual void HandleMenuOption(FrameworkDataElement data)
	{
		activeScreen?.HandleMenuOption(data);
	}

	public virtual void Update()
	{
		if (categoryBarLayoutGroup.enabled)
		{
			categoryBarLayoutFrames--;
			if (categoryBarLayoutFrames <= 0)
			{
				categoryBarLayoutGroup.enabled = false;
			}
		}
		if (!PickHide && hideWhenShown.Contains(GameManager.Instance.CurrentGameView))
		{
			if (canvas.enabled)
			{
				canvas.enabled = false;
			}
			PickHide = true;
		}
		else if (PickHide && GameManager.Instance.CurrentGameView == "StatusScreensScreen")
		{
			if (!canvas.enabled)
			{
				canvas.enabled = true;
			}
			PickHide = false;
		}
		if (!navigationContext.IsActive())
		{
			return;
		}
		if (previousContext != NavigationController.instance.activeContext || ControlManager.activeControllerType != previousType)
		{
			previousContext = NavigationController.instance.activeContext;
			previousType = ControlManager.activeControllerType;
			if (previousContext != null)
			{
				updateMenuBar = true;
			}
		}
		if (updateMenuBar)
		{
			menuOptionScroller.BeforeShow(null, NavigationController.GetMenuOptions(defaultMenuOptionOrder));
			menuOptionScroller.onSelected.RemoveAllListeners();
			menuOptionScroller.onSelected.AddListener(HandleMenuOption);
			updateMenuBar = false;
			previousContext = NavigationController.instance.activeContext;
		}
	}
}
