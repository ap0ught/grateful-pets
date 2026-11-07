using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using FuzzySharp;
using FuzzySharp.Extractor;
using Genkit;
using HistoryKit;
using Kobold;
using Qud.API;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Parts;

namespace Qud.UI;

[HasGameBasedStaticCache]
[UIView("JournalStatusScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menus", UICanvas = "StatusScreens", UICanvasHost = 1)]
public class JournalStatusScreen : BaseStatusScreen<JournalStatusScreen>
{
	public class CategoryInfo
	{
		public int N;

		public bool Selected;

		public string Name;

		public bool UsesCategories;

		public bool UsesMarginalia;

		public bool UsesMap;

		public bool SortCategoriesAZ;

		public bool CanAdd;

		public bool CanDelete;

		public bool UsesSultans;

		public Func<JournalLineData, string> CategoryFor;
	}

	public UnityEngine.GameObject sultanScroller;

	public List<JournalSultanStatueLine> sultanStatues = new List<JournalSultanStatueLine>();

	public List<int> sultanStatuePeriod = new List<int>();

	public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

	public FilterBar categoryBar;

	public FrameworkScroller controller;

	private Renderable icon;

	public int CurrentCategory;

	public int MaxCategory = 6;

	private List<string> usedCategories = new List<string>(64);

	private List<JournalLineData> lineData = new List<JournalLineData>();

	private List<JournalLineData> rawLineData = new List<JournalLineData>();

	private Dictionary<string, List<JournalLineData>> categories = new Dictionary<string, List<JournalLineData>>();

	[GameBasedStaticCache(true, false)]
	public static int? LastCategory = null;

	public Dictionary<string, bool> categoryCollapsed = new Dictionary<string, bool>();

	private IEnumerable<JournalLineData> searchResult;

	public UnityEngine.GameObject sultanShrineContentRoot;

	public UnityEngine.GameObject sultanShrineLinePrefab;

	public MapScrollerController marginaliaController;

	public UnityEngine.GameObject marginaliaScroller;

	public MapScrollerController mapController;

	public UnityEngine.GameObject mapScroller;

	public static string NO_ENTRIES_TEXT = " No entries found.";

	public List<CategoryInfo> categoryInfos = new List<CategoryInfo>
	{
		new CategoryInfo
		{
			N = 0,
			Name = JournalScreen.STR_LOCATIONS,
			UsesCategories = true,
			UsesMap = true,
			CanAdd = true,
			CanDelete = true,
			CategoryFor = LocationCategory,
			SortCategoriesAZ = true
		},
		new CategoryInfo
		{
			N = 1,
			Name = JournalScreen.STR_OBSERVATIONS,
			UsesCategories = true,
			CategoryFor = ObservationCategory,
			SortCategoriesAZ = true
		},
		new CategoryInfo
		{
			N = 2,
			Name = JournalScreen.STR_SULTANS,
			UsesSultans = true,
			UsesCategories = true,
			CategoryFor = SultanCategory
		},
		new CategoryInfo
		{
			N = 3,
			Name = JournalScreen.STR_VILLAGES,
			UsesCategories = true,
			UsesMap = true,
			CategoryFor = VillageCategory,
			SortCategoriesAZ = true
		},
		new CategoryInfo
		{
			N = 4,
			Name = JournalScreen.STR_CHRONOLOGY,
			UsesMarginalia = true,
			CanAdd = true,
			CanDelete = true,
			UsesCategories = false,
			CategoryFor = AccomplishmentCategory
		},
		new CategoryInfo
		{
			N = 5,
			Name = JournalScreen.STR_GENERAL,
			CanAdd = true,
			CanDelete = true,
			SortCategoriesAZ = true
		},
		new CategoryInfo
		{
			N = 6,
			Name = JournalScreen.STR_RECIPES,
			CanDelete = true
		}
	};

	public UITextSkin categoryText;

	public UnityEngine.GameObject categoryContainer;

	private List<FilterBarCategoryButtonData> categoryBarButtons;

	private FrameworkDataElement currentMapHighlight;

	public HorizontalLayoutGroup statueLayoutGroup;

