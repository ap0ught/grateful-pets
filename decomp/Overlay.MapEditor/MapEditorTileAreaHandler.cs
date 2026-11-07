using UnityEngine;
using UnityEngine.EventSystems;

namespace Overlay.MapEditor;

public class MapEditorTileAreaHandler : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IScrollHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
	public bool PointerInside;

	private MapEditorView MapEditor => MapEditorView.instance;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!eventData.dragging)
		{
			MapEditor.OnClicked(eventData);
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		GameManager.Instance.OnScroll(eventData.scrollDelta);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		PointerInside = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		PointerInside = false;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!PointerInside)
		{
			return;
		}
		RaycastHit[] array = Physics.RaycastAll(GameManager.MainCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition));
		foreach (RaycastHit raycastHit in array)
		{
			TileBehavior component = raycastHit.collider.gameObject.GetComponent<TileBehavior>();
			if (component != null)
			{
				MapEditorView.Instance.UpdateHoverXY(component.x, component.y);
			}
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		TileBehavior t = null;
		if (PointerInside)
		{
			RaycastHit[] array = Physics.RaycastAll(GameManager.MainCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition));
			foreach (RaycastHit raycastHit in array)
			{
				TileBehavior component = raycastHit.collider.gameObject.GetComponent<TileBehavior>();
				if (component != null)
				{
					t = component;
				}
			}
		}
		MapEditor.OnBeginDrag(eventData, t);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
	}

	public void OnDrag(PointerEventData eventData)
	{
		MapEditorView.instance.OnDrag(eventData);
	}
}
