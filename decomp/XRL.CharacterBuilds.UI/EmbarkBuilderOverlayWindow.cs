using System;
using System.Collections.Generic;
using Qud.UI;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.UI;

[UIView("EmbarkBuilder:Overlay", false, false, false, null, null, false, 0, false, NavCategory = "Chargen", UICanvas = "Chargen/Overlay", UICanvasHost = 1)]
public class EmbarkBuilderOverlayWindow : WindowBase, ControlManager.IControllerChangedEvent
{
	public EmbarkBuilderModuleBackButton backButton;

	public EmbarkBuilderModuleBackButton nextButton;

	public HorizontalIconScroller breadcrumbs;

	public NavigationContext globalContext = new NavigationContext();

	public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

	public ScrollContext<NavigationContext> midHorizNav = new ScrollContext<NavigationContext>();

	public EmbarkBuilder builder;

	public HorizontalMenuScroller menuBar;

	public HorizontalMenuScroller legendBar;

	public RectTransform safeArea;

	public int navButtonBreakpoint = 900;

	public int keyDescriptionsBreakpoint = 640;

	public EmbarkBuilderModuleWindowDescriptor currentWindowDescriptor;

	public GameObject nextButtonLg;

	public GameObject nextButtonSm;

	public GameObject backButtonSm;

	public GameObject backButtonLg;

	public bool hasCrumbs;

	public static MenuOption BackMenuOption = new MenuOption
	{
		Id = "Back",
		InputCommand = "Cancel",
		KeyDescription = "Esc",
		Description = "Back"
	};

	public static MenuOption NextMenuOption = new MenuOption
	{
		Id = "Next",
		InputCommand = "Page Right",
		KeyDescription = "End",
		Description = "Next"
	};

	public float _lastWidth;

	public float _lastHeight;

	public bool breakKeyDescriptions => _lastHeight <= (float)keyDescriptionsBreakpoint;

	public bool breakNavButtons => rectTransform.rect.width < (float)navButtonBreakpoint;

	public virtual void Back()
	{
		builder.back();
	}

	public virtual void Next()
	{
		builder.advance(force: false, editableOnly: true);
	}

	public void BeforeShowWithWindow(EmbarkBuilder builder, EmbarkBuilderModuleWindowDescriptor windowDescriptor)
	{
		this.builder = builder;
		currentWindowDescriptor = windowDescriptor;
		globalContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
		globalContext.buttonHandlers.Set(InputButtonTypes.CancelButton, XRL.UI.Framework.Event.Helpers.Handle(Back));
		globalContext.axisHandlers = new Dictionary<InputAxisTypes, Action>();
		globalContext.axisHandlers.Set(InputAxisTypes.NavigationPageXAxis, XRL.UI.Framework.Event.Helpers.Handle(XRL.UI.Framework.Event.Helpers.Axis(negative: Back, positive: Next)));
		if (Media.sizeClass < Media.SizeClass.Medium)
		{
			backButtonLg?.gameObject.SetActive(value: false);
			backButtonSm?.gameObject.SetActive(value: true);
			backButton = backButtonSm.GetComponent<EmbarkBuilderModuleBackButton>();
			if (backButton.navigationContext == null)
			{
				backButton.Awake();
			}
			backButton.navigationContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
			backButton.navigationContext.buttonHandlers.Set(InputButtonTypes.AcceptButton, XRL.UI.Framework.Event.Helpers.Handle(Back));
		}
		else
		{
			backButtonLg?.gameObject.SetActive(value: true);
			backButtonSm?.gameObject.SetActive(value: false);
			backButton = backButtonLg.GetComponent<EmbarkBuilderModuleBackButton>();
			if (backButton.navigationContext == null)
			{
				backButton.Awake();
			}
			backButton.navigationContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
			backButton.navigationContext.buttonHandlers.Set(InputButtonTypes.AcceptButton, XRL.UI.Framework.Event.Helpers.Handle(Back));
		}
		if (Media.sizeClass < Media.SizeClass.Medium)
		{
			nextButtonLg?.gameObject.SetActive(value: false);
			nextButtonSm?.gameObject.SetActive(value: true);
			nextButton = nextButtonSm.GetComponent<EmbarkBuilderModuleBackButton>();
			if (nextButton.navigationContext == null)
			{
				nextButton.Awake();
			}
			nextButton.navigationContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
			nextButton.navigationContext.buttonHandlers.Set(InputButtonTypes.AcceptButton, XRL.UI.Framework.Event.Helpers.Handle(Next));
		}
		else
		{
			nextButtonLg?.gameObject.SetActive(value: true);
			nextButtonSm?.gameObject.SetActive(value: false);
			nextButton = nextButtonLg.GetComponent<EmbarkBuilderModuleBackButton>();
			if (nextButton.navigationContext == null)
			{
				nextButton.Awake();
			}
			nextButton.navigationContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
			nextButton.navigationContext.buttonHandlers.Set(InputButtonTypes.AcceptButton, XRL.UI.Framework.Event.Helpers.Handle(Next));
		}
		vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		midHorizNav.SetAxis(InputAxisTypes.NavigationXAxis);
		UpdateBreadcrumbs();
		UpdateMenuBars(windowDescriptor);
		transform.SetAsLastSibling();
		WireNavContexts();
		UpdateSafeArea();
	}

