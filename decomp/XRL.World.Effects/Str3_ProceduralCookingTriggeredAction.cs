using System;

namespace XRL.World.Effects;

[Serializable]
public class Str3_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "+3 Str for the next three hours";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<Str3_ProceduralCookingTriggeredAction_Effect>())
		{
			go.ApplyEffect(new Str3_ProceduralCookingTriggeredAction_Effect());
		}
	}
}
