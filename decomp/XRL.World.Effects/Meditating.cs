using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Meditating : Effect, ITierInitialized
{
	public int DamageLeft;

	public bool FromResting;

	public Meditating()
	{
		DisplayName = "meditating";
		Duration = 1;
	}

	public Meditating(int Duration = 1, bool FromResting = false)
		: this()
	{
		base.Duration = Duration;
		this.FromResting = FromResting;
	}

	public override int GetEffectType()
	{
		return 218103810;
	}

	public override bool SameAs(Effect e)
	{
		Meditating meditating = e as Meditating;
		if (meditating.DamageLeft != DamageLeft)
		{
			return false;
		}
		if (meditating.FromResting != FromResting)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return "Regenerates at thrice the normal rate.\nNegative status effects wear off at thrice the normal rate.\nStops meditating if another action is taken or " + GetDamageThreshold() + " damage is taken in a single round.";
	}

	public override bool Apply(GameObject Object)
	{
		Meditating effect = Object.GetEffect<Meditating>();
		if (effect != null)
		{
			effect.Duration = Math.Max(effect.Duration, Duration);
			return false;
		}
		DamageLeft = GetDamageThreshold();
		if (!Object.FireEvent(Event.New("ApplyMeditating", "Effect", this)))
		{
			return false;
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_positiveVitality");
		DidX("begin", "meditating");
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			if (FromResting && DamageLeft > 0)
			{
				IComponent<GameObject>.AddPlayerMessage("You stop meditating and feel refreshed.", 'G');
			}
			else
			{
				IComponent<GameObject>.AddPlayerMessage("You stop meditating.", 'R');
			}
		}
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID && ID != SingletonEvent<UseEnergyEvent>.ID)
		{
			return ID == TookDamageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		DamageLeft = GetDamageThreshold();
		int i = 0;
		for (int count = Object.Effects.Count; i < count; i++)
		{
			Effect effect = Object.Effects[i];
			if (effect.Duration > 1 && effect.Duration != 9999 && effect.IsOfTypes(100663296))
			{
				effect.Duration--;
				if (effect.Duration > 1)
				{
					effect.Duration--;
				}
			}
		}
		Bleeding effect2 = Object.GetEffect<Bleeding>();
		if (effect2 != null && !effect2.RecoveryChance())
		{
			effect2.RecoveryChance();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (E.Object == Object)
		{
			DamageLeft -= E.Damage.Amount;
			if (DamageLeft <= 0)
			{
				Object.RemoveEffect(this);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UseEnergyEvent E)
	{
		if (!E.Passive || (E.Type != null && !E.Type.Contains("Pass")))
		{
			Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 25 && num < 35)
			{
				E.Tile = null;
				E.RenderString = "Z";
				E.ColorString = "&G";
			}
		}
		return true;
	}

	public int GetDamageThreshold()
	{
		return Math.Max(Object.Stat("Willpower") * 3 - 60, 1);
	}
}
