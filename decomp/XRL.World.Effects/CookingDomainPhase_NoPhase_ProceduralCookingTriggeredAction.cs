using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainPhase_NoPhase_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they can't be phased for 2 hours.";
	}

	public override string GetNotification()
	{
		return "@they become phase-anchored.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<NoPhase_ProceduralCookingTriggeredAction_Effect>())
		{
			go.ApplyEffect(new NoPhase_ProceduralCookingTriggeredAction_Effect());
		}
	}
}
