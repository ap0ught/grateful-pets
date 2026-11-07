using System.Collections.Generic;
using UnityEngine;

namespace Qud.UI;

public class QudMenuBottomContext : MonoBehaviour
{
	public GameObject leftBar;

	public GameObject rightBar;

	public GameObject buttonPrefab;

	public List<QudMenuItem> items;

	public List<SelectableTextMenuItem> buttons;

	public QudBaseMenuController controller;

	private int _lastHash;

	public void RefreshButtons()
	{
		while (buttons.Count > items.Count)
		{
			SelectableTextMenuItem selectableTextMenuItem = buttons[buttons.Count - 1];
			buttons.RemoveAt(buttons.Count - 1);
			selectableTextMenuItem.gameObject.DestroyImmediate();
		}
		while (items.Count > buttons.Count)
		{
			GameObject gameObject = buttonPrefab.Instantiate();
			gameObject.transform.SetParent(rightBar.transform.parent, worldPositionStays: false);
			gameObject.transform.SetSiblingIndex(rightBar.transform.GetSiblingIndex());
			buttons.Add(gameObject.GetComponent<SelectableTextMenuItem>());
		}
		for (int i = 0; i < buttons.Count; i++)
		{
			buttons[i].data = items[i];
			buttons[i].controller = controller;
			buttons[i].Update();
		}
	}

	public void Update()
	{
		RefreshButtons();
	}
}
