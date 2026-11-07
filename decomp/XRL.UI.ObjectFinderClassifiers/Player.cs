using XRL.World;

namespace XRL.UI.ObjectFinderClassifiers;

public class Player : ObjectFinder.Classifier
{
	public override bool Check(GameObject go, ObjectFinder.Context context)
	{
		return go == The.Player;
	}
}
