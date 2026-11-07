using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Refresh : Effect
{
	public int Damage = 2;

	public Refresh()
	{
		DisplayName = "{{G|refreshed}}";
	}

	public Refresh(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 4;
	}

	public override bool SameAs(Effect e)
	{
		if ((e as Refresh).Damage != Damage)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return "5% HP healed per turn.";
	}

	public override bool Apply(GameObject Object)
	{
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_positiveVitality");
		return true;
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
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
		if (Duration > 0)
		{
			if (Object.HasStat("Hitpoints"))
			{
				Object.Heal(Math.Max(Object.GetStat("Hitpoints").BaseValue / 20, 1));
			}
			if (Duration != 9999)
			{
				Duration--;
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Recuperating");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration <= 0)
		{
			return true;
		}
		int num = XRLCore.CurrentFrame % 60;
		if (num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "\u0001";
			E.ColorString = "&G";
		}
		return true;
	}
}
