using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleLib.Console;
using FuzzySharp;
using FuzzySharp.Extractor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using XRL;
using XRL.CharacterBuilds.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[UIView("Keybinds", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "Keybinds", UICanvasHost = 1)]
public class KeybindsScreen : SingletonWindowBase<KeybindsScreen>, ControlManager.IControllerChangedEvent
{
	public FrameworkContext inputTypeContext;

	public UITextSkin inputTypeText;

	public Image background;

	public FrameworkScroller hotkeyBar;

	public VisibleWindowScroller keybindsScroller;

	public FrameworkScroller categoryScroller;

	public FrameworkSearchInput searchInput;

	public RectTransform safeArea;

	public EmbarkBuilderModuleBackButton backButton;

	public TaskCompletionSource<bool> completionSource;

	public NavigationContext globalContext = new NavigationContext();

	public ScrollContext<NavigationContext> midHorizNav = new ScrollContext<NavigationContext>();

	public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

	public List<FrameworkDataElement> menuItems = new List<FrameworkDataElement>();

	public List<FrameworkDataElement> filteredItems = new List<FrameworkDataElement>();

	public Dictionary<string, bool> categoryExpanded = new Dictionary<string, bool>();

	public float lastWidth;

	public float breakpointBackButtonWidth;

	public bool wasInScroller;

	private bool SelectFirst = true;

	public static readonly MenuOption REMOVE_BIND = new MenuOption
	{
		Id = "REMOVE_BIND",
		InputCommand = "CmdDelete",
		Description = "remove keybind"
	};

	public static readonly MenuOption RESTORE_DEFAULTS = new MenuOption
	{
		Id = "RESTORE_DEFAULTS",
		InputCommand = "V Positive",
		Description = "restore defaults"
	};

	public List<MenuOption> keyMenuOptions = new List<MenuOption>();

	public string searchText;

	private KeybindDataRow searcher = new KeybindDataRow();

	public FrameworkDataElement lastSelectedElement;

	public bool bChangesMade;

	public Dictionary<ControlManager.InputDeviceType, string> ControlTypeDisplayName = new Dictionary<ControlManager.InputDeviceType, string>
	{
		{
			ControlManager.InputDeviceType.Keyboard,
			"Keyboard && Mouse"
		},
		{
			ControlManager.InputDeviceType.Gamepad,
			"Gamepad"
		}
	};

	public ControlManager.InputDeviceType currentControllerType = ControlManager.InputDeviceType.Keyboard;

	public bool breakBackButton => Media.sizeClass < Media.SizeClass.Medium;

	public static MenuOption BACK_BUTTON => EmbarkBuilderOverlayWindow.BackMenuOption;

	public InputDevice currentGamepad => Gamepad.current;

	public void getCurrentActiveControl()
	{
		if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
		{
			currentControllerType = ControlManager.InputDeviceType.Gamepad;
			ControlManager.instance.controllerFontType = ControlManager.ControllerFontType.XBox;
		}
		else
		{
			currentControllerType = ControlManager.InputDeviceType.Keyboard;
			ControlManager.instance.controllerFontType = ControlManager.ControllerFontType.Keyboard;
		}
	}

	public static async Task<bool> ShowKeybindsClick()
	{
		if (!Options.ModernUI)
		{
			KeyMappingUI.Show();
			return false;
		}
		return await SingletonWindowBase<KeybindsScreen>.instance.KeybindsMenu();
	}

	public async Task<bool> KeybindsMenu()
	{
		searchText = "";
		await The.UiContext;
		getCurrentActiveControl();
		QueryKeybinds();
		SelectFirst = true;
		completionSource?.TrySetCanceled();
		completionSource = new TaskCompletionSource<bool>();
		ControlManager.ResetInput();
		Show();
		bool info = await completionSource.Task;
		DisableNavContext();
		await The.UiContext;
		Hide();
		return info;
	}

	public async void Exit()
	{
		if (bChangesMade)
		{
			switch (await Popup.ShowYesNoCancelAsync("Would you like to save your changes?"))
			{
			case DialogResult.Cancel:
				return;
			case DialogResult.Yes:
				CommandBindingManager.SaveCurrentKeymap();
				bChangesMade = false;
				break;
			case DialogResult.No:
				CommandBindingManager.LoadCurrentKeymap();
				bChangesMade = false;
				break;
			}
		}
		completionSource?.TrySetResult(result: false);
	}

