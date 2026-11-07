using System.Collections.Generic;

namespace XRL.UI.Framework;

public class ProxyNavigationContext : NavigationContext
{
	public NavigationContext proxyTo;

	public override bool disabled
	{
		get
		{
			return proxyTo?.disabled ?? base.disabled;
		}
		set
		{
			if (proxyTo != null)
			{
				proxyTo.disabled = value;
			}
			else
			{
				base.disabled = value;
			}
		}
	}

	public override List<MenuOption> menuOptionDescriptions
	{
		get
		{
			return proxyTo?.menuOptionDescriptions ?? base.menuOptionDescriptions;
		}
		set
		{
			if (proxyTo != null)
			{
				proxyTo.menuOptionDescriptions = value;
			}
			else
			{
				base.menuOptionDescriptions = value;
			}
		}
	}

	public override bool hasChildren
	{
		get
		{
			if (proxyTo != null)
			{
				return true;
			}
			return base.hasChildren;
		}
	}

	public override void Setup()
	{
		if (proxyTo != null)
		{
			NavigationContext navigationContext = proxyTo;
			navigationContext.parentContext = this;
			navigationContext.Setup();
		}
		base.Setup();
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (proxyTo != null && !currentEvent.handled && !currentEvent.cancelled && (NavigationContext)currentEvent.data["to"] == this)
		{
			proxyTo.Activate();
			currentEvent.Cancel();
		}
	}
}
