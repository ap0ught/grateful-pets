using UnityEngine;
using UnityEngine.EventSystems;

public class DragTab : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IDragHandler
{
	public GameObject highlightTarget;

	public GameObject dragTarget;

	public void OnPointerEnter(PointerEventData eventData)
	{
		highlightTarget.SetActive(value: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		highlightTarget.SetActive(value: false);
	}

	public void OnDrag(PointerEventData eventData)
	{
		dragTarget.GetComponent<RectTransform>().sizeDelta -= new Vector2(0f, eventData.delta.y);
		dragTarget.GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, eventData.delta.y / 2f);
	}
}
