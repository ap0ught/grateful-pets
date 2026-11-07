using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class ProjectileReflectionShield : Effect, ITierInitialized
{
	public int Chance = 100;

	public string RetroVariance = "0";

	public string Details = "";

	public string Verb = "reflect";

	public ProjectileReflectionShield()
	{
		DisplayName = "{{Y|reflectively shielded}}";
		Duration = 9999;
		Chance = 100;
		Details = "Reflects incoming projectiles and thrown objects at a " + Chance + "% chance.";
	}

	public ProjectileReflectionShield(int Chance, string RetroVariance = "0", string Verb = "reflect")
		: this()
	{
		this.Chance = Chance;
		this.RetroVariance = RetroVariance;
		Details = "Reflects incoming projectiles and thrown objects at a " + Chance + "% chance.";
		Duration = 9999;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(50, 300);
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
		return Details;
	}

	public override int GetEffectType()
	{
		return 64;
	}

	public override bool Apply(GameObject Object)
	{
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_defensiveBuff");
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ReflectProjectile");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ReflectProjectile" && Chance.in100())
		{
			E.SetParameter("By", Object);
			if (!string.IsNullOrEmpty(RetroVariance))
			{
				float num = (float)E.GetParameter("Angle");
				E.SetParameter("Direction", (int)num + 180 + RetroVariance.RollCached());
			}
			if (!string.IsNullOrEmpty(Verb))
			{
				E.SetParameter("Verb", Verb);
			}
			return false;
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 40 && num < 60)
			{
				E.RenderString = "\t";
				E.ColorString = "&Y";
			}
		}
		return true;
	}
}
