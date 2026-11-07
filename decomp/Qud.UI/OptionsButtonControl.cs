using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[ExecuteInEditMode]
public class OptionsButtonControl : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts
{
	private class Context : NavigationContext
	{
	}

	public FrameworkContext frameworkContext;

	public OptionsMenuButtonRow data;

	public UITextSkin text;

	public bool selectedMode;

	private Context context;

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
		}
	}

	public NavigationContext GetNavigationContext()
	{
		return context;
	}

	public void setData(FrameworkDataElement data)
	{
		if (data is OptionsMenuButtonRow optionsMenuButtonRow)
		{
			this.data = optionsMenuButtonRow;
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
			text.SetText("{{W|" + data.Title + "}}");
		}
		else
		{
			text.SetText(data.Title);
		}
	}

	public void Update()
	{
		OptionsMenuButtonRow optionsMenuButtonRow = data;
		if ((optionsMenuButtonRow != null && optionsMenuButtonRow.ValueChangedSinceLastObserved(this)) || wasSelected != context.IsActive())
		{
			wasSelected = context.IsActive();
			Render();
		}
	}
}
