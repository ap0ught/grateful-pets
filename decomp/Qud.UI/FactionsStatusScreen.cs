using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using FuzzySharp;
using FuzzySharp.Extractor;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

[UIView("FactionsStatusScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menus", UICanvas = "StatusScreens", UICanvasHost = 1)]
public class FactionsStatusScreen : BaseStatusScreen<FactionsStatusScreen>
{
	public class Context : ProxyNavigationContext
	{
		public FactionsStatusScreen screen;
	}

	public FrameworkScroller controller;

	private Renderable icon;

	public HashSet<string> CollapsedFactions = new HashSet<string>();

	public Context context = new Context();

	public IEnumerable<FactionsLineData> dataList;

	public MenuOption EXPAND_ALL = new MenuOption
	{
		Id = "V Positive",
		InputCommand = "V Positive",
		Description = "Expand All"
	};

	public MenuOption COLLAPSE_ALL = new MenuOption
	{
		Id = "V Negative",
		InputCommand = "V Negative",
		Description = "Collapse All"
	};

	protected StatusScreensScreen parent;

	private FactionsLineData searcher = new FactionsLineData();

	private List<FactionsLineData> sortedData = new List<FactionsLineData>();

	private List<FactionsLineData> rawData = new List<FactionsLineData>();

	private StatusScreensScreen statusScreensScreen;

	private FilterBar filterBar;

	public int SortMode = 2;

	public override IRenderable GetTabIcon()
	{
		if (icon == null)
		{
			icon = new Renderable("Items/sw_unfurled_scroll1.bmp", " ", "&Y", null, 'w');
		}
		return icon;
	}

	public override string GetTabString()
	{
		_ = Media.sizeClass;
		_ = 100;
		return "Reputation";
	}

	public void ToggleExpanded(FactionsLineData data)
	{
		string id = data.id;
		if (CollapsedFactions.Contains(id))
		{
			CollapsedFactions.Remove(id);
		}
		else
		{
			CollapsedFactions.Add(id);
		}
		data.expanded = !CollapsedFactions.Contains(id);
		UpdateViewFromData();
		parent.updateMenuBar = true;
	}

	public void ExpandAll()
	{
		foreach (FactionsLineData data in dataList)
		{
			CollapsedFactions.Remove(data.id);
			data.expanded = true;
		}
		FrameworkScroller frameworkScroller = controller;
		IEnumerable<FactionsLineData> enumerable = sortedData;
		frameworkScroller.BeforeShow(null, enumerable ?? dataList);
		parent.updateMenuBar = true;
	}

	public void CollapseAll()
	{
		foreach (FactionsLineData data in dataList)
		{
			CollapsedFactions.Add(data.id);
			data.expanded = false;
		}
		FrameworkScroller frameworkScroller = controller;
		IEnumerable<FactionsLineData> enumerable = sortedData;
		frameworkScroller.BeforeShow(null, enumerable ?? dataList);
		parent.updateMenuBar = true;
	}

	public override void HandleMenuOption(FrameworkDataElement data)
	{
		if (data == EXPAND_ALL)
		{
			ExpandAll();
		}
		else if (data == COLLAPSE_ALL)
		{
			CollapseAll();
		}
		else
		{
			base.HandleMenuOption(data);
		}
	}

	public void OnSearchTextChange(string text)
	{
		filterText = text;
		if (text.IsNullOrEmpty())
		{
			searcher.searchText = text;
		}
		else
		{
			searcher.searchText = text.ToLower();
		}
		UpdateViewFromData();
	}

	public void ExpansionUpdated()
	{
		controller.BeforeShow(null, dataList);
	}

