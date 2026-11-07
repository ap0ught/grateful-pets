using UnityEngine;
using UnityEngine.UI;

namespace XRL.UI.Framework;

public class HorizontalIconScroller : HorizontalScroller
{
	public GameObject gridContainer;

	public RectTransform safeArea;

	private Media.SizeClass lastClass = Media.SizeClass.Unset;

	public void checkSafeArea()
	{
		if (!(safeArea == null) && Media.sizeClass != lastClass)
		{
			lastClass = Media.sizeClass;
			if (Media.sizeClass < Media.SizeClass.Medium)
			{
				safeArea.anchoredPosition = new Vector2(0f, 0f);
				safeArea.sizeDelta = new Vector2(0f, -150f);
			}
			else
			{
				safeArea.anchoredPosition = new Vector2(0f, -25f);
				safeArea.sizeDelta = new Vector2(-300f, -250f);
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(safeArea);
		}
	}

	public override void Awake()
	{
		checkSafeArea();
		base.Awake();
	}

	public override void Update()
	{
		checkSafeArea();
		base.Update();
	}
}