	public void Update()
	{
		if (globalContext.IsActive())
		{
			bool flag = NavigationController.instance.activeContext?.IsInside(keybindsScroller.GetNavigationContext()) ?? false;
			float width = rectTransform.rect.width;
			if (flag != wasInScroller || lastWidth != width)
			{
				wasInScroller = flag;
				lastWidth = width;
				backButton.gameObject.SetActive(!breakBackButton);
				safeArea.offsetMin = new Vector2(breakBackButton ? 5 : 150, safeArea.offsetMin.y);
				safeArea.offsetMax = new Vector2(breakBackButton ? (-150) : (-150), safeArea.offsetMax.y);
				UpdateMenuBars();
			}
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
		globalContext.commandHandlers = new Dictionary<string, Action>
		{
			{
				"CmdFilter",
				XRL.UI.Framework.Event.Helpers.Handle(searchInput.EnterAndOpen)
			},
			{
				BACK_BUTTON.InputCommand,
				XRL.UI.Framework.Event.Helpers.Handle(Exit)
			},
			{
				REMOVE_BIND.InputCommand,
				XRL.UI.Framework.Event.Helpers.Handle(() =>
				{
					HandleMenuOption(REMOVE_BIND);
				})
			},
			{
				RESTORE_DEFAULTS.InputCommand,
				XRL.UI.Framework.Event.Helpers.Handle(() =>
				{
					HandleMenuOption(RESTORE_DEFAULTS);
				})
			}
		};
		midHorizNav.SetAxis(InputAxisTypes.NavigationXAxis);
		midHorizNav.contexts.Clear();
		midHorizNav.contexts.Add(backButton.navigationContext);
		midHorizNav.contexts.Add(categoryScroller.GetNavigationContext());
		midHorizNav.contexts.Add(vertNav);
		vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		vertNav.contexts.Clear();
		vertNav.contexts.Add(searchInput.context);
		searchInput.OnSearchTextChange.RemoveListener(OnSearchTextChange);
		searchInput.OnSearchTextChange.AddListener(OnSearchTextChange);
		if (CapabilityManager.CurrentPlatformClassification() == CapabilityManager.PlatformClassification.PC)
		{
			vertNav.contexts.Add(inputTypeContext.RequireContext<NavigationContext>());
			inputTypeContext.context.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
			{
				InputButtonTypes.AcceptButton,
				XRL.UI.Framework.Event.Helpers.Handle(SelectInputType)
			} };
		}
		vertNav.contexts.Add(keybindsScroller.GetNavigationContext());
		vertNav.contexts.Add(hotkeyBar.GetNavigationContext());
		midHorizNav.Setup();
		midHorizNav.parentContext = globalContext;
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
			Description = "select"
		});
		keyMenuOptions.Add(REMOVE_BIND);
		if (breakBackButton)
		{
			keyMenuOptions.Add(BACK_BUTTON);
		}
		keyMenuOptions.Add(RESTORE_DEFAULTS);
		hotkeyBar.BeforeShow(null, keyMenuOptions);
		hotkeyBar.GetNavigationContext().disabled = false;
		hotkeyBar.onSelected.RemoveAllListeners();
		hotkeyBar.onSelected.AddListener(HandleMenuOption);
		foreach (NavigationContext item in hotkeyBar.scrollContext.contexts.GetRange(0, 3))
		{
			item.disabled = true;
		}
	}

	public async void HandleMenuOption(FrameworkDataElement data)
	{
		if (data == RESTORE_DEFAULTS)
		{
			if (await Popup.ShowYesNoAsync("Are you sure you want to override your bindings with the default?") == DialogResult.Yes)
			{
				bChangesMade = true;
				await CommandBindingManager.RestoreDefaults();
			}
			QueryKeybinds();
			Show();
		}
		else
		{
			if (data != REMOVE_BIND)
			{
				return;
			}
			int selectedPosition = keybindsScroller.scrollContext.selectedPosition;
			if (selectedPosition == -1)
			{
				return;
			}
			FrameworkDataElement frameworkDataElement = keybindsScroller.scrollContext.data[selectedPosition];
			if (!(frameworkDataElement is KeybindDataRow row))
			{
				return;
			}
			ScrollContext<int, KeybindRow.KeybindRowSubContext> rowScroller = keybindsScroller.GetPrefabForIndex(selectedPosition).GetComponent<KeybindRow>().subscroller;
			GameCommand gameCommand = CommandBindingManager.CommandsByID[row.KeyId];
			if (!gameCommand.CanRemoveBinding(currentControllerType))
			{
				await Popup.ShowAsync("Can not remove the last binding for " + Markup.Color("C", gameCommand.DisplayText) + ".");
				return;
			}
			if (await Popup.ShowYesNoAsync("Are you sure you want to clear the binding for {{C|" + row.KeyDescription + "}}?") == DialogResult.Yes)
			{
				bChangesMade = true;
				CommandBindingManager.ReplaceCommandBindingIndex(row.KeyId, rowScroller.selectedPosition, new List<string>(), currentControllerType);
				CommandBindingManager.InitializeInputManager();
			}
			QueryKeybinds();
			Show();
		}
	}

	public void FilterItems()
	{
		filteredItems.Clear();
		if (string.IsNullOrWhiteSpace(searchText))
		{
			searchText = "";
			filteredItems.AddRange(menuItems);
			return;
		}
		searcher.SearchWords = searchText;
		IEnumerable<ExtractedResult<KeybindDataRow>> source = Process.ExtractTop(searcher, menuItems.OfType<KeybindDataRow>(), (KeybindDataRow i) => i.SearchWords.ToLower(), null, menuItems.Count, 50);
		Dictionary<string, KeybindCategoryRow> dictionary = menuItems.OfType<KeybindCategoryRow>().ToDictionary((KeybindCategoryRow i) => i.CategoryId);
		foreach (IGrouping<string, ExtractedResult<KeybindDataRow>> item in from s in source
			group s by s.Value.CategoryId)
		{
			categoryExpanded[item.Key] = true;
			filteredItems.Add(dictionary[item.Key]);
			filteredItems.AddRange(item.Select((ExtractedResult<KeybindDataRow> match) => match.Value));
		}
	}

	public void OnSearchTextChange(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			searchText = "";
		}
		else
		{
			searchText = text;
		}
		FilterItems();
		Show();
	}

	public IEnumerable<FrameworkDataElement> GetMenuItems()
	{
		foreach (FrameworkDataElement filteredItem in filteredItems)
		{
			if (filteredItem is KeybindDataRow keybindDataRow)
			{
				if (categoryExpanded[keybindDataRow.CategoryId])
				{
					yield return filteredItem;
				}
			}
			else if (filteredItem is KeybindCategoryRow keybindCategoryRow)
			{
				keybindCategoryRow.Collapsed = !categoryExpanded[keybindCategoryRow.CategoryId];
				yield return filteredItem;
			}
		}
	}

	public IEnumerable<FrameworkDataElement> GetCategoryItems()
	{
		foreach (FrameworkDataElement filteredItem in filteredItems)
		{
			if (filteredItem is KeybindCategoryRow keybindCategoryRow)
			{
				keybindCategoryRow.Collapsed = !categoryExpanded[keybindCategoryRow.CategoryId];
				yield return filteredItem;
			}
		}
	}

	public override void Show()
	{
		base.Show();
		backButton?.gameObject.SetActive(!breakBackButton);
		safeArea.offsetMin = new Vector2(breakBackButton ? 5 : 150, safeArea.offsetMin.y);
		safeArea.offsetMax = new Vector2(breakBackButton ? (-150) : (-150), safeArea.offsetMax.y);
		if (backButton.navigationContext == null)
		{
			backButton.Awake();
		}
		backButton.navigationContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
		backButton.navigationContext.buttonHandlers.Set(InputButtonTypes.AcceptButton, XRL.UI.Framework.Event.Helpers.Handle(Exit));
		keybindsScroller.scrollContext.wraps = false;
		keybindsScroller.BeforeShow(null, GetMenuItems());
		categoryScroller.scrollContext.wraps = true;
		categoryScroller.BeforeShow(null, GetCategoryItems());
		foreach (KeybindRow item in keybindsScroller.selectionClones.Select((FrameworkUnityScrollChild s) => s.GetComponent<KeybindRow>()))
		{
			item.GetNavigationContext();
			if (item != null)
			{
				item.onRebind.RemoveAllListeners();
				item.onRebind.AddListener(HandleRebind);
			}
		}
		if (SelectFirst)
		{
			SelectFirst = false;
			keybindsScroller.scrollContext.selectedPosition = 1;
		}
		keybindsScroller.onSelected.RemoveAllListeners();
		keybindsScroller.onSelected.AddListener(HandleSelect);
		keybindsScroller.onHighlight.RemoveAllListeners();
		keybindsScroller.onHighlight.AddListener(HandleHighlight);
		categoryScroller.onSelected.RemoveAllListeners();
		categoryScroller.onSelected.AddListener(HandleSelectLeft);
		categoryScroller.onHighlight.RemoveAllListeners();
		categoryScroller.onHighlight.AddListener(HandleHighlightLeft);
		UpdateMenuBars();
		SetupContext();
		EnableNavContext();
		foreach (AbstractScrollContext gridSibling in GetGridSiblings())
		{
			gridSibling.gridSiblings = GetGridSiblings;
		}
	}

	public void HandleSelect(FrameworkDataElement element)
	{
		if (element is KeybindCategoryRow keybindCategoryRow)
		{
			categoryExpanded[keybindCategoryRow.CategoryId] = !categoryExpanded[keybindCategoryRow.CategoryId];
			Show();
			keybindsScroller.scrollContext.SelectIndex(GetMenuItems().ToList().FindIndex((FrameworkDataElement s) => s == element));
		}
		foreach (AbstractScrollContext gridSibling in GetGridSiblings())
		{
			gridSibling.gridSiblings = GetGridSiblings;
		}
	}

	public void HandleHighlight(FrameworkDataElement element)
	{
		lastSelectedElement = element;
		string catId = null;
		if (element is KeybindCategoryRow keybindCategoryRow)
		{
			catId = keybindCategoryRow.CategoryId;
		}
		else if (element is KeybindDataRow keybindDataRow)
		{
			catId = keybindDataRow.CategoryId;
		}
		if (catId != null)
		{
			FrameworkScroller frameworkScroller = categoryScroller;
			int selectedPosition = (categoryScroller.scrollContext.selectedPosition = GetCategoryItems().ToList().FindIndex((FrameworkDataElement s) => (s as KeybindCategoryRow)?.CategoryId == catId));
			frameworkScroller.selectedPosition = selectedPosition;
		}
	}

	public void HandleSelectLeft(FrameworkDataElement element)
	{
		KeybindCategoryRow cat = element as KeybindCategoryRow;
		if (cat != null)
		{
			categoryExpanded[cat.CategoryId] = true;
			Show();
			int num = GetMenuItems().ToList().FindIndex((FrameworkDataElement s) => (s as KeybindCategoryRow)?.CategoryId == cat.CategoryId);
			keybindsScroller.scrollContext.GetContextAt(num).Activate();
			keybindsScroller.ScrollSelectedIntoView();
			ScrollViewCalcs.ScrollToTopOfRect(keybindsScroller.GetPrefabForIndex(num).GetComponent<RectTransform>());
		}
	}

	public void HandleHighlightLeft(FrameworkDataElement element)
	{
		KeybindCategoryRow cat = element as KeybindCategoryRow;
		if (cat == null)
		{
			return;
		}
		FrameworkDataElement frameworkDataElement = GetMenuItems().Skip(keybindsScroller.selectedPosition).FirstOrDefault();
		string text;
		if (frameworkDataElement is KeybindCategoryRow keybindCategoryRow)
		{
			text = keybindCategoryRow.CategoryId;
		}
		else
		{
			text = ((frameworkDataElement is KeybindDataRow keybindDataRow) ? keybindDataRow.CategoryId : null);
		}
		if (text != cat.CategoryId)
		{
			int num = GetMenuItems().ToList().FindIndex((FrameworkDataElement s) => (s as KeybindCategoryRow)?.CategoryId == cat.CategoryId);
			VisibleWindowScroller visibleWindowScroller = keybindsScroller;
			int selectedPosition = (keybindsScroller.scrollContext.selectedPosition = num);
			visibleWindowScroller.selectedPosition = selectedPosition;
			keybindsScroller.ScrollSelectedIntoView();
			ScrollViewCalcs.ScrollToTopOfRect(keybindsScroller.GetPrefabForIndex(num).GetComponent<RectTransform>());
		}
	}

	public override void Hide()
	{
		base.Hide();
		DisableNavContext();
		gameObject.SetActive(value: false);
		ControlManager.ResetInput();
	}

	public void EnableNavContext()
	{
		globalContext.disabled = false;
		keybindsScroller.GetNavigationContext().ActivateAndEnable();
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

	public async void HandleRebind(KeybindDataRow binding, int bindIndex, KeybindRow unityRow)
	{
		KeybindBox box = bindIndex switch
		{
			0 => unityRow.box1, 
			1 => unityRow.box2, 
			2 => unityRow.box3, 
			3 => unityRow.box4, 
			_ => unityRow.box1, 
		};
		box.editMode = true;
		box.Update();
		await NavigationController.instance.SuspendContextWhile(async () =>
		{
			await HandleRebindAsync(binding, bindIndex);
			return true;
		});
		GameManager.Instance.SetActiveLayersForNavCategory("Keybind");
		box.editMode = false;
		QueryKeybinds();
		Show();
	}

	private async Task HandleRebindAsync(KeybindDataRow binding, int bindIndex)
	{
		bChangesMade |= await HandleRebindAsync(CommandBindingManager.CommandsByID[binding.KeyId], bindIndex, currentControllerType);
	}

	public static async Task<bool> HandleRebindAsync(GameCommand command, int bindIndex, ControlManager.InputDeviceType deviceType)
	{
		try
		{
			await The.UiContext;
			bool allowGamepadAlt = deviceType == ControlManager.InputDeviceType.Gamepad && command.ID != "GamepadAlt";
			List<string> rebind = await ControlManager.SuspendControlsWhile(() => CommandBindingManager.GetRebindAsync(deviceType, allowGamepadAlt, command.Type));
			if (rebind == null)
			{
				return false;
			}
			if (CommandBindingManager.CommandUsesBinding(command, rebind))
			{
				return false;
			}
			List<GameCommand> conflicts = CommandBindingManager.GetCommandsWithBinding(rebind, CommandBindingManager.ConflictChecker(command)).ToList();
			string bindingDisplayString = CommandBindingManager.GetBindingDisplayString(rebind);
			GameCommand gameCommand = conflicts.Find((GameCommand gameCommand2) => !gameCommand2.CanRemoveBinding(deviceType));
			if (gameCommand != null)
			{
				await RequiredConflictBind(bindingDisplayString, gameCommand.DisplayText);
				return false;
			}
			if (conflicts.Count > 0)
			{
				if (!(await ConfirmConflictBind(bindingDisplayString, conflicts, command.DisplayText)))
				{
					return false;
				}
				foreach (GameCommand item in conflicts)
				{
					CommandBindingManager.RemoveCommandBinding(item.ID, rebind);
				}
			}
			CommandBindingManager.ReplaceCommandBindingIndex(command.ID, bindIndex, rebind, deviceType);
			CommandBindingManager.InitializeInputManager(AllowLegacyUpgrade: false, restoreLayers: true);
			await Task.Yield();
			MetricsManager.LogKeybinding(command);
			while (CommandBindingManager.CommandBindings[command.ID].IsPressed())
			{
				await Task.Yield();
			}
			ControlManager.ResetInput();
			return true;
		}
		catch (Exception x)
		{
			MetricsManager.LogError("HandleRebind", x);
			return false;
		}
	}

	public IEnumerable<AbstractScrollContext> GetGridSiblings()
	{
		foreach (ScrollChildContext context in keybindsScroller.scrollContext.contexts)
		{
			if (context.proxyTo is AbstractScrollContext abstractScrollContext)
			{
				yield return abstractScrollContext;
			}
		}
	}

	public async void SelectInputType()
	{
		int num = await Popup.PickOptionAsync("Select Controller", null, "", ControlTypeDisplayName.Values.ToArray(), null, null, null, null, 0, 60, 0, -1, RespectOptionNewlines: false, AllowEscape: true);
		if (num >= 0)
		{
			currentControllerType = ControlTypeDisplayName.Keys.ElementAt(num);
			QueryKeybinds();
			Show();
		}
	}

	public static async Task<bool> ConfirmConflictBind(string KeyDescription, List<GameCommand> Conflicts, string NewCommand)
	{
		int index = -1;
		string text = Conflicts.Aggregate("", (string carry, GameCommand command) =>
		{
			index++;
			if (index == 0)
			{
				return Markup.Color("C", command.DisplayText);
			}
			return (index == Conflicts.Count - 1) ? (carry + " and " + Markup.Color("C", command.DisplayText)) : (carry + ", " + Markup.Color("C", command.DisplayText));
		});
		return await Popup.ShowYesNoAsync(Markup.Color("W", KeyDescription) + " is already bound to " + text + ".\r\n\r\nDo you want to bind it to " + Markup.Color("C", NewCommand) + " instead?") == DialogResult.Yes;
	}

	public static async Task<bool> ConfirmDynamicConflictBind(string KeyDescription, List<GameCommand> Conflicts, string NewCommand)
	{
		int index = -1;
		string text = Conflicts.Aggregate("", (string carry, GameCommand command) =>
		{
			index++;
			if (index == 0)
			{
				return Markup.Color("C", command.DisplayText);
			}
			return (index == Conflicts.Count - 1) ? (carry + " and " + Markup.Color("C", command.DisplayText)) : (carry + ", " + Markup.Color("C", command.DisplayText));
		});
		return await Popup.ShowYesNoAsync(Markup.Color("W", KeyDescription) + " is already bound to " + text + ".\r\n\r\nDo you want to bind it to " + Markup.Color("C", NewCommand) + " anyway?") == DialogResult.Yes;
	}

	public static async Task RequiredConflictBind(string KeyDescription, string CommandName)
	{
		await Popup.ShowAsync(Markup.Color("W", KeyDescription) + " is already bound to " + Markup.Color("C", CommandName) + ".  This is a required bind and can't be removed.\r\n\r\nChoose a new bind for " + Markup.Color("C", CommandName) + " first, and then rebind " + Markup.Color("W", KeyDescription) + ".");
	}

	public void QueryKeybinds()
	{
		menuItems.Clear();
		bool flag = currentControllerType == ControlManager.InputDeviceType.Keyboard;
		if (flag)
		{
			inputTypeText.SetText("{{C|Configuring Controller:}} {{c|Keyboard && Mouse}}");
		}
		else
		{
			inputTypeText.SetText("{{C|Configuring Controller:}} {{c|" + (currentGamepad?.name ?? "<no controller detected>") + "}}");
		}
		foreach (string item in CommandBindingManager.CategoriesInOrder)
		{
			if (Application.isConsolePlatform && item.Contains("mouse", StringComparison.InvariantCultureIgnoreCase))
			{
				continue;
			}
			menuItems.Add(new KeybindCategoryRow
			{
				CategoryId = item,
				CategoryDescription = item
			});
			if (!categoryExpanded.ContainsKey(item))
			{
				categoryExpanded[item] = true;
			}
			foreach (GameCommand item2 in CommandBindingManager.CommandsByCategory[item])
			{
				if (!((item2.Type != InputActionType.Button || item2.ID == "GamepadAlt") & flag))
				{
					CommandBindingManager.GetCommandBindings(item2.ID, currentControllerType, out var bind, out var bind2, out var bind3, out var bind4);
					menuItems.Add(new KeybindDataRow
					{
						CategoryId = item2.Category,
						KeyId = item2.ID,
						KeyDescription = item2.DisplayText,
						SearchWords = item + " " + item2.DisplayText,
						Bind1 = bind,
						Bind2 = bind2,
						Bind3 = bind3,
						Bind4 = bind4
					});
				}
			}
		}
		FilterItems();
	}
}
