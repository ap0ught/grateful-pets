using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainCloning_UnitMultipleClones : ProceduralCookingEffectUnit
{
	public GameObject Object;

	public override string GetDescription()
	{
		return "Causes @thisCreature to multiply 1 to 3 times.";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		int num = Stat.Random(1, 3);
		int numClones = num;
		Object.ApplyEffect(new Budding(Object, numClones));
	}

	public override void Remove(GameObject Object, Effect parent)
	{
	}
}
