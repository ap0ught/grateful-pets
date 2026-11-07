using UnityEngine;
using UnityEngine.EventSystems;

namespace RedShadow.CommonDialogs;

public class DragPanel : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IDragHandler
{
	public RectTransform TargetPanel;

	private Vector2 _originalLocalPointerPosition;

	private Vector3 _originalPanelLocalPosition;

	private RectTransform _panelRectTransform;

	private RectTransform _parentRectTransform;

	public Vector2 getPosition()
	{
		return _panelRectTransform.localPosition;
	}

	public void setPosition(Vector2 pos)
	{
		_panelRectTransform.localPosition = pos;
	}

	protected void Awake()
	{
		_panelRectTransform = TargetPanel;
		_parentRectTransform = TargetPanel.parent.GetComponent<RectTransform>();
	}

	public void OnPointerDown(PointerEventData data)
	{
		_originalPanelLocalPosition = _panelRectTransform.localPosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRectTransform, data.position, data.pressEventCamera, out _originalLocalPointerPosition);
	}

	public void OnDrag(PointerEventData data)
	{
		if (!(_panelRectTransform == null) && !(_parentRectTransform == null))
		{
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRectTransform, data.position, data.pressEventCamera, out var localPoint))
			{
				Vector3 vector = localPoint - _originalLocalPointerPosition;
				_panelRectTransform.localPosition = _originalPanelLocalPosition + vector;
			}
			clampToWindow();
		}
	}

	public void clampToWindow()
	{
		Vector3 localPosition = _panelRectTransform.localPosition;
		Vector3 vector = _parentRectTransform.rect.min - _panelRectTransform.rect.min;
		Vector3 vector2 = _parentRectTransform.rect.max - _panelRectTransform.rect.max;
		localPosition.x = Mathf.Clamp(localPosition.x, vector.x, vector2.x);
		localPosition.y = Mathf.Clamp(localPosition.y, vector.y, vector2.y);
		_panelRectTransform.localPosition = localPosition;
	}
}
