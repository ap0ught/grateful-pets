using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using FuzzySharp;
using FuzzySharp.Extractor;
using XRL;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Skill;
using XRL.World.Tinkering;

namespace Qud.UI;

[UIView("TinkeringStatusScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menus", UICanvas = "StatusScreens", UICanvasHost = 1)]
public class TinkeringStatusScreen : BaseStatusScreen<TinkeringStatusScreen>
{
	public class CategoryInfo
	{
		public int N;

		public bool Selected;

		public string Name;
	}

	public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

	public ScrollContext<NavigationContext> horizNav = new ScrollContext<NavigationContext>();

	public FrameworkScroller controller;

	public FrameworkScroller bitsController;

	private Renderable icon;

	private static GameObject startupModItem;

	private static List<TinkerData> BuildRecipes = new List<TinkerData>(64);

	private static List<TinkerData> ModRecipes = new List<TinkerData>(64);

	public int CurrentCategory;

	public int MaxCategory = 1;

	private List<TinkeringLineData> dataList = new List<TinkeringLineData>();

	private TinkeringLineData searcher = new TinkeringLineData
	{
		_sortString = ""
	};

	private List<TinkeringLineData> dataToFree = new List<TinkeringLineData>();

	private List<TinkeringLineData> listItems = new List<TinkeringLineData>();

	private List<string> usedCategories = new List<string>();

	private Dictionary<string, List<TinkeringLineData>> objectCategories = new Dictionary<string, List<TinkeringLineData>>();

	public Dictionary<int, Dictionary<string, bool>> _categoryCollapsed = new Dictionary<int, Dictionary<string, bool>>
	{
		{
			0,
			new Dictionary<string, bool>()
		},
		{
			1,
			new Dictionary<string, bool>()
		}
	};

	private List<string> filterBarCategories = new List<string>();

	private Dictionary<TinkerData, List<GameObject>> applicableObjects = new Dictionary<TinkerData, List<GameObject>>();

	public FilterBar filterBar;

	private Dictionary<int, string> bitDescriptions = new Dictionary<int, string>();

	private List<TinkeringBitsLineData> bitlockerData = new List<TinkeringBitsLineData>();

	public List<CategoryInfo> categoryInfos = new List<CategoryInfo>
	{
		new CategoryInfo
		{
			N = 0,
			Name = "Build"
		},
		new CategoryInfo
		{
			N = 1,
			Name = "Mod"
		}
	};

	private BitCost ActiveCost = new BitCost();

	public UIHotkeySkin modeToggleText;

	public StatusScreensScreen parent;

	public GameObject GO;

	public string SelectedSubCategory;

	public TinkeringDetailsLine detailsLine;

	public Dictionary<string, bool> categoryCollapsed => _categoryCollapsed[CurrentCategory];

	public static void StartupModItemWithTinkering(GameObject go)
	{
		startupModItem = go;
		SingletonWindowBase<StatusScreensScreen>.instance.SetPage(3);
	}

	public override IRenderable GetTabIcon()
	{
		if (icon == null)
		{
			icon = new Renderable("Items/sw_toolbox.bmp", " ", "&c", null, 'C');
		}
		return icon;
	}

	public override string GetTabString()
	{
		return "Tinkering";
	}

	public override bool WantsCategoryBar()
	{
		return true;
	}

	public void UpdateTinkeringData()
	{
		BuildRecipes.Clear();
		ModRecipes.Clear();
		TinkerData.KnownRecipes.Where((TinkerData d) => d.Blueprint == "HandENuke").ToList();
		foreach (TinkerData knownRecipe in TinkerData.KnownRecipes)
		{
			if (knownRecipe.Type == "Build")
			{
				BuildRecipes.Add(knownRecipe);
			}
			else if (knownRecipe.Type == "Mod")
			{
				ModRecipes.Add(knownRecipe);
			}
		}
	}

