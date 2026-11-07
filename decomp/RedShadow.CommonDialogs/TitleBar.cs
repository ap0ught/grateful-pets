using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RedShadow.CommonDialogs;

public class TitleBar : MonoBehaviour
{
	public Vector2 MinSize;

	private Text _titleText;

	private Button _closeButton;

	private Button _maximizeButton;

	private Button _restoreButton;

	private Rect _restoreRect;

	private DragPanel _dragPanel;

	[FormerlySerializedAs("onClose")]
	[SerializeField]
	private Button.ButtonClickedEvent _onClose = new Button.ButtonClickedEvent();

	public Button.ButtonClickedEvent onClose
	{
		get
		{
			return _onClose;
		}
		set
		{
			_onClose = value;
		}
	}

	public bool Maximized { get; private set; }

	protected void Awake()
	{
		_titleText = transform.Find("Panel/Text").GetComponent<Text>();
		_closeButton = transform.Find("Panel/CloseButton").GetComponent<Button>();
		_restoreButton = transform.Find("Panel/RestoreButton").GetComponent<Button>();
		_maximizeButton = transform.Find("Panel/MaximizeButton").GetComponent<Button>();
		_dragPanel = transform.Find("Panel/Text").GetComponent<DragPanel>();
	}

	public TitleBar showSizeButtons(bool show)
	{
		transform.Find("Panel/MinimizeButton").gameObject.SetActive(show);
		_restoreButton.gameObject.SetActive(show && Maximized);
		_maximizeButton.gameObject.SetActive(show && !Maximized);
		return this;
	}

	public TitleBar setTitleText(string text)
	{
		_titleText.text = text;
		return this;
	}

	public TitleBar showCloseButton(bool show)
	{
		_closeButton.gameObject.SetActive(show);
		return this;
	}

	public TitleBar setAllowDrag(bool draggable)
	{
		_dragPanel.gameObject.SetActive(draggable);
		return this;
	}

	public void onMinimize()
	{
		_restoreRect = new Rect(_dragPanel.TargetPanel.localPosition, _dragPanel.TargetPanel.sizeDelta);
		_dragPanel.TargetPanel.localPosition = Vector2.zero;
		_dragPanel.TargetPanel.sizeDelta = MinSize;
		_dragPanel.clampToWindow();
		_restoreButton.gameObject.SetActive(value: true);
		_maximizeButton.gameObject.SetActive(value: false);
	}

	public void onRestore()
	{
		_dragPanel.TargetPanel.localPosition = _restoreRect.position;
		_dragPanel.TargetPanel.sizeDelta = _restoreRect.size;
		_dragPanel.clampToWindow();
		_restoreButton.gameObject.SetActive(value: false);
		_maximizeButton.gameObject.SetActive(value: true);
	}

	public void onMaximize()
	{
		_restoreRect = new Rect(_dragPanel.TargetPanel.localPosition, _dragPanel.TargetPanel.sizeDelta);
		RectTransform component = _dragPanel.TargetPanel.parent.GetComponent<RectTransform>();
		_dragPanel.TargetPanel.localPosition = component.localPosition;
		_dragPanel.TargetPanel.sizeDelta = component.rect.size;
		_dragPanel.clampToWindow();
		_restoreButton.gameObject.SetActive(value: true);
		_maximizeButton.gameObject.SetActive(value: false);
		Maximized = true;
	}

	public void onCloseButton()
	{
		_onClose.Invoke();
	}
}
