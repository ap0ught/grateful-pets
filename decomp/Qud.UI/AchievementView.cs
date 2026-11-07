using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XRL;
using XRL.CharacterBuilds.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[UIView("AchievementView", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "AchievementView", UICanvasHost = 1)]
public class AchievementView : SingletonWindowBase<AchievementView>, ControlManager.IControllerChangedEvent
{
	protected List<FrameworkDataElement> Data;

	public FrameworkScroller HotkeyBar;

	public FrameworkScroller SavesScroller;

	public EmbarkBuilderModuleBackButton Back;

	public NavigationContext GlobalNavContext = new NavigationContext();

	public ScrollContext<NavigationContext> HorizontalNavigation = new ScrollContext<NavigationContext>();

	private bool SelectFirst = true;

	private TaskCompletionSource<bool> MenuClosed = new TaskCompletionSource<bool>();

	public bool wasInScroller;

	public void SetupContext()
	{
		GlobalNavContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
		GlobalNavContext.buttonHandlers.Set(InputButtonTypes.CancelButton, Event.Helpers.Handle(Exit));
		HorizontalNavigation.SetAxis(InputAxisTypes.NavigationXAxis);
		HorizontalNavigation.contexts.Clear();
		HorizontalNavigation.contexts.Add(Back.navigationContext);
		HorizontalNavigation.contexts.Add(SavesScroller.GetNavigationContext());
		HorizontalNavigation.Setup();
		HorizontalNavigation.parentContext = GlobalNavContext;
	}

	public override void Show()
	{
		base.Show();
		Back?.gameObject.SetActive(value: true);
		if (Back.navigationContext == null)
		{
			Back.Awake();
		}
		Back.navigationContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
		Back.navigationContext.buttonHandlers.Set(InputButtonTypes.AcceptButton, Event.Helpers.Handle(Exit));
		Dictionary<string, AchievementInfo> achievements = AchievementManager.State.Achievements;
		if (Data == null)
		{
			Data = new List<FrameworkDataElement>(achievements.Count);
		}
		Data.Clear();
		int num = 0;
		foreach (AchievementInfo item in from x in achievements.Values
			orderby x.Achieved descending, x.TimeStamp descending
			select x)
		{
			if (item.Hidden && !item.Achieved)
			{
				num++;
			}
			else
			{
				Data.Add(new AchievementInfoData(item));
			}
		}
		if (num > 0)
		{
			Data.Add(new HiddenAchievementData(num));
		}
		SavesScroller.scrollContext.wraps = true;
		SavesScroller.BeforeShow(null, Data);
		if (SelectFirst)
		{
			SelectFirst = false;
			SavesScroller.scrollContext.selectedPosition = 0;
		}
		else if (SavesScroller.scrollContext.selectedPosition >= Data.Count)
		{
			SavesScroller.scrollContext.selectedPosition = Math.Max(Data.Count - 1, 0);
		}
		SetupContext();
		EnableNavContext();
		UpdateMenuBars();
	}

	public async Task<bool> ShowMenuAsync()
	{
		MenuClosed.TrySetCanceled();
		MenuClosed = new TaskCompletionSource<bool>();
		await The.UiContext;
		UIManager.showWindow("AchievementView");
		return await MenuClosed.Task;
	}

	public override void Hide()
	{
		base.Hide();
		DisableNavContext();
		gameObject.SetActive(value: false);
	}

	public void EnableNavContext()
	{
		GlobalNavContext.disabled = false;
		SavesScroller.GetNavigationContext().ActivateAndEnable();
	}

	public void DisableNavContext(bool deactivate = true)
	{
		if (deactivate)
		{
			NavigationContext activeContext = NavigationController.instance.activeContext;
			if (activeContext != null && activeContext.IsInside(GlobalNavContext))
			{
				NavigationController.instance.activeContext = null;
			}
		}
		GlobalNavContext.disabled = true;
	}

	public void Exit()
	{
		MenuClosed.TrySetResult(result: true);
		Hide();
	}

	public void UpdateMenuBars()
	{
		if (GlobalNavContext.IsActive())
		{
			List<MenuOption> list = new List<MenuOption>();
			list.Add(new MenuOption
			{
				InputCommand = "NavigationXYAxis",
				Description = "navigate"
			});
			HotkeyBar.GetNavigationContext().disabled = true;
			HotkeyBar.BeforeShow(null, list);
		}
	}

	public void Update()
	{
		if ((NavigationController.instance.activeContext?.IsInside(SavesScroller.GetNavigationContext()) ?? false) != wasInScroller)
		{
			UpdateMenuBars();
		}
	}

	public void ControllerChanged()
	{
		UpdateMenuBars();
	}
}
