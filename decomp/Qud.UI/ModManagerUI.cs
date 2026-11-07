using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Genkit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XRL;
using XRL.UI;

namespace Qud.UI;

[UIView("ModManager", false, false, false, null, null, false, 0, false, NavCategory = "Popup", UICanvas = "ModManager", UICanvasHost = 1, OverlayMode = 1)]
public class ModManagerUI : SingletonWindowBase<ModManagerUI>
{
	public class ModManagerSorter : Comparer<ModInfo>
	{
		public override int Compare(ModInfo a, ModInfo b)
		{
			return ConsoleLib.Console.ColorUtility.CompareExceptFormattingNoCase(a.DisplayTitle, b.DisplayTitle);
		}
	}

	public ModScrollerOne ms1;

	public UITextSkin SelectedModTitle;

	public GameObject SelectedModAuthorSpacer;

	public UITextSkin SelectedModAuthor;

	public UITextSkin SelectedModDescription;

	public ImageTinyFrame SelectedModImage;

	public UnityEvent nextHideCallback;

	private TaskCompletionSource<bool> menuclosed = new TaskCompletionSource<bool>();

	private int _initialHash;

	private Dictionary<bool, string> _shouldEnableTexts = new Dictionary<bool, string>
	{
		{ false, "{{W|[v]}} {{y|Disable all}}" },
		{ true, "{{W|[v]}} {{y|Enable all}}" }
	};

	private Dictionary<bool, string> _shouldEnableTextsJoystick = new Dictionary<bool, string>
	{
		{ false, "{{y|Disable all}}" },
		{ true, "{{y|Enable all}}" }
	};

	private Dictionary<ModState, string> _spaceButtonTexts = new Dictionary<ModState, string>
	{
		{
			ModState.Disabled,
			"{{W|[space]}} {{y|Enable mod}}"
		},
		{
			ModState.MissingDependency,
			"{{W|[space]}} {{y|View dependencies}}"
		},
		{
			ModState.Enabled,
			"{{W|[space]}} {{y|Disable mod}}"
		},
		{
			ModState.Failed,
			"{{W|[space]}} {{y|Recompile mod}}"
		}
	};

	private Dictionary<bool, string> shouldEnableTexts
	{
		get
		{
			if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
			{
				return _shouldEnableTextsJoystick;
			}
			return _shouldEnableTexts;
		}
	}

	private Dictionary<ModState, string> spaceButtonTexts
	{
		get
		{
			if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
			{
				return new Dictionary<ModState, string>
				{
					{
						ModState.Disabled,
						"{{W|[" + ControlManager.getCommandInputDescription("Accept") + "]}} {{y|Enable mod}}"
					},
					{
						ModState.MissingDependency,
						"{{W|[" + ControlManager.getCommandInputDescription("Accept") + "]}} {{y|View dependencies}}"
					},
					{
						ModState.Enabled,
						"{{W|[" + ControlManager.getCommandInputDescription("Accept") + "]}} {{y|Disable mod}}"
					},
					{
						ModState.Failed,
						"{{W|[" + ControlManager.getCommandInputDescription("Accept") + "]}} {{y|Recompile mod}}"
					}
				};
			}
			return _spaceButtonTexts;
		}
	}

	public async Task<bool> ShowMenuAsync()
	{
		menuclosed.TrySetCanceled();
		menuclosed = new TaskCompletionSource<bool>();
		await The.UiContext;
		if (ModManager.IsScriptingUndetermined())
		{
			await PromptScripting();
			await Task.Delay(250);
		}
		GameManager.Instance.PushGameView("ModManager");
		return await menuclosed.Task;
	}

	public override void Hide()
	{
		OldMainMenuView.instance?.DisplayAlert();
		nextHideCallback.Invoke();
		nextHideCallback.RemoveAllListeners();
		base.Hide();
		gameObject.SetActive(value: false);
	}

	public override void Show()
	{
		if (CapabilityManager.CurrentPlatformClassification() == CapabilityManager.PlatformClassification.Console)
		{
			SetBackButtonText("Back");
		}
		gameObject.SetActive(value: true);
		base.Show();
		SetupMods();
		ms1.Reselect(0);
	}

	public void SetBackButtonText(string text)
	{
		QudMenuItem value = ms1.bottomContextOptions[0];
		if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
		{
			value.text = "{{W|[" + ControlManager.getCommandInputDescription("Cancel") + "]}} {{y|" + text + "}}";
		}
		else
		{
			value.text = "{{W|[Esc]}} {{y|" + text + "}}";
		}
		ms1.bottomContextOptions[0] = value;
	}

	public void SetupMods()
	{
		if (ms1 != null)
		{
			List<ModInfo> list = new List<ModInfo>(ModManager.Mods);
			list.Sort(new ModManagerSorter());
			ms1.mods = list;
			ms1.isCurrentWindow = IsCurrentWindow;
			_initialHash = GetModsHash();
		}
	}

