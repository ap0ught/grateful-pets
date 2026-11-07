using System;
using UnityEngine;
using UnityEngine.UI;

namespace RedShadow.CommonDialogs;

public class MessageDialog : DialogBase
{
	public Button ButtonPrefab;

	public Sprite InfoSprite;

	public Sprite QuestionSprite;

	public Sprite ErrorSprite;

	public Sprite WarningSprite;

	private GameObject _buttonPanel;

	private Image _icon;

	private Text _messageText;

	private Action<Buttons> _callback;

	private Buttons _buttons;

	public Buttons Result { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		Result = Buttons.Cancel;
		_messageText = transform.Find("Window/MessagePanel/Text").GetComponent<Text>();
		_buttonPanel = transform.Find("Window/ButtonPanel").gameObject;
		_icon = transform.Find("Window/MessagePanel/Icon").GetComponent<Image>();
	}

	public override void Update()
	{
		base.Update();
		if (isTop())
		{
			Buttons buttons = Buttons.None;
			if ((_buttons & Buttons.Ok) == Buttons.Ok && Input.GetKeyDown(KeyCode.Return))
			{
				buttons = Buttons.Ok;
			}
			if ((_buttons & Buttons.Cancel) == Buttons.Cancel && Input.GetKeyDown(KeyCode.C))
			{
				buttons = Buttons.Cancel;
			}
			if ((_buttons & Buttons.Yes) == Buttons.Yes && Input.GetKeyDown(KeyCode.Y))
			{
				buttons = Buttons.Yes;
			}
			if ((_buttons & Buttons.No) == Buttons.No && Input.GetKeyDown(KeyCode.N))
			{
				buttons = Buttons.No;
			}
			if ((_buttons & Buttons.Abort) == Buttons.Abort && Input.GetKeyDown(KeyCode.A))
			{
				buttons = Buttons.Abort;
			}
			if ((_buttons & Buttons.Retry) == Buttons.Retry && Input.GetKeyDown(KeyCode.R))
			{
				buttons = Buttons.Retry;
			}
			if ((_buttons & Buttons.Ignore) == Buttons.Ignore && Input.GetKeyDown(KeyCode.I))
			{
				buttons = Buttons.Ignore;
			}
			if (buttons != Buttons.None)
			{
				Result = buttons;
				hide();
			}
		}
	}

	public void show(string text, Action<Buttons> callback, Sprite icon, Buttons buttons = Buttons.Ok)
	{
		Result = Buttons.Cancel;
		_callback = callback;
		_buttons = buttons;
		setText(text);
		setIcon(icon);
		foreach (Transform item in _buttonPanel.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		if ((buttons & Buttons.Ok) == Buttons.Ok)
		{
			addButton("Ok", Buttons.Ok);
		}
		if ((buttons & Buttons.Cancel) == Buttons.Cancel)
		{
			addButton("Cancel", Buttons.Cancel);
		}
		if ((buttons & Buttons.Yes) == Buttons.Yes)
		{
			addButton("Yes", Buttons.Yes);
		}
		if ((buttons & Buttons.No) == Buttons.No)
		{
			addButton("No", Buttons.No);
		}
		if ((buttons & Buttons.Abort) == Buttons.Abort)
		{
			addButton("Abort", Buttons.Abort);
		}
		if ((buttons & Buttons.Retry) == Buttons.Retry)
		{
			addButton("Retry", Buttons.Retry);
		}
		if ((buttons & Buttons.Ignore) == Buttons.Ignore)
		{
			addButton("Ignore", Buttons.Ignore);
		}
		StartCoroutine(show_co(0.01f));
	}

	public MessageDialog setText(string text)
	{
		_messageText.text = text;
		return this;
	}

	public MessageDialog setIcon(Sprite icon)
	{
		_icon.sprite = icon;
		_icon.gameObject.SetActive(icon != null);
		return this;
	}

	public Button addButton(string text, Buttons result)
	{
		Button button = UnityEngine.Object.Instantiate(ButtonPrefab);
		button.transform.SetParent(_buttonPanel.transform, worldPositionStays: false);
		button.transform.Find("Text").GetComponent<Text>().text = text;
		button.onClick.AddListener(() =>
		{
			Result = result;
			hide();
		});
		return button;
	}

	protected override void hide()
	{
		base.hide();
		_callback(Result);
	}
}
