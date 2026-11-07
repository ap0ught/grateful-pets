using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ConsoleLib.Console;
using FuzzySharp;
using FuzzySharp.Extractor;
using ModelShark;
using Qud.API;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Anatomy;

namespace Qud.UI;

[UIView("InventoryAndEquipmentStatusScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menus", UICanvas = "StatusScreens", UICanvasHost = 1)]
public class InventoryAndEquipmentStatusScreen : BaseStatusScreen<InventoryAndEquipmentStatusScreen>
{
	public enum SortMode
	{
		AZ,
		Category
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class EquipmentScreenCommandsCallbackAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class HasEquipmentScreenCommandsCallbackAttribute : Attribute
	{
	}

	public SortMode sortMode = SortMode.Category;

	public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

	public ScrollContext<NavigationContext> horizNav = new ScrollContext<NavigationContext>();

	public UnityEngine.GameObject equipmentPaperdollScroller;

	public UnityEngine.GameObject equipmentListScroller;

	public PaperdollScroller equipmentPaperdollController;

	public FrameworkScroller equipmentListController;

	public FrameworkScroller inventoryController;

	private Renderable icon;

	public UITextSkin priceText;

	public UITextSkin weightText;

	private InventoryLineData searcher = new InventoryLineData();

	private List<string> usedCategories = new List<string>();

	private List<InventoryLineData> listItems = new List<InventoryLineData>();

	private Dictionary<string, List<InventoryLineData>> objectCategories = new Dictionary<string, List<InventoryLineData>>();

	private Dictionary<string, bool> categoryCollapsed = new Dictionary<string, bool>();

	private List<string> filterBarCategories = new List<string>();

	private Dictionary<string, int> categoryWeight = new Dictionary<string, int>();

	private Dictionary<string, int> categoryAmount = new Dictionary<string, int>();

	private FilterBar filterBar;

	public List<EquipmentLineData> lines = new List<EquipmentLineData>(64);

	public UIHotkeySkin cyberneticsHotkeySkin;

	public UIHotkeySkin cyberneticsHotkeySkinForList;

	public bool showCybernetics;

	public static string EQUIPMENT_MODE_PAPERDOLL = "Paperdoll";

	public static string EQUIPMENT_MODE_LIST = "List";

	public string EquipmentMode = EQUIPMENT_MODE_PAPERDOLL;

	public static string SEARCH_MODE_FUZZY = "Fuzzy";

	public static string SEARCH_MODE_STRICT = "Strict";

	public string SearchMode = SEARCH_MODE_STRICT;

	private StatusScreensScreen statusScreensScreen;

	public XRL.World.GameObject GO;

	public static bool equipmentPaneHighlighted;

	public LayoutElement equipmentListLayoutElement;

	public LayoutElement equipmentPaperdollLayoutElement;

	public LayoutElement inventoryLayoutElement;

	public static bool _EnteringFromEquipmentSide = false;

	public static bool expandHighlightedPaneMode;

	public static bool paginationMode;

	private HotkeySpread inventorySpread;

	public MenuOption CMD_SHOWCYBERNETICS = new MenuOption
	{
		Id = "CmdShowCybernetics",
		InputCommand = "Toggle",
		Description = "Toggle Cybernetics"
	};

	public MenuOption CMD_OPTIONS = new MenuOption
	{
		Id = "CmdShowOptions",
		InputCommand = "CmdOptions",
		Description = "Display Options"
	};

	public MenuOption SET_PRIMARY_LIMB = new MenuOption
	{
		Id = "CmdInsert",
		InputCommand = "CmdInsert",
		Description = "Set Primary Limb"
	};

	public MenuOption SHOW_TOOLTIP = new MenuOption
	{
		Id = "CmdShowTooltip",
		InputCommand = "Highlight",
		Description = "Show Tooltip"
	};

	public MenuOption QUICK_DROP = new MenuOption
	{
		Id = "InventoryQuickDrop",
		InputCommand = "InventoryQuickDrop",
		Description = "Quick Drop"
	};

	public MenuOption QUICK_EAT = new MenuOption
	{
		Id = "InventoryQuickEat",
		InputCommand = "InventoryQuickEat",
		Description = "Quick Eat"
	};

