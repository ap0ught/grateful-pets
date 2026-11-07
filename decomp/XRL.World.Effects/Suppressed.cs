using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Suppressed : Effect, ITierInitialized
{
	public Suppressed()
	{
		DisplayName = "suppressed";
	}

	public Suppressed(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(2, 8);
	}

	public override int GetEffectType()
	{
		return 100663424;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Can't move.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.IsPotentiallyMobile())
		{
			return false;
		}
		if (!Object.FireEvent("ApplySuppressed"))
		{
			return false;
		}
		Suppressed effect = Object.GetEffect<Suppressed>();
		if (effect != null && Duration > effect.Duration)
		{
			effect.Duration = Duration;
		}
		if (Object.IsPlayer())
		{
			DidX("are", "suppressed", "!", null, null, null, Object);
		}
		Object.ParticleText("*suppressed*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == LeaveCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(LeaveCellEvent E)
	{
		if (Duration > 0 && !E.Forced && E.Type != "Teleporting")
		{
			Object.UseEnergy(1000, "Suppression");
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 50 && num < 60)
			{
				E.Tile = null;
				E.RenderString = "\u000f";
				E.ColorString = "&C^K";
			}
		}
		return true;
	}
}
