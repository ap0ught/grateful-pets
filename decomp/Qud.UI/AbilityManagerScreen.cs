using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleLib.Console;
using FuzzySharp;
using FuzzySharp.Extractor;
using UnityEngine;
using XRL;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Parts;

namespace Qud.UI;

[UIView("AbilityManagerScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "AbilityManagerScreen", UICanvasHost = 1)]
public class AbilityManagerScreen : SingletonWindowBase<AbilityManagerScreen>, ControlManager.IControllerChangedEvent
{
	public class Context : NavigationContext
	{
	}

	public struct Result
	{
		public ActivatedAbilityEntry ability;
	}

	public enum SortMode
	{
		Custom,
		Class
	}

	protected TaskCompletionSource<Result> menucomplete = new TaskCompletionSource<Result>();

	public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

	public Context navigationContext = new Context
	{
		buttonHandlers = null
	};

	public FrameworkScroller hotkeyBar;

	public FrameworkScroller leftSideScroller;

	protected ActivatedAbilities activatedAbilities;

	public List<AbilityManagerLineData> leftSideItems = new List<AbilityManagerLineData>();

	public List<AbilityManagerLineData> filteredItems = new List<AbilityManagerLineData>();

	public string searchText = "";

	public UITextSkin rightSideHeaderText;

	public UITextSkin rightSideDescriptionArea;

	public UIThreeColorProperties rightSideIcon;

	public bool RealityWeak;

	public Dictionary<string, bool> classCollapsed = new Dictionary<string, bool>();

	public SortMode sortMode;

	public static readonly MenuOption TOGGLE_SORT = new MenuOption
	{
		Id = "TOGGLE_SORT",
		KeyDescription = "Toggle Sort",
		InputCommand = "Toggle",
		Description = "toggle sort"
	};

	public static readonly MenuOption FILTER_ITEMS = new MenuOption
	{
		Id = "FILTER_ITEMS",
		InputCommand = "CmdFilter",
		Description = "search"
	};

	public static List<MenuOption> defaultMenuOptions = new List<MenuOption>
	{
		new MenuOption
		{
			Id = "Cancel",
			InputCommand = "Cancel",
			Description = "Close Menu"
		},
		new MenuOption
		{
			InputCommand = "NavigationXYAxis",
			Description = "navigate",
			disabled = true
		},
		TOGGLE_SORT,
		new MenuOption
		{
			InputCommand = "Accept",
			Description = "Activate Selected Ability",
			disabled = true
		},
		FILTER_ITEMS
	};

	private readonly AbilityManagerLineData searcher = new AbilityManagerLineData();

	private NavigationContext lastContext;

	private static List<string> hideWhenShown = new List<string> { "PickDirection", "PickTarget" };

	private int skipFrame = 2;

	private bool PickHide;

	public static string sortModeDescription => "sort: " + Markup.Color((SingletonWindowBase<AbilityManagerScreen>.instance.sortMode == SortMode.Custom) ? "w" : "y", "custom") + "/" + Markup.Color((SingletonWindowBase<AbilityManagerScreen>.instance.sortMode == SortMode.Class) ? "w" : "y", "by class");

	public void SetupContext()
	{
		vertNav.parentContext = navigationContext;
		vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		vertNav.contexts.Add(leftSideScroller.scrollContext);
		Context context = navigationContext;
		if (context.buttonHandlers == null)
		{
			context.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
			{
				InputButtonTypes.CancelButton,
				XRL.UI.Framework.Event.Helpers.Handle(Cancel)
			} };
		}
		context = navigationContext;
		if (context.commandHandlers == null)
		{
			context.commandHandlers = new Dictionary<string, Action>
			{
				{
					TOGGLE_SORT.InputCommand,
					XRL.UI.Framework.Event.Helpers.Handle(() =>
					{
						HandleMenuOption(TOGGLE_SORT);
					})
				},
				{
					FILTER_ITEMS.InputCommand,
					XRL.UI.Framework.Event.Helpers.Handle(() =>
					{
						HandleMenuOption(FILTER_ITEMS);
					})
				}
			};
		}
		TOGGLE_SORT.Description = sortModeDescription;
		vertNav.wraps = true;
		vertNav.Setup();
		leftSideScroller.scrollContext.parentContext = vertNav;
	}

	protected async Task<Result> showScreen(XRL.World.GameObject GO)
	{
		skipFrame = 2;
		The.Player.IsRealityDistortionUsable();
		AbilityManagerLine.dragging = false;
		menucomplete.TrySetCanceled();
		menucomplete = new TaskCompletionSource<Result>();
		activatedAbilities = GO.ActivatedAbilities;
		if (activatedAbilities == null)
		{
			MetricsManager.LogWarning("Opening ability manager with no activated abilities part");
			Hide();
			await Popup.ShowAsync("You have no activated abilities.");
			return default;
		}
		GameManager.Instance.PushGameView("AbilityManagerScreen");
		try
		{
			MinEvent.UIHold = true;
			await The.UiContext;
			searchText = "";
			BeforeShow();
			if (leftSideItems.Count == 0)
			{
				Hide();
				await Popup.ShowAsync("You have no activated abilities.");
				return default;
			}
			Show();
			leftSideScroller.scrollContext.ActivateAndEnable();
			return await menucomplete.Task;
		}
		finally
		{
			GameManager.Instance.PopGameView(bHard: true);
			MinEvent.UIHold = false;
		}
	}

	public static Task<Result> OpenAbilityManager(XRL.World.GameObject GO)
	{
		return NavigationController.instance.SuspendContextWhile(() => SingletonWindowBase<AbilityManagerScreen>.instance.showScreen(GO));
	}

	public bool isCollapsed(string key)
	{
		if (!classCollapsed.TryGetValue(key, out var value))
		{
			return false;
		}
		return value;
	}

	public void BeforeShow()
	{
		AbilityManagerLine.dragging = false;
		SetupContext();
		List<UnityEngine.KeyCode> hotkeySpread = ControlManager.GetHotkeySpread(new List<string> { "Menus", "UINav" });
		RealityWeak = The.Player.IsRealityDistortionUsable();
		if (sortMode == SortMode.Class)
		{
			leftSideItems.Clear();
			AbilityManager.PlayerAbilityLock.EnterReadLock();
			try
			{
				foreach (KeyValuePair<string, List<ActivatedAbilityEntry>> item in AbilityManager.PlayerAbilitiesByClass)
				{
					AbilityManagerLineData abilityManagerLineData = new AbilityManagerLineData
					{
						Id = "Category " + item.Key,
						category = item.Key,
						collapsed = isCollapsed(item.Key)
					};
					leftSideItems.Add(abilityManagerLineData);
					if (abilityManagerLineData.collapsed)
					{
						continue;
					}
					foreach (ActivatedAbilityEntry item2 in item.Value)
					{
						using IEnumerator<string> enumerator3 = CommandBindingManager.GetCommandBindings(item2.Command).GetEnumerator();
						string hotkeyDescription = null;
						if (enumerator3.MoveNext())
						{
							hotkeyDescription = enumerator3.Current;
						}
						leftSideItems.Add(new AbilityManagerLineData
						{
							Id = item2.ID.ToString(),
							ability = item2,
							searchText = item2.DisplayName + " " + item2.Description,
							realityIsWeak = RealityWeak,
							hotkeyDescription = hotkeyDescription
						});
					}
				}
			}
			finally
			{
				AbilityManager.PlayerAbilityLock.ExitReadLock();
			}
		}
		else
		{
			leftSideItems = activatedAbilities.GetAbilityListOrderedByPreference().Select((ActivatedAbilityEntry abil, int i) =>
			{
				using IEnumerator<string> enumerator5 = CommandBindingManager.GetCommandBindings(abil.Command).GetEnumerator();
				string hotkeyDescription2 = null;
				if (enumerator5.MoveNext())
				{
					hotkeyDescription2 = enumerator5.Current;
				}
				return new AbilityManagerLineData
				{
					Id = abil.ID.ToString(),
					ability = abil,
					searchText = abil.DisplayName + " " + abil.Description,
					realityIsWeak = RealityWeak,
					hotkeyDescription = hotkeyDescription2
				};
			}).ToList();
		}
		FilterItems();
		int num = 0;
		int num2 = 0;
		for (; num < filteredItems.Count; num++)
		{
			if (filteredItems[num].ability != null)
			{
				filteredItems[num].quickKey = getHotkeyChar(num2++);
			}
		}
		leftSideScroller.BeforeShow(filteredItems);
		leftSideScroller.onSelected.RemoveAllListeners();
		leftSideScroller.onSelected.AddListener(HandleSelectLeft);
		leftSideScroller.onHighlight.RemoveAllListeners();
		leftSideScroller.onHighlight.AddListener(HandleHighlightLeft);
		leftSideScroller.scrollContext.wraps = false;
		foreach (FrameworkUnityScrollChild selectionClone in leftSideScroller.selectionClones)
		{
			selectionClone.GetComponent<AbilityManagerLine>().screen = this;
		}
		UpdateMenuBars();
		char getHotkeyChar(int n)
		{
			if (hotkeySpread.Count <= n)
			{
				return '\0';
			}
			return Keyboard.ConvertKeycodeToLowercaseChar(hotkeySpread[n]);
		}
	}

	public void Refresh(bool ForceActivate = true)
	{
		if (leftSideScroller.selectedPosition < 0 || leftSideScroller.selectedPosition >= leftSideScroller.choices.Count)
		{
			BeforeShow();
			return;
		}
		FrameworkDataElement currentSelected = leftSideScroller.choices[leftSideScroller.selectedPosition];
		bool flag = ForceActivate || leftSideScroller.scrollContext.GetContextAt(leftSideScroller.selectedPosition).IsActive();
		BeforeShow();
		int num = leftSideScroller.choices.FindIndex((FrameworkDataElement item) => item.Id == currentSelected.Id);
		if (num != -1)
		{
			leftSideScroller.selectedPosition = num;
			if (flag)
			{
				NavigationContext navigationContext;
				for (navigationContext = leftSideScroller.scrollContext.GetContextAt(num); navigationContext is ProxyNavigationContext { proxyTo: not null } proxyNavigationContext; navigationContext = proxyNavigationContext.proxyTo)
				{
				}
				navigationContext.Activate();
				UpdateMenuBars();
			}
		}
		else if (ForceActivate)
		{
			leftSideScroller.selectedPosition = 0;
			leftSideScroller.scrollContext.GetContextAt(0).Activate();
			UpdateMenuBars();
		}
	}

	public void UpdateMenuBars()
	{
		hotkeyBar.BeforeShow(null, NavigationController.GetMenuOptions(defaultMenuOptions));
		hotkeyBar.GetNavigationContext().disabled = true;
		hotkeyBar.onSelected.RemoveAllListeners();
		hotkeyBar.onSelected.AddListener(HandleMenuOption);
	}

	public void HandleMenuOption(FrameworkDataElement element)
	{
		if (element.Id == "Cancel")
		{
			Cancel();
		}
		if (element == TOGGLE_SORT)
		{
			if (sortMode == SortMode.Custom)
			{
				sortMode = SortMode.Class;
			}
			else
			{
				sortMode = SortMode.Custom;
			}
			Refresh();
		}
		if (element == FILTER_ITEMS)
		{
			HandleFilterItems();
		}
	}

	public async void HandleFilterItems()
	{
		string text = await Popup.AskStringAsync("Search text:", searchText, 80, 0, null, ReturnNullForEscape: true);
		if (text != null && text != null)
		{
			searchText = text;
			FilterItems();
			if (filteredItems.Count == 0)
			{
				await Popup.ShowAsync("No activated abilites found for '" + searchText + "'");
				searchText = "";
				FilterItems();
			}
			Refresh();
		}
	}

	public void FilterItems()
	{
		filteredItems.Clear();
		if (string.IsNullOrWhiteSpace(searchText))
		{
			FILTER_ITEMS.Description = "search";
			filteredItems.AddRange(leftSideItems);
			return;
		}
		FILTER_ITEMS.Description = "search: " + Markup.Color("w", searchText);
		searcher.searchText = searchText;
		HashSet<AbilityManagerLineData> hashSet = (from item in Process.ExtractTop(searcher, leftSideItems, (AbilityManagerLineData i) => i.searchText.ToLower(), null, leftSideItems.Count, 50)
			select item.Value).ToHashSet();
		foreach (AbilityManagerLineData leftSideItem in leftSideItems)
		{
			if (hashSet.Contains(leftSideItem))
			{
				filteredItems.Add(leftSideItem);
			}
		}
	}

	public override void Hide()
	{
		CursorManager.setStyle(CursorManager.Style.Pointer);
		base.Hide();
	}

	public void Cancel()
	{
		if (!AbilityManagerLine.dragging)
		{
			ControlManager.ResetInput();
			Hide();
			menucomplete.TrySetResult(default);
		}
	}

	void ControlManager.IControllerChangedEvent.ControllerChanged()
	{
	}

	public void HandleSelectLeft(FrameworkDataElement element)
	{
		if (!AbilityManagerLine.dragging && element is AbilityManagerLineData abilityManagerLineData)
		{
			if (abilityManagerLineData.category != null)
			{
				classCollapsed[abilityManagerLineData.category] = !isCollapsed(abilityManagerLineData.category);
				BeforeShow();
			}
			else if (!string.IsNullOrEmpty(abilityManagerLineData.ability.NotUsableDescription))
			{
				Popup.Show(abilityManagerLineData.ability.NotUsableDescription);
			}
			else
			{
				menucomplete.TrySetResult(new Result
				{
					ability = abilityManagerLineData.ability
				});
			}
		}
	}

	public void HandleHighlightLeft(FrameworkDataElement element)
	{
		if (!(element is AbilityManagerLineData abilityManagerLineData))
		{
			return;
		}
		if (abilityManagerLineData.ability != null)
		{
			rightSideIcon.FromRenderable(abilityManagerLineData.ability.GetUITile());
			rightSideHeaderText.SetText(abilityManagerLineData.ability.DisplayName);
			StringBuilder stringBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(abilityManagerLineData.ability.Class))
			{
				stringBuilder.AppendColored("y", "Type: ");
				stringBuilder.Append(abilityManagerLineData.ability.Class);
				stringBuilder.Append("\n");
			}
			if (abilityManagerLineData.ability.Cooldown > 0)
			{
				stringBuilder.AppendColored("C", "Cooldown Remaining Turns: ");
				stringBuilder.Append(abilityManagerLineData.ability.CooldownRounds);
				stringBuilder.Append("\n");
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append("\n");
			}
			stringBuilder.Append(abilityManagerLineData.ability.Description);
			rightSideDescriptionArea.SetText(stringBuilder.ToString());
		}
		else
		{
			rightSideIcon.FromRenderable(null);
			rightSideHeaderText.SetText(abilityManagerLineData.category);
			rightSideDescriptionArea.SetText("");
		}
	}

	public void Update()
	{
		if (!PickHide && hideWhenShown.Contains(GameManager.Instance.CurrentGameView))
		{
			if (canvas.enabled)
			{
				canvas.enabled = false;
			}
			PickHide = true;
		}
		else if (PickHide && GameManager.Instance.CurrentGameView == "AbilityManagerScreen")
		{
			if (!canvas.enabled)
			{
				canvas.enabled = true;
			}
			PickHide = false;
		}
		if (isCurrentWindow() && navigationContext.IsActive() && skipFrame <= 0)
		{
			if (filteredItems != null)
			{
				foreach (AbilityManagerLineData filteredItem in filteredItems)
				{
					if (ControlManager.isCharDown(filteredItem.quickKey))
					{
						ControlManager.ConsumeCurrentInput();
						HandleSelectLeft(filteredItem);
					}
				}
			}
			if (lastContext != NavigationController.instance.activeContext)
			{
				lastContext = NavigationController.instance.activeContext;
				UpdateMenuBars();
			}
		}
		if (skipFrame > 0)
		{
			skipFrame--;
		}
	}

	public void MoveItem(AbilityManagerLineData data, int direction)
	{
		if (AbilityManagerLine.dragging || sortMode != SortMode.Custom)
		{
			return;
		}
		int num = leftSideScroller.choices.IndexOf(data);
		if (num == -1)
		{
			return;
		}
		int num2 = num + direction;
		if (num2 >= 0 && num2 < leftSideScroller.choices.Count)
		{
			List<string> list = (from item in leftSideScroller.choices
				where item != data
				select (!(item is AbilityManagerLineData abilityManagerLineData)) ? null : abilityManagerLineData.ability.Command).ToList();
			list.Insert(num2, data.ability.Command);
			ActivatedAbilities.PreferenceOrder = list;
			BeforeShow();
			leftSideScroller.scrollContext.contexts[num2].Activate();
		}
	}

	public void SerializeUnityOrdering()
	{
		if (sortMode == SortMode.Custom)
		{
			ActivatedAbilities.PreferenceOrder = CurrentOrdering();
		}
		IEnumerable<string> CurrentOrdering()
		{
			for (int x = 0; x < leftSideScroller.childRoot.transform.childCount; x++)
			{
				Transform element = leftSideScroller.childRoot.transform.GetChild(x);
				int num = leftSideScroller.selectionClones.FindIndex((FrameworkUnityScrollChild i) => i.transform == element);
				if (num != -1 && leftSideScroller.choices[num] is AbilityManagerLineData abilityManagerLineData)
				{
					yield return abilityManagerLineData.ability.Command;
				}
			}
		}
	}

	public IEnumerator ShowNextCycle()
	{
		yield return 0;
		yield return 0;
		BeforeShow();
	}

	public static async Task HandleRebindAsync(ActivatedAbilityEntry ability, string layerToResetTo = null)
	{
		await The.UiContext;
		try
		{
			ControlManager.InputDeviceType currentControllerType = ControlManager.InputDeviceType.Keyboard;
			using CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
			Task popupTask = Popup.ShowKeybindAsync("Press the keyboard key to bind to {{w|" + ability.DisplayName + "}}", cancelTokenSource.Token);
			List<string> rebind = await ControlManager.SuspendControlsWhile(() => CommandBindingManager.GetRebindAsync(currentControllerType, AllowGamepadAlt: false));
			cancelTokenSource.Cancel();
			await popupTask;
			if (rebind == null || CommandBindingManager.CommandUsesBinding(ability.Command, rebind))
			{
				return;
			}
			List<GameCommand> list = CommandBindingManager.GetCommandsWithBinding(rebind, CommandBindingManager.DynamicLayerConflictChecker()).ToList();
			IEnumerable<GameCommand> commandsWithBinding = CommandBindingManager.GetCommandsWithBinding(rebind);
			if (commandsWithBinding.Any((GameCommand b) => b.ID == "CmdSystemMenu"))
			{
				await Popup.ShowAsync(CommandBindingManager.GetBindingDisplayString(rebind) + " is already bound to the system menu.");
				return;
			}
			if (commandsWithBinding.Any((GameCommand b) => b.ID == "CmdAbilities"))
			{
				await Popup.ShowAsync(CommandBindingManager.GetBindingDisplayString(rebind) + " is already bound to the ability picker.");
				return;
			}
			GameCommand gameCommand = list.Find((GameCommand gameCommand2) => !gameCommand2.CanRemoveBinding(currentControllerType));
			if (gameCommand != null)
			{
				await KeybindsScreen.RequiredConflictBind(CommandBindingManager.GetBindingDisplayString(rebind), gameCommand.DisplayText);
				return;
			}
			if (list.Count > 0 && !(await KeybindsScreen.ConfirmDynamicConflictBind(CommandBindingManager.GetBindingDisplayString(rebind), list, ability.DisplayName)))
			{
				return;
			}
			AbilityManager.PlayerAbilityLock.EnterReadLock();
			try
			{
				foreach (ActivatedAbilityEntry playerAbility in AbilityManager.PlayerAbilities)
				{
					string command = playerAbility.Command;
					if (command != null && command != ability.Command && CommandBindingManager.CommandUsesBinding(command, rebind))
					{
						CommandBindingManager.ReplaceCommandBindingIndex(command, 0, new List<string>(), currentControllerType);
					}
				}
			}
			finally
			{
				AbilityManager.PlayerAbilityLock.ExitReadLock();
			}
			CommandBindingManager.ReplaceCommandBindingIndex(ability.Command, 0, rebind, currentControllerType);
			CommandBindingManager.InitializeInputManager();
			CommandBindingManager.SaveCurrentKeymap();
			if (layerToResetTo != null)
			{
				GameManager.Instance.SetActiveLayersForNavCategory("Menu");
			}
			await Task.Yield();
			MetricsManager.LogKeybinding(ability.Command);
			ControlManager.ResetInput();
			AbilityBar.markDirty();
		}
		catch (Exception x)
		{
			MetricsManager.LogError("HandleRebind", x);
		}
		finally
		{
			_ = 0;
		}
	}

	public static async Task<bool> HandleRemoveBindAsync(ActivatedAbilityEntry abilityEntry)
	{
		await The.UiContext;
		return await NavigationController.instance.SuspendContextWhile(async () =>
		{
			if (await Popup.ShowYesNoAsync("Are you sure you wish to remove the binding for " + abilityEntry.DisplayName + "?") == DialogResult.Yes)
			{
				CommandBindingManager.RemoveCommandBinding(abilityEntry.Command, 0);
				CommandBindingManager.InitializeInputManager(AllowLegacyUpgrade: false, restoreLayers: true);
				CommandBindingManager.SaveCurrentKeymap();
				AbilityBar.markDirty();
				return true;
			}
			return false;
		});
	}
}