	public Media.SizeClass lastClass = Media.SizeClass.Unset;

	public MenuOption CMD_INSERT = new MenuOption
	{
		Id = "CmdInsert",
		InputCommand = "CmdInsert",
		Description = "Add"
	};

	public MenuOption CMD_DELETE = new MenuOption
	{
		Id = "CmdDelete",
		InputCommand = "CmdDelete",
		Description = "Delete"
	};

	private JournalLineData searcher = new JournalLineData();

	private FilterBar filterBar;

	public XRL.World.GameObject GO;

	public CategoryInfo currentInfo => categoryInfos[CurrentCategory];

	public override IRenderable GetTabIcon()
	{
		if (icon == null)
		{
			icon = new Renderable("Items/sw_book1.bmp", " ", "&w", null, 'W');
		}
		return icon;
	}

	public override string GetTabString()
	{
		return "Journal";
	}

	public override bool WantsCategoryBar()
	{
		return true;
	}

	public void HandleNavigationXAxis()
	{
	}

	public bool isCollapsed(string category, bool defaultValue)
	{
		if (!categoryCollapsed.TryGetValue(category, out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public void UpdateData()
	{
		if (CurrentCategory == LastCategory)
		{
			return;
		}
		foreach (JournalLineData rawLineDatum in rawLineData)
		{
			rawLineDatum.free();
		}
		rawLineData.Clear();
		rawLineData.AddRange(from n in JournalScreen.GetRawEntriesFor(categoryInfos[CurrentCategory].Name)
			select PooledFrameworkDataElement<JournalLineData>.next().set(n, this));
		if (filterText.IsNullOrEmpty())
		{
			searchResult = rawLineData;
		}
		else
		{
			searchResult = from m in Process.ExtractTop(searcher, rawLineData, (JournalLineData i) => i.searchText, null, Enumerable.Count(rawLineData), 50)
				select m.Value;
		}
		if (!currentInfo.UsesCategories)
		{
			return;
		}
		foreach (KeyValuePair<string, List<JournalLineData>> category in categories)
		{
			category.Value.Clear();
		}
		foreach (JournalLineData item in searchResult)
		{
			string key = currentInfo.CategoryFor(item);
			if (!categories.ContainsKey(key))
			{
				categories.Add(key, new List<JournalLineData>());
			}
			categories[key].Add(item);
		}
	}

	public void highlightSultanPeriod(int Period)
	{
		for (int i = 0; i < sultanStatuePeriod.Count; i++)
		{
			sultanStatues[i].SetHighlight(sultanStatuePeriod[i] == Period);
		}
	}

	public void UpdateStatues()
	{
		int num = 0;
		List<HistoricEntitySnapshot> list = (from s in HistoryAPI.GetKnownSultans()
			select s.GetCurrentSnapshot()).ToList();
		list.Sort((HistoricEntitySnapshot a, HistoricEntitySnapshot b) => a.entity.lastYear.CompareTo(b.entity.lastYear));
		sultanStatuePeriod.Clear();
		foreach (HistoricEntitySnapshot item in list)
		{
			int num2 = int.Parse(item.GetProperty("period", "0"));
			string property = item.GetProperty("name", "<unknown>");
			string path = item.GetProperty("statueTile", null) ?? SultanShrine.GetStatueForSultan(property, num2);
			sultanStatuePeriod.Add(num2);
			if (sultanStatues.Count <= num)
			{
				UnityEngine.GameObject gameObject = UnityEngine.Object.Instantiate(sultanShrineLinePrefab);
				sultanStatues.Add(gameObject.GetComponent<JournalSultanStatueLine>());
				gameObject.transform.SetParent(sultanShrineContentRoot.transform, worldPositionStays: false);
			}
			sultanStatues[num].gameObject.SetActive(value: true);
			sultanStatues[num].SetHighlight(highlightState: false);
			sultanStatues[num].SetBase(item.GetProperty("statueBase") == "true");
			sultanStatues[num].text.SetText(property);
			sultanStatues[num].fadedImage.sprite = SpriteManager.GetUnitySprite(path);
			sultanStatues[num].fullImage.sprite = SpriteManager.GetUnitySprite(path);
			num++;
		}
		for (; num < sultanStatues.Count; num++)
		{
			sultanStatues[num].gameObject.SetActive(value: false);
		}
	}

	public override void UpdateViewFromData()
	{
		marginaliaScroller.SetActive(currentInfo.UsesMarginalia);
		mapScroller.SetActive(currentInfo.UsesMap);
		if (currentInfo.UsesSultans)
		{
			UpdateStatues();
		}
		categoryText.SetText(JournalScreen.GetTabDisplayName(currentInfo.Name));
		categoryText.style = ((Media.sizeClass >= Media.SizeClass.Medium) ? UITextSkin.Size.header : UITextSkin.Size.normal);
		UpdateData();
		updateCategoryButtonBar();
		usedCategories.Clear();
		if (currentInfo.UsesCategories)
		{
			List<string> list = categories.Keys.ToList();
			if (currentInfo.SortCategoriesAZ)
			{
				list.Sort();
			}
			lineData.Clear();
			int num = 0;
			foreach (string item in list)
			{
				List<JournalLineData> list2 = categories[item];
				if (list2.Count > 0)
				{
					string text = item;
					usedCategories.Add(item);
					int sultanPeriod = 0;
					if (num < sultanStatuePeriod.Count)
					{
						sultanPeriod = sultanStatuePeriod[num];
					}
					if (currentInfo.Name == JournalScreen.STR_SULTANS)
					{
						text = "{{W|HISTORY OF " + text.ToUpper() + "}}";
					}
					JournalLineData journalLineData = PooledFrameworkDataElement<JournalLineData>.next().set(null, this, category: true, !isCollapsed(text, currentInfo.UsesSultans), text, null, sultanPeriod);
					lineData.Add(journalLineData);
					rawLineData.Add(journalLineData);
					JournalLineData journalLineData2 = list2.FirstOrDefault();
					if (journalLineData2 != null)
					{
						journalLineData.sultanPeriod = journalLineData2.sultanPeriod;
					}
					if (journalLineData2 != null)
					{
						journalLineData._mapTarget = journalLineData2.mapTarget;
					}
					if (!isCollapsed(text, currentInfo.UsesSultans))
					{
						int num2 = 0;
						foreach (JournalLineData item2 in list2)
						{
							item2.categoryName = item;
							item2.categoryOffset = num2 + 1;
							lineData.Add(item2);
							num2++;
						}
					}
				}
				num++;
			}
			if (lineData.Count == 0)
			{
				lineData.Add(PooledFrameworkDataElement<JournalLineData>.next().set(null, this, category: true, categoryExpanded: false, NO_ENTRIES_TEXT));
				if (sultanScroller.activeSelf)
				{
					ResetLayoutFrame();
				}
				sultanScroller.SetActive(value: false);
			}
			else
			{
				if (sultanScroller.activeSelf != currentInfo.UsesSultans)
				{
					ResetLayoutFrame();
				}
				sultanScroller.SetActive(currentInfo.UsesSultans);
			}
			controller.BeforeShow(lineData);
			lineData.Clear();
		}
		else if (searchResult.Count() == 0)
		{
			controller.BeforeShow(new List<FrameworkDataElement> { PooledFrameworkDataElement<JournalLineData>.next().set(null, this, category: true, categoryExpanded: false, NO_ENTRIES_TEXT) });
			if (sultanScroller.activeSelf)
			{
				ResetLayoutFrame();
			}
			sultanScroller.SetActive(value: false);
		}
		else
		{
			if (sultanScroller.activeSelf != currentInfo.UsesSultans)
			{
				ResetLayoutFrame();
			}
			sultanScroller.SetActive(currentInfo.UsesSultans);
			controller.BeforeShow(searchResult);
			HandleHighlightItem(searchResult.FirstOrDefault());
		}
	}

	public static string ObservationCategory(JournalLineData data)
	{
		return data?.entry?.LearnedFrom ?? "Unknown";
	}

	public static string LocationCategory(JournalLineData data)
	{
		if (data.entry is JournalMapNote journalMapNote)
		{
			return journalMapNote?.Category ?? "Unknown";
		}
		return "Unknown";
	}

	public static string SultanCategory(JournalLineData data)
	{
		if (data.entry is JournalSultanNote journalSultanNote)
		{
			return HistoryAPI.GetEntityName(journalSultanNote?.SultanID) ?? "Unknown";
		}
		return "Unknown";
	}

	public static string VillageCategory(JournalLineData data)
	{
		if (data.entry is JournalVillageNote journalVillageNote)
		{
			return HistoryAPI.GetEntityName(journalVillageNote?.VillageID) ?? "Unknown";
		}
		return "Unknown";
	}

	public static string AccomplishmentCategory(JournalLineData data)
	{
		if (data.entry is JournalAccomplishment journalAccomplishment)
		{
			return journalAccomplishment?.Category;
		}
		return "Unknown";
	}

	public override void FilterUpdated(string filterText)
	{
		int num = categoryBarButtons.IndexOf(categoryBarButtons.Where((FilterBarCategoryButtonData b) => b.category == filterBar.enabledCategories.FirstOrDefault()).FirstOrDefault());
		if (num >= 0)
		{
			CurrentCategory = num;
		}
		OnSearchTextChange(filterText);
	}

	public void updateCategoryButtonBar()
	{
		if (categoryBarButtons == null)
		{
			categoryBarButtons = new List<FilterBarCategoryButtonData>(categoryInfos.Count);
			categoryBarButtons.AddRange(categoryInfos.Select((CategoryInfo s) => new FilterBarCategoryButtonData
			{
				category = s.Name
			}));
			for (int num = 0; num < categoryBarButtons.Count; num++)
			{
				int n = num;
				categoryBarButtons[num].onSelect = (string c) =>
				{
					filterBar.CategorySelected(categoryBarButtons[n].category);
				};
			}
		}
		int num2 = 0;
		for (int count = categoryBarButtons.Count; num2 < count; num2++)
		{
			FilterBarCategoryButtonData filterBarCategoryButtonData = categoryBarButtons[num2];
			filterBarCategoryButtonData.tooltip = JournalScreen.GetTabDisplayName(filterBarCategoryButtonData.category);
		}
		categoryBar.SetCategoriesViaButtonBarData(categoryBarButtons);
	}

	public void HandleSelectItem(FrameworkDataElement element)
	{
		if (!(element is JournalLineData journalLineData))
		{
			return;
		}
		if (journalLineData.entry is JournalMapNote journalMapNote)
		{
			journalMapNote.Tracked = !journalMapNote.Tracked;
			UpdateViewFromData();
			ZoneActivatedEvent.Send(The.ZoneManager.GetZone("JoppaWorld"));
		}
		else if (journalLineData.category)
		{
			if (!categoryCollapsed.ContainsKey(journalLineData.categoryName))
			{
				categoryCollapsed.Add(journalLineData.categoryName, currentInfo.UsesSultans);
			}
			categoryCollapsed[journalLineData.categoryName] = !categoryCollapsed[journalLineData.categoryName];
			UpdateViewFromData();
		}
	}

	public void HandleHighlightItem(FrameworkDataElement element)
	{
		if (!(element is JournalLineData journalLineData))
		{
			return;
		}
		if (currentInfo.UsesSultans)
		{
			highlightSultanPeriod(journalLineData.sultanPeriod);
		}
		if (journalLineData.entry is JournalAccomplishment a)
		{
			marginaliaController.RenderAccomplishment(a);
			return;
		}
		if (journalLineData.mapTarget == null)
		{
			mapController.SetHighlights(null);
		}
		else
		{
			mapController.SetHighlights(new List<Location2D> { journalLineData.mapTarget });
		}
		if (currentInfo.UsesMap)
		{
			if (currentMapHighlight == element)
			{
				return;
			}
			currentMapHighlight = element;
			mapController.RefreshMap();
		}
		else
		{
			mapController.SetHighlights(null);
			mapController.RefreshMap();
		}
		if (currentInfo.UsesMap && journalLineData.mapTarget != null)
		{
			mapController.SetTarget(journalLineData.mapTarget.X, journalLineData.mapTarget.Y);
		}
	}

	public void Update()
	{
		if (lastClass != Media.sizeClass)
		{
			lastClass = Media.sizeClass;
			statueLayoutGroup.spacing = ((lastClass < Media.SizeClass.Medium) ? (-32) : 16);
		}
	}

	public async void HandleInsert()
	{
		Debug.Log("journal insert");
		if (currentInfo.CanDelete)
		{
			await APIDispatch.RunAndWaitAsync(() => JournalScreen.HandleInsert(currentInfo.Name, GO));
			UpdateViewFromData();
		}
	}

	public async void HandleDelete(JournalLineData line)
	{
		Debug.Log("journal delete");
		if (currentInfo.CanDelete)
		{
			await APIDispatch.RunAndWaitAsync(() => JournalScreen.HandleDelete(currentInfo.Name, line.entry, GO));
			UpdateViewFromData();
		}
	}

	public void OnSearchTextChange(string text)
	{
		filterText = text;
		if (string.IsNullOrWhiteSpace(text))
		{
			searcher.searchText = text;
		}
		else
		{
			searcher.searchText = text.ToLower();
		}
		UpdateViewFromData();
	}

	public void HandleVPositive()
	{
		categoryCollapsed.Clear();
		UpdateViewFromData();
	}

	public void HandleVNegative()
	{
		categoryCollapsed.Clear();
		foreach (string usedCategory in usedCategories)
		{
			categoryCollapsed[usedCategory] = true;
		}
		UpdateViewFromData();
	}

	public override NavigationContext ShowScreen(XRL.World.GameObject GO, StatusScreensScreen parent)
	{
		currentMapHighlight = null;
		categoryBar = parent.filterBar;
		categoryBar.SingleCategoryOnly = true;
		FilterBar filterBar = categoryBar;
		if (filterBar.enabledCategories == null)
		{
			filterBar.enabledCategories = new List<string>();
		}
		categoryBar.enabledCategories.Clear();
		categoryBar.enabledCategories.Add(currentInfo?.Name ?? JournalScreen.STR_LOCATIONS);
		this.GO = GO;
		this.filterBar = parent.filterBar;
		vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		vertNav.contexts.Add(controller.scrollContext);
		vertNav.wraps = false;
		controller.onSelected.RemoveAllListeners();
		controller.onSelected.AddListener(HandleSelectItem);
		controller.onHighlight.RemoveAllListeners();
		controller.onHighlight.AddListener(HandleHighlightItem);
		controller.scrollContext.wraps = false;
		controller.selectedPosition = 0;
		controller.scrollContext.ActivateAndEnable();
		controller.scrollContext.SetAxis(InputAxisTypes.NavigationYAxis);
		ScrollContext<FrameworkDataElement, NavigationContext> scrollContext = controller.scrollContext;
		if (scrollContext.commandHandlers == null)
		{
			scrollContext.commandHandlers = new Dictionary<string, Action>();
		}
		controller.scrollContext.commandHandlers["V Positive"] = HandleVPositive;
		controller.scrollContext.commandHandlers["V Negative"] = HandleVNegative;
		ScrollContext<NavigationContext> scrollContext2 = vertNav;
		if (scrollContext2.axisHandlers == null)
		{
			scrollContext2.axisHandlers = new Dictionary<InputAxisTypes, Action>();
		}
		scrollContext2 = vertNav;
		if (scrollContext2.menuOptionDescriptions == null)
		{
			ScrollContext<NavigationContext> scrollContext3 = scrollContext2;
			List<MenuOption> list = new List<MenuOption> { CMD_INSERT, CMD_DELETE };
			List<MenuOption> list2 = list;
			scrollContext3.menuOptionDescriptions = list;
		}
		scrollContext2 = vertNav;
		if (scrollContext2.commandHandlers == null)
		{
			scrollContext2.commandHandlers = new Dictionary<string, Action>();
		}
		vertNav.commandHandlers["CmdInsert"] = HandleInsert;
		vertNav.Setup();
		OnSearchTextChange(null);
		base.ShowScreen(GO, parent);
		return vertNav;
	}
}
