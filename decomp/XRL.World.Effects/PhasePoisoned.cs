using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class PhasePoisoned : Effect, ITierInitialized
{
	public GameObject Owner;

	public string DamageIncrement;

	public int Level;

	public PhasePoisoned()
	{
		DisplayName = "{{g|phase spider venom}}";
	}

	public PhasePoisoned(int Duration, string DamageIncrement, int Level, GameObject Owner)
		: this()
	{
		base.Duration = Duration;
		this.DamageIncrement = DamageIncrement;
		this.Level = Level;
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
		Duration = Stat.Random(4, 10);
		Level = (int)((double)Tier * 1.5);
		DamageIncrement = Level + "d2";
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override string GetDescription()
	{
		return "{{g|phase spider venom}}";
	}

	public override string GetStateDescription()
	{
		return "{{g|poisoned with phase spider venom}}";
	}

	public override string GetDetails()
	{
		return "Phased.\n(" + DamageIncrement + ")/2 damage per turn.\nDoesn't regenerate hit points.\nHealing effects are only half as effective.\nWill become ill once the poison runs its course.";
	}

	public override bool Apply(GameObject Object)
	{
		Poisoned effect = Object.GetEffect<Poisoned>();
		if (effect != null)
		{
			if (Duration > effect.Duration)
			{
				effect.Duration = Duration;
			}
			if (DamageIncrement.RollMax() > effect.DamageIncrement.RollMax())
			{
				effect.DamageIncrement = DamageIncrement;
			}
			if (Level > effect.Level)
			{
				effect.Level = Level;
			}
			return false;
		}
		Object.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_poisoned");
		if (Object.FireEvent("ApplyPoison") && ApplyEffectEvent.Check(Object, "Poison", this))
		{
			DidX("have", "been poisoned", "!", null, null, null, Object);
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID)
		{
			return ID == SingletonEvent<GeneralAmnestyEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		GameObject.Validate(ref Owner);
		if (Duration > 0)
		{
			if (Object.HasEffect<Phased>())
			{
				Object.GetEffect<Phased>().Duration += Stat.Random(1, 2);
			}
			else
			{
				Object.ApplyEffect(new Phased(Stat.Random(3, 6)));
			}
			if (!Object.CurrentCell.HasObjectWithPart("GasPoison"))
			{
				Object.TakeDamage((int)Math.Ceiling((float)DamageIncrement.RollCached() / 2f), "from %t {{g|poison}}!", "Poison Unavoidable", null, null, Owner, null, null, null, null, Accidental: false, Environmental: false, Indirect: true);
				if (Duration > 0 && Duration != 9999)
				{
					Duration--;
				}
				if (Duration <= 0)
				{
					int duration = Stat.Random((int)(35f + 0.8f * (float)Level * 4.5f), (int)(35f + 1.2f * (float)Level * 4.5f));
					Object.ApplyEffect(new Ill(duration, Level));
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		Owner = null;
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Healing");
		Registrar.Register("Recuperating");
		Registrar.Register("Regenerating");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "\u0003";
			E.ColorString = "&G^k";
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Healing")
		{
			E.SetParameter("Amount", E.GetIntParameter("Amount") / 2);
		}
		else
		{
			if (E.ID == "Regenerating")
			{
				E.SetParameter("Amount", 0);
				return false;
			}
			if (E.ID == "Recuperating")
			{
				Duration = 0;
				DidX("are", "no longer poisoned", "!", null, null, Object);
			}
		}
		return base.FireEvent(E);
	}
}
