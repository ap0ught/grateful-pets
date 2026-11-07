using UnityEngine;
using UnityEngine.UI;

namespace XRL.UI.Framework;

public class HorizontalScrollerScroller : HorizontalScroller
{
	public RectTransform safeArea;

	private Media.SizeClass lastClass = Media.SizeClass.Unset;

	public override ScrollChildContext MakeContextFor(FrameworkDataElement data, int index)
	{
		return new ScrollChildContext
		{
			proxyTo = GetPrefabForIndex(index).GetComponent<IFrameworkControl>().GetNavigationContext()
		};
	}

	public void UpdateHighlightText()
	{
		int num = scrollContext.selectedPosition;
		FrameworkScroller component = GetPrefabForIndex(num).GetComponent<FrameworkScroller>();
		if (descriptionText != null)
		{
			string description = scrollContext.data[num].Description;
			if (component != null)
			{
				int index = component.scrollContext.selectedPosition;
				description = component.scrollContext.data[index].Description;
			}
			descriptionText?.SetText(description);
		}
	}

	public override void UpdateSelection()
	{
		base.UpdateSelection();
		UpdateHighlightText();
	}

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
