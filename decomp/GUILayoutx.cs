using UnityEngine;

public class GUILayoutx
{
	public delegate void DoubleClickCallback(int index);

	public static int SelectionList(int selected, GUIContent[] list)
	{
		return SelectionList(selected, list, "Label", null);
	}

	public static int SelectionList(int selected, GUIContent[] list, GUIStyle elementStyle)
	{
		return SelectionList(selected, list, elementStyle, null);
	}

	public static int SelectionList(int selected, GUIContent[] list, DoubleClickCallback callback)
	{
		return SelectionList(selected, list, "Label", callback);
	}

	public static int SelectionList(int selected, GUIContent[] list, GUIStyle elementStyle, DoubleClickCallback callback)
	{
		for (int i = 0; i < list.Length; i++)
		{
			Rect rect = GUILayoutUtility.GetRect(list[i], elementStyle);
			bool flag = rect.Contains(Event.current.mousePosition);
			if (flag && Event.current.type == EventType.MouseDown)
			{
				selected = i;
				callback(i);
				Event.current.Use();
			}
			else if (flag && callback != null && Event.current.type == EventType.MouseUp && Event.current.clickCount == 2)
			{
				Event.current.Use();
			}
			else if (Event.current.type == EventType.Repaint)
			{
				elementStyle.Draw(rect, list[i], flag, isActive: false, i == selected, hasKeyboardFocus: false);
			}
		}
		return selected;
	}

	public static int SelectionList(int selected, string[] list)
	{
		return SelectionList(selected, list, "Label", null);
	}

	public static int SelectionList(int selected, string[] list, GUIStyle elementStyle)
	{
		return SelectionList(selected, list, elementStyle, null);
	}

	public static int SelectionList(int selected, string[] list, DoubleClickCallback callback)
	{
		return SelectionList(selected, list, "Label", callback);
	}

	public static int SelectionList(int selected, string[] list, GUIStyle elementStyle, DoubleClickCallback callback)
	{
		for (int i = 0; i < list.Length; i++)
		{
			Rect rect = GUILayoutUtility.GetRect(new GUIContent(list[i]), elementStyle);
			bool flag = rect.Contains(Event.current.mousePosition);
			if (flag && Event.current.type == EventType.MouseDown)
			{
				selected = i;
				Event.current.Use();
			}
			else if (flag && callback != null && Event.current.type == EventType.MouseUp && Event.current.clickCount == 2)
			{
				callback(i);
				Event.current.Use();
			}
			else if (Event.current.type == EventType.Repaint)
			{
				elementStyle.Draw(rect, list[i], flag, isActive: false, i == selected, hasKeyboardFocus: false);
			}
		}
		return selected;
	}
}
