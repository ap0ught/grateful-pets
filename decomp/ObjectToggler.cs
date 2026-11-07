using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectToggler : MonoBehaviour
{
	public List<GameObject> objects = new List<GameObject>();

	public bool toggled = true;

	public Selectable button;

	public void PoolReset()
	{
		button.interactable = false;
		button.interactable = true;
		toggled = true;
		transform.Find("Plus").GetComponent<Text>().text = (toggled ? "-" : "+");
		objects.Clear();
	}

	public void Toggle()
	{
		toggled = !toggled;
		for (int i = 0; i < objects.Count; i++)
		{
			objects[i].SetActive(toggled);
		}
		transform.Find("Plus").GetComponent<Text>().text = (toggled ? "-" : "+");
	}
}
