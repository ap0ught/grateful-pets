using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RedShadow.CommonDialogs;

public class ToolBarButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public Color NormalColor;

	public Color CheckedColor;

	public Material GrayScaleMaterial;

	[FormerlySerializedAs("onDoubleClick")]
	[SerializeField]
	private Button.ButtonClickedEvent _onDoubleClick = new Button.ButtonClickedEvent();

	[FormerlySerializedAs("onRightClick")]
	[SerializeField]
	private Button.ButtonClickedEvent _onRightClick = new Button.ButtonClickedEvent();

	public bool Checkable { get; private set; }

	public bool Checked { get; private set; }

	public string Text { get; private set; }

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

	public ToolBarButton setText(string text)
	{
		Text = text;
		return this;
	}

	public ToolBarButton setEnabled(bool enabled)
	{
		GetComponent<Button>().interactable = enabled;
		transform.Find("Image").GetComponent<Image>().material = (enabled ? null : GrayScaleMaterial);
		return this;
	}

	public ToolBarButton setCallback(UnityAction callback)
	{
		GetComponent<Button>().onClick.AddListener(callback);
		return this;
	}

	public ToolBarButton setIcon(Sprite icon)
	{
		transform.Find("Image").GetComponent<Image>().sprite = icon;
		setChecked(Checked);
		return this;
	}

	public ToolBarButton setCheckable(bool checkable)
	{
		Checkable = true;
		setChecked(Checked);
		return this;
	}

	public ToolBarButton setChecked(bool check)
	{
		if (!Checkable)
		{
			return this;
		}
		Checked = check;
		GetComponent<Button>().image.color = (Checked ? CheckedColor : NormalColor);
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
}
