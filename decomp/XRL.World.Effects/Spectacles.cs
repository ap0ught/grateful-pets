using System;

namespace XRL.World.Effects;

[Serializable]
public class Spectacles : Effect, ITierInitialized
{
	public Spectacles()
	{
		DisplayName = "corrected vision";
		Duration = 9999;
	}

	public void Initialize(int Tier)
	{
		Duration = 9999;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 16908288;
	}

	public override string GetDescription()
	{
		if (Object.GetEffect<Spectacles>() != this)
		{
			return null;
		}
		return base.GetDescription();
	}

	public override string GetDetails()
	{
		return "Can see things at normal distances.";
	}

	public override bool Apply(GameObject Object)
	{
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_genericBuff");
		return base.Apply(Object);
	}
}
