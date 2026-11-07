using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectPosition : MonoBehaviour
{
	private RectTransform scrollRectTransform;

	private RectTransform contentPanel;

	private RectTransform selectedRectTransform;

	private GameObject lastSelected;

	private void Start()
	{
		scrollRectTransform = GetComponent<RectTransform>();
		contentPanel = GetComponent<ScrollRect>().content;
	}

	private void Update()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (currentSelectedGameObject == null)
		{
			return;
		}
		float num = 0f;
		if (currentSelectedGameObject.transform.parent != contentPanel.transform)
		{
			if (currentSelectedGameObject.transform.parent == null || currentSelectedGameObject.transform.parent.parent != contentPanel.transform)
			{
				return;
			}
			num = 0f - (currentSelectedGameObject.transform.parent.GetComponent<RectTransform>().anchoredPosition.y - 50f);
		}
		if (!(currentSelectedGameObject == lastSelected))
		{
			selectedRectTransform = currentSelectedGameObject.GetComponent<RectTransform>();
			float num2 = Mathf.Abs(selectedRectTransform.anchoredPosition.y) + selectedRectTransform.rect.height / 2f + num;
			float num3 = Mathf.Abs(selectedRectTransform.anchoredPosition.y) - selectedRectTransform.rect.height / 2f + num;
			float y = contentPanel.anchoredPosition.y;
			float num4 = contentPanel.anchoredPosition.y + scrollRectTransform.rect.height;
			if (num2 > num4)
			{
				float y2 = num2 - scrollRectTransform.rect.height;
				contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x, y2);
			}
			else if (num3 < y)
			{
				contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x, num3 - selectedRectTransform.sizeDelta.y);
			}
			lastSelected = currentSelectedGameObject;
		}
	}
}
