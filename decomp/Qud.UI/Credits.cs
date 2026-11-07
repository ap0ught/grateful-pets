using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qud.API;
using UnityEngine.UI;
using XRL.CharacterBuilds.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[UIView("Credits", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "Credits", UICanvasHost = 1)]
public class Credits : SingletonWindowBase<Credits>
{
	protected List<SaveInfoData> saves;

	public Image background;

	public FrameworkScroller hotkeyBar;

	public FrameworkScroller creditsScroller;

	public EmbarkBuilderModuleBackButton backButton;

	public NavigationContext globalContext = new NavigationContext();

	public ScrollContext<NavigationContext> midHorizNav = new ScrollContext<NavigationContext>();

	private bool SelectFirst = true;

	public bool wasInScroller;

	public void SetupContext()
	{
		globalContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
		globalContext.buttonHandlers.Set(InputButtonTypes.CancelButton, Event.Helpers.Handle(Exit));
		midHorizNav.SetAxis(InputAxisTypes.NavigationXAxis);
		midHorizNav.contexts.Clear();
		midHorizNav.contexts.Add(backButton.navigationContext);
		midHorizNav.contexts.Add(creditsScroller.GetNavigationContext());
		midHorizNav.Setup();
		midHorizNav.parentContext = globalContext;
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
		backButton.navigationContext.buttonHandlers.Set(InputButtonTypes.AcceptButton, Event.Helpers.Handle(Exit));
		Task<List<SaveGameInfo>> savedGameInfo = SavesAPI.GetSavedGameInfo();
		Task.WaitAll(savedGameInfo);
		saves = savedGameInfo.Result.Select((SaveGameInfo info) => new SaveInfoData
		{
			SaveGame = info
		}).ToList();
		creditsScroller.scrollContext.wraps = true;
		creditsScroller.BeforeShow(null, saves);
		foreach (SaveManagementRow item in creditsScroller.selectionClones.Select((FrameworkUnityScrollChild s) => s.GetComponent<SaveManagementRow>()))
		{
			_ = item != null;
		}
		if (SelectFirst)
		{
			SelectFirst = false;
			creditsScroller.scrollContext.selectedPosition = 0;
		}
		else if (creditsScroller.scrollContext.selectedPosition >= saves.Count)
		{
			creditsScroller.scrollContext.selectedPosition = Math.Max(saves.Count - 1, 0);
		}
		creditsScroller.onSelected.RemoveAllListeners();
		SetupContext();
		EnableNavContext();
		UpdateMenuBars();
	}

	public override void Hide()
	{
		base.Hide();
		DisableNavContext();
		gameObject.SetActive(value: false);
	}

	public void EnableNavContext()
	{
		globalContext.disabled = false;
		creditsScroller.GetNavigationContext().ActivateAndEnable();
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

	public void Exit()
	{
		UIManager.showWindow("MainMenu");
		Hide();
	}

	public void UpdateMenuBars()
	{
		List<MenuOption> list = new List<MenuOption>();
		list.Add(new MenuOption
		{
			InputCommand = "NavigationXYAxis",
			Description = "navigate"
		});
		list.Add(new MenuOption
		{
			KeyDescription = "space",
			Description = "select"
		});
		hotkeyBar.GetNavigationContext().disabled = true;
		hotkeyBar.BeforeShow(null, list);
	}

	public void Update()
	{
		if (globalContext.IsActive() && NavigationController.instance?.activeContext?.IsInside(creditsScroller?.GetNavigationContext()) == true != wasInScroller)
		{
			UpdateMenuBars();
		}
	}
}
