using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Qud.UI;

public class HoverControlsImageAlpha : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public Image image;

	private void Awake()
	{
		if (image != null)
		{
			image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (image != null)
		{
			image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (image != null)
		{
			image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
		}
	}
}
