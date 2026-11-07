using UnityEngine;
using UnityEngine.EventSystems;

namespace AiUnity.Common.LogUI.Scripts;

public class ResizePanel : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
	public Vector2 maxSize;

	public Vector2 minSize;

	public Texture2D cursorTexture;

	private Vector2 cursorHotspot;

	private Vector2 currentPointerPosition;

	private Vector2 previousPointerPosition;

	private RectTransform rectTransform;

	public void OnDrag(PointerEventData data)
	{
		if (!(rectTransform == null))
		{
			Vector2 sizeDelta = rectTransform.sizeDelta;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out currentPointerPosition);
			Vector2 vector = currentPointerPosition - previousPointerPosition;
			sizeDelta += new Vector2(vector.x, 0f - vector.y);
			sizeDelta = new Vector2(Mathf.Clamp(sizeDelta.x, minSize.x, maxSize.x), Mathf.Clamp(sizeDelta.y, minSize.y, maxSize.y));
			rectTransform.sizeDelta = sizeDelta;
			previousPointerPosition = currentPointerPosition;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}

	public void OnPointerDown(PointerEventData data)
	{
		rectTransform.SetAsLastSibling();
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out previousPointerPosition);
	}

	private void Awake()
	{
		rectTransform = transform.parent.GetComponent<RectTransform>();
		cursorHotspot = new Vector2(cursorTexture.width / 4, cursorTexture.height / 4);
	}
}
