using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XRL.UI.Framework;

[RequireComponent(typeof(FrameworkContext))]
public class FrameworkClickToAccept : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler
{
	public bool requireActiveContext = true;

	public NavigationContext context => GetComponent<FrameworkContext>()?.context;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!requireActiveContext || (context != null && context.IsActive()))
		{
			if (!requireActiveContext && !context.IsActive(checkParents: false))
			{
				context.Activate();
			}
			NavigationController.instance.FireInputButtonEvent(InputButtonTypes.AcceptButton, new Dictionary<string, object> { { "PointerEventData", eventData } });
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	public void OnPointerUp(PointerEventData eventData)
	{
	}
}
