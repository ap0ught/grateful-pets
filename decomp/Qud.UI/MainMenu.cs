using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ConsoleLib.Console;
using LaundryBear.PlatformServices;
using Qud.API;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[UIView("MainMenu", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "MainMenu", UICanvasHost = 1)]
public class MainMenu : SingletonWindowBase<MainMenu>, ControlManager.IControllerChangedEvent
{
	protected List<SaveInfoData> saves;

	public Image background;

	public CanvasGroup logoFader;

	public CanvasGroup leftFader;

	public RectTransform centerTransform;

	public VerticalLayoutGroup centerFlyoutGroup;

	public FrameworkScroller hotkeyBar;

	public FrameworkScroller leftScroller;

	public FrameworkScroller rightScroller;

	public TaskCompletionSource<SaveGameInfo> completionSource;

	public NavigationContext globalContext = new NavigationContext();

	public ScrollContext<NavigationContext> midHorizNav = new ScrollContext<NavigationContext>();

	public UITextSkin versionText;

	public GameObject[] backgrounds;

	public static List<MainMenuOptionData> LeftOptions = new List<MainMenuOptionData>
	{
		new MainMenuOptionData
		{
			Text = "New Game",
			Command = "Pick:New Game",
			Shortcut = UnityEngine.KeyCode.N
		},
		new MainMenuOptionData
		{
			Text = "Continue",
			Command = "Pick:Continue",
			Shortcut = UnityEngine.KeyCode.C
		},
		new MainMenuOptionData
		{
			Text = "Records",
			Command = "Pick:High Scores",
			Shortcut = UnityEngine.KeyCode.H
		},
		new MainMenuOptionData
		{
			Text = "Options",
			Command = "Pick:Options",
			Shortcut = UnityEngine.KeyCode.O
		},
		new MainMenuOptionData
		{
			Text = "Mods",
			Command = "Pick:Installed Mod Configuration",
			Alert = MainMenuOptionData.AlertMode.ModStatus,
			AllowedPlatforms = LaundryBear.PlatformServices.Platform.Desktop
		}
	};

	public static List<MainMenuOptionData> RightOptions = new List<MainMenuOptionData>
	{
		new MainMenuOptionData
		{
			Text = "Redeem Code",
			Command = "Pick:Redeem Code",
			AllowedPlatforms = LaundryBear.PlatformServices.Platform.Desktop
		},
		new MainMenuOptionData
		{
			Text = "Modding Toolkit",
			Command = "Pick:Modding Utilities",
			Shortcut = UnityEngine.KeyCode.M,
			AllowedPlatforms = LaundryBear.PlatformServices.Platform.Desktop
		},
		new MainMenuOptionData
		{
			Text = "Credits",
			Command = "Pick:Credits"
		},
		new MainMenuOptionData
		{
			Text = "Help",
			Command = "Pick:Help",
			Shortcut = UnityEngine.KeyCode.F1
		}
	};

	public GameObject diasporaCard;

	public TextMeshProUGUI diasporaText;

	public ScrollContext<NavigationContext> rightVertNav = new ScrollContext<NavigationContext>();

	public static bool EnsureReselect = false;

	private bool SelectFirst = true;

	public bool FirstShow = true;

	public GameObject steamInputWarning;

	public FrameworkClickToAccept diasporaCardContext;

	public static bool HasDromadDeluxeEntitlement = false;

	public static bool HasDiasporaEntitlement = false;

	public RectTransform dromadDeluxeBadge;

	public bool wasInScroller;

	public async void DiasporaClick()
	{
		if (HasDiasporaEntitlement)
		{
			if (await Popup.ShowYesNoAsync("You must switch to the Diaspora branch to access the expansion content. Would you like to make that switch?") == DialogResult.Yes)
			{
				Debug.Log("setting branch to diaspora");
				if (SteamApps.SetActiveBeta("diaspora"))
				{
					Debug.Log("set branch to diaspora");
					await Popup.ShowAsync("Steam has been switched to the diaspora branch. Caves of Qud will now close to update.\n\nYou can switch back to the main branch at any time. From Caves of Qud in Steam, choose Properties, Betas, and then select the 'default' branch.");
					Application.Quit();
				}
				else
				{
					Debug.LogWarning("FAILED to set branch to diaspora");
					await Popup.ShowAsync("Steam failed to switch to the diaspora branch. Please email support@freeholdgames.com and we can help you out.");
				}
			}
			return;
		}
		try
		{
			SteamFriends.ActivateGameOverlayToWebPage("https://store.steampowered.com/app/5249200/Caves_of_Qud_Diaspora/");
		}
		catch
		{
			Application.OpenURL("https://store.steampowered.com/app/5249200/Caves_of_Qud_Diaspora/");
		}
	}