	public MenuOption QUICK_DRINK = new MenuOption
	{
		Id = "InventoryQuickDrink",
		InputCommand = "InventoryQuickDrink",
		Description = "Quick Drink"
	};

	public MenuOption QUICK_APPLY = new MenuOption
	{
		Id = "InventoryQuickApply",
		InputCommand = "InventoryQuickApply",
		Description = "Quick Apply"
	};

	public bool IsPaperdollMode => EquipmentMode == EQUIPMENT_MODE_PAPERDOLL;

	public static bool EnteringFromEquipmentSide
	{
		get
		{
			return _EnteringFromEquipmentSide;
		}
		set
		{
			_EnteringFromEquipmentSide = value;
			equipmentPaneHighlighted = value;
		}
	}

	public override IRenderable GetTabIcon()
	{
		if (icon == null)
		{
			icon = new Renderable("Tiles/sw_chest.bmp", " ", "&w", null, 'W');
		}
		return icon;
	}

	public override string GetTabString()
	{
		if (Media.sizeClass < Media.SizeClass.Medium)
		{
			return "Eq";
		}
		return "Equipment";
	}

	public override bool WantsCategoryBar()
	{
		return true;
	}

	public async void HandleQuickDrop(FrameworkDataElement element)
	{
		bool bDone = false;
		InventoryLineData inventoryLine = element as InventoryLineData;
		if (inventoryLine == null || inventoryLine.category)
		{
			return;
		}
		await APIDispatch.RunAndWaitAsync(() =>
		{
			Zone currentZone = GO.CurrentZone;
			if (currentZone == null || !currentZone.IsWorldMap())
			{
				InventoryActionEvent.Check(ref bDone, GO, GO, inventoryLine.go, "CommandDropObject");
			}
		});
		if (bDone)
		{
			statusScreensScreen.Exit();
		}
		else
		{
			UpdateViewFromData();
		}
	}

	public async void HandleQuickApply(FrameworkDataElement element)
	{
		bool bDone = false;
		InventoryLineData inventoryLine = element as InventoryLineData;
		if (inventoryLine != null && !inventoryLine.category)
		{
			IEvent sentEvent = null;
			await APIDispatch.RunAndWaitAsync(() =>
			{
				InventoryActionEvent.Check(out sentEvent, inventoryLine.go, GO, inventoryLine.go, "Apply", false, false, false, false, false, 0, 0, 0, (XRL.World.GameObject)null, (Cell)null, (Cell)null, (IInventory)null);
			});
			if (bDone)
			{
				statusScreensScreen.Exit();
			}
			else
			{
				UpdateViewFromData();
			}
		}
	}

	public async void HandleQuickDrink(FrameworkDataElement element)
	{
		bool bDone = false;
		InventoryLineData inventoryLine = element as InventoryLineData;
		if (inventoryLine != null && !inventoryLine.category)
		{
			IEvent sentEvent = null;
			await APIDispatch.RunAndWaitAsync(() =>
			{
				InventoryActionEvent.Check(out sentEvent, inventoryLine.go, GO, inventoryLine.go, "Drink", false, false, false, false, false, 0, 0, 0, (XRL.World.GameObject)null, (Cell)null, (Cell)null, (IInventory)null);
			});
			if (bDone)
			{
				statusScreensScreen.Exit();
			}
			else
			{
				UpdateViewFromData();
			}
		}
	}

	public async void HandleQuickEat(FrameworkDataElement element)
	{
		bool bDone = false;
		InventoryLineData inventoryLine = element as InventoryLineData;
		if (inventoryLine != null && !inventoryLine.category)
		{
			IEvent sentEvent = null;
			await APIDispatch.RunAndWaitAsync(() =>
			{
				InventoryActionEvent.Check(out sentEvent, inventoryLine.go, GO, inventoryLine.go, "Eat", false, false, false, false, false, 0, 0, 0, (XRL.World.GameObject)null, (Cell)null, (Cell)null, (IInventory)null);
			});
			if (bDone)
			{
				statusScreensScreen.Exit();
			}
			else
			{
				UpdateViewFromData();
			}
		}
	}

	public async void HandleDragDropAutoequip(InventoryLineData item)
	{
		if (item?.go != null)
		{
			await APIDispatch.RunAndWaitAsync(() =>
			{
				The.Player.AutoEquip(item.go);
			});
			The.Player.CheckStacks();
			UpdateViewFromData();
		}
	}

