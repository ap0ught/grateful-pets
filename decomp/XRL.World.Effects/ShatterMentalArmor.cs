using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class ShatterMentalArmor : IShatterEffect, ITierInitialized
{
	public int MAPenalty = 1;

	public GameObject Owner;

	public ShatterMentalArmor()
	{
		DisplayName = "{{psionic|psionically cleaved}}";
	}

	public ShatterMentalArmor(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public ShatterMentalArmor(int Duration, GameObject Owner = null, int MAPenalty = 1)
		: this(Duration)
	{
		this.Owner = Owner;
		this.MAPenalty = MAPenalty;
	}

	public void Initialize(int Tier)
	{
		Tier = Stat.Random(Tier - 2, Tier + 2);
		if (Tier < 1)
		{
			Tier = 1;
		}
		if (Tier > 8)
		{
			Tier = 8;
		}
		Duration = 300;
		MAPenalty = Stat.Random(1, 10);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override bool SameAs(Effect e)
	{
		ShatterMentalArmor shatterMentalArmor = e as ShatterMentalArmor;
		if (shatterMentalArmor.MAPenalty != MAPenalty)
		{
			return false;
		}
		if (shatterMentalArmor.Owner != Owner)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDescription()
	{
		return "{{psionic|psionically cleaved (-" + MAPenalty + " MA)}}";
	}

	public override string GetStateDescription()
	{
		return "{{psionic|psionically cleaved}}";
	}

	public override string GetDetails()
	{
		return "-" + MAPenalty + " MA";
	}

	public override int GetPenalty()
	{
		return MAPenalty;
	}

	public override void IncrementPenalty()
	{
		MAPenalty++;
	}

	public override GameObject GetOwner()
	{
		return Owner;
	}

	public override void SetOwner(GameObject Owner)
	{
		this.Owner = Owner;
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasStat("MA"))
		{
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyShatterMentalArmor", "Owner", Owner, "Effect", this)) && ApplyEffectEvent.Check(Object, "ShatterMentalArmor", this))
		{
			return false;
		}
		Object.PlayWorldSound("breakage", 0.5f, 0f, Combat: true);
		if (Object.TryGetEffect<ShatterMentalArmor>(out var Effect))
		{
			if (Duration > Effect.Duration)
			{
				Effect.Duration = Duration;
			}
			Effect.UnapplyStats();
			Effect.MAPenalty += MAPenalty;
			Effect.ApplyStats();
			Object.ParticleText("*psionic cleave (-" + Effect.MAPenalty + " MA)*", 'b');
			return false;
		}
		ApplyStats();
		Object.ParticleText("*psionic cleave (-" + MAPenalty + " MA)*", 'b');
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift("MA", -MAPenalty);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts();
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 40 && num < 55)
			{
				E.Tile = null;
				E.RenderString = "X";
				E.ColorString = "&M^k";
			}
		}
		return true;
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
