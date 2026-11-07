using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Rubbergum_Tonic_Allergy : Effect, ITierInitialized
{
	public Rubbergum_Tonic_Allergy()
	{
		DisplayName = "{{W|floppy}}";
	}

	public Rubbergum_Tonic_Allergy(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Roll(41, 100);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "{{W|floppy}}";
	}

	public override string GetDetails()
	{
		return "35% chance to fall prone when moving.";
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override bool Apply(GameObject Object)
	{
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_negativeVitality");
		return base.Apply(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!Object.IsFlying && 35.in100())
		{
			Object.ApplyEffect(new Prone());
		}
		return base.HandleEvent(E);
	}
}
