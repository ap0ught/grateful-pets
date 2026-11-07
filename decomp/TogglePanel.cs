using UnityEngine;

public class TogglePanel : MonoBehaviour
{
	public void ToggleClick()
	{
		gameObject.SetActive(!gameObject.activeInHierarchy);
	}
}
