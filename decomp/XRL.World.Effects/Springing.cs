using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Springing : Effect, ITierInitialized
{
	public GameObject Source;

	public Springing()
	{
		DisplayName = "springing";
		Duration = 9999;
	}

	public Springing(GameObject Source)
		: this()
	{
		this.Source = Source;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(100, 300);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 16777344;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "springing";
	}

	public override string GetDetails()
	{
		return "Sprints or power skates at thrice move speed.";
	}

	public override bool Apply(GameObject Object)
	{
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_movementBuff");
		if (Source == null)
		{
			Springing effect = Object.GetEffect((Springing springing) => springing.Source == null);
			if (effect != null)
			{
				effect.Duration = Math.Max(effect.Duration, Duration);
				return false;
			}
		}
		return base.Apply(Object);
	}
}
