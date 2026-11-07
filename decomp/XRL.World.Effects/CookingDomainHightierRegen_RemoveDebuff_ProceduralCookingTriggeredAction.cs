using System;
using System.Collections.Generic;
using System.Linq;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainHightierRegen_RemoveDebuff_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "one of @their major negative status effects is removed at random. If no major effects, then a minor effect is removed instead.";
	}

	public override string GetNotification()
	{
		return "@they feel less afflicted.";
	}

	public override void Apply(GameObject go)
	{
		IEnumerable<Effect> enumerable = go.YieldEffects((Effect FX) => !FX.IsOfType(16777216) && FX.IsOfTypes(100663296) && !FX.IsOfType(134217728));
		if (enumerable.Any())
		{
			go.RemoveEffect(enumerable.GetRandomElement());
			return;
		}
		enumerable = go.YieldEffects((Effect FX) => FX.IsOfType(16777216) && FX.IsOfTypes(100663296) && !FX.IsOfType(134217728));
		if (enumerable.Any())
		{
			go.RemoveEffect(enumerable.GetRandomElement());
		}
	}
}
