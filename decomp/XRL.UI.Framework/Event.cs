using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace XRL.UI.Framework;

public class Event
{
	public enum Type
	{
		Enter,
		Exit,
		Input
	}

	public static class Helpers
	{
		public static Action Handle(Action act)
		{
			return () =>
			{
				Event currentEvent = NavigationController.currentEvent;
				if (currentEvent == null || !currentEvent.handled)
				{
					act();
				}
				NavigationController.currentEvent?.Handle();
			};
		}

		public static Action IfAxisNeg(Action act)
		{
			return () =>
			{
				Event currentEvent = NavigationController.currentEvent;
				if (currentEvent != null && currentEvent.axisValue < 0)
				{
					act();
				}
			};
		}

		public static Action Axis(Action positive = null, Action negative = null, Action zero = null)
		{
			return () =>
			{
				Event currentEvent = NavigationController.currentEvent;
				if (currentEvent != null && currentEvent.axisValue < 0)
				{
					if (negative != null)
					{
						negative();
					}
				}
				else
				{
					Event currentEvent2 = NavigationController.currentEvent;
					if (currentEvent2 != null && currentEvent2.axisValue > 0)
					{
						if (positive != null)
						{
							positive();
						}
					}
					else if (zero != null)
					{
						zero();
					}
				}
			};
		}

		public static Action HandleIfAxisNeg(Action act)
		{
			return IfAxisNeg(Handle(act));
		}
	}

	public bool cancelled;

	public bool handled;

	public int? axisValue;

	public Dictionary<string, object> data;

	public Type type;

	public bool IsRightClick()
	{
		data.TryGetValue("PointerEventData", out var value);
		if (value is PointerEventData)
		{
			return (value as PointerEventData).button == PointerEventData.InputButton.Right;
		}
		return false;
	}

	public void Cancel()
	{
		cancelled = true;
	}

	public void Handle()
	{
		handled = true;
	}
}
