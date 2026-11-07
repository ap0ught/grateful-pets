using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace QupKit;

public class ButtonControl : BaseControl
{
	public ButtonControl(string ID, string Text, int Width, int Height, Action<PointerEventData> OnClick = null)
	{
		rootObject = PrefabManager.Create("Button");
		rootObject.name = ID;
		rootObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Width, Height);
		Label = Text;
		base.Width = rootObject.GetComponent<RectTransform>().sizeDelta.x;
		base.Height = rootObject.GetComponent<RectTransform>().sizeDelta.y;
		OnClicked = OnClick;
	}
}
