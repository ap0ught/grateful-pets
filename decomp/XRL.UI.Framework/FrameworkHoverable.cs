using UnityEngine;
using UnityEngine.EventSystems;

namespace XRL.UI.Framework;

[RequireComponent(typeof(FrameworkContext))]
public class FrameworkHoverable : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public float movementOffset = 2f;

	public bool waitingToMove;

	public Vector2 waitingPointerLocation;

	public NavigationContext context => GetComponent<FrameworkContext>()?.context;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (eventData.IsPointerMoving() && !Input.GetMouseButton(0))
		{
			context?.Activate();
			return;
		}
		waitingToMove = true;
		waitingPointerLocation = Input.mousePosition;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		waitingToMove = false;
	}

	public void Update()
	{
		if (waitingToMove && !Input.GetMouseButton(0) && (waitingPointerLocation - (Vector2)Input.mousePosition).magnitude >= movementOffset)
		{
			context?.Activate();
			waitingToMove = false;
		}
	}
}
