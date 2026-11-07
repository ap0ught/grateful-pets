using System;

namespace XRL.World.Effects;

[Serializable]
public class ProceduralCookingEffectUnit_XP : ProceduralCookingEffectUnit
{
	public override string GetDescription()
	{
		return "+XP";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.AwardXP(1000);
	}

	public override void Remove(GameObject Object, Effect parent)
	{
	}
}
