using System;

namespace XRL.World.Effects;

[Serializable]
public class FungalCureQueasy : Effect
{
	public FungalCureQueasy()
	{
		DisplayName = "{{W|queasy}}";
	}

	public FungalCureQueasy(int Duration = 100)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 4;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		return null;
	}

	public override string GetDetails()
	{
		return "";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.TryGetEffect<FungalCureQueasy>(out var Effect))
		{
			Effect.Duration = Math.Max(Effect.Duration, Duration);
			return false;
		}
		if (Object.IsPlayerControlled())
		{
			IComponent<GameObject>.EmitMessage(Object, (Object.IsPlayer() ? "You" : Object.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true)) + Object.GetVerb("feel") + " a little queasy.", ' ', FromDialog: false, UsePopup: true);
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayerControlled())
		{
			IComponent<GameObject>.EmitMessage(Object, Object.Poss("queasiness passes."), ' ', FromDialog: true);
		}
		base.Remove(Object);
	}
}
