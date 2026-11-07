using UnityEngine;

namespace AiUnity.Common.LogUI.Scripts;

public class MaximizePanel : MonoBehaviour
{
	private RectTransform GameConsolePanelTransform;

	private bool isMaximized;

	private RectTransformStorage WindowStorage;

	public void MaxHandler()
	{
		if (isMaximized)
		{
			WindowStorage.Restore(GameConsolePanelTransform);
			isMaximized = false;
			return;
		}
		WindowStorage.Store(GameConsolePanelTransform);
		GameConsolePanelTransform.anchorMin = new Vector2(0f, 1f);
		GameConsolePanelTransform.anchorMax = new Vector2(1f, 1f);
		GameConsolePanelTransform.sizeDelta = new Vector2(0f, GameConsolePanelTransform.sizeDelta.y);
		GameConsolePanelTransform.anchoredPosition = Vector3.zero;
		isMaximized = true;
	}

	private void Start()
	{
		GameConsolePanelTransform = (RectTransform)transform;
	}
}