	public bool isCollapsed(string category)
	{
		if (!_categoryCollapsed[CurrentCategory].TryGetValue(category, out var value))
		{
			return true;
		}
		return value;
	}

	public void SetCategoryExpanded(string category, bool state)
	{
		state = !state;
		if (isCollapsed(category) != state)
		{
			categoryCollapsed[category] = state;
			UpdateViewFromData();
		}
	}

	public override void FilterUpdated(string filterText)
	{
		OnSearchTextChange(filterText);
	}

	public override void UpdateViewFromData()
	{
		UpdateTinkeringData();
		UpdateBitlocker();
		usedCategories.Clear();
		listItems.Clear();
		dataToFree.ForEach((TinkeringLineData l) =>
		{
			l.free();
		});
		dataToFree.Clear();
		modeToggleText.SetText((CurrentCategory == 0) ? "{{hotkey|[~Toggle]}} switch to modifications" : "{{hotkey|[~Toggle]}} switch to build");
		filterBarCategories.Clear();
		filterBarCategories.Add("*All");
		if (CurrentCategory == 0)
		{
			dataList.Clear();
			dataList.AddRange(BuildRecipes.Select((TinkerData n) => PooledFrameworkDataElement<TinkeringLineData>.next().set(this, CurrentCategory, category: false, null, 0, categoryExpanded: true, 0, n, null)));
			dataToFree.AddRange(dataList);
			foreach (TinkeringLineData data in dataList)
			{
				string uiCategory = data.uiCategory;
				if (!filterBarCategories.Contains(uiCategory))
				{
					filterBarCategories.Add(uiCategory);
				}
			}
			IEnumerable<TinkeringLineData> enumerable;
			if (searcher.sortString.IsNullOrEmpty() && filterBar.enabledCategories.Count == 1 && filterBar.enabledCategories[0] == "*All")
			{
				enumerable = dataList;
			}
			else if (searcher.sortString.IsNullOrEmpty())
			{
				enumerable = dataList.Where((TinkeringLineData i) => filterBar.enabledCategories.Contains("*All") || filterBar.enabledCategories.Contains(i.uiCategory) || filterBar.enabledCategories.Contains(i.uiCategory)).ToList();
			}
			else
			{
				enumerable = dataList.Where((TinkeringLineData i) => filterBar.enabledCategories.Contains("*All") || filterBar.enabledCategories.Contains(i.uiCategory) || filterBar.enabledCategories.Contains(i.uiCategory)).ToList();
				IEnumerable<TinkeringLineData> enumerable2;
				if (!searcher.sortString.IsNullOrEmpty())
				{
					enumerable2 = from m in Process.ExtractTop(searcher, enumerable, (TinkeringLineData i) => i.sortString, null, enumerable.Count(), 50)
						select m.Value;
				}
				else
				{
					IEnumerable<TinkeringLineData> enumerable3 = dataList;
					enumerable2 = enumerable3;
				}
				enumerable = enumerable2;
			}
			filterBarCategories.Sort();
			filterBar.SetCategories(filterBarCategories);
			foreach (KeyValuePair<string, List<TinkeringLineData>> objectCategory in objectCategories)
			{
				objectCategory.Value.Clear();
			}
			foreach (TinkeringLineData item5 in enumerable)
			{
				if (item5.data != null)
				{
					string uiCategory2 = item5.uiCategory;
					if (!objectCategories.ContainsKey(uiCategory2))
					{
						objectCategories.Add(uiCategory2, new List<TinkeringLineData>());
					}
					objectCategories[uiCategory2].Add(item5);
					if (!usedCategories.Contains(uiCategory2))
					{
						usedCategories.Add(uiCategory2);
					}
				}
			}
			usedCategories.Sort();
			int num = 0;
			foreach (string item6 in usedCategories.Where((string k) => objectCategories[k].Count > 0))
			{
				listItems.Add(PooledFrameworkDataElement<TinkeringLineData>.next().set(this, CurrentCategory, category: true, item6, objectCategories[item6].Count, !isCollapsed(item6), 0, null, null));
				num++;
				if (isCollapsed(item6))
				{
					continue;
				}
				objectCategories[item6].Sort((TinkeringLineData a, TinkeringLineData b) => a.sortString.CompareTo(b.sortString));
				int num2 = 0;
				foreach (TinkeringLineData item7 in objectCategories[item6])
				{
					item7.categoryOffset = num2 + 1;
					item7.categoryName = item6;
					listItems.Add(item7);
					num++;
					num2++;
				}
			}
		}
		else if (CurrentCategory == 1)
		{
			dataList.Clear();
			dataList.AddRange(ModRecipes.Select((TinkerData n) => PooledFrameworkDataElement<TinkeringLineData>.next().set(this, CurrentCategory, category: false, null, 0, categoryExpanded: true, 0, n, null)));
			dataToFree.AddRange(dataList);
			Action<GameObject> action = (GameObject obj) =>
			{
				string text = ItemModding.ModKey(obj);
				if (text != null && obj.Understood())
				{
					foreach (TinkeringLineData data2 in dataList)
					{
						if (data2.data.CanMod(text) && ItemModding.ModificationApplicable(data2.data.PartName, obj, The.Player))
						{
							string[] array = text.Split(',');
							foreach (string item4 in array)
							{
								if (!filterBarCategories.Contains(item4))
								{
									filterBarCategories.Add(item4);
								}
							}
							if (!data2.applicableKeys.Contains(text))
							{
								data2.applicableKeys.Add(text);
							}
							data2.applicableObjects.Add(obj);
						}
					}
				}
			};
			GO.Inventory.ForeachObject(action);
			GO.Body.ForeachEquippedObject(action);
			_ = dataList;
			if (startupModItem != null)
			{
				dataList = dataList.Where((TinkeringLineData m) => m.applicableObjects.Count > 0).ToList();
			}
			IEnumerable<TinkeringLineData> enumerable4;
			if (searcher.sortString.IsNullOrEmpty() && filterBar.enabledCategories.Count == 1 && filterBar.enabledCategories[0] == "*All")
			{
				enumerable4 = dataList;
			}
			else if (searcher.sortString.IsNullOrEmpty())
			{
				enumerable4 = dataList.Where((TinkeringLineData i) => filterBar.enabledCategories.Contains("*All") || filterBar.enabledCategories.Any((string e) => i.applicableKeys.Any((string k) => k.Contains(e)))).ToList();
			}
			else
			{
				enumerable4 = dataList.Where((TinkeringLineData i) => filterBar.enabledCategories.Contains("*All") || filterBar.enabledCategories.Any((string e) => i.applicableKeys.Any((string k) => k.Contains(e)))).ToList();
				IEnumerable<TinkeringLineData> enumerable5;
				if (!searcher.sortString.IsNullOrEmpty())
				{
					enumerable5 = from m in Process.ExtractTop(searcher, enumerable4, (TinkeringLineData i) => i.sortString, null, enumerable4.Count(), 50)
						select m.Value;
				}
				else
				{
					IEnumerable<TinkeringLineData> enumerable3 = dataList;
					enumerable5 = enumerable3;
				}
				enumerable4 = enumerable5;
			}
			foreach (TinkeringLineData item8 in enumerable4)
			{
				if (!usedCategories.Contains(item8.data.DisplayName))
				{
					usedCategories.Add(item8.data.DisplayName);
				}
			}
			filterBarCategories.Sort();
			filterBar.SetCategories(filterBarCategories);
			usedCategories.Sort();
			int num3 = 0;
			foreach (string category in usedCategories)
			{
				TinkeringLineData tinkeringLineData = dataList.FirstOrDefault((TinkeringLineData d) => d.data.DisplayName == category);
				List<GameObject> list = tinkeringLineData.applicableObjects;
				if (startupModItem != null)
				{
					list = list.Where((GameObject o) => o == startupModItem).ToList();
				}
				if (Enumerable.Count(list) == 0 && startupModItem != null)
				{
					continue;
				}
				TinkeringLineData item = PooledFrameworkDataElement<TinkeringLineData>.next().set(this, CurrentCategory, category: true, category, list.Count, !isCollapsed(category), 0, tinkeringLineData.data, null);
				dataToFree.Add(item);
				listItems.Add(item);
				num3++;
				if (isCollapsed(category))
				{
					continue;
				}
				if (Enumerable.Count(list) == 0)
				{
					TinkeringLineData item2 = PooledFrameworkDataElement<TinkeringLineData>.next().set(this, CurrentCategory, category: false, category, 0, categoryExpanded: false, 1, tinkeringLineData.data, null);
					dataToFree.Add(item2);
					listItems.Add(item2);
					num3++;
					continue;
				}
				list.Sort((GameObject a, GameObject b) => a.DisplayName.CompareTo(b.DisplayName));
				int num4 = 0;
				foreach (GameObject item9 in list)
				{
					TinkeringLineData tinkeringLineData2 = PooledFrameworkDataElement<TinkeringLineData>.next();
					int currentCategory = CurrentCategory;
					GameObject modObject = item9;
					TinkeringLineData item3 = tinkeringLineData2.set(this, currentCategory, category: false, category, 0, categoryExpanded: false, num4 + 1, tinkeringLineData.data, modObject);
					dataToFree.Add(item3);
					listItems.Add(item3);
					num3++;
					num4++;
				}
			}
		}
		if (listItems.Count == 0)
		{
			controller.BeforeShow(new List<TinkeringLineData>
			{
				new TinkeringLineData
				{
					category = true,
					categoryExpanded = false,
					categoryName = "~<none>"
				}
			});
		}
		else
		{
			controller.BeforeShow(listItems);
		}
		if (controller.selectedPosition < (controller.choices?.Count ?? 0))
		{
			HandleHighlightObject(controller.choices[controller.selectedPosition]);
		}
		base.UpdateViewFromData();
	}

