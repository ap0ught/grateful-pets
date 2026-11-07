using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainHP_IncreaseHP_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingEffect
{
	public int Tier;

	public int Bonus;

	public CookingDomainHP_IncreaseHP_ProceduralCookingTriggeredActionEffect()
	{
		Duration = 50;
	}

	public CookingDomainHP_IncreaseHP_ProceduralCookingTriggeredActionEffect(int Tier)
	{
		this.Tier = Tier;
		Duration = 50;
	}

	public override string GetDetails()
	{
		return Tier.Signed() + "% max HP";
	}

	public override bool Apply(GameObject Object)
	{
		Bonus = (int)Math.Ceiling((float)Object.Statistics["Hitpoints"].BaseValue * ((float)Tier * 0.01f));
		Object.Statistics["Hitpoints"].BaseValue += Bonus;
		return true;
	}

	public override void Remove(GameObject Object)
	{
		Object.Statistics["Hitpoints"].BaseValue -= Bonus;
		Bonus = 0;
	}
}
