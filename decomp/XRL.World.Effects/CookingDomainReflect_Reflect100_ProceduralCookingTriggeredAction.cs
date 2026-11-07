using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainReflect_Reflect100_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they reflect 100% damage the next time @they take damage within 50 turns.";
	}

	public override string GetNotification()
	{
		return "@they grow tiny spines all over your body.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<CookingDomainReflect_Reflect100_ProceduralCookingTriggeredAction_Effect>())
		{
			go.ApplyEffect(new CookingDomainReflect_Reflect100_ProceduralCookingTriggeredAction_Effect());
		}
	}
}