	public async void HandleLookAtDefaultEquipmnent(EquipmentLineData from)
	{
		if (from?.bodyPart?.DefaultBehavior != null)
		{
			await APIDispatch.RunAndWaitAsync(() =>
			{
				InventoryActionEvent.Check(from?.bodyPart?.DefaultBehavior, GO, from?.bodyPart?.DefaultBehavior, "Look");
			});
			The.Player.CheckStacks();
			UpdateViewFromData();
		}
	}

	public async void HandleDragDrop(EquipmentLineData from, EquipmentLineData to)
	{
		if (to == null)
		{
			await APIDispatch.RunAndWaitAsync(() =>
			{
				EquipmentAPI.UnequipObject(from.bodyPart.Equipped);
			});
			from.line?.GetNavigationContext().Activate();
			The.Player.CheckStacks();
		}
		else
		{
			await APIDispatch.RunAndWaitAsync(() =>
			{
				EquipmentAPI.EquipObjectToPlayer(from.bodyPart.Equipped, to.bodyPart);
			});
			to.line?.GetNavigationContext().Activate();
			The.Player.CheckStacks();
		}
		UpdateViewFromData();
	}

	public async void HandleDragDrop(InventoryLineData from, EquipmentLineData to)
	{
		await APIDispatch.RunAndWaitAsync(() =>
		{
			EquipmentAPI.EquipObjectToPlayer(from.go, to.bodyPart);
			The.Player.CheckStacks();
		});
		UpdateViewFromData();
		to.line?.GetNavigationContext().Activate();
	}

	public void SetCategoryExpanded(string categoryName, bool state)
	{
		state = !state;
		if (!categoryCollapsed.ContainsKey(categoryName))
		{
			categoryCollapsed.Add(categoryName, value: false);
		}
		if (categoryCollapsed[categoryName] != state)
		{
			categoryCollapsed[categoryName] = state;
			UpdateViewFromData();
		}
	}

	public async void HandleSelectItem(FrameworkDataElement element)
	{
		InventoryLineData inventoryLine = element as InventoryLineData;
		if (inventoryLine != null)
		{
			if (inventoryLine.category)
			{
				categoryCollapsed[inventoryLine.categoryName] = !isCollapsed(inventoryLine.categoryName);
				UpdateViewFromData();
			}
			else if (TutorialManager.AllowInventoryInteract(inventoryLine.go))
			{
				bool bDone = false;
				InventoryAction resultingAction = null;
				await APIDispatch.RunAndWaitAsync(() =>
				{
					EquipmentAPI.TwiddleObject(The.Player, inventoryLine.go, ref bDone, out resultingAction);
				});
				if (resultingAction != null && resultingAction.Command == "Mod")
				{
					TinkeringStatusScreen.StartupModItemWithTinkering(inventoryLine.go);
				}
				if (bDone)
				{
					statusScreensScreen.Exit();
				}
				else
				{
					UpdateViewFromData();
				}
			}
			return;
		}
		EquipmentLineData equipmentLine = element as EquipmentLineData;
		if (equipmentLine == null)
		{
			return;
		}
		bool flag = NavigationController.currentEvent?.IsRightClick() ?? false;
		bool bDone2 = false;
		if (equipmentLine.showCybernetics)
		{
			if (equipmentLine.bodyPart.Cybernetics == null)
			{
				return;
			}
			await APIDispatch.RunAndWaitAsync(() =>
			{
				EquipmentAPI.TwiddleObject(GO, equipmentLine.bodyPart.Cybernetics, ref bDone2);
			});
		}
		else if (equipmentLine.bodyPart.Equipped != null)
		{
			InventoryAction resultingAction2 = null;
			await APIDispatch.RunAndWaitAsync(() =>
			{
				EquipmentAPI.TwiddleObject(GO, equipmentLine.bodyPart.Equipped, ref bDone2, out resultingAction2);
			});
			if (resultingAction2 != null && resultingAction2.Command == "Mod")
			{
				TinkeringStatusScreen.StartupModItemWithTinkering(equipmentLine.bodyPart.Equipped);
			}
			if (bDone2)
			{
				statusScreensScreen.Exit();
			}
			else
			{
				UpdateViewFromData();
			}
		}
		else if (!flag || equipmentLine?.bodyPart?.DefaultBehavior == null)
		{
			await APIDispatch.RunAndWaitAsync(() =>
			{
				EquipmentScreen.ShowBodypartEquipUI(GO, equipmentLine.bodyPart);
			});
		}
		else
		{
			await APIDispatch.RunAndWaitAsync(() =>
			{
				InventoryActionEvent.Check(equipmentLine?.bodyPart?.DefaultBehavior, GO, equipmentLine?.bodyPart?.DefaultBehavior, "Look");
			});
		}
		if (bDone2)
		{
			statusScreensScreen.Exit();
		}
		else
		{
			UpdateViewFromData();
		}
	}