	public void SetupContext()
	{
		globalContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
		if (CapabilityManager.CurrentPlatformClassification() == CapabilityManager.PlatformClassification.PC)
		{
			globalContext.buttonHandlers.Set(InputButtonTypes.CancelButton, XRL.UI.Framework.Event.Helpers.Handle(Quit));
		}
		rightVertNav.parentContext = midHorizNav;
		leftScroller.scrollContext.parentContext = midHorizNav;
		rightScroller.scrollContext.parentContext = midHorizNav;
		rightVertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		rightVertNav.contexts.Clear();
		rightVertNav.contexts.Add(diasporaCardContext.context);
		midHorizNav.parentContext = globalContext;
		midHorizNav.SetAxis(InputAxisTypes.NavigationXAxis);
		midHorizNav.contexts.Clear();
		midHorizNav.contexts.Add(rightScroller.scrollContext);
		midHorizNav.contexts.Add(leftScroller.scrollContext);
		if (diasporaCard.activeSelf)
		{
			midHorizNav.contexts.Add(rightVertNav);
		}
		midHorizNav.Setup();
		NavigationContext context = diasporaCardContext.context;
		if (context.buttonHandlers == null)
		{
			context.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
		}
		diasporaCardContext.context.buttonHandlers[InputButtonTypes.AcceptButton] = DiasporaClick;
	}

	public IEnumerator DoIntroTween(GameObject activeBackground)
	{
		float introTime = 0f;
		LeanTweenType EASE_TYPE = LeanTweenType.easeOutCubic;
		if (activeBackground != null)
		{
			activeBackground.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
			LeanTween.scale(activeBackground, new Vector3(1.01f, 1.01f, 1f), 6f).setEase(EASE_TYPE);
		}
		leftFader.alpha = 0f;
		logoFader.alpha = 0f;
		centerFlyoutGroup.padding.top = -290;
		centerTransform.localScale = new Vector3(0f, 1f, 1f);
		while (introTime < 1f)
		{
			introTime += Time.deltaTime;
			yield return null;
		}
		SoundManager.PlayUISound("Sounds/UI/ui_mainMenu_popUp", 1f, Combat: false, Interface: true);
		LeanTween.alpha(logoFader.gameObject, 1f, 2f).setEase(EASE_TYPE);
		LeanTween.alpha(leftFader.gameObject, 1f, 2f).setEase(EASE_TYPE);
		float flywideStart = 0f;
		while (centerTransform.localScale.x < 1f)
		{
			flywideStart += Time.deltaTime;
			centerTransform.localScale = new Vector3(Mathf.Lerp(0f, 1f, flywideStart * 5f), 1f, 1f);
			LayoutRebuilder.MarkLayoutForRebuild(centerTransform);
			yield return null;
		}
		float flyoutStart = 0f;
		while (centerFlyoutGroup.padding.top < 0)
		{
			flyoutStart += Time.deltaTime;
			centerFlyoutGroup.padding.top = (int)Mathf.Lerp(-290f, 0f, Easing.SineEaseIn(flyoutStart));
			LayoutRebuilder.MarkLayoutForRebuild(centerTransform);
			yield return null;
		}
	}

