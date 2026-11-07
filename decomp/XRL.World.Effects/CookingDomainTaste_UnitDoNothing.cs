using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainTaste_UnitDoNothing : ProceduralCookingEffectUnit
{
	public override string GetDescription()
	{
		return "Mmmm.";
	}

	public override string GetTemplatedDescription()
	{
		return "Guaranteed to be tasty if eaten while hungry.";
	}

	public override void Init(GameObject target)
	{
	}

	public override void Apply(GameObject Object, Effect parent)
	{
	}

	public override void Remove(GameObject Object, Effect parent)
	{
	}
}
