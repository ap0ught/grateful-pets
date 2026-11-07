using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class BoostStatistic : Effect, ITierInitialized
{
	public string Statistic = "Strength";

	public int Bonus;

	public static string[] Statistics = new string[6] { "Strength", "Agility", "Toughness", "Intelligence", "Willpower", "Ego" };

	public BoostStatistic()
	{
		DisplayName = "boosted " + Statistic.ToLower();
	}

	public BoostStatistic(int Duration, string Statistic, int Amount)
		: this()
	{
		base.Duration = Duration;
		this.Statistic = Statistic;
		Bonus = Amount;
		DisplayName = "boosted " + Statistic.ToLower();
	}

	public override string GetDescription()
	{
		return "boosted " + Statistic.ToLower();
	}

	public void Initialize(int Tier)
	{
		Tier = Math.Max(1, Math.Min(8, Stat.Random(Tier - 2, Tier + 2)));
		Statistic = Statistics.GetRandomElement();
		Duration = Stat.Random(40, 200);
		Bonus = Tier + 2;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		int num = 0;
		num = ((!XRL.World.Statistic.IsMental(Statistic)) ? (num | 4) : (num | 2));
		if (Bonus < 0)
		{
			num |= 0x2000000;
		}
		return num;
	}

	public override bool SameAs(Effect e)
	{
		BoostStatistic boostStatistic = e as BoostStatistic;
		if (boostStatistic.Statistic != Statistic)
		{
			return false;
		}
		if (boostStatistic.Bonus != Bonus)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return Bonus.Signed() + " " + Statistic;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.FireEvent(Event.New("ApplyBoostStatistic", "Event", this)))
		{
			if (Bonus >= 0)
			{
				Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_genericBuff");
			}
			else
			{
				Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_genericBuff");
			}
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your " + Statistic + " " + ((Bonus >= 0) ? "increases" : "decreases") + ".", (Bonus >= 0) ? 'g' : 'r');
			}
			ApplyStats();
			return true;
		}
		return false;
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift(Object, Statistic, Bonus);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts(Object);
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("Your " + Statistic + " returns to normal.", 'r');
		}
		UnapplyStats();
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeepCopyWithoutEffects")
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
