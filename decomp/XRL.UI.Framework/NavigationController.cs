using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace XRL.UI.Framework;

public class NavigationController
{
	public class DragContext : NavigationContext
	{
		public Action EndDragDelegate;

		public DragContext(Action A)
		{
			EndDragDelegate = A;
			if (commandHandlers == null)
			{
				commandHandlers = new Dictionary<string, Action>();
			}
			commandHandlers.Set("Cancel", () =>
			{
				if (EndDragDelegate != null)
				{
					EndDragDelegate();
				}
				NavigationController.currentEvent.Cancel();
			});
		}
	}

	public class SuspensionNavigationContext : NavigationContext
	{
		public SuspensionNavigationContext()
			: base("suspension context")
		{
		}
	}

	public static DragContext dragContext = null;

	public static NavigationContext dragPriorContext = null;

	public static Action endDragDelegate = null;

	public static NavigationController instance = new NavigationController();

	public SuspensionNavigationContext suspensionContext = new SuspensionNavigationContext();

	private Event _currentEvent;

	private NavigationContext _activeContext;

	private NavigationContext _fromContext;

	public static Event currentEvent => instance._currentEvent;

	public NavigationContext activeContext
	{
		get
		{
			return _activeContext;
		}
		set
		{
			if (_activeContext != value)
			{
				FrameworkUnityController frameworkUnityController = FrameworkUnityController.instance;
				if ((object)frameworkUnityController != null && frameworkUnityController.DebugContextActivity)
				{
					UnityEngine.Debug.LogError($"set activecontext {value} at {new StackTrace().ToString()}");
				}
				_ = _activeContext;
				_ = suspensionContext;
				if (_fromContext == null)
				{
					_fromContext = _activeContext;
				}
				Dictionary<string, object> data = new Dictionary<string, object>
				{
					{ "from", _fromContext },
					{ "to", value },
					{
						"triggeringEvent",
						triggeringEvent ?? _currentEvent
					}
				};
				FireEvent(Event.Type.Exit, data);
				_activeContext = value;
				FireEvent(Event.Type.Enter, data);
				_fromContext = null;
			}
		}
	}

	public Event triggeringEvent
	{
		get
		{
			object value = null;
			if (_currentEvent?.data?.TryGetValue("triggeringEvent", out value) == true)
			{
				return value as Event;
			}
			return null;
		}
	}

	public static bool BeginDragWithContext(DragContext context, Action endDrag)
	{
		endDragDelegate = endDrag;
		dragPriorContext = instance.activeContext;
		dragContext = context;
		context.Activate();
		return true;
	}

	public static bool EndDragWithContext(NavigationContext context)
	{
		if (instance.activeContext == context)
		{
			dragPriorContext?.Activate();
		}
		dragContext = null;
		endDragDelegate = null;
		dragPriorContext = null;
		return true;
	}

	public async Task SuspendContextWhile(Func<Task> taskCreator)
	{
		NavigationContext oldContext = activeContext;
		NavigationContext globalContext = activeContext?.parents.LastOrDefault();
		bool? globalContextDisabled = globalContext?.disabled;
		if (globalContext != null)
		{
			globalContext.disabled = true;
		}
		activeContext = suspensionContext;
		try
		{
			await taskCreator();
		}
		finally
		{
			activeContext = oldContext;
			if (globalContext != null)
			{
				globalContext.disabled = globalContextDisabled == true;
			}
		}
	}

	public async Task<T> SuspendContextWhile<T>(Func<Task<T>> taskCreator)
	{
		NavigationContext oldContext = activeContext;
		NavigationContext globalContext = activeContext?.parents.LastOrDefault();
		bool? globalContextDisabled = globalContext?.disabled;
		if (globalContext != null)
		{
			globalContext.disabled = true;
		}
		activeContext = suspensionContext;
		try
		{
			return await taskCreator();
		}
		finally
		{
			activeContext = oldContext;
			if (globalContext != null)
			{
				globalContext.disabled = globalContextDisabled == true;
			}
		}
	}

	public string EditorStatus()
	{
		if (_activeContext == null)
		{
			return "<no active context>";
		}
		return _activeContext.ToString();
	}

	public Event FireEvent(Event.Type type, Dictionary<string, object> data = null)
	{
		return FireEvent(new Event
		{
			type = type,
			data = data
		});
	}

	public Event FireEvent(Event e)
	{
		NavigationContext parentContext = _activeContext;
		Event obj = _currentEvent;
		Event obj2 = (_currentEvent = e);
		try
		{
			while (parentContext != null && obj2 != null && obj2 != null && !obj2.cancelled && obj2 != null && !obj2.handled)
			{
				if (obj2.type == Event.Type.Enter)
				{
					parentContext.OnEnter();
				}
				else if (obj2.type == Event.Type.Exit)
				{
					parentContext.OnExit();
				}
				else
				{
					parentContext.OnInput(e);
				}
				parentContext = parentContext.parentContext;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Exception during UI.Framework::FireEvent", x);
		}
		finally
		{
			_currentEvent = obj;
		}
		return obj2;
	}

	public Event FireInputCommandEvent(string commandId, Dictionary<string, object> additionalData = null)
	{
		Dictionary<string, object> dictionary = additionalData ?? new Dictionary<string, object>();
		dictionary.Set("commandId", commandId);
		Event obj = FireEvent(Event.Type.Input, dictionary);
		if (obj.handled)
		{
			ControlManager.ConsumeCurrentInput();
		}
		return obj;
	}

	public Event FireInputButtonEvent(InputButtonTypes buttonType, Dictionary<string, object> additionalData = null)
	{
		Dictionary<string, object> dictionary = additionalData ?? new Dictionary<string, object>();
		dictionary.Set("button", buttonType);
		Event obj = FireEvent(Event.Type.Input, dictionary);
		if (obj.handled)
		{
			ControlManager.ConsumeCurrentInput();
		}
		return obj;
	}

	public Event FireInputAxisEvent(InputAxisTypes axisType, Dictionary<string, object> additionalData = null, int value = 0)
	{
		Dictionary<string, object> dictionary = additionalData ?? new Dictionary<string, object>();
		dictionary.Set("axis", axisType);
		Event e = new Event
		{
			type = Event.Type.Input,
			axisValue = value,
			data = dictionary
		};
		return FireEvent(e);
	}

	public static IEnumerable<MenuOption> GetMenuOptions()
	{
		HashSet<string> commandsSent = new HashSet<string>();
		for (NavigationContext context = instance.activeContext; context != null; context = context.parentContext)
		{
			if (context.menuOptionDescriptions != null)
			{
				foreach (MenuOption desc in context.menuOptionDescriptions)
				{
					if (!commandsSent.Contains(desc.InputCommand))
					{
						yield return desc;
					}
					commandsSent.Add(desc.InputCommand);
				}
			}
		}
	}

	public static IEnumerable<MenuOption> GetMenuOptions(IEnumerable<MenuOption> defaultList)
	{
		Dictionary<string, MenuOption> menuOptions = GetMenuOptions().ToDictionary((MenuOption menuOption) => menuOption.InputCommand);
		foreach (MenuOption option in defaultList)
		{
			if (menuOptions.ContainsKey(option.InputCommand))
			{
				yield return menuOptions[option.InputCommand];
				menuOptions.Remove(option.InputCommand);
			}
			else
			{
				yield return option;
			}
		}
		foreach (MenuOption value in menuOptions.Values)
		{
			yield return value;
		}
	}
}
