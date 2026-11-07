using XRL.World;

namespace XRL.UI.ObjectFinderSorters;

public class IdSorter : ObjectFinder.Sorter
{
	public override string GetDisplayName()
	{
		return "Id";
	}

	public override int Compare((GameObject go, ObjectFinder.Context context) a, (GameObject go, ObjectFinder.Context context) b)
	{
		return string.Compare(a.go.HasID ? a.go.ID : null, b.go.HasID ? b.go.ID : null);
	}
}
