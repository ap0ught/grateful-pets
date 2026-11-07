using ConsoleLib.Console;
using QupKit;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.QupKit.Components;

public class HotkeyCommand : MonoBehaviour
{
	public string CommandID = "";

	public UnityEngine.KeyCode Hotkey;

	public bool HotkeyCapitalized;

	private Selectable parentControl;

	public void Awake()
	{
		if (parentControl == null)
		{
			parentControl = GetComponent<Button>();
		}
	}

	public void Update()
	{
		if ((parentControl == null || parentControl.interactable) && Hotkey != UnityEngine.KeyCode.None && Input.GetKeyDown(Hotkey) && HotkeyCapitalized == (GameManager.capslock || Input.GetKey(UnityEngine.KeyCode.LeftShift) || Input.GetKey(UnityEngine.KeyCode.RightShift)))
		{
			Keyboard.ClearInput();
			LegacyViewManager.Instance.OnCommand(CommandID);
		}
	}
}