	public bool IsCurrentWindow()
	{
		if (GameManager.Instance.CurrentGameView == "ModManager")
		{
			return raycaster.enabled;
		}
		return false;
	}

	public int GetModsHash()
	{
		uint num = 2166136261u;
		foreach (ModInfo mod in ModManager.Mods)
		{
			if (mod.IsEnabled)
			{
				num = Hash.FNV1A32(mod.ID, num);
			}
		}
		return (int)num;
	}

	public static async Task PromptScripting()
	{
		QudMenuItem qudMenuItem = await Popup.NewPopupMessageAsync("Scripting mods are currently disabled. Do you want to enable them?\n\nScripts may contain malicious code with unfettered access to your computer.\nYou can change this option again later in Options -> Mods -> Allow scripting mods.", PopupMessage.YesNoButton, null, "{{W|Unapproved Scripting}}");
		string value = ((qudMenuItem.command == "Yes") ? "Yes" : "No");
		Options.SetOption("OptionAllowCSMods", value);
		if (qudMenuItem.command == "Yes")
		{
			GameManager.Restart();
		}
	}

	public override void Init()
	{
		base.Init();
	}

	public void OnSelect(ModInfo modInfo)
	{
		SelectedModTitle.SetText("{{W|" + modInfo.DisplayTitle + "}}");
		if (string.IsNullOrEmpty(modInfo.Manifest.Author))
		{
			SelectedModAuthor.gameObject.SetActive(value: false);
			SelectedModAuthorSpacer.gameObject.SetActive(value: false);
		}
		else
		{
			SelectedModAuthor.gameObject.SetActive(value: true);
			SelectedModAuthorSpacer.gameObject.SetActive(value: true);
			SelectedModAuthor.SetText("{{C|by " + modInfo.Manifest.Author + "}}");
		}
		SelectedModDescription.SetText("{{c|" + modInfo.Manifest.Description + "}}");
		SelectedModImage.image.sprite = modInfo.GetSprite();
	}

	public void Awake()
	{
	}

	public void Update()
	{
		bool key = ms1.mods.FindCount((ModInfo i) => i.IsEnabled) < ms1.mods.Count / 2;
		int index = ms1.bottomContextOptions.FindIndex((QudMenuItem i) => i.command == "ToggleAll");
		if (ms1.bottomContextOptions[index].text != shouldEnableTexts[key])
		{
			QudMenuItem value = ms1.bottomContextOptions[index];
			value.text = shouldEnableTexts[key];
			ms1.bottomContextOptions[index] = value;
		}
		int index2 = ms1.bottomContextOptions.FindIndex((QudMenuItem i) => i.command == "");
		string text = "{{W|[space]}} {{y|press button}}";
		if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
		{
			text = "{{W|[" + ControlManager.getCommandInputDescription("Accept") + "]}} {{y|press button}}";
		}
		if (ms1.selectedOption < ms1.mods.Count)
		{
			text = spaceButtonTexts[ms1.mods[ms1.selectedOption].State];
		}
		if (ms1.bottomContextOptions[index2].text != text)
		{
			QudMenuItem value2 = ms1.bottomContextOptions[index2];
			value2.text = text;
			ms1.bottomContextOptions[index2] = value2;
		}
		if (ms1.menuBottomContext.GetComponent<RectTransform>().rect.width < ms1.menuBottomContext.GetComponent<LayoutGroup>().preferredWidth)
		{
			ms1.menuBottomContext.buttons[index2].gameObject.SetActive(value: false);
		}
	}

	public void OnCancel()
	{
		if (GetModsHash() != _initialHash)
		{
			Popup.WaitNewPopupMessage("Your mod configuration has changed, do you want to apply your changes and restart the game?", PopupMessage.YesNoButton, (QudMenuItem i) =>
			{
				if (i.command == "Yes")
				{
					ModManager.WriteModSettings();
					GameManager.Restart();
				}
				else
				{
					ModManager.ReadModSettings(Repopulate: true);
					SetupMods();
				}
				GameManager.Instance.RemoveGameView("ModManager");
				menuclosed.TrySetResult(result: true);
			});
		}
		else
		{
			GameManager.Instance.RemoveGameView("ModManager");
			menuclosed.TrySetResult(result: true);
		}
	}

	public void OnActivateContext(QudMenuItem data)
	{
		QudMenuItem qudMenuItem = data;
		Debug.Log("Activate Context: " + qudMenuItem.ToString());
		if (data.command == "ToggleAll")
		{
			int num = ms1.mods.FindCount((ModInfo i) => i.IsEnabled);
			bool shouldEnable = num < ms1.mods.Count / 2;
			ms1.mods.ForEach((ModInfo i) =>
			{
				i.IsEnabled = shouldEnable;
			});
		}
		else if (data.command == "Undo")
		{
			ModManager.ReadModSettings(Repopulate: true);
			SetupMods();
		}
		else if (data.command == "Reload" && GetModsHash() != _initialHash)
		{
			ModManager.WriteModSettings();
			GameManager.Restart();
		}
	}
}
