using System;
using XRL.Language;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Emboldened : Effect, ITierInitialized
{
	public string Statistic = "Hitpoints";

	public int Bonus;

	public Emboldened()
	{
		DisplayName = "Boosted " + Grammar.MakeTitleCase(XRL.World.Statistic.GetStatDisplayName(Statistic));
	}

	public Emboldened(int Duration, string Statistic, int Amount)
	{
		base.Duration = Duration;
		this.Statistic = Statistic;
		Bonus = Amount;
		DisplayName = "Boosted " + Grammar.MakeTitleCase(XRL.World.Statistic.GetStatDisplayName(Statistic));
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(200, 1000);
		Bonus = Stat.Random(1, 60) * 5;
	}

	public override int GetEffectType()
	{
		return 2;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "emboldened";
	}

	public override string GetDetails()
	{
		return Bonus.Signed() + " " + Statistic;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.TryGetEffect<Emboldened>(out var Effect))
		{
			if (Duration > Effect.Duration)
			{
				Effect.Duration = Duration;
			}
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyBoostStatistic", "Event", this)))
		{
			return false;
		}
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("Your " + XRL.World.Statistic.GetStatDisplayName(Statistic) + " " + (XRL.World.Statistic.IsStatPlural(Statistic) ? "increase" : "increase") + "!");
		}
		StatShifter.SetStatShift(Statistic, Bonus);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("Your " + XRL.World.Statistic.GetStatDisplayName(Statistic) + " " + (XRL.World.Statistic.IsStatPlural(Statistic) ? "return" : "returns") + " to normal.");
		}
		StatShifter.RemoveStatShifts();
	}
}
