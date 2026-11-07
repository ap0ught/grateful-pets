using System;

namespace XRL.World.Effects;

[Serializable]
public class NoThirst_ProceduralCookingTriggeredAction_Effect : BasicTriggeredCookingEffect
{
	public override string GetDetails()
	{
		return "@they don't thirst.";
	}

	public NoThirst_ProceduralCookingTriggeredAction_Effect()
	{
		Duration = 1;
		DisplayName = null;
	}

	public override void ApplyEffect(GameObject Object)
	{
		Duration = 1200;
		Object.RegisterEffectEvent(this, "CalculatingThirst");
		base.ApplyEffect(Object);
	}

	public override void RemoveEffect(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "CalculatingThirst");
		base.RemoveEffect(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CalculatingThirst")
		{
			E.SetParameter("Amount", 0);
			return false;
		}
		return base.FireEvent(E);
	}
}
