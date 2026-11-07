using UnityEngine;
using UnityEngine.EventSystems;

namespace AiUnity.Common.LogUI.Scripts;

public class FocusPanel : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	private RectTransform panel;

	public void OnPointerDown(PointerEventData data)
	{
		panel.SetAsLastSibling();
	}

	private void Awake()
	{
		panel = GetComponent<RectTransform>();
	}
}
