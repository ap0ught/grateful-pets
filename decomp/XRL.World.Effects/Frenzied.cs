using System;
using System.Text;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Frenzied : Effect, ITierInitialized
{
	public const int DEFAULT_MAX_KILL_RADIUS_BONUS = 10;

	public const int DEFAULT_QUICKNESS_BONUS = 10;

	public const int DEFAULT_BERSERK_DURATION = 5;

	public int QuicknessBonus = 10;

	public int MaxKillRadiusBonus = 10;

	public int BerserkDuration = 5;

	public bool BerserkImmediately;

	public bool BerserkOnDealDamage;

	public bool PreferBleedingTarget;

	public Frenzied()
	{
		DisplayName = "{{B|frenzied}}";
		Duration = 1;
	}

	public Frenzied(int Duration = 1, int QuicknessBonus = 10, int MaxKillRadiusBonus = 10, int BerserkDuration = 5, bool BerserkImmediately = false, bool BerserkOnDealDamage = false, bool PreferBleedingTarget = false)
		: this()
	{
		base.Duration = Duration;
		this.QuicknessBonus = QuicknessBonus;
		this.MaxKillRadiusBonus = MaxKillRadiusBonus;
		this.BerserkDuration = BerserkDuration;
		this.BerserkImmediately = BerserkImmediately;
		this.BerserkOnDealDamage = BerserkOnDealDamage;
		this.PreferBleedingTarget = PreferBleedingTarget;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(100, 200);
	}

	public override int GetEffectType()
	{
		return 83886082;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (QuicknessBonus != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append('\n');
			}
			if (QuicknessBonus > 0)
			{
				stringBuilder.Append('+');
			}
			stringBuilder.Append(QuicknessBonus).Append(" Quickness");
		}
		stringBuilder.Compound("More likely to attack things.", '\n');
		if (PreferBleedingTarget)
		{
			stringBuilder.Compound("Will prefer to attack creatures who are bleeding.", '\n');
		}
		if (BerserkOnDealDamage)
		{
			stringBuilder.Compound("Will go berserk the next time ").Append(Object.it).Append(Object.GetVerb("deal"))
				.Append(" damage in combat.");
		}
		return stringBuilder.ToString();
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Frenzied>())
		{
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyFrenzied", "Duration", Duration)))
		{
			return false;
		}
		Object.Brain?.Goals.Clear();
		ApplyChanges();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyChanges();
		base.Remove(Object);
	}

	private void ApplyChanges()
	{
		if (MaxKillRadiusBonus != 0 && Object.Brain != null)
		{
			Object.Brain.MaxKillRadius += MaxKillRadiusBonus;
		}
		StatShifter.SetStatShift("Speed", QuicknessBonus);
		if (BerserkImmediately)
		{
			TriggerBerserk();
		}
	}

	private void UnapplyChanges()
	{
		if (MaxKillRadiusBonus != 0 && Object.Brain != null)
		{
			Object.Brain.MaxKillRadius -= MaxKillRadiusBonus;
		}
		StatShifter.RemoveStatShifts();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != AttackerDealtDamageEvent.ID || !BerserkOnDealDamage))
		{
			if (ID == PooledEvent<PreferTargetEvent>.ID)
			{
				return PreferBleedingTarget;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(AttackerDealtDamageEvent E)
	{
		if (BerserkOnDealDamage && !Object.HasEffect<Berserk>())
		{
			TriggerBerserk();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PreferTargetEvent E)
	{
		if (E.Result == 0)
		{
			E.Result = (E.Target1.HasEffect<Bleeding>() && E.Target1.GetBleedLiquid() == "blood-1000").CompareTo(E.Target2.HasEffect<Bleeding>() && E.Target2.GetBleedLiquid() == "blood-1000");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetFeelingEvent E)
	{
		if (!E.Target.HasEffect(typeof(Frenzied)))
		{
			E.Feeling = -100;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register(PooledEvent<GetFeelingEvent>.ID);
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeepCopyWithoutEffects")
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
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 10)
			{
				E.RenderString = "!";
				E.ColorString = "&R";
			}
		}
		return true;
	}

	public void TriggerBerserk()
	{
		if (Object.ApplyEffect(new Berserk(BerserkDuration)))
		{
			DidX("enter", "a berserk fury", "!", null, null, Object);
			BerserkOnDealDamage = false;
		}
	}
}
