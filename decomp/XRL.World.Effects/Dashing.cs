using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Dashing : Effect, ITierInitialized
{
	public Dashing()
	{
		DisplayName = "{{W|dashing}}";
	}

	public Dashing(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(10, 20);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 128;
	}

	public override string GetDetails()
	{
		return "Dashes to the nearest obstacle when moving in a direction.";
	}

	public override bool Apply(GameObject Object)
	{
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_ability_mutation_flamingRay_attack");
		return true;
	}

	public override void Remove(GameObject Object)
	{
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (Duration > 0 && num > 15 && num < 25)
		{
			E.RenderString = "\u001a";
			int num2 = Stat.Random(0, 2);
			if (num2 == 0)
			{
				E.ColorString = "&b";
			}
			if (num2 == 1)
			{
				E.ColorString = "&B";
			}
			if (num2 == 2)
			{
				E.ColorString = "&Y";
			}
		}
		return true;
	}
}