	public void UpdateBitlocker()
	{
		if (GO == null)
		{
			return;
		}
		BitLocker bitLocker = ((StatusScreensScreen.GO.GetPart<Tinkering>() == null) ? StatusScreensScreen.GO.GetPart<BitLocker>() : StatusScreensScreen.GO.RequirePart<BitLocker>());
		bitlockerData.ForEach((TinkeringBitsLineData l) =>
		{
			l.free();
		});
		bitlockerData.Clear();
		if (bitLocker != null)
		{
			for (int num = 0; num < BitType.BitOrder.Count; num++)
			{
				char c = BitType.BitOrder[num];
				BitType bitType = BitType.BitMap[c];
				if (!bitDescriptions.TryGetValue(num, out var value))
				{
					value = $"{{{{{bitType.Color}|{(Options.AlphanumericBits ? BitType.CharTranslateBit(bitType.Color) : '\a')} {bitType.Description}}}}}";
					bitDescriptions.Add(num, value);
				}
				bitlockerData.Add(PooledFrameworkDataElement<TinkeringBitsLineData>.next().set(c, value, bitLocker.GetBitCount(c), ActiveCost));
			}
		}
		bitsController.BeforeShow(bitlockerData);
	}

	public void categoryBarClicked(FrameworkDataElement el)
	{
		if (el is ButtonBar.ButtonBarButtonData buttonBarButtonData)
		{
			buttonBarButtonData?.onSelect();
		}
	}

