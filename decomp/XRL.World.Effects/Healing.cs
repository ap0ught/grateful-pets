using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Healing : Effect, ITierInitialized
{
	private int DamageLeft;

	public Healing()
	{
		DisplayName = "healing";
	}

	public Healing(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = 5;
	}

	public override int GetEffectType()
	{
		return 83886084;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "5% HP healed per turn.\nStops healing if another action is taken or damage is taken.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Healing>())
		{
			Duration = 5;
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyingHealing", "Effect", this)))
		{
			return false;
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_positiveVitality");
		DidX("begin", "healing", null, null, null, Object);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == SingletonEvent<UseEnergyEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (!Object.CanMoveExtremities(null, ShowMessage: false, Involuntary: false, AllowTelekinetic: true))
		{
			Duration = 0;
		}
		else if (Duration > 0 && Duration != 9999)
		{
			Duration--;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Object.Heal(Math.Max(Object.BaseStat("Hitpoints") / 20, 1));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UseEnergyEvent E)
	{
		if (!E.Passive || (E.Type != null && !E.Type.Contains("Pass")))
		{
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your healing is interrupted!", 'r');
			}
			Duration = 0;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("TakeDamage");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "TakeDamage" && (E.GetParameter("Damage") as Damage).Amount > 0)
		{
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your healing is interrupted!", 'r');
			}
			Duration = 0;
		}
		return base.FireEvent(E);
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
				E.ColorString = "&g";
			}
		}
		return true;
	}
}
