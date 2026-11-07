using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using FuzzySharp;
using FuzzySharp.Extractor;
using XRL;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

[UIView("MessageLogStatusScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menus", UICanvas = "StatusScreens", UICanvasHost = 1)]
public class MessageLogStatusScreen : BaseStatusScreen<MessageLogStatusScreen>
{
	public class CategoryInfo
	{
		public int N;

		public bool Selected;

		public string Name;
	}

	public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

	public ButtonBar categoryBar;

	public FrameworkScroller controller;

	private Renderable icon;

	public int CurrentCategory;

	public int MaxCategory = 6;

	public List<CategoryInfo> categoryInfos = new List<CategoryInfo>
	{
		new CategoryInfo
		{
			N = 0,
			Name = JournalScreen.STR_OBSERVATIONS
		},
		new CategoryInfo
		{
			N = 1,
			Name = JournalScreen.STR_LOCATIONS
		},
		new CategoryInfo
		{
			N = 2,
			Name = JournalScreen.STR_SULTANS
		},
		new CategoryInfo
		{
			N = 3,
			Name = JournalScreen.STR_VILLAGES
		},
		new CategoryInfo
		{
			N = 4,
			Name = JournalScreen.STR_CHRONOLOGY
		},
		new CategoryInfo
		{
			N = 5,
			Name = JournalScreen.STR_GENERAL
		},
		new CategoryInfo
		{
			N = 6,
			Name = JournalScreen.STR_RECIPES
		}
	};

	public string SelectedSubCategory;

	private MessageLogLineData searcher = new MessageLogLineData();

	private List<MessageLogLineData> logLines = new List<MessageLogLineData>();

	private FilterBar filterBar;

	public override IRenderable GetTabIcon()
	{
		if (icon == null)
		{
			icon = new Renderable("Items/sw_crayons.bmp", " ", "&w", null, 'R');
		}
		return icon;
	}

	public override string GetTabString()
	{
		if (Media.sizeClass < Media.SizeClass.Medium)
		{
			return "Log";
		}
		return "Message Log";
	}

	public void HandleNavigationXAxis()
	{
		NavigationController.currentEvent.handled = true;
		MetricsManager.LogInfo($"NAVCATEGORY {NavigationController.currentEvent.axisValue}");
		CurrentCategory += ((NavigationController.currentEvent.axisValue > 0) ? 1 : (-1));
		if (CurrentCategory < 0)
		{
			CurrentCategory = MaxCategory;
		}
		if (CurrentCategory > MaxCategory)
		{
			CurrentCategory = 0;
		}
		controller.BeforeShow(The.Game.Player.Messages.Messages.Select((string t) => new MessageLogLineData().set(t)));
	}

	public void updateCategoryButtonBar()
	{
		categoryBar.SetButtons(categoryInfos.Select((CategoryInfo s) => new ButtonBar.ButtonBarButtonData
		{
			label = s.Name,
			Highlighted = ((s.N == CurrentCategory) ? ButtonBar.ButtonBarButtonData.HighlightState.Highlighted : ButtonBar.ButtonBarButtonData.HighlightState.NotHighlighted)
		}));
	}

	public override void FilterUpdated(string filterText)
	{
		OnSearchTextChange(filterText);
	}

	public void OnSearchTextChange(string text)
	{
		filterText = text;
		if (string.IsNullOrWhiteSpace(text))
		{
			searcher.sortText = text;
		}
		else
		{
			searcher.sortText = text.ToLower();
		}
		UpdateViewFromData();
	}

	public override void UpdateViewFromData()
	{
		XRL.World.Event.ResetToPin();
		logLines.ForEach((MessageLogLineData l) =>
		{
			l.free();
		});
		logLines.Clear();
		foreach (string message in The.Game.Player.Messages.Messages)
		{
			logLines.AddRange(from t in message.Split('\n')
				select PooledFrameworkDataElement<MessageLogLineData>.next().set(t));
		}
		IEnumerable<MessageLogLineData> selections = ((!string.IsNullOrEmpty(searcher.sortText)) ? (from m in Process.ExtractTop(searcher, logLines, (MessageLogLineData i) => i.sortText, null, Enumerable.Count(logLines), 50)
			select m.Value) : logLines);
		controller.BeforeShow(selections);
	}

	public override NavigationContext ShowScreen(GameObject GO, StatusScreensScreen parent)
	{
		filterBar = parent.filterBar;
		vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		vertNav.contexts.Add(controller.scrollContext);
		vertNav.wraps = false;
		vertNav.Setup();
		controller.onSelected.RemoveAllListeners();
		controller.onHighlight.RemoveAllListeners();
		controller.scrollContext.wraps = false;
		base.ShowScreen(GO, parent);
		UpdateViewFromData();
		controller.scrollContext.selectedPosition = controller.choices.Count - 1;
		controller.scrollContext.ActivateAndEnable();
		return vertNav;
	}
}