	public async void HandleSelectItem(FrameworkDataElement data)
	{
		TinkeringLineData d = data as TinkeringLineData;
		if (d == null)
		{
			return;
		}
		if (d.category)
		{
			if (!categoryCollapsed.ContainsKey(d.categoryName))
			{
				categoryCollapsed.Add(d.categoryName, value: true);
			}
			categoryCollapsed[d.categoryName] = !categoryCollapsed[d.categoryName];
			UpdateViewFromData();
			return;
		}
		XRL.World.Event FromEvent = new XRL.World.Event("InterfaceTest");
		bool exit = false;
		await APIDispatch.RunAndWaitAsync(() =>
		{
			if (d.mode == 0)
			{
				if (!TinkeringScreen.PerformUITinkerBuild(GO, d.data, FromEvent) || FromEvent.InterfaceExitRequested())
				{
					exit = true;
				}
			}
			else if (d.mode == 1)
			{
				bool didMod = false;
				if (!TinkeringScreen.PerformUITinkerMod(GO, d.modObject, d.data, d.cost, FromEvent, ref didMod) || FromEvent.InterfaceExitRequested())
				{
					exit = true;
				}
			}
		});
		if (exit)
		{
			parent.Exit();
		}
		else
		{
			UpdateViewFromData();
		}
	}