	public override void UpdateViewFromData()
	{
		rawData.ForEach((FactionsLineData e) =>
		{
			e.free();
		});
		rawData.Clear();
		rawData.AddRange(from faction in FactionsScreen.getFactionsByName().Select(Factions.Get)
			where faction.Visible
			select PooledFrameworkDataElement<FactionsLineData>.next().set(faction.Name, ColorUtility.CapitalizeExceptFormatting(faction.GetFormattedName()), faction.Emblem, !CollapsedFactions.Contains(faction.Name)));
		if (filterText.IsNullOrEmpty())
		{
			dataList = rawData;
		}
		else
		{
			dataList = from i in Process.ExtractTop(searcher, rawData, (FactionsLineData i) => i.searchText, null, Enumerable.Count(rawData), 50)
				select i.Value;
		}
		sortedData.Clear();
		sortedData.AddRange(dataList);
		if (SortMode == 0)
		{
			sortedData.Sort((FactionsLineData a, FactionsLineData b) => b.rep.CompareTo(a.rep));
		}
		else if (SortMode == 1)
		{
			sortedData.Sort((FactionsLineData a, FactionsLineData b) => a.rep.CompareTo(b.rep));
		}
		else
		{
			sortedData.Sort((FactionsLineData a, FactionsLineData b) => a.name.CompareTo(b.name));
		}
		controller.BeforeShow(sortedData);
	}

	public async void HandleCmdOptions()
	{
		int num = await Popup.PickOptionAsync("Sort By", null, "", new string[3]
		{
			(SortMode == 0) ? "{{W|Highest reputation}}" : "{{y|Highest reputation}}",
			(SortMode == 1) ? "{{W|Lowest reputation}}" : "{{y|Lowest reputation}}",
			(SortMode == 2) ? "{{W|Alphabetical}}" : "{{y|Alphabetical}}"
		}, null, null, null, null, 0, 60, SortMode, -1, RespectOptionNewlines: false, AllowEscape: true);
		if (num >= 0)
		{
			SortMode = num;
		}
		UpdateViewFromData();
	}

	public override void FilterUpdated(string filterText)
	{
		OnSearchTextChange(filterText);
	}

	public override NavigationContext ShowScreen(GameObject GO, StatusScreensScreen parent)
	{
		statusScreensScreen = parent;
		filterBar = parent.filterBar;
		this.parent = parent;
		this.context.screen = this;
		controller.onSelected.RemoveAllListeners();
		controller.onHighlight.RemoveAllListeners();
		controller.scrollContext.wraps = false;
		this.context.proxyTo = controller.scrollContext;
		controller.scrollContext.parentContext = this.context;
		Context context = this.context;
		if (context.axisHandlers == null)
		{
			context.axisHandlers = new Dictionary<InputAxisTypes, Action>();
		}
		this.context.axisHandlers[InputAxisTypes.NavigationVAxis] = XRL.UI.Framework.Event.Helpers.Handle(XRL.UI.Framework.Event.Helpers.Axis(ExpandAll, CollapseAll));
		context = this.context;
		if (context.menuOptionDescriptions == null)
		{
			Context context2 = context;
			List<MenuOption> list = new List<MenuOption>
			{
				EXPAND_ALL,
				COLLAPSE_ALL,
				new MenuOption
				{
					Id = "CmdOptions",
					InputCommand = "CmdOptions",
					Description = "Sort Options"
				},
				new MenuOption
				{
					Id = "CmdFilter",
					InputCommand = "CmdFilter",
					Description = "Filter"
				}
			};
			List<MenuOption> list2 = list;
			context2.menuOptionDescriptions = list;
		}
		context = this.context;
		if (context.commandHandlers == null)
		{
			context.commandHandlers = new Dictionary<string, Action>();
		}
		this.context.commandHandlers["CmdOptions"] = () =>
		{
			HandleCmdOptions();
		};
		FilterUpdated(null);
		controller.selectedPosition = 0;
		controller.scrollContext.ActivateAndEnable();
		base.ShowScreen(GO, parent);
		return this.context;
	}

	public override void Prewarm()
	{
		List<FactionsLineData> list = new List<FactionsLineData>();
		for (int i = 0; i < 40; i++)
		{
			list.Add(new FactionsLineData
			{
				id = "Bears",
				label = "label",
				expanded = true,
				name = "Bears",
				rep = 600
			});
		}
		controller.BeforeShow(list);
	}
}
