using ConsoleLib.Console;
using QupKit;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.QupKit.Components;

public class ButtonKeyShortcut : MonoBehaviour
{
	public string CommandID = string.Empty;

	public UnityEngine.KeyCode Hotkey;

	private Button parentButton;

	public bool AllowWithRewired = true;

	public bool init;

	public void Update()
	{
		if (parentButton != null && parentButton.interactable && Hotkey != UnityEngine.KeyCode.None && Input.GetKeyDown(Hotkey))
		{
			Keyboard.ClearInput();
			ControlManager.ResetInput();
			LegacyViewManager.Instance.OnCommand(CommandID);
		}
	}

	public void Awake()
	{
		if (init)
		{
			return;
		}
		init = true;
		parentButton = GetComponent<Button>();
		GetComponent<Button>().onClick.RemoveAllListeners();
		GetComponent<Button>().onClick.AddListener(() =>
		{
			if (parentButton.enabled)
			{
				LegacyViewManager.Instance.OnCommand(CommandID);
			}
		});
	}
}