	public bool isCollapsed(string category)
	{
		if (!categoryCollapsed.TryGetValue(category, out var value))
		{
			return false;
		}
		return value;
	}

	public override void UpdateViewFromData()
	{
		usedCategories.Clear();
		listItems.ForEach((InventoryLineData l) =>
		{
			l.free();
		});
		listItems.Clear();
		foreach (KeyValuePair<string, List<InventoryLineData>> objectCategory in objectCategories)
		{
			objectCategory.Value.Clear();
		}
		IEnumerable<InventoryLineData> enumerable;
		if (GO != null)
		{
			enumerable = from go in GO.Inventory.Objects
				where !go.HasTag("HiddenInInventory")
				select PooledFrameworkDataElement<InventoryLineData>.next().set(category: false, go.GetInventoryCategory(), categoryExpanded: false, 0, 0, 0, go, inventorySpread, this);
		}
		else
		{
			IEnumerable<InventoryLineData> enumerable2 = new List<InventoryLineData>();
			enumerable = enumerable2;
		}
		IEnumerable<InventoryLineData> enumerable3 = enumerable;
		filterBarCategories.Clear();
		filterBarCategories.Add("*All");
		foreach (InventoryLineData item in enumerable3)
		{
			if (item.go != null)
			{
				string inventoryCategory = item.go.GetInventoryCategory();
				if (!filterBarCategories.Contains(inventoryCategory))
				{
					filterBarCategories.Add(inventoryCategory);
				}
			}
		}
		IEnumerable<InventoryLineData> enumerable4;
		if (filterText.IsNullOrEmpty() && filterBar.enabledCategories.Count == 1 && filterBar.enabledCategories[0] == "*All")
		{
			enumerable4 = enumerable3;
		}
		else if (filterText.IsNullOrEmpty())
		{
			enumerable4 = enumerable3.Where((InventoryLineData i) => filterBar.enabledCategories.Contains("*All") || filterBar.enabledCategories.Contains(i.categoryName) || (i.go != null && filterBar.enabledCategories.Contains(i.categoryName))).ToList();
		}
		else
		{
			enumerable4 = enumerable3.Where((InventoryLineData i) => filterBar.enabledCategories.Contains("*All") || filterBar.enabledCategories.Contains(i.categoryName) || (i.go != null && filterBar.enabledCategories.Contains(i.categoryName))).ToList();
			if (SearchMode == SEARCH_MODE_FUZZY)
			{
				enumerable4 = (filterText.IsNullOrEmpty() ? enumerable3 : (from m in Process.ExtractTop(searcher, enumerable4, (InventoryLineData i) => i.sortString, null, enumerable4.Count(), 50)
					select m.Value));
			}
			else
			{
				enumerable4 = enumerable4.Where((InventoryLineData item) => item.sortString.Contains(searcher.sortString, CompareOptions.IgnoreCase));
			}
		}
		filterBar.SetCategories(filterBarCategories);
		foreach (string item2 in categoryWeight.Keys.ToList())
		{
			categoryWeight[item2] = 0;
		}
		foreach (string item3 in categoryAmount.Keys.ToList())
		{
			categoryAmount[item3] = 0;
		}
		foreach (InventoryLineData item4 in enumerable4)
		{
			if (item4.go != null)
			{
				string inventoryCategory2 = item4.go.GetInventoryCategory();
				if (!objectCategories.ContainsKey(inventoryCategory2))
				{
					objectCategories.Add(inventoryCategory2, new List<InventoryLineData>());
				}
				if (!categoryWeight.ContainsKey(inventoryCategory2))
				{
					categoryWeight.Add(inventoryCategory2, 0);
				}
				if (!categoryAmount.ContainsKey(inventoryCategory2))
				{
					categoryAmount.Add(inventoryCategory2, 0);
				}
				categoryAmount[inventoryCategory2]++;
				categoryWeight[inventoryCategory2] += item4.go.Weight;
				objectCategories[inventoryCategory2].Add(item4);
				if (!usedCategories.Contains(inventoryCategory2))
				{
					usedCategories.Add(inventoryCategory2);
				}
			}
		}
		usedCategories.Sort();
		if (sortMode == SortMode.Category)
		{
			int num = 0;
			foreach (string usedCategory in usedCategories)
			{
				listItems.Add(PooledFrameworkDataElement<InventoryLineData>.next().set(category: true, usedCategory, !isCollapsed(usedCategory), categoryWeight: categoryWeight.ContainsKey(usedCategory) ? categoryWeight[usedCategory] : 0, categoryAmount: categoryAmount.ContainsKey(usedCategory) ? categoryAmount[usedCategory] : 0, categoryOffset: 0, go: null, spread: inventorySpread, screen: this));
				num++;
				if (isCollapsed(usedCategory))
				{
					continue;
				}
				objectCategories[usedCategory].Sort((InventoryLineData a, InventoryLineData b) => a.displayName.CompareTo(b.displayName));
				int num2 = 1;
				foreach (InventoryLineData item5 in objectCategories[usedCategory])
				{
					item5.categoryOffset = num2;
					num2++;
					listItems.Add(item5);
					num++;
				}
			}
		}
		else if (sortMode == SortMode.AZ)
		{
			listItems.AddRange(enumerable4);
			listItems.Sort((InventoryLineData a, InventoryLineData b) => a.displayName.CompareTo(b.displayName));
		}
		if (GO != null)
		{
			priceText.SetText($"{{{{B|${GO.GetFreeDrams()}}}}}");
			weightText.SetText($"{{{{C|{GO.GetCarriedWeight()}{{{{K|/{GO.GetMaxCarriedWeight()}}}}} lbs. }}}}");
		}
		horizNav.contexts.Clear();
		horizNav.contexts.Add(IsPaperdollMode ? equipmentPaperdollController.scrollContext : equipmentListController.scrollContext);
		horizNav.contexts.Add(inventoryController.scrollContext);
		inventoryController.BeforeShow(listItems);
		foreach (EquipmentLineData line in lines)
		{
			line.free();
		}
		lines.Clear();
		if (IsPaperdollMode)
		{
			if (GO != null)
			{
				lines.AddRange(from bodyPart in GO.Body.GetParts()
					select PooledFrameworkDataElement<EquipmentLineData>.next().set(showCybernetics, bodyPart, this, inventorySpread));
			}
			equipmentPaperdollController.BeforeShow(lines);
			cyberneticsHotkeySkin.text = (showCybernetics ? "{{hotkey|[~Toggle]}} show equipment" : "{{hotkey|[~Toggle]}} show cybernetics");
		}
		else
		{
			if (GO != null)
			{
				lines.AddRange(from bodyPart in GO.Body.LoopParts()
					select PooledFrameworkDataElement<EquipmentLineData>.next().set(showCybernetics, bodyPart, this, inventorySpread));
			}
			equipmentListController.BeforeShow(lines);
			cyberneticsHotkeySkinForList.text = (showCybernetics ? "{{hotkey|[~Toggle]}} show equipment" : "{{hotkey|[~Toggle]}} show cybernetics");
		}
		TooltipManager.Instance.CloseAll();
	}

