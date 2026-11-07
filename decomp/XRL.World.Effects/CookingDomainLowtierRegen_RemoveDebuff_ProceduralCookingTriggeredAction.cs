using System;
using System.Collections.Generic;
using System.Linq;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainLowtierRegen_RemoveDebuff_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "one of @their minor negative status effects is removed at random.";
	}

	public override string GetNotification()
	{
		return "@they feel less afflicted.";
	}

	public override void Apply(GameObject go)
	{
		IEnumerable<Effect> enumerable = go.YieldEffects((Effect FX) => FX.IsOfType(16777216) && FX.IsOfTypes(100663296) && !FX.IsOfType(134217728));
		if (enumerable.Any())
		{
			go.RemoveEffect(enumerable.GetRandomElement());
		}
	}
}
