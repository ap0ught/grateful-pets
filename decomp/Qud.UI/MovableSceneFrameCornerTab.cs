using UnityEngine;
using UnityEngine.EventSystems;

namespace Qud.UI;

public class MovableSceneFrameCornerTab : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IDragHandler
{
	public GameObject highlightTarget;

	public GameObject dragTarget;

	public string prefsKey = "MessageLog";

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (dragTarget.GetComponent<RectTransform>().anchorMin.x == 1f)
		{
			dragTarget.GetComponent<RectTransform>().sizeDelta += new Vector2(eventData.delta.x, 0f - eventData.delta.y) / UIManager.scale;
			dragTarget.GetComponent<RectTransform>().anchoredPosition += new Vector2(eventData.delta.x, eventData.delta.y) / UIManager.scale;
		}
		else if (dragTarget.GetComponent<RectTransform>().anchorMin.x == 0f)
		{
			dragTarget.GetComponent<RectTransform>().sizeDelta += new Vector2(eventData.delta.x, 0f - eventData.delta.y) / UIManager.scale;
			dragTarget.GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, eventData.delta.y) / UIManager.scale;
		}
	}
}