	public void HandleShowCybernetics()
	{
		showCybernetics = !showCybernetics;
		UpdateViewFromData();
	}

	public async void HandleShowOptions()
	{
		int num = await Popup.PickOptionAsync("Options", null, "", new string[3]
		{
			(EquipmentMode == EQUIPMENT_MODE_PAPERDOLL) ? "Equipment View: {{W|Paperdoll}}/List" : "Equipment View: Paperdoll/{{W|List}}",
			(sortMode == SortMode.Category) ? "Sort Mode: {{W|Category}}/A-Z" : "Sort Mode: Category/{{W|A-Z}}",
			(SearchMode == SEARCH_MODE_STRICT) ? "Search Mode: {{W|Strict}}/Fuzzy" : "Search Mode: Strict/{{W|Fuzzy}}"
		}, null, null, null, null, 0, 60, 0, -1, RespectOptionNewlines: false, AllowEscape: true);
		if (num == 0)
		{
			HandleToggleEquipmentMode();
		}
		if (num == 1)
		{
			HandleToggleSortMode();
		}
		if (num == 2)
		{
			HandleToggleSearchMode();
		}
	}

	public void HandleToggleSearchMode()
	{
		if (SearchMode == SEARCH_MODE_FUZZY)
		{
			SearchMode = SEARCH_MODE_STRICT;
		}
		else
		{
			SearchMode = SEARCH_MODE_FUZZY;
		}
		PlayerPrefs.SetString("InventoryScreenSearchMode", SearchMode);
		UpdateViewFromData();
	}

