using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LaundryBear.PlatformServices;
using Platform.IO;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.CharacterBuilds.UI;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World.Conversations.Parts;

namespace Qud.UI;

[UIView("GameSummaryScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "GameSummary", UICanvasHost = 1)]
public class GameSummaryScreen : SingletonWindowBase<GameSummaryScreen>
{
	public TaskCompletionSource<bool> completionSource;

	public FrameworkScroller hotkeyBar;

	public FrameworkScroller summaryScroller;

	public NavigationContext vertNav = new NavigationContext();

	public Sprite[] endmarks;

	public Image ultraMark;

	public Image tombstoneImage;

	public UITextSkin detailsText;

	public UITextSkin nameText;

	public UITextSkin causeText;

	public string Name;

	public string Details;

	private bool bReal;

	public Image topperBackground;

	public bool wasInScroller;

	public List<MenuOption> keyMenuOptions = new List<MenuOption>();

	public static MenuOption BACK_BUTTON => EmbarkBuilderOverlayWindow.BackMenuOption;

	public static async Task<bool> ShowGameSummary(string name, string cause, string details, bool real)
	{
		return await SingletonWindowBase<GameSummaryScreen>.instance._ShowGameSummary(name, cause, details, real);
	}

	public void SetEndmark(string mark, bool ultra)
	{
		tombstoneImage.sprite = endmarks.Where((Sprite e) => e.name == mark).FirstOrDefault();
		ultraMark.enabled = ultra;
	}

	public async Task<bool> _ShowGameSummary(string name, string cause, string details, bool real)
	{
		Details = details;
		Name = name;
		bReal = real;
		completionSource = new TaskCompletionSource<bool>();
		await The.UiContext;
		ControlManager.ResetInput();
		Show();
		if (EndGame.IsAnyEnding)
		{
			if (The.Game.HasStringGameState("FinalEndmark"))
			{
				SetEndmark(The.Game.GetStringGameState("FinalEndmark"), EndGame.IsUltra);
			}
			else
			{
				topperBackground.enabled = true;
				if (EndGame.IsBrightsheol)
				{
					SetEndmark("brightsheol", EndGame.IsUltimate);
					cause = ((!EndGame.IsUltimate) ? "You freed the Spindle for another's ascent and crossed into Brightsheol." : "You annulled the plagues of the Gyre.\n\nThen you freed the Spindle for another's ascent and crossed into Brightsheol");
				}
				else if (EndGame.IsMarooned)
				{
					The.Game.SetStringGameState("FinalEndmark", "marooned");
					SetEndmark("marooned", EndGame.IsUltimate);
					cause = ((!EndGame.IsUltimate) ? "You destroyed Resheph to save the burgeoning world but marooned yourself at the North Sheva." : "You annulled the plagues of the Gyre.\n\nThen you destroyed Resheph to save the burgeoning world but marooned yourself at the North Sheva.");
				}
				else if (EndGame.IsCovenant)
				{
					The.Game.SetStringGameState("FinalEndmark", "covenant");
					SetEndmark("covenant", EndGame.IsUltimate);
					cause = ((!EndGame.IsUltimate) ? "You entered into a covenant with Resheph to help prepare Qud for the Coven's return." : "You annulled the plagues of the Gyre.\n\nThen you entered into a covenant with Resheph to help prepare Qud for the Coven's return.");
				}
				else if (EndGame.IsReturn)
				{
					The.Game.SetStringGameState("FinalEndmark", "return");
					SetEndmark("return", EndGame.IsUltimate);
					cause = (EndGame.IsArkOpened ? ((!EndGame.IsUltimate) ? "You destroyed Resheph and returned to Qud to help garden the burgeoning world." : "You annulled the plagues of the Gyre.\n\nThen you destroyed Resheph and returned to Qud to help garden the burgeoning world.") : ((!EndGame.IsUltimate) ? "You rebuked Resheph and returned to Qud to help garden the burgeoning world." : "You annulled the plagues of the Gyre.\n\nThen you returned to Qud to help garden the burgeoning world."));
				}
				else if (EndGame.IsAccede)
				{
					The.Game.SetStringGameState("FinalEndmark", "accede");
					SetEndmark("accede", EndGame.IsUltimate);
					cause = ((!EndGame.IsUltimate) ? "You acceded to Resheph's plan to purge the world of higher life in preparation for the Coven's return." : "You annulled the plagues of the Gyre.\n\nThen you reversed course and acceded to Resheph's plan to purge the world of higher life, in preparation for the Coven's return.");
				}
				else if (EndGame.IsLaunch)
				{
					The.Game.SetStringGameState("FinalEndmark", "spaceship");
					SetEndmark("spaceship", EndGame.IsUltimate);
					cause = (EndGame.IsArkOpened ? (EndGame.IsWithBarathrum ? ((!EndGame.IsUltimate) ? "You destroyed Resheph and launched yourself into the dusted cosmos to ply the stars with Barathrum." : "You annulled the plagues of the Gyre.\n\nThen you destroyed Resheph and launched yourself into the dusted cosmos to ply the stars with Barathrum.") : ((!EndGame.IsUltimate) ? "You destroyed Resheph and launched yourself into the dusted cosmos to ply the stars." : "You annulled the plagues of the Gyre.\n\nThen you destroyed Resheph and launched yourself into the dusted cosmos to ply the stars.")) : (EndGame.IsWithBarathrum ? ((!EndGame.IsUltimate) ? "You launched yourself into the dusted cosmos to ply the stars with Barathrum." : "You annulled the plagues of the Gyre.\n\nThen you launched yourself into the dusted cosmos to ply the stars with Barathrum.") : ((!EndGame.IsUltimate) ? "You launched yourself into the dusted cosmos to ply the stars." : "You annulled the plagues of the Gyre.\n\nThen you launched yourself into the dusted cosmos to ply the stars.")));
				}
				else
				{
					topperBackground.enabled = EndGame.IsUltimate;
					The.Game.SetStringGameState("FinalEndmark", "tombstone");
					SetEndmark("tombstone", EndGame.IsUltimate);
				}
			}
		}
		else
		{
			SetEndmark("tombstone", ultra: false);
		}
		nameText.SetText(name ?? "");
		causeText.SetText(cause);
		detailsText.SetText(details);
		bool info = await completionSource.Task;
		await The.UiContext;
		Hide();
		if (EndGame.IsAnyEnding)
		{
			FadeToBlackHigh.FadeIn(10f);
		}
		return info;
	}

	public void Exit()
	{
		completionSource?.TrySetResult(result: false);
	}

	public void Update()
	{
	}

	public void SetupContext()
	{
	}

	public void UpdateMenuBars()
	{
		keyMenuOptions.Clear();
		if (!Application.isConsolePlatform)
		{
			keyMenuOptions.Add(new MenuOption
			{
				InputCommand = "CmdHelp",
				Description = "Save Tombstone File"
			});
		}
		keyMenuOptions.Add(new MenuOption
		{
			InputCommand = "Cancel",
			Description = "Exit"
		});
		hotkeyBar.BeforeShow(null, keyMenuOptions);
		hotkeyBar.GetNavigationContext().disabled = false;
		hotkeyBar.onSelected.RemoveAllListeners();
		hotkeyBar.onSelected.AddListener(HandleMenuOption);
		foreach (NavigationContext item in hotkeyBar.scrollContext.contexts.GetRange(0, keyMenuOptions.Count))
		{
			item.disabled = true;
		}
		NavigationContext navigationContext = vertNav;
		if (navigationContext.commandHandlers == null)
		{
			navigationContext.commandHandlers = new Dictionary<string, Action>();
		}
		vertNav.commandHandlers["Cancel"] = XRL.UI.Framework.Event.Helpers.Handle(Exit);
		if (!Application.isConsolePlatform)
		{
			vertNav.commandHandlers["CmdHelp"] = XRL.UI.Framework.Event.Helpers.Handle(SaveTombstone);
		}
		navigationContext = vertNav;
		if (navigationContext.axisHandlers == null)
		{
			navigationContext.axisHandlers = new Dictionary<InputAxisTypes, Action>();
		}
		vertNav.axisHandlers.Set(InputAxisTypes.NavigationYAxis, XRL.UI.Framework.Event.Helpers.Handle(XRL.UI.Framework.Event.Helpers.Axis(summaryScroller.DoScrollDown, summaryScroller.DoScrollUp)));
		vertNav.axisHandlers.Set(InputAxisTypes.NavigationPageYAxis, XRL.UI.Framework.Event.Helpers.Handle(XRL.UI.Framework.Event.Helpers.Axis(summaryScroller.DoPageDown, summaryScroller.DoPageUp)));
	}

	public void SaveTombstone()
	{
		Debug.Log("Saving tombstone...");
		if (!bReal)
		{
			Exit();
			Debug.Log("Not real tombstone... early outing");
			return;
		}
		string text = Name + "-" + DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString();
		string text2 = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
		for (int i = 0; i < text2.Length; i++)
		{
			text = text.Replace(text2[i].ToString(), "");
		}
		text = DataManager.SavePath(text + ".txt");
		StorageResult storageResult = Blob.WriteAllText(text, Details);
		if (storageResult.WasSuccessful())
		{
			Popup.Show("Your tombstone file was saved:\n\n" + text.ToString());
		}
		else
		{
			Popup.Show($"There was an error {storageResult} saving: " + text.ToString());
		}
	}

	public void HandleMenuOption(FrameworkDataElement data)
	{
	}

	public IEnumerable<FrameworkDataElement> GetMenuItems()
	{
		yield return new CyberneticsTerminalLineData
		{
			Text = "Back"
		};
	}

	public override void Show()
	{
		base.Show();
		summaryScroller.scrollContext.wraps = false;
		summaryScroller.scrollContext.selectedPosition = 0;
		UpdateMenuBars();
		SetupContext();
		summaryScroller.scrollRect.normalizedPosition = new Vector2(0f, 1f);
		vertNav.disabled = false;
		vertNav.ActivateAndEnable();
	}

	public override void Hide()
	{
		NavigationContext activeContext = NavigationController.instance.activeContext;
		if (activeContext != null && activeContext.IsInside(vertNav))
		{
			NavigationController.instance.activeContext = null;
		}
		vertNav.disabled = true;
		base.Hide();
		ControlManager.ResetInput();
		gameObject.SetActive(value: false);
	}
}
