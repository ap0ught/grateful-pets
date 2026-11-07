using System;

namespace XRL.World.Effects;

[Serializable]
public class ProceduralCookingEffectUnit_1Str : ProceduralCookingEffectUnit
{
	public override string GetDescription()
	{
		return "+1 STR";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.GetStat("Strength").Bonus++;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.GetStat("Strength").Bonus--;
	}
}
