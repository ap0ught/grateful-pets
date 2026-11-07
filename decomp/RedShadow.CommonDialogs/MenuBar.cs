using System.Collections.Generic;
using UnityEngine;

namespace RedShadow.CommonDialogs;

public class MenuBar : MonoBehaviour
{
	public MenuBarButton MenuBarButtonPrefab;

	private readonly List<MenuBarButton> _menus = new List<MenuBarButton>();

	public MenuBarButton addMenu(string text, Menu menu)
	{
		menu.DestroyOnClose = false;
		MenuBarButton component = Object.Instantiate(MenuBarButtonPrefab).GetComponent<MenuBarButton>();
		GameObject gameObject = transform.Find("Panel").gameObject;
		component.transform.SetParent(gameObject.transform, worldPositionStays: false);
		component.setText(text);
		component.Menu = menu;
		_menus.Add(component);
		return component;
	}

	public MenuBarButton insertMenu(string text, Menu menu, string before)
	{
		MenuBarButton menuBarButton = addMenu(text, menu);
		int num = _menus.FindIndex((MenuBarButton b) => b.Text == text);
		if (num != -1)
		{
			int siblingIndex = _menus[num].transform.GetSiblingIndex();
			menuBarButton.transform.SetSiblingIndex(siblingIndex);
		}
		return menuBarButton;
	}

	public MenuItem findItem(string text)
	{
		foreach (MenuBarButton menu in _menus)
		{
			MenuItem menuItem = menu.Menu.findItem(text);
			if ((bool)menuItem)
			{
				return menuItem;
			}
		}
		return null;
	}
}
