using System;
using System.Text;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CoatedInPlasma : Effect, ITierInitialized
{
	public GameObject Owner;

	public CoatedInPlasma()
	{
		DisplayName = "{{coated in plasma|coated in plasma}}";
	}

	public CoatedInPlasma(int Duration = 1, GameObject Owner = null)
		: this()
	{
		base.Duration = Duration;
		this.Owner = Owner;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Roll(20, 200);
	}

	public override int GetEffectType()
	{
		return 100663328;
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
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("-100 heat resistance\n").Append("-100 cold resistance\n").Append("-100 electric resistance\n")
			.Append("Temperature does not passively return to ambient temperature\n")
			.Append("Patting or rolling firefighting actions are 25% as effective\n")
			.Append("Removes liquid coatings\n");
		return stringBuilder.ToString();
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent("CanApplyCoatedInPlasma"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyCoatedInPlasma"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "CoatedInPlasma", this))
		{
			return false;
		}
		CoatedInPlasma effect = Object.GetEffect<CoatedInPlasma>();
		if (effect != null)
		{
			if (Duration > effect.Duration)
			{
				effect.Duration = Duration;
			}
			if (!GameObject.Validate(ref effect.Owner) && GameObject.Validate(ref Owner))
			{
				effect.Owner = Owner;
			}
			return false;
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_electricalEffect");
		Object.RemoveAllEffects<LiquidCovered>();
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
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != PooledEvent<CanTemperatureReturnToAmbientEvent>.ID && ID != PooledEvent<GetFirefightingPerformanceEvent>.ID)
		{
			return ID == SingletonEvent<GeneralAmnestyEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		GameObject.Validate(ref Owner);
		Object.RemoveAllEffects<LiquidCovered>();
		if (Object.IsPlayer() && Object.IsAflame())
		{
			Achievement.AURORAL.Unlock();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanTemperatureReturnToAmbientEvent E)
	{
		if (E.Object == Object)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetFirefightingPerformanceEvent E)
	{
		if (E.Object == Object)
		{
			E.Result /= 4;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		Owner = null;
		return base.HandleEvent(E);
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

	public void ApplyStats()
	{
		StatShifter.SetStatShift("HeatResistance", -100);
		StatShifter.SetStatShift("ColdResistance", -100);
		StatShifter.SetStatShift("ElectricResistance", -100);
	}

	public void UnapplyStats()
	{
		StatShifter.RemoveStatShifts();
	}
}
