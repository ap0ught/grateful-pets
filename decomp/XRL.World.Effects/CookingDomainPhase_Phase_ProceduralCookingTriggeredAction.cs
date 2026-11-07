using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainPhase_Phase_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they phase out for 20 turns.";
	}

	public override string GetNotification()
	{
		return "@they phase out.";
	}

	public override void Apply(GameObject go)
	{
		if (go.IsPlayer() && !go.HasEffect<Phased>() && IComponent<GameObject>.CheckRealityDistortionAccessibility(go) && !go.OnWorldMap())
		{
			go.ApplyEffect(new Phased(20));
		}
	}
}
