using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainSelfHarm_UnitSelfGlotrot : ProceduralCookingEffectUnit
{
	public GameObject Object;

	public override string GetDescription()
	{
		return "Causes @their tongue to rot away.";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.ApplyEffect(new GlotrotOnset());
	}

	public override void Remove(GameObject Object, Effect parent)
	{
	}
}
