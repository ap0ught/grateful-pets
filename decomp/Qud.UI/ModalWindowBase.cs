using UnityEngine;

namespace Qud.UI;

public class ModalWindowBase<T> : WindowBase where T : WindowBase, new()
{
	public static T instance;

	public static void ShowModal()
	{
		instance.Show();
	}

	public override void Init()
	{
		instance = this as T;
	}

	public override void Hide()
	{
		canvas.enabled = false;
		raycaster.enabled = false;
		if (this != instance)
		{
			Object.Destroy(gameObject);
		}
	}

	public override void Show()
	{
		GameObject gameObject = Object.Instantiate(base.gameObject);
		gameObject.SetActive(value: true);
		gameObject.GetComponent<ModalWindowBase<T>>().canvas.enabled = true;
		gameObject.GetComponent<ModalWindowBase<T>>().raycaster.enabled = true;
		gameObject.transform.SetParent(base.gameObject.transform.parent, worldPositionStays: false);
		gameObject.transform.SetAsLastSibling();
		RectTransform component = GetComponent<RectTransform>();
		RectTransform component2 = gameObject.GetComponent<RectTransform>();
		component2.anchorMin = component.anchorMin;
		component2.anchorMax = component.anchorMax;
		component2.anchoredPosition = component.anchoredPosition;
		component2.sizeDelta = component.sizeDelta;
	}
}
