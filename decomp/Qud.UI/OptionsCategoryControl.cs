using System.Collections.Generic;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[ExecuteInEditMode]
public class OptionsCategoryControl : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts
{
	private class Context : NavigationContext
	{
	}

	public OptionsCategoryRow data;

	public UITextSkin categoryExpander;

	public UITextSkin title;

	private Context context;

	public bool selectedMode;

	public static MenuOption TOGGLE_OPTION = new MenuOption
	{
		InputCommand = "Accept",
		Description = "Toggle Visibilty",
		disabled = true
	};

	public void SetupContexts(ScrollChildContext scontext)
	{
		if (this.context != null && this.context.IsActive() && this.context.parentContext is ScrollChildContext scrollChildContext && scrollChildContext.index != scontext.index)
		{
			this.context = new Context();
		}
		else if (NavigationController.instance.activeContext is Context { parentContext: ScrollChildContext parentContext } context && parentContext.index == scontext.index)
		{
			this.context = context;
		}
		if (scontext != null)
		{
			scontext.proxyTo = this.context ?? (this.context = new Context());
			Context context2 = this.context;
			if (context2.menuOptionDescriptions == null)
			{
				Context context3 = context2;
				List<MenuOption> list = new List<MenuOption> { TOGGLE_OPTION };
				List<MenuOption> list2 = list;
				context3.menuOptionDescriptions = list;
			}
			this.context.parentContext = scontext;
		}
	}

	public NavigationContext GetNavigationContext()
	{
		return context;
	}

	public void setData(FrameworkDataElement data)
	{
		if (data is OptionsCategoryRow optionsCategoryRow)
		{
			this.data = optionsCategoryRow;
			Render();
		}
		else
		{
			this.data = null;
		}
	}

	public void Render()
	{
		title.SetText("{{C|" + data.Title.ToUpper() + "}}");
		if (data.categoryExpanded)
		{
			categoryExpander.SetText("{{C|[-]}}");
		}
		else
		{
			categoryExpander.SetText("{{C|[+]}}");
		}
	}

	public void Update()
	{
		OptionsCategoryRow optionsCategoryRow = data;
		if (optionsCategoryRow != null && optionsCategoryRow.ValueChangedSinceLastObserved(this))
		{
			Render();
		}
	}
}
