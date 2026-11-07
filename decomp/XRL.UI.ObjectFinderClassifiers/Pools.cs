using XRL.World;

namespace XRL.UI.ObjectFinderClassifiers;

public class Pools : ObjectFinder.Classifier
{
	public override bool Check(GameObject go, ObjectFinder.Context context)
	{
		return go.IsOpenLiquidVolume();
	}
}
