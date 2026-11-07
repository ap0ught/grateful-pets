using XRL.World;

namespace XRL.UI.ObjectFinderClassifiers;

public class Everything : ObjectFinder.Classifier
{
	public override bool Check(GameObject gameObject, ObjectFinder.Context context)
	{
		return true;
	}
}
