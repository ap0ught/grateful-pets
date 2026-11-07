using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using XRL.UI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;

namespace XRL.World;

public class PointOfInterest
{
	public GameObject Object;

	public string _DisplayName;

	public string Explanation;

	public string Key;

	public string Preposition = "at";

	public int Radius;

	public int Order;

	public Location2D Location;

	public IRenderable Icon;

	private static List<string> DisplayItems = new List<string>();

	public string DisplayName
	{
		get
		{
			if (_DisplayName == null)
			{
				if (Object != null)
				{
					_DisplayName = Object.GetReferenceDisplayName();
				}
				if (_DisplayName == null && !Explanation.IsNullOrEmpty())
				{
					_DisplayName = Explanation;
					Explanation = null;
				}
				if (_DisplayName == null)
				{
					_DisplayName = "?";
				}
			}
			return _DisplayName;
		}
		set
		{
			_DisplayName = value;
		}
	}

	public string GetDisplayName(GameObject Observer)
	{
		DisplayItems.Clear();
		string text = DisplayName;
		if (!Explanation.IsNullOrEmpty())
		{
			DisplayItems.Add(Explanation);
		}
		string text2 = null;
		if (Observer != null)
		{
			if (IsAt(Observer))
			{
				text2 = "here";
			}
			else if (Location != null)
			{
				text2 = Observer.DescribeDirectionToward(Location, General: true, Short: true);
			}
			else if (Object != null)
			{
				text2 = Observer.DescribeDirectionToward(Object, General: true, Short: true);
			}
		}
		if (!text2.IsNullOrEmpty())
		{
			DisplayItems.Add(text2);
		}
		if (DisplayItems.Count > 0)
		{
			text = text + " [" + string.Join(", ", DisplayItems.ToArray()) + "]";
		}
		return text;
	}

	public string GetSentenceName(GameObject Observer)
	{
		string text = DisplayName;
		if (Object != null)
		{
			text = Object.the + text;
		}
		else if (!text.StartsWith("the ", StringComparison.InvariantCultureIgnoreCase) && !text.StartsWith("a ", StringComparison.InvariantCultureIgnoreCase) && !text.StartsWith("an ", StringComparison.InvariantCultureIgnoreCase))
		{
			text = "the " + text;
		}
		return text;
	}

	public bool IsAt(GameObject Observer, int Radius)
	{
		return GetDistanceTo(Observer) <= Radius;
	}

	public bool IsAt(GameObject Observer)
	{
		return IsAt(Observer, GetAppropriateRadius(Observer));
	}

	public int GetAppropriateRadius(GameObject Observer)
	{
		if (Radius >= 0)
		{
			return Radius;
		}
		if (Object != null && (Object.IsCreature || Object.ConsiderSolidFor(Observer)))
		{
			return 1;
		}
		return 0;
	}

	public int GetDistanceTo(GameObject Observer)
	{
		if (!(Location != null))
		{
			return Observer.DistanceTo(Object);
		}
		return Observer.DistanceTo(Location);
	}

	public IRenderable GetIcon()
	{
		object obj = Icon;
		if (obj == null)
		{
			GameObject gameObject = Object;
			if (gameObject == null)
			{
				return null;
			}
			obj = gameObject.RenderForUI();
		}
		return (IRenderable)obj;
	}

	public bool NavigateTo(GameObject Observer)
	{
		if (!GameObject.Validate(ref Observer))
		{
			return false;
		}
		int appropriateRadius = GetAppropriateRadius(Observer);
		if (IsAt(Observer, appropriateRadius))
		{
			if (Observer.IsPlayer())
			{
				Popup.ShowFail("You are already " + Preposition + " " + GetSentenceName(Observer) + ".");
			}
			return false;
		}
		if (Observer.IsPlayer())
		{
			string text = ((appropriateRadius > 0) ? ("P" + appropriateRadius + ":") : "M");
			if (Object != null)
			{
				AutoAct.Setting = text + Object.ID;
				Observer.ForfeitTurn();
			}
			else if (Location != null)
			{
				int x = Location.X;
				string text2 = x.ToString();
				x = Location.Y;
				AutoAct.Setting = text + text2 + "," + x;
				Observer.ForfeitTurn();
			}
			else
			{
				Popup.ShowFail("Somehow there seems to be no location for " + GetSentenceName(Observer) + ".");
			}
		}
		else if (Location != null)
		{
			Observer.Brain?.PushGoal(new MoveTo(Location, careful: true, overridesCombat: false, appropriateRadius));
		}
		else if (Object != null)
		{
			Observer.Brain?.PushGoal(new MoveTo(Object, careful: true, overridesCombat: false, appropriateRadius));
		}
		return true;
	}

	public static int Compare(PointOfInterest a, PointOfInterest b)
	{
		int num = a.Order.CompareTo(b.Order);
		if (num != 0)
		{
			return num;
		}
		int num2 = ColorUtility.CompareExceptFormattingAndCase(a.DisplayName, b.DisplayName);
		if (num2 != 0)
		{
			return num2;
		}
		return a.GetDistanceTo(The.Player).CompareTo(b.GetDistanceTo(The.Player));
	}
}
