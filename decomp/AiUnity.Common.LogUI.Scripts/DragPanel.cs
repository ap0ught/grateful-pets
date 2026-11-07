using UnityEngine;
using UnityEngine.EventSystems;

namespace AiUnity.Common.LogUI.Scripts;

public class DragPanel : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IDragHandler
{
	public RectTransform panelRectTransform;

	private RectTransform canvasRectTransform;

	private Vector2 pointerOffset;

	public void OnDrag(PointerEventData data)
	{
		if (enabled && !(panelRectTransform == null))
		{
			Vector2 screenPoint = ClampToWindow(data);
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, data.pressEventCamera, out var localPoint))
			{
				panelRectTransform.localPosition = localPoint - pointerOffset;
			}
		}
	}

	public void OnPointerDown(PointerEventData data)
	{
		if (enabled)
		{
			panelRectTransform.SetAsLastSibling();
			RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, data.position, data.pressEventCamera, out pointerOffset);
		}
	}

	private void Awake()
	{
		Canvas componentInParent = GetComponentInParent<Canvas>();
		if (componentInParent != null)
		{
			canvasRectTransform = componentInParent.transform as RectTransform;
		}
	}

	private Vector2 ClampToWindow(PointerEventData data)
	{
		Vector2 position = data.position;
		Vector3[] array = new Vector3[4];
		canvasRectTransform.GetWorldCorners(array);
		float x = Mathf.Clamp(position.x, array[0].x, array[2].x);
		float y = Mathf.Clamp(position.y, array[0].y, array[2].y);
		return new Vector2(x, y);
	}
}
