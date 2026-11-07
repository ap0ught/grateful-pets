using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Qud.UI;

public class FocusOnClick : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public Selectable objectToFocus;

	public void OnPointerClick(PointerEventData eventData)
	{
		objectToFocus.Select();
		if (objectToFocus.gameObject.GetComponent<InputField>() != null)
		{
			objectToFocus.gameObject.GetComponent<InputField>().ActivateInputField();
		}
	}
}
