using System;

namespace XRL.World.Effects;

[Serializable]
public class HulkHoney_Tonic_Allergy : Effect
{
	public HulkHoney_Tonic_Allergy()
	{
		DisplayName = "{{R|enraged}}";
	}

	public HulkHoney_Tonic_Allergy(int duration)
		: this()
	{
		Duration = duration;
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "{{R|enraged}}";
	}

	public override string GetDetails()
	{
		return "ARRRRRRHGHH";
	}

	public override bool Apply(GameObject Object)
	{
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_enraged");
		HulkHoney_Tonic_Allergy effect = Object.GetEffect<HulkHoney_Tonic_Allergy>();
		if (effect != null)
		{
			if (effect.Duration < Duration)
			{
				effect.Duration = Duration;
			}
			return false;
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
	}
}