	public void HandleToggleSortMode()
	{
		if (sortMode == SortMode.Category)
		{
			sortMode = SortMode.AZ;
		}
		else
		{
			sortMode = SortMode.Category;
		}
		UpdateViewFromData();
	}

	public void HandleToggleEquipmentMode()
	{
		if (EquipmentMode == EQUIPMENT_MODE_LIST)
		{
			EquipmentMode = EQUIPMENT_MODE_PAPERDOLL;
		}
		else
		{
			EquipmentMode = EQUIPMENT_MODE_LIST;
		}
		PlayerPrefs.SetString("EquipmentScreenListMode", EquipmentMode);
		ShowScreen(GO, statusScreensScreen);
	}

	public void OnSearchTextChange(string text)
	{
		filterText = text;
		if (string.IsNullOrEmpty(text))
		{
			searcher.sortString = text;
		}
		else
		{
			searcher.sortString = text.ToLower();
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
		foreach (string usedCategory in usedCategories)
		{
			categoryCollapsed[usedCategory] = true;
		}
		UpdateViewFromData();
	}

	public override void FilterUpdated(string filterText)
	{
		OnSearchTextChange(filterText);
	}

	public void UpdatePaneSize()
	{
		if (!expandHighlightedPaneMode)
		{
			if (equipmentPaperdollLayoutElement.flexibleWidth != 1f)
			{
				equipmentPaperdollLayoutElement.flexibleWidth = 1f;
			}
			if (equipmentListLayoutElement.flexibleWidth != 1f)
			{
				equipmentListLayoutElement.flexibleWidth = 1f;
			}
			if (inventoryLayoutElement.flexibleWidth != 1.5f)
			{
				inventoryLayoutElement.flexibleWidth = 1.5f;
			}
		}
		else if (equipmentPaneHighlighted)
		{
			if (equipmentPaperdollLayoutElement.flexibleWidth != 2f)
			{
				equipmentPaperdollLayoutElement.flexibleWidth = 2f;
			}
			if (equipmentListLayoutElement.flexibleWidth != 2f)
			{
				equipmentListLayoutElement.flexibleWidth = 2f;
			}
			if (inventoryLayoutElement.flexibleWidth != 1f)
			{
				inventoryLayoutElement.flexibleWidth = 1f;
			}
		}
		else
		{
			if (equipmentPaperdollLayoutElement.flexibleWidth != 1f)
			{
				equipmentPaperdollLayoutElement.flexibleWidth = 1f;
			}
			if (equipmentListLayoutElement.flexibleWidth != 1f)
			{
				equipmentListLayoutElement.flexibleWidth = 1f;
			}
			if (inventoryLayoutElement.flexibleWidth != 2f)
			{
				inventoryLayoutElement.flexibleWidth = 2f;
			}
		}
	}

	public void Update()
	{
		UpdatePaneSize();
	}

	public override NavigationContext ShowScreen(XRL.World.GameObject GO, StatusScreensScreen parent)
	{
		expandHighlightedPaneMode = Options.GetOption("OptionExpandHighlightedInventoryAndEquipmentPane") == "Yes";
		paginationMode = Options.GetOption("OptionNavigateInventoryAndEquipmentWithPagination") == "Yes";
		UpdatePaneSize();
		ResetLayoutFrame();
		EquipmentMode = PlayerPrefs.GetString("EquipmentScreenListMode", EQUIPMENT_MODE_PAPERDOLL);
		equipmentPaperdollScroller.SetActive(EquipmentMode == EQUIPMENT_MODE_PAPERDOLL);
		equipmentListScroller.SetActive(EquipmentMode == EQUIPMENT_MODE_LIST);
		SearchMode = PlayerPrefs.GetString("InventoryScreenSearchMode", SEARCH_MODE_STRICT);
		statusScreensScreen = parent;
		filterBar = parent.filterBar;
		inventorySpread = HotkeySpread.get("Menu");
		statusScreensScreen = parent;
		this.GO = GO;
		horizNav.SetAxis(paginationMode ? InputAxisTypes.NavigationPageXAxis : InputAxisTypes.NavigationXAxis);
		horizNav.wraps = !paginationMode;
		horizNav.contexts.Clear();
		horizNav.contexts.Add(IsPaperdollMode ? equipmentPaperdollController.scrollContext : equipmentListController.scrollContext);
		horizNav.contexts.Add(inventoryController.scrollContext);
		vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		vertNav.contexts.Clear();
		vertNav.contexts.Add(horizNav);
		vertNav.wraps = false;
		OnSearchTextChange(null);
		if (IsPaperdollMode)
		{
			equipmentPaperdollController.onSelected.RemoveAllListeners();
			equipmentPaperdollController.onSelected.AddListener(HandleSelectItem);
			equipmentPaperdollController.onHighlight.RemoveAllListeners();
		}
		else
		{
			equipmentListController.onSelected.RemoveAllListeners();
			equipmentListController.onSelected.AddListener(HandleSelectItem);
			equipmentListController.onHighlight.RemoveAllListeners();
		}
		inventoryController.onSelected.RemoveAllListeners();
		inventoryController.onSelected.AddListener(HandleSelectItem);
		inventoryController.onHighlight.RemoveAllListeners();
		inventoryController.scrollContext.wraps = false;
		if (IsPaperdollMode)
		{
			equipmentPaperdollController.selectedPosition = 0;
		}
		else
		{
			equipmentListController.selectedPosition = 0;
		}
		inventoryController.selectedPosition = 0;
		if (EnteringFromEquipmentSide)
		{
			if (IsPaperdollMode)
			{
				equipmentPaperdollController.scrollContext.ActivateAndEnable();
			}
			else
			{
				equipmentListController.scrollContext.ActivateAndEnable();
			}
		}
		else
		{
			inventoryController.scrollContext.ActivateAndEnable();
		}
		horizNav.Setup();
		vertNav.Setup();
		NavigationContext screenGlobalContext = parent.screenGlobalContext;
		if (screenGlobalContext.menuOptionDescriptions == null)
		{
			List<MenuOption> list = (screenGlobalContext.menuOptionDescriptions = new List<MenuOption>());
		}
		parent.screenGlobalContext.menuOptionDescriptions.Add(SET_PRIMARY_LIMB);
		parent.screenGlobalContext.menuOptionDescriptions.Add(SHOW_TOOLTIP);
		parent.screenGlobalContext.menuOptionDescriptions.Add(CMD_OPTIONS);
		screenGlobalContext = parent.screenGlobalContext;
		if (screenGlobalContext.commandHandlers == null)
		{
			screenGlobalContext.commandHandlers = new Dictionary<string, Action>();
		}
		parent.screenGlobalContext.commandHandlers["Toggle"] = HandleShowCybernetics;
		parent.screenGlobalContext.commandHandlers["CmdOptions"] = HandleShowOptions;
		foreach (MethodInfo item in ModManager.GetMethodsWithAttribute(typeof(EquipmentScreenCommandsCallbackAttribute), typeof(HasEquipmentScreenCommandsCallbackAttribute)))
		{
			try
			{
				object[] parameters = new object[2] { parent.screenGlobalContext, this };
				item.Invoke(null, parameters);
			}
			catch (Exception arg)
			{
				MetricsManager.LogAssemblyError(item, $"Error invoking {item.DeclaringType.FullName}.{item.Name}: {arg}");
			}
		}
		base.ShowScreen(GO, parent);
		ScrollContext<NavigationContext> scrollContext = vertNav;
		if (scrollContext.commandHandlers == null)
		{
			scrollContext.commandHandlers = new Dictionary<string, Action>();
		}
		vertNav.commandHandlers["V Positive"] = HandleVPositive;
		vertNav.commandHandlers["V Negative"] = HandleVNegative;
		UpdatePaneSize();
		return vertNav;
	}
}
