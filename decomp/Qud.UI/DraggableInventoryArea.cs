using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Qud.UI;

public class DraggableInventoryArea : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public string ID;

	public static DraggableInventoryArea inside;

	public Image highlight;

	public void Awake()
	{
		if (highlight != null)
		{
			highlight.enabled = false;
		}
	}

	public void Update()
	{
		if (highlight != null)
		{
			highlight.enabled = highlight != null && inside == this && ((ID == "Inventory" && EquipmentLine.dragging && InventoryLine.dragObject == null) || (ID == "Equipment" && InventoryLine.dragging && EquipmentLine.dragObject == null));
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		inside = this;
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		inside = null;
	}
}