	public override void Show()
	{
		TutorialManager.EndTutorial();
		versionText.SetText($"{XRLGame.MarketingVersion}{XRLGame.MarketingPostfix}\n{{{{K|build {XRLGame.CoreVersion}}}}}");
		base.Show();
		string option = Options.GetOption("OptionMainMenuBackground");
		GameObject gameObject = null;
		backgrounds[0].SetActive(option == "Modern");
		backgrounds[1].SetActive(option == "Classic");
		if (option == "Modern")
		{
			gameObject = backgrounds[0];
		}
		if (option == "Classic")
		{
			gameObject = backgrounds[1];
		}
		if (FirstShow)
		{
			FirstShow = false;
			StartCoroutine(DoIntroTween(gameObject));
		}
		else
		{
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			leftFader.alpha = 1f;
			logoFader.alpha = 1f;
			centerFlyoutGroup.padding.top = 0;
			centerTransform.localScale = new Vector3(1f, 1f, 1f);
		}
		LeftOptions[1].Enabled = SavesAPI.HasSavedGameInfo();
		LaundryBear.PlatformServices.Platform currentPlatform = Utilities.GetCurrentPlatform();
		for (int num = LeftOptions.Count - 1; num > -1; num--)
		{
			if ((LeftOptions[num].AllowedPlatforms & currentPlatform) == 0)
			{
				LeftOptions.RemoveAt(num);
			}
		}
		leftScroller.scrollContext.wraps = true;
		leftScroller.BeforeShow(null, LeftOptions);
		leftScroller.onSelected.RemoveAllListeners();
		leftScroller.onSelected.AddListener(SelectedInfo);
		leftScroller.scrollContext.parentContext = globalContext;
		for (int num2 = RightOptions.Count - 1; num2 > -1; num2--)
		{
			if ((RightOptions[num2].AllowedPlatforms & currentPlatform) == 0)
			{
				RightOptions.RemoveAt(num2);
			}
		}
		rightScroller.scrollContext.wraps = true;
		rightScroller.BeforeShow(null, RightOptions);
		rightScroller.onSelected.RemoveAllListeners();
		rightScroller.onSelected.AddListener(SelectedInfo);
		rightScroller.scrollContext.parentContext = globalContext;
		if (SelectFirst)
		{
			SelectFirst = false;
			leftScroller.scrollContext.selectedPosition = 0;
			leftScroller.scrollContext.Activate();
		}
		SetupContext();
		EnableNavContext();
		UpdateMenuBars();
		TutorialManager.EndTutorial();
		if (File.Exists(DataManager.DLCPath("OSTEntitlement.txt")) && File.Exists(DataManager.DLCPath("PetPack1Entitlement.txt")))
		{
			HasDromadDeluxeEntitlement = true;
		}
	}

	public async void Reshow()
	{
		await The.UiContext;
		EnableNavContext();
		leftScroller.scrollContext.ActivateAndEnable();
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
		leftScroller.GetNavigationContext().ActivateAndEnable();
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
		leftScroller.GetNavigationContext().disabled = true;
	}

