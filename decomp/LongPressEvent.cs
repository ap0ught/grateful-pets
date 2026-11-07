using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class LongPressEvent : UIBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerExitHandler
{
	[Tooltip("How long must pointer be down on this object to trigger a long press")]
	public float durationThreshold = 1f;

	public UnityEvent onLongPress = new UnityEvent();

	private bool isPointerDown;

	private bool longPressTriggered;

	private float timePressStarted;

	private void Update()
	{
		if (isPointerDown && !longPressTriggered && Time.time - timePressStarted > durationThreshold)
		{
			longPressTriggered = true;
			onLongPress.Invoke();
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		timePressStarted = Time.time;
		isPointerDown = true;
		longPressTriggered = false;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		isPointerDown = false;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isPointerDown = false;
	}
}
