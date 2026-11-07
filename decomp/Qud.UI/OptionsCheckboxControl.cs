using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[ExecuteInEditMode]
public class OptionsCheckboxControl : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts
{
	private class Context : NavigationContext
	{
	}

	public FrameworkContext frameworkContext;

	public OptionsCheckboxRow data;

	public UITextSkin text;

	public bool selectedMode;

	private Context context;

	public static MenuOption TOGGLE_OPTION = new MenuOption
	{
		InputCommand = "Accept",
		Description = "Toggle Option",
		disabled = true
	};

	protected const string EMPTY_CHECK = "[ ] ";

	protected const string CHECKED = "[■] ";

	protected const string CHECKED_ACTIVE = "[{{W|■}}] ";

	private bool? wasSelected;

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
			this.context.parentContext = scontext;
			Context context2 = this.context;
			if (context2.menuOptionDescriptions == null)
			{
				Context context3 = context2;
				List<MenuOption> list = new List<MenuOption> { TOGGLE_OPTION };
				List<MenuOption> list2 = list;
				context3.menuOptionDescriptions = list;
			}
			context2 = this.context;
			if (context2.buttonHandlers == null)
			{
				context2.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
				{
					InputButtonTypes.AcceptButton,
					DoToggle
				} };
			}
		}
	}

	public void DoToggle()
	{
		data.Value = !data.Value;
		Render();
		Options.SetOption(data.Id, data.Value);
		NavigationController.currentEvent.Handle();
	}

	public NavigationContext GetNavigationContext()
	{
		return context;
	}

	public void setData(FrameworkDataElement data)
	{
		if (data is OptionsCheckboxRow optionsCheckboxRow)
		{
			this.data = optionsCheckboxRow;
			Render();
		}
		else
		{
			this.data = null;
		}
	}

	public void Render()
	{
		if (wasSelected == true)
		{
			text.SetText((data.Value ? "[{{W|■}}] " : "[ ] ") + "{{W|" + data.Title + "}}");
		}
		else
		{
			text.SetText((data.Value ? "[■] " : "[ ] ") + data.Title);
		}
		data.ValueChangedSinceLastObserved(this);
	}

	public void Update()
	{
		if (context != null)
		{
			OptionsCheckboxRow optionsCheckboxRow = data;
			if ((optionsCheckboxRow != null && optionsCheckboxRow.ValueChangedSinceLastObserved(this)) || wasSelected != context.IsActive())
			{
				wasSelected = context.IsActive();
				Render();
			}
		}
	}
}
