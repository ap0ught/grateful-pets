using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RedShadow.CommonDialogs;

public class MenuItem : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public Color NormalColor;

	public Color HoverColor;

	public Sprite CheckedSprite;

	public Sprite UncheckedSprite;

	public Material GrayScaleMaterial;

	public bool Radio;

	public bool Checkable;

	[FormerlySerializedAs("onDoubleClick")]
	[SerializeField]
	private Button.ButtonClickedEvent _onDoubleClick = new Button.ButtonClickedEvent();

	[FormerlySerializedAs("onRightClick")]
	[SerializeField]
	private Button.ButtonClickedEvent _onRightClick = new Button.ButtonClickedEvent();

	public Action onMenuItemClicked;

	public bool Checked { get; private set; }

	public string Text { get; private set; }

	public string HotKeyText { get; private set; }

	public Button.ButtonClickedEvent onDoubleClick
	{
		get
		{
			return _onDoubleClick;
		}
		set
		{
			_onDoubleClick = value;
		}
	}

	public Button.ButtonClickedEvent onRightClick
	{
		get
		{
			return _onRightClick;
		}
		set
		{
			_onRightClick = value;
		}
	}

	public void onMouseEnter()
	{
		GetComponent<Button>().image.color = HoverColor;
	}

	public void onMouseExit()
	{
		GetComponent<Button>().image.color = NormalColor;
	}

	public MenuItem setText(string text)
	{
		Text = text;
		transform.Find("Text").GetComponent<TextMeshProUGUI>().text = text;
		return this;
	}

	public MenuItem setHotKeyText(string text)
	{
		HotKeyText = text;
		transform.Find("HotKeyText").GetComponent<Text>().text = text;
		return this;
	}

	public MenuItem setEnabled(bool enabled)
	{
		GetComponent<Button>().interactable = enabled;
		transform.Find("Text").GetComponent<Text>().color = (enabled ? Color.white : Color.gray);
		transform.Find("HotKeyText").GetComponent<Text>().color = (enabled ? Color.white : Color.gray);
		transform.Find("Image").GetComponent<Image>().material = (enabled ? null : GrayScaleMaterial);
		transform.Find("CheckBoxImage").GetComponent<Image>().material = (enabled ? null : GrayScaleMaterial);
		return this;
	}

	public MenuItem setCallback(UnityAction callback)
	{
		GetComponent<Button>().onClick.AddListener(callback);
		return this;
	}

	public MenuItem setIcon(Sprite icon)
	{
		Image component = transform.Find("Image").GetComponent<Image>();
		component.sprite = icon;
		component.gameObject.SetActive(icon != null);
		setChecked(Checked);
		return this;
	}

	public MenuItem setCheckable(bool checkable)
	{
		Checkable = true;
		setChecked(Checked);
		return this;
	}

	public void showCheckbox(bool show)
	{
		transform.Find("CheckBoxImage").gameObject.SetActive(show);
		transform.Find("CheckBoxImage").GetComponent<Image>().enabled = Checkable;
	}

	public MenuItem setChecked(bool check)
	{
		if (!Checkable)
		{
			return this;
		}
		Checked = check;
		transform.Find("CheckBoxImage").GetComponent<Image>().sprite = (Checked ? CheckedSprite : UncheckedSprite);
		if (Radio & check)
		{
			int childCount = gameObject.transform.parent.childCount;
			for (int i = 0; i < childCount; i++)
			{
				MenuItem component = gameObject.transform.parent.GetChild(i).gameObject.GetComponent<MenuItem>();
				if (component != null && component != this && component.Radio)
				{
					component.setChecked(check: false);
				}
			}
		}
		return this;
	}

	public void toggle()
	{
		setChecked(!Checked);
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			_onRightClick.Invoke();
		}
		if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
		{
			_onDoubleClick.Invoke();
		}
	}

	public MenuItem setMenuItemClicked(Action a)
	{
		onMenuItemClicked = a;
		return this;
	}

	public void OnMenuItemClicked()
	{
		if (onMenuItemClicked != null)
		{
			onMenuItemClicked();
		}
	}
}
