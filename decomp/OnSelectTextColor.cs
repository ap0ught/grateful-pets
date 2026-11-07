using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnSelectTextColor : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public Text textToChange;

	public Color onSelectColor;

	public Color onDeselectColor;

	void ISelectHandler.OnSelect(BaseEventData eventData)
	{
		textToChange.color = onSelectColor;
	}

	public void OnDeselect(BaseEventData eventData)
	{
		textToChange.color = onDeselectColor;
	}
}
