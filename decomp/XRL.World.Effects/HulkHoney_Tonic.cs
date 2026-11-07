using System;
using HistoryKit;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class HulkHoney_Tonic : ITonicEffect, ITierInitialized
{
	private bool bOverdosed;

	private int BonusHP;

	private float HPLoss;

	[NonSerialized]
	private int TempHPPenalty;

	public int x;

	public int y;

	public string zone;

	public HulkHoney_Tonic()
	{
	}

	public HulkHoney_Tonic(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		if (If.CoinFlip())
		{
			bOverdosed = true;
		}
		Duration = Stat.Roll(41, 50);
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "{{G|hulk}} {{w|honey}} tonic";
	}

	public override string GetDetails()
	{
		if (Object.IsTrueKin())
		{
			return "+9 Strength\n+3 temporary hit points per level.\n-25 Move Speed\nCan't feel pain.\nImmune to fear.\nSuffers 1% of max hit points in damage each turn (can't be reduced below one hit point by this effect).";
		}
		return "+6 Strength\n+2 temporary hit points per level.\n-25 Move Speed\nCan't feel pain.\nImmune to fear.\nSuffers 1% of max hit points in damage each turn (can't be reduced below one hit point by this effect).";
	}

	public override bool Apply(GameObject Object)
	{
		bool result = true;
		HulkHoney_Tonic effect = Object.GetEffect<HulkHoney_Tonic>();
		if (effect != null)
		{
			if (effect.Duration < Duration)
			{
				effect.Duration = Duration;
			}
			result = false;
		}
		else
		{
			Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_hulkTonic");
			if (Object.IsPlayer())
			{
				Popup.Show("Your muscles bulge grotesquely.");
			}
			ApplyChanges();
		}
		int num = 0;
		while (Object.HasEffect<Terrified>() && ++num < 10)
		{
			Object.RemoveEffect<Terrified>();
		}
		if (Object.GetLongProperty("Overdosing", 0L) == 1 || bOverdosed)
		{
			FireEvent(Event.New("Overdose"));
		}
		return result;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("Your muscles deflate to their usual size.");
		}
		UnapplyChanges();
	}

	private void ApplyChanges()
	{
		BonusHP = Object.Stat("Level") * (Object.IsTrueKin() ? 3 : 2);
		StatShifter.SetStatShift(Object, "MoveSpeed", 25);
		StatShifter.SetStatShift(Object, "Strength", Object.IsTrueKin() ? 9 : 6);
		StatShifter.SetStatShift(Object, "Hitpoints", BonusHP, baseValue: true);
		if (Object.HasStat("Hitpoints"))
		{
			Statistic statistic = Object.Statistics["Hitpoints"];
			if (TempHPPenalty > 0)
			{
				statistic.Penalty += TempHPPenalty;
				TempHPPenalty = 0;
			}
		}
		Object.ModIntProperty("Analgesia", 1);
	}

	private void UnapplyChanges()
	{
		if (Object.HasStat("Hitpoints"))
		{
			Statistic statistic = Object.Statistics["Hitpoints"];
			if (statistic.Penalty >= statistic.BaseValue - BonusHP)
			{
				int num = statistic.BaseValue - BonusHP - 1;
				TempHPPenalty = statistic.Penalty - num;
				statistic.Penalty = num;
			}
		}
		StatShifter.RemoveStatShifts(Object);
		Object.ModIntProperty("Analgesia", -1, RemoveIfZero: true);
		if (bOverdosed)
		{
			Object.ModIntProperty("ConfusionLevel", -1);
			Object.ModIntProperty("FuriousConfusionLevel", -1);
			bOverdosed = false;
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<BeginTakeActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0 && Object.HasStat("Hitpoints"))
		{
			HPLoss += (float)Object.BaseStat("Hitpoints") * 0.01f;
			int num = Math.Min(Object.hitpoints - 1, (int)Math.Floor(HPLoss));
			if (num > 0)
			{
				Object.TakeDamage(num, "from the {{G|hulk}} {{w|honey}}!", "Metabolic Expected Unavoidable");
				HPLoss -= num;
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("ApplyFear");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("CanApplyFear");
		Registrar.Register("Overdose");
		base.Register(Object, Registrar);
	}

	public override void ApplyAllergy(GameObject Object)
	{
		if (Object.GetLongProperty("Overdosing", 0L) == 1)
		{
			Popup.Show("Your mutant physiology reacts adversely to the tonic. Aaaaaaaaargh!");
		}
		else
		{
			Popup.Show("The tonics you ingested react adversely to each other. Aaaaaaaaargh!");
		}
		Object.ApplyEffect(new HulkHoney_Tonic_Allergy(Duration));
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyFear" || E.ID == "ApplyFear")
		{
			return false;
		}
		if (E.ID == "Overdose")
		{
			if (Duration > 0 && Object.IsPlayer())
			{
				Achievement.OVERDOSE_HULKHONEY.Unlock();
				if (Object.GetLongProperty("Overdosing", 0L) == 1)
				{
					Popup.Show("Your mutant physiology reacts adversely to the tonic. Aaaaaaaaargh!");
				}
				else
				{
					Popup.Show("The tonics you ingested react adversely to each other. Aaaaaaaaargh!");
				}
				bOverdosed = true;
				Object.ModIntProperty("ConfusionLevel", 1);
				Object.ModIntProperty("FuriousConfusionLevel", 1);
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyChanges();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyChanges();
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (Duration > 0 && num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "!";
			if (Stat.RandomCosmetic(1, 3) == 1)
			{
				E.ColorString = "&G";
			}
		}
		return true;
	}
}
