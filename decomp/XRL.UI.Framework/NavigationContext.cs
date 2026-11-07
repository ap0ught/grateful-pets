using System;
using System.Collections.Generic;

namespace XRL.UI.Framework;

/// <summary>
/// Navigation Context describes a smaller section of a larger screen (or perhaps a whole screen for some simple dialogs.)
/// </summary>
public class NavigationContext
{
	public string displayName;

	public Action enterHandler;

	public Action exitHandler;

	public Dictionary<InputButtonTypes, Action> buttonHandlers;

	public Dictionary<InputAxisTypes, Action> axisHandlers;

	public Dictionary<string, Action> commandHandlers;

	public NavigationContext parentContext;

	public string parentNames
	{
		get
		{
			if (parentContext != null)
			{
				return " < " + parentContext.displayName + parentContext.parentNames;
			}
			return "";
		}
	}

	public virtual List<MenuOption> menuOptionDescriptions { get; set; }

	public NavigationController navigationController => NavigationController.instance;

	public Event currentEvent => NavigationController.currentEvent;

	public virtual bool disabled { get; set; }

	public virtual bool hasChildren => false;

	public virtual IEnumerable<NavigationContext> parents
	{
		get
		{
			for (NavigationContext p = parentContext; p != null; p = p.parentContext)
			{
				yield return p;
			}
		}
	}

	public NavigationContext()
	{
	}

	public NavigationContext(string debugName)
	{
		displayName = debugName;
	}

	public override string ToString()
	{
		return GetType().Name + ": " + displayName + parentNames;
	}

	public virtual void Setup()
	{
	}

	public virtual bool isDisabled()
	{
		for (NavigationContext navigationContext = this; navigationContext != null; navigationContext = navigationContext.parentContext)
		{
			if (navigationContext.disabled)
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool ActivateAndEnable()
	{
		return Activate(evenIfDisabled: true);
	}

	public virtual bool Activate(bool evenIfDisabled = false)
	{
		if (isDisabled() & evenIfDisabled)
		{
			for (NavigationContext navigationContext = this; navigationContext != null; navigationContext = navigationContext.parentContext)
			{
				if (navigationContext.disabled)
				{
					navigationContext.disabled = false;
					if (FrameworkUnityController.instance.DebugContextActivity)
					{
						MetricsManager.LogError("DEBUG:Forced a UI Attempt to activate disabled context");
					}
				}
			}
		}
		FrameworkUnityController instance = FrameworkUnityController.instance;
		if ((object)instance == null || !instance.DebugContextActivity || isDisabled())
		{
		}
		if (!isDisabled() | evenIfDisabled)
		{
			if (navigationController.activeContext != this)
			{
				SoundManager.PlayUISound("Sounds/UI/ui_cursor_scroll", 1f, Combat: false, Interface: true);
			}
			navigationController.activeContext = this;
		}
		return IsActive();
	}

	public virtual bool IsActive(bool checkParents = true)
	{
		NavigationContext activeContext = navigationController.activeContext;
		while (checkParents && activeContext != null && activeContext != this)
		{
			activeContext = activeContext.parentContext;
		}
		return activeContext == this;
	}

	public bool IsInside(NavigationContext test)
	{
		for (NavigationContext navigationContext = this; navigationContext != null; navigationContext = navigationContext.parentContext)
		{
			if (navigationContext == test)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void OnEnter()
	{
		if (enterHandler != null)
		{
			enterHandler();
		}
	}

	public virtual void OnExit()
	{
		if (exitHandler != null)
		{
			exitHandler();
		}
	}

	public virtual void OnInput(Event e)
	{
		if (currentEvent.data.TryGetValue("commandId", out var value))
		{
			string key = value as string;
			if (commandHandlers != null && commandHandlers.TryGetValue(key, out var value2))
			{
				value2();
			}
		}
		else if (currentEvent.data.TryGetValue("button", out value))
		{
			InputButtonTypes key2 = (InputButtonTypes)value;
			if (buttonHandlers != null && buttonHandlers.TryGetValue(key2, out var value3))
			{
				value3();
			}
		}
		else if (currentEvent.data.TryGetValue("axis", out value))
		{
			InputAxisTypes key3 = (InputAxisTypes)value;
			if (axisHandlers != null && axisHandlers.TryGetValue(key3, out var value4))
			{
				value4();
			}
		}
	}
}
