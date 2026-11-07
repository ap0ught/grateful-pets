using XRL.UI.ObjectFinderContexts;
using XRL.World;

namespace XRL.UI.ObjectFinderSorters;

public class ValueSorter : ObjectFinder.Sorter
{
	public override string GetDisplayName()
	{
		return "Value";
	}

	public override int Compare((GameObject go, ObjectFinder.Context context) a, (GameObject go, ObjectFinder.Context context) b)
	{
		if (a.context != b.context)
		{
			if (a.context is AutogotItems)
			{
				return -1;
			}
			if (b.context is AutogotItems)
			{
				return 1;
			}
		}
		return b.go.ValueEach.CompareTo(a.go.ValueEach);
	}
}
