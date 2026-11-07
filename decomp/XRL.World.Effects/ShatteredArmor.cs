using System;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class ShatteredArmor : IShatterEffect, ITierInitialized
{
	public int Amount;

	public GameObject Owner;

	public ShatteredArmor()
	{
		DisplayName = "cracked";
		Duration = 1;
	}

	public ShatteredArmor(int Amount)
		: this()
	{
		this.Amount = Amount;
	}

	public ShatteredArmor(int Amount, int Duration)
		: this(Amount)
	{
		base.Duration = Duration;
	}

	public ShatteredArmor(int Amount, int Duration, GameObject Owner)
		: this(Amount, Duration)
	{
		this.Owner = Owner;
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
		Amount = Stat.Random(1, 10);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 117441536;
	}

	public override bool SameAs(Effect e)
	{
		ShatteredArmor shatteredArmor = e as ShatteredArmor;
		if (shatteredArmor.Amount != Amount)
		{
			return false;
		}
		if (shatteredArmor.Owner != Owner)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override int GetPenalty()
	{
		return Amount;
	}

	public override void IncrementPenalty()
	{
		Amount++;
	}

	public override GameObject GetOwner()
	{
		return Owner;
	}

	public override void SetOwner(GameObject Owner)
	{
		this.Owner = Owner;
	}

	public override string GetDetails()
	{
		return "-" + Amount + " AV";
	}

	public override string GetDescription()
	{
		return "{{r|cracked}}";
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
	}

	public override bool Apply(GameObject Object)
	{
		if (Object == null || Object.Equipped == null)
		{
			return false;
		}
		Armor part = Object.GetPart<Armor>();
		if (part == null)
		{
			return false;
		}
		if (Amount > part.AV)
		{
			Amount = part.AV;
		}
		if (Amount <= 0)
		{
			return false;
		}
		if (!Object.FireEvent("ApplyShatteredArmor"))
		{
			return false;
		}
		bool result = true;
		Object.PlayWorldSound("breakage", 0.5f, 0f, Combat: true);
		if (Object.TryGetEffect<ShatteredArmor>(out var Effect))
		{
			Effect.UnapplyStats();
			Effect.Amount += Amount;
			Effect.ApplyStats();
			if (Effect.Duration < Duration)
			{
				Effect.Duration = Duration;
			}
			result = false;
		}
		else
		{
			ApplyStats();
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_physicalRupture");
		Object?.Equipped?.ParticleText("*" + Object.ShortDisplayNameStripped + " cracked*", IComponent<GameObject>.ConsequentialColorChar(null, Object.Equipped));
		if (Object != null && Object.Equipped?.IsPlayer() == true)
		{
			IComponent<GameObject>.AddPlayerMessage(Object.Does("were") + " cracked.", 'R');
		}
		return result;
	}

	private void ApplyStats()
	{
		Armor armor = Object?.GetPart<Armor>();
		if (armor != null)
		{
			armor.AV -= Amount;
		}
	}

	private void UnapplyStats()
	{
		Armor armor = Object?.GetPart<Armor>();
		if (armor != null)
		{
			armor.AV += Amount;
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustValueEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<IsRepairableEvent>.ID)
		{
			return ID == PooledEvent<RepairedEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Reference)
		{
			E.AddTag("[{{r|cracked}}]");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AdjustValueEvent E)
	{
		if (Amount > 1)
		{
			E.AdjustValue(1.0 / (double)Amount);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsRepairableEvent E)
	{
		return false;
	}

	public override bool HandleEvent(RepairedEvent E)
	{
		Object.RemoveEffect(this);
		return base.HandleEvent(E);
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
