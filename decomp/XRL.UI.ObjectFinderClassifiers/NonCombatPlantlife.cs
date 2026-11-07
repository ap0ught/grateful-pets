using XRL.World;

namespace XRL.UI.ObjectFinderClassifiers;

public class NonCombatPlantlife : ObjectFinder.Classifier
{
	public override bool Check(GameObject go, ObjectFinder.Context context)
	{
		if (go.HasTag("Plant") || go.HasTag("PlantLike") || go.HasTag("Fungus"))
		{
			return !go.IsCombat;
		}
		return false;
	}
}
