using UnityEngine;
using UnityEngine.EventSystems;

namespace RedShadow.CommonDialogs;

public class ResizePanel : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IDragHandler
{
	public RectTransform TargetPanel;

	public Vector2 MinSize = new Vector2(100f, 100f);

	public Vector2 MaxSize = new Vector2(400f, 400f);

	private RectTransform _panelRectTransform;

	private RectTransform _parentRectTransform;

	private Vector2 _originalLocalPointerPosition;

	private Vector2 _originalSizeDelta;

	public Vector2 getSize()
	{
		return _panelRectTransform.sizeDelta;
	}

	public void setSize(Vector2 size)
	{
		_panelRectTransform.sizeDelta = size;
	}

	protected void Awake()
	{
		if (TargetPanel == null)
		{
			_panelRectTransform = GetComponent<RectTransform>();
			_parentRectTransform = _panelRectTransform.parent.GetComponent<RectTransform>();
		}
		else
		{
			_panelRectTransform = TargetPanel;
			_parentRectTransform = TargetPanel.parent.GetComponent<RectTransform>();
		}
		if (MaxSize.x <= 0f)
		{
			MaxSize.x = Screen.width;
		}
		if (MaxSize.y <= 0f)
		{
			MaxSize.y = Screen.height;
		}
	}

	public void OnPointerDown(PointerEventData data)
	{
		_originalSizeDelta = _panelRectTransform.sizeDelta;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_panelRectTransform, data.position, data.pressEventCamera, out _originalLocalPointerPosition);
	}

	public void OnDrag(PointerEventData data)
	{
		if (!(_panelRectTransform == null))
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_panelRectTransform, data.position, data.pressEventCamera, out var localPoint);
			Vector3 vector = localPoint - _originalLocalPointerPosition;
			Vector2 vector2 = _originalSizeDelta + new Vector2(vector.x, 0f - vector.y);
			vector2 = new Vector2(Mathf.Clamp(vector2.x, MinSize.x, MaxSize.x), Mathf.Clamp(vector2.y, MinSize.y, MaxSize.y));
			_panelRectTransform.sizeDelta = vector2;
			clampToWindow();
		}
	}

	private void clampToWindow()
	{
		Vector3 localPosition = _panelRectTransform.localPosition;
		Vector3 vector = _parentRectTransform.rect.min - _panelRectTransform.rect.min;
		Vector3 vector2 = _parentRectTransform.rect.max - _panelRectTransform.rect.max;
		localPosition.x = Mathf.Clamp(localPosition.x, vector.x, vector2.x);
		localPosition.y = Mathf.Clamp(localPosition.y, vector.y, vector2.y);
		Vector3 vector3 = localPosition - _panelRectTransform.localPosition;
		_panelRectTransform.sizeDelta += new Vector2(vector3.x, 0f - vector3.y);
	}
}
