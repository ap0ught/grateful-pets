using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RedShadow.CommonDialogs;

public class FileListItem : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	public Color NormalColor;

	public Color HoverColor;

	public Color HighlightedColor;

	public string Filename;

	[FormerlySerializedAs("onDoubleClick")]
	[SerializeField]
	private Button.ButtonClickedEvent _onDoubleClick = new Button.ButtonClickedEvent();

	[FormerlySerializedAs("onRightClick")]
	[SerializeField]
	private Button.ButtonClickedEvent _onRightClick = new Button.ButtonClickedEvent();

	public bool Highlighted { get; private set; }

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

	public void setHighlighted(bool highlighted)
	{
		if (highlighted == Highlighted)
		{
			return;
		}
		Button component = gameObject.GetComponent<Button>();
		Highlighted = highlighted;
		if (Highlighted)
		{
			component.image.color = HighlightedColor;
		}
		else
		{
			component.image.color = NormalColor;
		}
		if (!highlighted)
		{
			return;
		}
		int childCount = transform.parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			FileListItem component2 = gameObject.transform.parent.GetChild(i).gameObject.GetComponent<FileListItem>();
			if (component2 != null && component2 != this)
			{
				component2.setHighlighted(highlighted: false);
			}
		}
	}

	public void toggle()
	{
		setHighlighted(!Highlighted);
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

	public void OnPointerEnter(PointerEventData eventData)
	{
		gameObject.GetComponent<Button>().image.color = (Highlighted ? HighlightedColor : HoverColor);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		gameObject.GetComponent<Button>().image.color = (Highlighted ? HighlightedColor : NormalColor);
	}
}
