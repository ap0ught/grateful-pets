using ConsoleLib.Console;
using Qud.UI;
using QupKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.QupKit.Components;

public class HoverTextButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public string CommandID;

	public AudioClip HighlightClip;

	public UnityEngine.KeyCode Hotkey;

	public bool HotkeyCapitalized;

	private AudioSource HighlightSource;

	public Color NormalColor = new Color(1f, 1f, 1f, 0.8f);

	public Color HighlightColor = new Color(1f, 1f, 1f, 1f);

	private Text _theText;

	private Button parentButton;

	private Text theText
	{
		get
		{
			if (_theText == null)
			{
				_theText = gameObject.GetComponent<Text>();
			}
			return _theText;
		}
	}

	public void Update()
	{
		if (parentButton != null && parentButton.interactable && Hotkey != UnityEngine.KeyCode.None && Input.GetKeyDown(Hotkey) && HotkeyCapitalized == (GameManager.capslock || Input.GetKey(UnityEngine.KeyCode.LeftShift) || Input.GetKey(UnityEngine.KeyCode.RightShift)) && UIManager.instance.AllowPassthroughInput())
		{
			Keyboard.ClearInput();
			LegacyViewManager.Instance.OnCommand(CommandID);
		}
	}

	public void Awake()
	{
		if (theText != null)
		{
			theText.color = NormalColor;
		}
		if (HighlightClip != null)
		{
			HighlightSource = gameObject.AddComponent<AudioSource>();
			HighlightSource.volume = 1f;
			HighlightSource.clip = HighlightClip;
			HighlightSource.playOnAwake = false;
		}
		parentButton = GetComponent<Button>();
		GetComponent<Button>().onClick.AddListener(() =>
		{
			LegacyViewManager.Instance?.OnCommand(CommandID);
		});
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (theText != null)
		{
			theText.color = HighlightColor;
		}
		if (HighlightSource != null)
		{
			HighlightSource.PlayOneShot(HighlightClip);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (theText != null)
		{
			theText.color = NormalColor;
		}
	}
}
