using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qud.API;
using UnityEngine.UI;
using XRL;
using XRL.CharacterBuilds.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[UIView("SaveManagement", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "SaveManagement", UICanvasHost = 1)]
public class SaveManagement : SingletonWindowBase<SaveManagement>, ControlManager.IControllerChangedEvent
{
	protected List<SaveInfoData> saves;

	public Image background;

	public FrameworkScroller hotkeyBar;

	public FrameworkScroller savesScroller;

	public EmbarkBuilderModuleBackButton backButton;

	public TaskCompletionSource<SaveGameInfo> completionSource;

	public NavigationContext navigationContext = new NavigationContext();

	public ScrollContext<NavigationContext> midHorizNav = new ScrollContext<NavigationContext>();

	private bool SelectFirst = true;

	public bool wasInScroller;

	public void SetupContext()
	{
		navigationContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
		navigationContext.buttonHandlers.Set(InputButtonTypes.CancelButton, Event.Helpers.Handle(Exit));
		midHorizNav.SetAxis(InputAxisTypes.NavigationXAxis);
		midHorizNav.contexts.Clear();
		midHorizNav.contexts.Add(backButton.navigationContext);
		midHorizNav.contexts.Add(savesScroller.GetNavigationContext());
		midHorizNav.Setup();
		midHorizNav.parentContext = navigationContext;
	}

	public override void Show()
	{
		Task<List<SaveGameInfo>> savedGameInfo = SavesAPI.GetSavedGameInfo();
		Task.WaitAll(savedGameInfo);
		List<SaveGameInfo> result = savedGameInfo.Result;
		List<SaveInfoData> selections = (saves = SaveGameInfosToUIElements(result));
		if (result.Count == 0)
		{
			completionSource.TrySetResult(null);
			Exit();
			return;
		}
		base.Show();
		backButton?.gameObject.SetActive(value: true);
		if (backButton.navigationContext == null)
		{
			backButton.Awake();
		}
		backButton.navigationContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
		backButton.navigationContext.buttonHandlers.Set(InputButtonTypes.AcceptButton, Event.Helpers.Handle(Exit));
		savesScroller.scrollContext.wraps = true;
		savesScroller.BeforeShow(null, selections);
		foreach (SaveManagementRow item in savesScroller.selectionClones.Select((FrameworkUnityScrollChild s) => s.GetComponent<SaveManagementRow>()))
		{
			if (item != null)
			{
				item.deleteButton.context.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
				{
					InputButtonTypes.AcceptButton,
					Event.Helpers.Handle(HandleDelete)
				} };
				item.context.context.commandHandlers = new Dictionary<string, Action> { 
				{
					"CmdDelete",
					Event.Helpers.Handle(HandleDelete)
				} };
			}
		}
		if (SelectFirst)
		{
			SelectFirst = false;
			savesScroller.scrollContext.selectedPosition = 0;
		}
		else if (savesScroller.scrollContext.selectedPosition >= result.Count)
		{
			savesScroller.scrollContext.selectedPosition = Math.Max(result.Count - 1, 0);
		}
		savesScroller.onSelected.RemoveAllListeners();
		savesScroller.onSelected.AddListener(SelectedInfo);
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
		navigationContext.disabled = false;
		savesScroller.GetNavigationContext().ActivateAndEnable();
	}

	public void DisableNavContext(bool deactivate = true)
	{
		if (deactivate)
		{
			NavigationContext activeContext = NavigationController.instance.activeContext;
			if (activeContext != null && activeContext.IsInside(navigationContext))
			{
				NavigationController.instance.activeContext = null;
			}
		}
		navigationContext.disabled = true;
	}

	public void SelectedInfo(FrameworkDataElement data)
	{
		if (data is SaveInfoData saveInfoData)
		{
			completionSource?.TrySetResult(saveInfoData.SaveGame);
		}
	}

	public async Task<XRLGame> ContinueMenu()
	{
		SelectFirst = true;
		while (true)
		{
			completionSource?.TrySetCanceled();
			completionSource = new TaskCompletionSource<SaveGameInfo>();
			await The.UiContext;
			ControlManager.ResetInput();
			Show();
			SaveGameInfo info = await completionSource.Task;
			DisableNavContext();
			await The.UiContext;
			if (info == null)
			{
				break;
			}
			try
			{
				SaveGameJSON json = info.json;
				if (json != null && json.SaveVersion < 395)
				{
					await Popup.ShowAsync("That save file looks like it's from an older save format revision (" + info.json.GameVersion + "). Sorry!\n\nYou can probably change to a previous branch in your game client and get it to load if you want to finish it off.");
				}
				else if (await info.TryRestoreModsAndLoadAsync())
				{
					Hide();
					return The.Game;
				}
			}
			catch (Exception)
			{
			}
		}
		Hide();
		return null;
	}

	public void Exit()
	{
		completionSource?.TrySetResult(null);
		ControlManager.ResetInput();
	}

	public async void HandleDelete()
	{
		if (!NavigationController.instance.activeContext.IsInside(savesScroller.GetNavigationContext()))
		{
			return;
		}
		SaveInfoData saveInfo = saves[savesScroller.selectedPosition];
		List<QudMenuItem> buttons = ((CapabilityManager.CurrentPlatformClassification() != CapabilityManager.PlatformClassification.PC) ? PopupMessage.AcceptCancelButtonWithoutHotkey : PopupMessage.AcceptCancelButton);
		string title = "{{R|Delete " + saveInfo.SaveGame.Name + "}}";
		if ((await Popup.NewPopupMessageAsync("Are you sure you want to delete the save game for " + saveInfo.SaveGame.Name + "?", buttons, null, title, null, 1)).command != "Cancel")
		{
			DisableNavContext();
			saveInfo.SaveGame.Delete();
			EnableNavContext();
			await Popup.NewPopupMessageAsync("Game Deleted!", PopupMessage.AcceptButton);
			saves = SaveGameInfosToUIElements(await SavesAPI.GetSavedGameInfo());
			if (saves.Count == 0)
			{
				Exit();
			}
			else
			{
				Show();
			}
		}
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
			KeyDescription = ControlManager.getCommandInputDescription("Accept"),
			Description = "select"
		});
		list.Add(new MenuOption
		{
			KeyDescription = ControlManager.getCommandInputDescription("CmdDelete"),
			Description = "delete"
		});
		hotkeyBar.GetNavigationContext().disabled = true;
		hotkeyBar.BeforeShow(null, list);
	}

	public void Update()
	{
		if (navigationContext.IsActive() && (NavigationController.instance.activeContext?.IsInside(savesScroller.GetNavigationContext()) ?? false) != wasInScroller)
		{
			UpdateMenuBars();
		}
	}

	public void ControllerChanged()
	{
		UpdateMenuBars();
	}

	public static List<SaveInfoData> SaveGameInfosToUIElements(List<SaveGameInfo> saveGameInfos)
	{
		return saveGameInfos.Select((SaveGameInfo info) => new SaveInfoData
		{
			SaveGame = info
		}).ToList();
	}
}
