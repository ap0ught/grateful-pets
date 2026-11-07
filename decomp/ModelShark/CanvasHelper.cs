using UnityEngine;

namespace ModelShark;

public static class CanvasHelper
{
	public static Canvas GetRootCanvas()
	{
		Canvas[] array = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.InstanceID);
		if (array.Length == 0)
		{
			Debug.LogError("No canvas found in scene.");
			return null;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].isRootCanvas)
			{
				return array[i];
			}
		}
		Debug.LogError("No canvas found at the root level of the scene.");
		return null;
	}
}
