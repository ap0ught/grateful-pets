using System;
using System.Collections.Generic;
using System.Linq;
using Qud.UI;
using UnityEngine.Events;

namespace XRL.UI.Framework;

public class FilterBar : HorizontalScroller
{
	public ScrollContext<NavigationContext> rootContext = new ScrollContext<NavigationContext>();

	public List<string> enabledCategories = new List<string>(24);

	public FrameworkSearchInput searchInput;

	public HashSet<string> visibleCategories = new HashSet<string>();

	public List<FilterBarCategoryButton> categoryButtons;

	public UnityEvent<string> OnSearchTextChange = new UnityEvent<string>();

	public UnityEvent<string> OnCategorySelected = new UnityEvent<string>();

	public bool SingleCategoryOnly;

	public Action filtersUpdated;

	public override bool allowPrefabPoolingDuringLayoutChildren => true;

	public void SetupContext(bool includeScrollContext = true)
	{
		rootContext.SetAxis(InputAxisTypes.NavigationXAxis);
		rootContext.contexts.Clear();
		rootContext.contexts.Add(searchInput.context);
		if (includeScrollContext)
		{
			rootContext.contexts.Add(scrollContext);
		}
		rootContext.Setup();
		searchInput.OnSearchTextChange.RemoveAllListeners();
		searchInput.OnSearchTextChange.AddListener((string s) =>
		{
			OnSearchTextChange.Invoke(s);
		});
		scrollContext.SetAxis(NavigationAxis);
		scrollContext.wraps = false;
		scrollContext.contexts.Add(rootContext);
		scrollContext.Setup();
	}

	public void CategorySelected(string category)
	{
		if (SingleCategoryOnly)
		{
			enabledCategories.Clear();
			enabledCategories.Add(category);
		}
		else if (category == "*All")
		{
			if (!enabledCategories.Contains(category))
			{
				enabledCategories.Clear();
				enabledCategories.Add("*All");
			}
			else
			{
				enabledCategories.Remove("*All");
			}
		}
		else if (!enabledCategories.Contains(category))
		{
			enabledCategories.Add(category);
			enabledCategories.Remove("*All");
		}
		else
		{
			enabledCategories.Remove(category);
		}
		if (enabledCategories.Count == 0)
		{
			enabledCategories.Add("*All");
		}
		if (filtersUpdated != null)
		{
			filtersUpdated();
		}
	}

	public void ResetFilters()
	{
		if (searchInput?.context?.inputText != null)
		{
			searchInput.context.inputText = "";
		}
		enabledCategories.Clear();
		enabledCategories.Add("*All");
	}

	public void CategoryLeft()
	{
		if (choices.Count != 0)
		{
			int num = choices.FindIndex((FrameworkDataElement i) => (i as FilterBarCategoryButtonData).category == enabledCategories.FirstOrDefault());
			num--;
			if (num < 0)
			{
				num = Math.Max(0, choices.Count - 1);
			}
			enabledCategories.Clear();
			enabledCategories.Add((choices[num] as FilterBarCategoryButtonData).category);
			UpdateViewFromData();
			if (filtersUpdated != null)
			{
				filtersUpdated();
			}
		}
	}

	public void CategoryRight()
	{
		if (choices.Count != 0)
		{
			int num = choices.FindIndex((FrameworkDataElement i) => (i as FilterBarCategoryButtonData).category == enabledCategories.FirstOrDefault());
			num++;
			if (num >= choices.Count)
			{
				num = 0;
			}
			enabledCategories.Clear();
			enabledCategories.Add((choices[num] as FilterBarCategoryButtonData).category);
			UpdateViewFromData();
			if (filtersUpdated != null)
			{
				filtersUpdated();
			}
		}
	}

	public void UpdateViewFromData()
	{
		foreach (FrameworkDataElement choice in choices)
		{
			if (choice is FilterBarCategoryButtonData filterBarCategoryButtonData)
			{
				filterBarCategoryButtonData.button.FiltersUpdated(this);
			}
		}
	}

	public void SetCategoriesViaButtonBarData(IEnumerable<FilterBarCategoryButtonData> data)
	{
		choices.Clear();
		choices.AddRange(data);
		lastContexts.Clear();
		lastContexts.AddRange(scrollContext.contexts);
		scrollContext.contexts.Clear();
		LayoutChildren();
		scrollContext.Setup();
		UpdateViewFromData();
	}

	public void SetCategories(IEnumerable<string> categories)
	{
		choices.Clear();
		if (categories != null)
		{
			choices.AddRange(categories.Select((string c) => new FilterBarCategoryButtonData(c, (string category) =>
			{
				CategorySelected(category);
			})));
		}
		lastContexts.Clear();
		lastContexts.AddRange(scrollContext.contexts);
		scrollContext.contexts.Clear();
		LayoutChildren();
		scrollContext.Setup();
		UpdateViewFromData();
	}
}
