using System;
using Qud.UI;

namespace XRL.UI.Framework;

public class FilterBarCategoryButtonData : FrameworkDataElement
{
	public FilterBarCategoryButton button;

	public string category;

	public string tooltip;

	public Action<string> onSelect;

	public FilterBarCategoryButtonData()
	{
	}

	public FilterBarCategoryButtonData(string category, Action<string> onSelect)
	{
		this.category = category;
		this.onSelect = onSelect;
	}
}
