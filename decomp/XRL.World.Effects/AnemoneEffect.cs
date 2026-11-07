using System;
using System.Text;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class AnemoneEffect : Effect
{
	public const string DAMAGE_BONUS_ATTR = "Bleeding";

	public const string SAVE_PENALTY_VS = "Bleeding";

	public string DamageBonus;

	public int SavePenalty;

	public AnemoneEffect()
	{
		DisplayName = "{{r|anticoagulated}}";
	}

	public AnemoneEffect(string DamageBonus = null, int SavePenalty = 0, int Duration = 1)
		: this()
	{
		this.DamageBonus = DamageBonus;
		this.SavePenalty = SavePenalty;
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 100663824;
	}

	public override string GetDescription()
	{
		return null;
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (DamageBonus != null)
		{
			stringBuilder.Compound("Additional ", "\n").Append(DamageBonus).Append(" bleeding damage per turn.");
		}
		if (SavePenalty != 0)
		{
			SavingThrows.AppendSaveBonusDescription(stringBuilder, -SavePenalty, "Bleeding");
		}
		return stringBuilder.ToString();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID && (ID != BeforeApplyDamageEvent.ID || DamageBonus == null))
		{
			if (ID == ModifyDefendingSaveEvent.ID)
			{
				return SavePenalty != 0;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (SavingThrows.Applicable("Bleeding", E))
		{
			E.Roll -= SavePenalty;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Damage.HasAttribute("Bleeding"))
		{
			E.Damage.Amount = Math.Max(0, E.Damage.Amount + DamageBonus.RollCached());
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Duration > 0 && Duration != 9999)
		{
			Duration--;
		}
		return base.HandleEvent(E);
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.TryGetEffect<AnemoneEffect>(out var Effect))
		{
			Effect.Duration += Duration;
			return false;
		}
		return true;
	}
}
