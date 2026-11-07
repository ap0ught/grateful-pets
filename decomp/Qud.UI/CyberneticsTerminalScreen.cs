using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using XRL;
using XRL.CharacterBuilds.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[UIView("CyberneticsTerminalScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "CyberneticsTerminal", UICanvasHost = 1)]
public class CyberneticsTerminalScreen : SingletonWindowBase<CyberneticsTerminalScreen>, ControlManager.IControllerChangedEvent
{
	public FrameworkScroller hotkeyBar;

	public FrameworkScroller displayScroller;

	public RectTransform safeArea;

	public EmbarkBuilderModuleBackButton backButton;

	public TaskCompletionSource<bool> completionSource;

	public NavigationContext globalContext = new NavigationContext();

	public ScrollContext<NavigationContext> midHorizNav = new ScrollContext<NavigationContext>();

	public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

	public Dictionary<string, bool> categoryExpanded = new Dictionary<string, bool>();

	public float lastWidth;

	public float breakpointBackButtonWidth;

	public string FooterText = "";

	private GenericTerminal genericTerminal;

	private CyberneticsTerminal cyberneticsTerminal;

	public static bool initReady;

	private ScrollViewCalcs _svc = new ScrollViewCalcs();

	public bool wasInScroller;

	public List<MenuOption> keyMenuOptions = new List<MenuOption>();

	public UITextSkin footerTextSkin;

	public bool breakBackButton => lastWidth <= breakpointBackButtonWidth;

	public static MenuOption BACK_BUTTON => EmbarkBuilderOverlayWindow.BackMenuOption;

	public static async Task<bool> ShowCyberneticsTerminal(CyberneticsTerminal terminal)
	{
		return await SingletonWindowBase<CyberneticsTerminalScreen>.instance._ShowCyberneticsTerminal(terminal);
	}

	public static async Task<bool> ShowGenericTerminal(GenericTerminal terminal)
	{
		return await SingletonWindowBase<CyberneticsTerminalScreen>.instance._ShowGenericTerminal(terminal);
	}

	public async Task<bool> _ShowCyberneticsTerminal(CyberneticsTerminal terminal)
	{
		genericTerminal = null;
		cyberneticsTerminal = terminal;
		displayScroller.ScrollOnSelection = ShouldScrollToSelection;
		completionSource?.TrySetCanceled();
		completionSource = new TaskCompletionSource<bool>();
		await The.UiContext;
		ControlManager.ResetInput();
		FooterText = "";
		initReady = false;
		await APIDispatch.RunAndWaitAsync(() =>
		{
			cyberneticsTerminal.CurrentScreen.BeforeRender(null, ref FooterText);
		});
		Show();
		initReady = true;
		bool info = await completionSource.Task;
		DisableNavContext();
		await The.UiContext;
		Hide();
		return info;
	}

	public async Task<bool> _ShowGenericTerminal(GenericTerminal terminal)
	{
		genericTerminal = terminal;
		cyberneticsTerminal = null;
		displayScroller.ScrollOnSelection = ShouldScrollToSelection;
		completionSource?.TrySetCanceled();
		completionSource = new TaskCompletionSource<bool>();
		await The.UiContext;
		ControlManager.ResetInput();
		FooterText = "";
		initReady = false;
		await APIDispatch.RunAndWaitAsync(() =>
		{
			genericTerminal.currentScreen.BeforeRender(null, ref FooterText);
		});
		Show();
		initReady = true;
		bool info = await completionSource.Task;
		DisableNavContext();
		await The.UiContext;
		Hide();
		return info;
	}

	public bool ShouldScrollToSelection()
	{
		return !ScrollViewCalcs.GetScrollViewCalcs(displayScroller.GetPrefabForIndex(displayScroller.selectedPosition).transform as RectTransform, _svc).isAnyInView;
	}

	public void Exit()
	{
		completionSource?.TrySetResult(result: false);
	}

	public void Update()
	{
		if (!globalContext.IsActive())
		{
			return;
		}
		bool flag = NavigationController.instance.activeContext?.IsInside(displayScroller.GetNavigationContext()) ?? false;
		float width = rectTransform.rect.width;
		if (flag != wasInScroller || lastWidth != width)
		{
			wasInScroller = flag;
			lastWidth = width;
			if (safeArea != null)
			{
				safeArea.offsetMin = new Vector2(breakBackButton ? 10 : 150, safeArea.offsetMin.y);
				safeArea.offsetMax = new Vector2(breakBackButton ? (-10) : (-150), safeArea.offsetMax.y);
			}
			UpdateMenuBars();
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
		midHorizNav.contexts.Add(vertNav);
		vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		vertNav.contexts.Clear();
		vertNav.contexts.Add(displayScroller.GetNavigationContext());
		vertNav.contexts.Add(hotkeyBar.GetNavigationContext());
		midHorizNav.Setup();
		midHorizNav.parentContext = globalContext;
		displayScroller.scrollContext.wraps = false;
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
			Description = "accept"
		});
		keyMenuOptions.Add(new MenuOption
		{
			InputCommand = "Cancel",
			Description = "quit"
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

	public async Task HandleTextComplete()
	{
		TerminalScreen lastScreen = cyberneticsTerminal?.CurrentScreen;
		await APIDispatch.RunAndWaitAsync(() =>
		{
			cyberneticsTerminal?.CurrentScreen?.TextComplete();
		});
		if (cyberneticsTerminal?.CurrentScreen != lastScreen)
		{
			Show();
		}
	}

	public IEnumerable<FrameworkDataElement> GetMenuItems()
	{
		yield return new CyberneticsTerminalLineData
		{
			Text = (cyberneticsTerminal?.CurrentScreen?.RenderedTextForModernUI ?? genericTerminal?.currentScreen?.RenderedTextForModernUI),
			OptionID = -1,
			screen = this
		};
		if (cyberneticsTerminal != null)
		{
			for (int o = 0; o < cyberneticsTerminal.CurrentScreen.Options.Count; o++)
			{
				yield return new CyberneticsTerminalLineData
				{
					Text = cyberneticsTerminal.CurrentScreen.Options[o],
					screen = this,
					OptionID = o
				};
			}
		}
		if (genericTerminal != null)
		{
			for (int o = 0; o < genericTerminal.currentScreen.Options.Count; o++)
			{
				yield return new CyberneticsTerminalLineData
				{
					Text = genericTerminal.currentScreen.Options[o],
					screen = this,
					OptionID = o
				};
			}
		}
	}

	public override void Show()
	{
		base.Show();
		footerTextSkin.SetText(FooterText);
		displayScroller.scrollContext.wraps = false;
		ControlManager.ResetInput(disableLayers: false, LeaveMovementEvents: false);
		List<FrameworkDataElement> list = GetMenuItems().ToList();
		for (int i = 0; i < list.Count - 1; i++)
		{
			((CyberneticsTerminalLineData)list[i]).nextCursorData = (CyberneticsTerminalLineData)list[i + 1];
		}
		displayScroller.BeforeShow(null, list);
		if (list.Count > 0)
		{
			((CyberneticsTerminalLineData)list[0]).row.currentCursor = true;
		}
		displayScroller.scrollContext.selectedPosition = 1;
		displayScroller.onSelected.RemoveAllListeners();
		displayScroller.onSelected.AddListener(HandleSelect);
		displayScroller.onHighlight.RemoveAllListeners();
		UpdateMenuBars();
		SetupContext();
		EnableNavContext();
		displayScroller.scrollContext.ActivateAndEnable();
	}

	public async void HandleSelect(FrameworkDataElement element)
	{
		if (!(element is CyberneticsTerminalLineData { CursorDone: not false } cat))
		{
			return;
		}
		if (cyberneticsTerminal != null)
		{
			cyberneticsTerminal.Selected = cat.OptionID;
			if (cyberneticsTerminal.Selected > -1)
			{
				await APIDispatch.RunAndWaitAsync(() =>
				{
					cyberneticsTerminal.CurrentScreen.Activate();
				});
				if (cyberneticsTerminal.CurrentScreen == null)
				{
					completionSource?.TrySetResult(result: false);
				}
				else
				{
					FooterText = "";
					initReady = false;
					await APIDispatch.RunAndWaitAsync(() =>
					{
						cyberneticsTerminal.CurrentScreen.BeforeRender(null, ref FooterText);
					});
					Show();
					initReady = true;
				}
			}
		}
		if (genericTerminal == null)
		{
			return;
		}
		genericTerminal.nSelected = cat.OptionID;
		if (genericTerminal.nSelected <= -1)
		{
			return;
		}
		await APIDispatch.RunAndWaitAsync(() =>
		{
			genericTerminal.currentScreen.Activate();
		});
		if (genericTerminal.currentScreen == null)
		{
			completionSource?.TrySetResult(result: false);
			return;
		}
		FooterText = "";
		initReady = false;
		await APIDispatch.RunAndWaitAsync(() =>
		{
			genericTerminal.currentScreen.BeforeRender(null, ref FooterText);
		});
		Show();
		initReady = true;
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
		displayScroller.GetNavigationContext().ActivateAndEnable();
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
