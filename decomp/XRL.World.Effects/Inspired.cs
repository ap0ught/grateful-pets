using System;

namespace XRL.World.Effects;

[Serializable]
public class Inspired : Effect, ITierInitialized
{
	public Inspired()
	{
		Duration = 1;
		DisplayName = "{{M|inspired}}";
	}

	public Inspired(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 2;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect(typeof(Inspired)))
		{
			return false;
		}
		return Object.ShowSuccess("You swell with inspiration to cook a mouthwatering meal.");
	}

	public override string GetDetails()
	{
		return "The next time you cook a meal by choosing ingredients, you get a choice of three dynamically-generated effects to apply. You create a recipe for the chosen effect.";
	}
}