	public void UpdateBreadcrumbs()
	{
		if (builder == null || builder.SkippingUIUpdates)
		{
			return;
		}
		List<ChoiceWithColorIcon> list = new List<ChoiceWithColorIcon>(builder.GetBreadcrumbs());
		hasCrumbs = list.Count > 0;
		if (hasCrumbs)
		{
			breadcrumbs.scrollContext.parentContext = vertNav;
			breadcrumbs.BeforeShow(null, list);
			for (int i = 0; i < breadcrumbs.scrollContext.length; i++)
			{
				breadcrumbs.GetPrefabForIndex(i).GetComponent<TitledIconButton>().TitleText.gameObject.SetActive(value: true);
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
			Canvas.ForceUpdateCanvases();
			bool flag = false;
			int num = 0;
			while (!flag && num < breadcrumbs.scrollContext.length)
			{
				RectTransform component = breadcrumbs.GetPrefabForIndex(num).GetComponent<TitledIconButton>().TitleText.GetComponent<RectTransform>();
				float height = component.rect.height;
				float minHeight = LayoutUtility.GetMinHeight(component);
				if (height > minHeight)
				{
					flag = true;
				}
				num++;
			}
			for (int j = 0; j < breadcrumbs.scrollContext.length; j++)
			{
				breadcrumbs.GetPrefabForIndex(j).GetComponent<TitledIconButton>().TitleText.gameObject.SetActive(!flag);
			}
			breadcrumbs.onSelected.RemoveAllListeners();
			breadcrumbs.onSelected.AddListener(builder.ShowFromCrumb);
			if (!breadcrumbs.gameObject.activeInHierarchy)
			{
				breadcrumbs.gameObject.SetActive(value: true);
			}
		}
		else
		{
			breadcrumbs.gameObject.SetActive(value: false);
		}
	}

	public void UpdateSafeArea()
	{
		_ = safeArea == null;
	}

	public void WireNavContexts()
	{
		vertNav.contexts.Clear();
		midHorizNav.contexts.Clear();
		if (!breakNavButtons)
		{
			midHorizNav.contexts.Add(backButton.navigationContext);
		}
		if (hasCrumbs)
		{
			vertNav.contexts.Add(breadcrumbs.scrollContext);
		}
		NavigationContext navigationContext = currentWindowDescriptor?.getWindow()?.GetNavigationContext();
		if (navigationContext != null)
		{
			if (breakNavButtons)
			{
				vertNav.contexts.Add(navigationContext);
			}
			else
			{
				midHorizNav.contexts.Add(navigationContext);
			}
		}
		if (midHorizNav.contexts.Count > 0 && !breakNavButtons)
		{
			vertNav.contexts.Add(midHorizNav);
		}
		midHorizNav.contexts.Add(nextButton.navigationContext);
		vertNav.contexts.Add(menuBar.scrollContext);
		vertNav.Setup();
		vertNav.parentContext = globalContext;
	}

	public IEnumerable<MenuOption> GetKeyMenuBar()
	{
		if (breakNavButtons)
		{
			yield return BackMenuOption;
			yield return NextMenuOption;
		}
	}

	public void UpdateMenuBars()
	{
		if ((!(builder != null) || !builder.SkippingUIUpdates) && currentWindowDescriptor?.getWindow()?.isActiveAndEnabled == true)
		{
			UpdateMenuBars(currentWindowDescriptor);
		}
	}

	public void UpdateMenuBars(EmbarkBuilderModuleWindowDescriptor windowDescriptor)
	{
		if (builder != null && builder.SkippingUIUpdates)
		{
			return;
		}
		AbstractBuilderModuleWindowBase abstractBuilderModuleWindowBase = windowDescriptor?.getWindow();
		if (!(abstractBuilderModuleWindowBase != null))
		{
			return;
		}
		List<MenuOption> list = new List<MenuOption>(GetKeyMenuBar());
		list.AddRange(abstractBuilderModuleWindowBase.GetKeyMenuBar());
		globalContext.commandHandlers = new Dictionary<string, Action>();
		list.ForEach((MenuOption option) =>
		{
			if (!string.IsNullOrEmpty(option.InputCommand))
			{
				globalContext.commandHandlers.Add(option.InputCommand, () =>
				{
					HandleMenuOption(option);
				});
			}
		});
		menuBar?.BeforeShow(windowDescriptor, list);
		menuBar.onSelected.RemoveAllListeners();
		menuBar.onSelected.AddListener(HandleMenuOption);
		legendBar.gameObject.SetActive(!breakKeyDescriptions);
		if (!breakKeyDescriptions)
		{
			legendBar.BeforeShow(windowDescriptor, abstractBuilderModuleWindowBase.GetKeyLegend());
			legendBar.scrollContext.disabled = true;
		}
		backButton.menuOption = BackMenuOption;
		backButton.ForceUpdate();
		nextButton.menuOption = NextMenuOption;
		nextButton.navigationContext.disabled = abstractBuilderModuleWindowBase.DataErrors() != null;
		nextButton.ForceUpdate();
	}

	public void HandleMenuOption(FrameworkDataElement option)
	{
		if (option.Id == "Back")
		{
			Back();
		}
		else if (option.Id == "Next")
		{
			Next();
		}
		else
		{
			currentWindowDescriptor?.getWindow()?.HandleMenuOption(option);
		}
	}

	public void Update()
	{
		if (!canvas.enabled)
		{
			gameObject.SetActive(value: false);
		}
		if (rectTransform.rect.width != _lastWidth || rectTransform.rect.height != _lastHeight)
		{
			_lastWidth = rectTransform.rect.width;
			_lastHeight = rectTransform.rect.height;
			UpdateBreadcrumbs();
			UpdateMenuBars(currentWindowDescriptor);
			WireNavContexts();
			backButton.gameObject.SetActive(!breakNavButtons);
			nextButton.gameObject.SetActive(!breakNavButtons);
			UpdateSafeArea();
		}
	}

	public void ControllerChanged()
	{
		UpdateMenuBars();
	}
}
