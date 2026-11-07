using System;

namespace XRL.World.Effects;

[Serializable]
public class Str3_ProceduralCookingTriggeredAction_Effect : BasicTriggeredCookingEffect
{
	public string stat = "Strength";

	public int amount = 3;

	public Str3_ProceduralCookingTriggeredAction_Effect()
	{
		Duration = 1;
		DisplayName = null;
	}

	public override void ApplyEffect(GameObject Object)
	{
		DisplayName = "&Wwell fed (+" + amount + " " + stat + ")&y";
		Object.Statistics[stat].Bonus += amount;
		base.ApplyEffect(Object);
	}

	public override void RemoveEffect(GameObject Object)
	{
		Object.Statistics[stat].Bonus -= amount;
		amount = 0;
		base.RemoveEffect(Object);
	}
}
