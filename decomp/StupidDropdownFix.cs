using UnityEngine;

public class StupidDropdownFix : MonoBehaviour
{
	private Canvas c;

	private void Update()
	{
		if (c == null)
		{
			c = GetComponent<Canvas>();
		}
		if (c != null && !c.overrideSorting)
		{
			c.overrideSorting = true;
			c.sortingLayerName = "User Interface";
		}
	}
}
