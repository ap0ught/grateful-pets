using System;
using System.Text;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Shamed : Effect, ITierInitialized
{
	public int Penalty = 4;

	public int SpeedPenaltyPercent = 10;

	public int SpeedPenalty;

	public Shamed()
	{
		DisplayName = "{{r|shamed}}";
	}

	public Shamed(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(50, 200);
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(-Penalty).Append(" DV\n").Append(-Penalty)
			.Append(" to-hit\n")
			.Append(-Penalty)
			.Append(" Willpower\n")
			.Append(-Penalty)
			.Append(" Ego\n");
		if (Object != null)
		{
			if (SpeedPenalty != 0)
			{
				stringBuilder.Append(-SpeedPenalty).Append(" Quickness");
			}
		}
		else
		{
			stringBuilder.Append(-SpeedPenaltyPercent).Append("% Quickness");
		}
		return stringBuilder.ToString();
	}

	public override bool SameAs(Effect e)
	{
		Shamed shamed = e as Shamed;
		if (shamed.Penalty != Penalty)
		{
			return false;
		}
		if (shamed.SpeedPenaltyPercent != SpeedPenaltyPercent)
		{
			return false;
		}
		if (shamed.SpeedPenalty != SpeedPenalty)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent("CanApplyShamed"))
		{
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyShamed", "Duration", Duration)))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Shamed", this))
		{
			return false;
		}
		ApplyStats();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetToHitModifierEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetToHitModifierEvent E)
	{
		if (E.Actor == Object && E.Checking == "Actor")
		{
			E.Modifier -= Penalty;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("ApplyShamed");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("CanApplyShamed");
		base.Register(Object, Registrar);
	}

	private void ApplyStats()
	{
		SpeedPenalty = Object.Stat("Speed") * SpeedPenaltyPercent / 100;
		StatShifter.SetStatShift("DV", -Penalty);
		StatShifter.SetStatShift("Ego", -Penalty);
		StatShifter.SetStatShift("Willpower", -Penalty);
		StatShifter.SetStatShift("Speed", -SpeedPenalty);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts();
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 25 && num < 40)
			{
				E.Tile = null;
				E.RenderString = ";";
				E.ColorString = "&r";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyShamed")
		{
			return false;
		}
		if (E.ID == "ApplyShamed")
		{
			if (Duration > 0)
			{
				Duration = E.GetIntParameter("Duration");
				return false;
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}
}
