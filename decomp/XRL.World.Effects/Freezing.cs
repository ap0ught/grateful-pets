using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Freezing : Effect
{
	public Freezing()
	{
		DisplayName = "{{freezing|freezing}}";
	}

	public Freezing(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override string GetDescription()
	{
		return null;
	}

	public static bool CanApply(GameObject Object)
	{
		if (!CanApplyEffectEvent.Check<Freezing>(Object))
		{
			return false;
		}
		if (Object.HasEffect<Freezing>())
		{
			return false;
		}
		return true;
	}

	public static bool ApplyTo(GameObject Object)
	{
		if (!CanApply(Object))
		{
			return false;
		}
		return Object.ApplyEffect(new Freezing(1));
	}

	public override bool Render(RenderEvent E)
	{
		if (Object.Brain != null)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				E.ApplyColors("&C", 200);
				E.RenderString = 'ø'.GetString();
				return false;
			}
		}
		return true;
	}
}