	public async void SelectedInfo(FrameworkDataElement data)
	{
		if (!(data is MainMenuOptionData mainMenuOptionData))
		{
			return;
		}
		if (mainMenuOptionData.Command == "Pick:Redeem Code")
		{
			try
			{
				string text = await Popup.AskStringAsync("Redeem a Code", "", 32);
				if (!string.IsNullOrEmpty(text))
				{
					CodeRedemptionManager.redeemNoProgress(text);
				}
				return;
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Code Redemption", x);
				return;
			}
		}
		if (mainMenuOptionData.Command == "Pick:Installed Mod Configuration")
		{
			try
			{
				await NavigationController.instance.SuspendContextWhile(SingletonWindowBase<ModManagerUI>.instance.ShowMenuAsync);
				return;
			}
			finally
			{
				UIManager.showWindow("MainMenu");
				Reshow();
			}
		}
		if (mainMenuOptionData.Command == "Pick:Modding Utilities")
		{
			try
			{
				await NavigationController.instance.SuspendContextWhile(SingletonWindowBase<ModToolkit>.instance.ShowMenuAsync);
				return;
			}
			finally
			{
				UIManager.showWindow("MainMenu");
			}
		}
		if (mainMenuOptionData.Command == "Pick:Achievements")
		{
			try
			{
				await NavigationController.instance.SuspendContextWhile(SingletonWindowBase<AchievementView>.instance.ShowMenuAsync);
				return;
			}
			finally
			{
				UIManager.showWindow("MainMenu");
			}
		}
		Keyboard.PushMouseEvent(mainMenuOptionData.Command);
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
			SelectFirst = true;
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
				if (info.json.SaveVersion < 395)
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

	public async void Quit()
	{
		if (await Popup.ShowYesNoAsync("Are you sure you want to quit?") == DialogResult.Yes)
		{
			GameManager.Instance.Quit();
		}
	}

	public void Exit()
	{
		completionSource?.TrySetResult(null);
		ControlManager.ResetInput();
	}

	public async void HandleDelete()
	{
		if (NavigationController.instance.activeContext.IsInside(leftScroller.GetNavigationContext()))
		{
			SaveInfoData saveInfo = saves[leftScroller.selectedPosition];
			List<QudMenuItem> buttons = ((CapabilityManager.CurrentPlatformClassification() != CapabilityManager.PlatformClassification.PC) ? PopupMessage.AcceptCancelButtonWithoutHotkey : PopupMessage.AcceptCancelButton);
			string title = "{{R|Delete " + saveInfo.SaveGame.Name + "}}";
			if ((await Popup.NewPopupMessageAsync("Are you sure you want to delete the save game for " + saveInfo.SaveGame.Name + "?", buttons, null, title, null, 1)).command != "Cancel")
			{
				DisableNavContext();
				saveInfo.SaveGame.Delete();
				Show();
				await Popup.NewPopupMessageAsync("Game Deleted!", PopupMessage.AcceptButton);
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
		if (CapabilityManager.CurrentPlatformClassification() == CapabilityManager.PlatformClassification.PC)
		{
			list.Add(new MenuOption
			{
				KeyDescription = ControlManager.getCommandInputDescription("Cancel"),
				Description = "quit"
			});
		}
		hotkeyBar.GetNavigationContext().disabled = true;
		hotkeyBar.BeforeShow(null, list);
	}

	public async void CheckDiaspora()
	{
		if (!PlatformManager.Steam.Initialized)
		{
			return;
		}
		if (Options.GetOption("OptionDisableMarketingCards") == "Yes" || !SteamUser.BLoggedOn())
		{
			if (diasporaCard?.gameObject.activeSelf ?? false)
			{
				diasporaCard.gameObject.SetActive(value: false);
				SetupContext();
			}
			if (NavigationController.instance.activeContext?.IsInside(rightVertNav) ?? false)
			{
				leftScroller.scrollContext.ActivateAndEnable();
			}
		}
		else if (PlatformManager.IsInitialized() && !(diasporaCard?.gameObject.activeSelf ?? false) && !Options.GetOptionBool("OptionDisableMarketingCards"))
		{
			if (HasDiasporaEntitlement)
			{
				diasporaText.text = "Play Now!";
			}
			else
			{
				diasporaText.text = "December 9th\r\nWishlist Now!";
			}
			diasporaCard.SetActive(value: true);
			if (HasDiasporaEntitlement && !PlayerPrefs.HasKey("DiasporaSwitchPrompt"))
			{
				PlayerPrefs.SetInt("DiasporaSwitchPrompt", 1);
				DiasporaClick();
			}
			SetupContext();
		}
	}

	public void Update()
	{
		CheckDiaspora();
		if (HasDromadDeluxeEntitlement)
		{
			RectTransform rectTransform = dromadDeluxeBadge;
			if (((object)rectTransform == null || !rectTransform.gameObject.activeSelf) && Options.GetOption("OptionShowCollectionBadges") != "No")
			{
				dromadDeluxeBadge.gameObject.SetActive(value: true);
				dromadDeluxeBadge.localScale = new Vector3(1f, 1f, 1f);
			}
		}
		RectTransform rectTransform2 = dromadDeluxeBadge;
		if (((object)rectTransform2 == null || !rectTransform2.gameObject.activeSelf) && (!HasDromadDeluxeEntitlement || Options.GetOption("OptionShowCollectionBadges") != "Yes"))
		{
			dromadDeluxeBadge.gameObject.SetActive(value: false);
		}
		if (!Options.ModernUI)
		{
			Hide();
			return;
		}
		NavigationContext activeContext = NavigationController.instance.activeContext;
		bool flag = (activeContext != null && activeContext.IsInside(leftScroller.GetNavigationContext())) || (NavigationController.instance.activeContext?.IsInside(rightScroller.GetNavigationContext()) ?? false);
		if (flag != wasInScroller)
		{
			UpdateMenuBars();
			wasInScroller = flag;
		}
		if (flag)
		{
			LeftOptions.ForEach((MainMenuOptionData o) =>
			{
				if (ControlManager.isHotkeyDown(o.Shortcut))
				{
					SelectedInfo(o);
				}
			});
			RightOptions.ForEach((MainMenuOptionData o) =>
			{
				if (ControlManager.isHotkeyDown(o.Shortcut))
				{
					SelectedInfo(o);
				}
			});
		}
		if ((bool)steamInputWarning && PlatformManager.Steam.Initialized && ControlManager.SuspectSteamInput && !steamInputWarning.activeInHierarchy)
		{
			steamInputWarning.SetActive(value: true);
		}
	}

	public void ControllerChanged()
	{
		UpdateMenuBars();
	}

	private void LateUpdate()
	{
		if (EnsureReselect)
		{
			EnsureReselect = false;
			EnableNavContext();
			leftScroller.scrollContext.ActivateAndEnable();
		}
	}
}
