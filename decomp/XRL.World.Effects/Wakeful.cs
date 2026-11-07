using System;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Wakeful : Effect, ITierInitialized
{
	public Wakeful()
	{
		DisplayName = "{{W|wakeful}}";
	}

	public Wakeful(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(500, 2000);
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override int GetEffectType()
	{
		return 83886082;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		if (Asleep.UsesSleepMode(Object))
		{
			return "{{W|safe mode}}";
		}
		return base.GetDescription();
	}

	public override string GetDetails()
	{
		if (Asleep.UsesSleepMode(Object))
		{
			return "Cannot be put in sleep mode.";
		}
		return "Cannot fall asleep.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasPart<Brain>())
		{
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyWakeful")) || !ApplyEffectEvent.Check(Object, "Wakeful", this))
		{
			return false;
		}
		if (Object.TryGetEffect<Wakeful>(out var Effect))
		{
			Effect.Duration += Duration;
			return false;
		}
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyInvoluntarySleep");
		Registrar.Register("CanApplyInvoluntarySleep");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyInvoluntarySleep" || E.ID == "ApplyInvoluntarySleep")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
