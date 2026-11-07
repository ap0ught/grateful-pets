using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace AiUnity.Common.LogUI.Scripts;

public class DoubleClickEvent : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public UnityEvent doubleClickEvent;

	public void OnPointerClick(PointerEventData eventData)
	{
		_ = eventData.clickCount;
		_ = 1;
	}
}
