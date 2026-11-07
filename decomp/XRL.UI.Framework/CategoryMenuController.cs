using UnityEngine;

namespace XRL.UI.Framework;

public class CategoryMenuController : MonoBehaviour, IFrameworkControl
{
	private FrameworkScroller _scroller;

	public FrameworkScroller scroller => _scroller ?? (_scroller = GetComponent<FrameworkScroller>());

	public NavigationContext GetNavigationContext()
	{
		return GetComponent<FrameworkScroller>().scrollContext;
	}

	public void setData(FrameworkDataElement dataElement)
	{
		CategoryMenuData categoryMenuData = dataElement as CategoryMenuData;
		scroller.BeforeShow(null, categoryMenuData.menuOptions);
		scroller.titleText.SetText(categoryMenuData.Title);
	}
}
