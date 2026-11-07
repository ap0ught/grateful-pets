using UnityEngine;

namespace XRL.UI.Framework;

public class CategoryIconScroller : MonoBehaviour, IFrameworkControl
{
	private FrameworkScroller _scroller;

	public FrameworkScroller scroller => _scroller ?? (_scroller = GetComponent<FrameworkScroller>());

	public void setData(FrameworkDataElement data)
	{
		CategoryIcons categoryIcons = data as CategoryIcons;
		scroller.BeforeShow(null, categoryIcons.Choices);
		scroller.titleText.color = Color.white;
		scroller.titleText.SetText(categoryIcons.Title);
	}

	public NavigationContext GetNavigationContext()
	{
		return scroller?.GetNavigationContext();
	}
}
