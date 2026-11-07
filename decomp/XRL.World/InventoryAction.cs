using System.Collections.Generic;
using XRL.World.Parts.Mutation;

namespace XRL.World;

public class InventoryAction
{
	public class Comparer : IComparer<InventoryAction>
	{
		public bool priorityFirst;

		public int Compare(InventoryAction a, InventoryAction b)
		{
			if (!priorityFirst)
			{
				return SortCompare(a, b);
			}
			return PriorityCompare(a, b);
		}
	}

	public string Name;

	public char Key;

	public string Display;

	public string Command;

	public string PreferToHighlight;

	public int Default;

	public int Priority;

	public bool FireOnActor;

	public bool WorksAtDistance;

	public bool WorksTelekinetically;

	public bool WorksTelepathically;

	public bool AsMinEvent;

	public GameObject FireOn;

	public bool ReturnToModernUI;

	public bool IsUsable(GameObject Twiddler, GameObject Twiddlee, bool Distant = false)
	{
		if (!Distant)
		{
			return true;
		}
		if (WorksAtDistance)
		{
			return true;
		}
		if (WorksTelepathically && Twiddler.CanMakeTelepathicContactWith(Twiddlee))
		{
			return true;
		}
		if (WorksTelekinetically && Twiddler.CanManipulateTelekinetically(Twiddlee))
		{
			return true;
		}
		return false;
	}

	public bool IsUsable(GameObject Twiddler, GameObject Twiddlee, bool Distant, out bool Telekinetic)
	{
		Telekinetic = false;
		if (!Distant)
		{
			return true;
		}
		if (WorksAtDistance)
		{
			return true;
		}
		if (WorksTelepathically && Twiddler.CanMakeTelepathicContactWith(Twiddlee))
		{
			return true;
		}
		if (WorksTelekinetically && Twiddler.HasPart<Telekinesis>())
		{
			Telekinetic = true;
			if (Twiddler.CanManipulateTelekinetically(Twiddlee))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsVisible(GameObject Twiddler, GameObject Twiddlee, bool Distant = false)
	{
		if (!Distant)
		{
			return true;
		}
		if (WorksAtDistance)
		{
			return true;
		}
		if (WorksTelepathically && Twiddler.CanMakeTelepathicContactWith(Twiddlee))
		{
			return true;
		}
		if (WorksTelekinetically && Twiddler.HasPart<Telekinesis>())
		{
			return true;
		}
		return false;
	}

	public IEvent Process(GameObject GO, GameObject Owner, bool Telekinetic = false)
	{
		if (Telekinetic)
		{
			GO.TelekinesisBlip();
		}
		Owner.FireEvent(Event.New("InvCommandActivating", "Object", GO));
		GameObject gameObject = (FireOnActor ? Owner : (FireOn ?? GO));
		IEvent GeneratedEvent;
		if (AsMinEvent)
		{
			InventoryActionEvent.Check(out GeneratedEvent, gameObject, Owner, GO, Command, false, false, false, false, false, 0, 0, 0, (GameObject)null, (Cell)null, (Cell)null, (IInventory)null);
		}
		else
		{
			Event obj = Event.New(Command, "Object", GO, "Owner", Owner);
			GeneratedEvent = obj;
			gameObject.FireEvent(obj);
		}
		return GeneratedEvent;
	}

	public static int SortCompare(InventoryAction a, InventoryAction b)
	{
		bool flag = a.Key == ' ';
		bool flag2 = b.Key == ' ';
		if (!flag && !flag2)
		{
			int num = char.ToUpper(a.Key).CompareTo(char.ToUpper(b.Key));
			if (num != 0)
			{
				return num;
			}
			if (a.Key != b.Key)
			{
				return -a.Key.CompareTo(b.Key);
			}
			int num2 = a.Default.CompareTo(b.Default);
			if (num2 != 0)
			{
				return -num2;
			}
		}
		else if (flag | flag2)
		{
			return flag.CompareTo(flag2);
		}
		return PriorityCompare(a, b);
	}

	public static int PriorityCompare(InventoryAction a, InventoryAction b)
	{
		int num = a.Priority.CompareTo(b.Priority);
		if (num != 0)
		{
			return -num;
		}
		return -a.Display.CompareTo(b.Display);
	}
}
