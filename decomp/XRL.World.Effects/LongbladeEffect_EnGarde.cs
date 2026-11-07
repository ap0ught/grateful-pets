using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class LongbladeEffect_EnGarde : Effect, ITierInitialized
{
	public LongbladeEffect_EnGarde()
	{
		DisplayName = "{{G|En garde!}}";
		Duration = 9999;
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
		return "Lunge and Swipe have no cooldown.";
	}

	public override bool Apply(GameObject Object)
	{
		Object.RemoveEffect<LongbladeEffect_EnGarde>();
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0 && XRLCore.CurrentFrame % 20 > 10)
		{
			E.RenderString = "!";
			E.ColorString = "&G";
			E.DetailColor = "W";
		}
		return true;
	}
}
