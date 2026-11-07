using ConsoleLib.Console;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Qud.UI;

[ExecuteInEditMode]
public class AbilityBarButton : MonoBehaviour
{
	public GameObject highlightOverlay;

	public TextMeshProUGUI Text;

	public TextMeshProUGUI Hotkey;

	public UIThreeColorProperties Icon;

	public string command;

	public bool _highlighted;

	public bool highlighted
	{
		get
		{
			return _highlighted;
		}
		set
		{
			if (value && !_highlighted)
			{
				UpdateText();
				highlightOverlay.SetActive(value: true);
			}
			else if (!value && _highlighted)
			{
				highlightOverlay.SetActive(value: false);
			}
			_highlighted = value;
		}
	}

	public bool disabled
	{
		get
		{
			return !gameObject.GetComponent<Button>().interactable;
		}
		set
		{
			gameObject.GetComponent<Button>().interactable = !value;
		}
	}

	public void UpdateText()
	{
		Hotkey.text = ControlManager.getCommandInputDescription("Use Ability", mapGlyphs: true, allowAlt: true);
	}

	[ExecuteInEditMode]
	public void Update()
	{
		if (highlightOverlay.activeInHierarchy && !_highlighted)
		{
			highlightOverlay.SetActive(value: false);
		}
		if (!highlightOverlay.activeInHierarchy && _highlighted)
		{
			highlightOverlay.SetActive(value: true);
		}
	}

	public void LateUpdate()
	{
		if (ControlManager.ControllerChangedThisLateUpdate && _highlighted)
		{
			UpdateText();
		}
	}

	public void Awake()
	{
		highlightOverlay.SetActive(highlighted);
	}

	public void OnClick()
	{
		if (!(GameManager.Instance.CurrentGameView != "Stage") && !disabled)
		{
			Keyboard.PushMouseEvent("Command:" + command);
		}
	}
}
