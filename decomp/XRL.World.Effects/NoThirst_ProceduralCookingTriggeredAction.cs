using System;

namespace XRL.World.Effects;

[Serializable]
public class NoThirst_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they don't thirst for the next 12 hours.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<NoThirst_ProceduralCookingTriggeredAction_Effect>())
		{
			go.ApplyEffect(new NoThirst_ProceduralCookingTriggeredAction_Effect());
		}
	}
}
