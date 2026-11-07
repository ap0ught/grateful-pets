using System.Collections.Generic;
using UnityEngine;

namespace QupKit;

public class ScrollViewControl : BaseControl
{
	public GameObject Viewport;

	public GameObject Content;

	public Vector3 TopCorner = new Vector3(25f, -80f, 0f);

	public LayoutSides ChildBorder = new LayoutSides(10f, 0f, 0f, 0f);

	public List<GameObject> ListChildren = new List<GameObject>();

	public ScrollViewControl(GameObject ScrollView)
	{
		rootObject = ScrollView;
		Width = ScrollView.GetComponent<RectTransform>().sizeDelta.x;
		Height = ScrollView.GetComponent<RectTransform>().sizeDelta.y;
		rootObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Width, Height);
		Viewport = rootObject.transform.GetChild(0).gameObject;
		Content = Viewport.transform.GetChild(0).gameObject;
		foreach (GameObject item in Content.transform)
		{
			ListChildren.Add(item);
		}
	}

	public void AddChild(GameObject NewChild)
	{
		NewChild.transform.parent = Content.transform;
		ListChildren.Add(NewChild);
	}

	public void AddChild(GameObject NewChild, string NewID)
	{
		NewChild.name = NewID;
		AddChild(NewChild);
	}

	public override void BeforeLayout()
	{
		Vector3 topCorner = TopCorner;
		for (int i = 0; i < ListChildren.Count; i++)
		{
			RectTransform component = ListChildren[i].GetComponent<RectTransform>();
			topCorner.y -= ChildBorder.Top;
			ListChildren[i].GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
			ListChildren[i].GetComponent<RectTransform>().anchorMax = new Vector2(0f, 1f);
			ListChildren[i].GetComponent<RectTransform>().anchoredPosition3D = new Vector3(component.sizeDelta.x / 2f + topCorner.x, (0f - component.sizeDelta.y) / 2f + topCorner.y, 0f);
			topCorner.y -= component.sizeDelta.y;
			topCorner.y -= ChildBorder.Bottom;
		}
		Content.GetComponent<RectTransform>().sizeDelta = new Vector2(Content.GetComponent<RectTransform>().sizeDelta.x, 0f - topCorner.y);
	}
}
