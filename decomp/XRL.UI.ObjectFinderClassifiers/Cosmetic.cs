using XRL.World;

namespace XRL.UI.ObjectFinderClassifiers;

public class Cosmetic : ObjectFinder.Classifier
{
	public override bool Check(GameObject go, ObjectFinder.Context context)
	{
		return go.HasTag("Cosmetic");
	}
}
