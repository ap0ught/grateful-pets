using System;
using XRL.Messages;

namespace XRL.World.Effects;

[Serializable]
public class NoPhase_ProceduralCookingTriggeredAction_Effect : BasicTriggeredCookingEffect
{
	public override string GetDetails()
	{
		return "Can't be phased.";
	}

	public NoPhase_ProceduralCookingTriggeredAction_Effect()
	{
		Duration = 100;
		DisplayName = null;
	}

	public override void ApplyEffect(GameObject Object)
	{
		Duration = 100;
		Object.RegisterEffectEvent(this, "ApplyPhased");
		base.ApplyEffect(Object);
	}

	public override void RemoveEffect(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "ApplyPhased");
		base.RemoveEffect(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyPhased")
		{
			if (Object != null && Object.IsPlayer())
			{
				MessageQueue.AddPlayerMessage("Your phase remains stable.");
			}
			return false;
		}
		return base.FireEvent(E);
	}
}