	public void HandleHighlightObject(FrameworkDataElement data)
	{
		ActiveCost.Clear();
		if (data is TinkeringLineData tinkeringLineData)
		{
			if (!tinkeringLineData.category)
			{
				tinkeringLineData.cost.CopyTo(ActiveCost);
				UpdateBitlocker();
			}
			detailsLine.setData(data);
		}
	}

	public void OnSearchTextChange(string text)
	{
		startupModItem = null;
		if (string.IsNullOrWhiteSpace(text))
		{
			searcher._sortString = text;
		}
		else
		{
			searcher._sortString = text.ToLower();
		}
		UpdateViewFromData();
	}

	public void HandleModeToggle()
	{
		startupModItem = null;
		CurrentCategory = 1 - CurrentCategory;
		UpdateViewFromData();
	}

	public void HandleVPositive()
	{
		categoryCollapsed.Clear();
		foreach (string usedCategory in usedCategories)
		{
			categoryCollapsed[usedCategory] = false;
		}
		UpdateViewFromData();
	}

	public void HandleVNegative()
	{
		categoryCollapsed.Clear();
		UpdateViewFromData();
	}

	public override NavigationContext ShowScreen(GameObject GO, StatusScreensScreen parent)
	{
		detailsLine.gameObject.SetActive(value: false);
		filterBar = parent.filterBar;
		this.parent = parent;
		this.GO = GO;
		vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		vertNav.contexts.Add(controller.scrollContext);
		vertNav.wraps = false;
		vertNav.Setup();
		ScrollContext<NavigationContext> scrollContext = vertNav;
		if (scrollContext.commandHandlers == null)
		{
			scrollContext.commandHandlers = new Dictionary<string, Action>();
		}
		vertNav.commandHandlers["Toggle"] = HandleModeToggle;
		controller.onSelected.RemoveAllListeners();
		controller.onSelected.AddListener(HandleSelectItem);
		controller.onHighlight.RemoveAllListeners();
		controller.onHighlight.AddListener(HandleHighlightObject);
		controller.scrollContext.wraps = false;
		ScrollContext<FrameworkDataElement, NavigationContext> scrollContext2 = controller.scrollContext;
		if (scrollContext2.commandHandlers == null)
		{
			scrollContext2.commandHandlers = new Dictionary<string, Action>();
		}
		controller.scrollContext.commandHandlers["V Positive"] = HandleVPositive;
		controller.scrollContext.commandHandlers["V Negative"] = HandleVNegative;
		if (startupModItem == null)
		{
			OnSearchTextChange(null);
		}
		else
		{
			searcher._sortString = null;
			CurrentCategory = 1;
			filterBar.searchInput.InputText.SetText(startupModItem.DisplayNameOnlyStripped.ToLower());
			UpdateViewFromData();
		}
		controller.selectedPosition = 0;
		controller.scrollContext.ActivateAndEnable();
		base.ShowScreen(GO, parent);
		return vertNav;
	}

	public override bool Exit()
	{
		startupModItem = null;
		return true;
	}
}
