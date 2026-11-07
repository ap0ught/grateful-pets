using ConsoleLib.Console;
using Qud.UI;
using QupKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL;
using XRL.UI;

[UIView("OldMainMenu", true, true, false, "Menu", "OldMainMenu", false, 0, false)]
public class OldMainMenuView : BaseView
{
	public static OldMainMenuView instance;

	public override void Update()
	{
		if (EventSystem.current.currentSelectedGameObject == null)
		{
			Select(FindChild("New Game"));
		}
	}

	public override void Enter()
	{
		instance = this;
		base.Enter();
		Select(FindChild("New Game"));
		FindChild("Version").GetComponent<UnityEngine.UI.Text>().text = XRLGame.CoreVersion.ToString();
		DisplayAlert();
	}

	public void DisplayAlert()
	{
		GameObject gameObject = FindChild("Mod List/Alert");
		Image component = gameObject.GetComponent<Image>();
		if (ModManager.IsAnyModFailed())
		{
			component.color = Color.red;
			gameObject.SetActive(value: true);
		}
		else if (ModManager.IsAnyModMissingDependency())
		{
			component.color = Color.yellow;
			gameObject.SetActive(value: true);
		}
		else if (ModManager.IsScriptingUndetermined())
		{
			component.color = Color.white;
			gameObject.SetActive(value: true);
		}
		else
		{
			gameObject.SetActive(value: false);
		}
	}

	public override void OnCommand(string Command)
	{
		if (Command == "NewGame")
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.N, 'n'));
		}
		if (Command == "Continue")
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.C, 'c'));
		}
		if (Command == "Options")
		{
			Keyboard.PushMouseEvent("Options");
		}
		if (Command == "RedeemCode")
		{
			Keyboard.PushMouseEvent("RedeemCode");
		}
		if (Command == "HighScores")
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.H, 'h'));
		}
		if (Command == "Help")
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Question, '?'));
		}
		if (Command == "ExitGame")
		{
			GameManager.Instance.Quit();
		}
		if (Command == "Credits")
		{
			UIManager.showWindow("Credits");
		}
		if (Command == "Modkit")
		{
			UIManager.showWindow("ModToolkit");
		}
		if (Command == "ModList")
		{
			UIManager.showWindow("ModManager");
		}
		if (Command == "Achievements")
		{
			UIManager.pushWindow("AchievementView");
		}
		if (Command == "ChoicePopupTest")
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.P, 'p'));
		}
	}
}
