using System;
using XRL.Core;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class Poisoned : Effect, ITierInitialized
{
	public GameObject Owner;

	public string DamageIncrement;

	public int Level;

	public bool StartMessageUsePopup;

	public bool StopMessageUsePopup;

	public Poisoned()
	{
		DisplayName = "{{G|poisoned}}";
	}

	public Poisoned(int Duration, string DamageIncrement, int Level, GameObject Owner = null, bool StartMessageUsePopup = false, bool StopMessageUsePopup = false)
		: this()
	{
		base.Duration = Duration;
		this.DamageIncrement = DamageIncrement;
		this.Level = Level;
		this.Owner = Owner;
		this.StartMessageUsePopup = StartMessageUsePopup;
		this.StopMessageUsePopup = StopMessageUsePopup;
	}

	public void Initialize(int Tier)
	{
		Tier = XRL.World.Capabilities.Tier.Constrain(Stat.Random(Tier - 2, Tier + 2));
		Duration = Stat.Random(4, 10);
		Level = (int)((double)Tier * 1.5);
		DamageIncrement = Level + "d2";
	}

	public override int GetEffectType()
	{
		return 117506052;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "(" + DamageIncrement + ")/2 damage per turn.\nDoesn't regenerate hit points.\nHealing effects are only half as effective.\nWill become ill once the poison runs its course.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.TryGetEffect<Poisoned>(out var Effect))
		{
			if (Duration > Effect.Duration)
			{
				Effect.Duration = Duration;
			}
			if (DamageIncrement.RollMax() > Effect.DamageIncrement.RollMax())
			{
				Effect.DamageIncrement = DamageIncrement;
			}
			if (Level > Effect.Level)
			{
				Effect.Level = Level;
			}
			return false;
		}
		if (!Object.FireEvent("ApplyPoison") || !ApplyEffectEvent.Check(Object, "Poison", this))
		{
			return false;
		}
		Object.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_poisoned");
		DidX("have", "been poisoned", "!", null, null, null, Object, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StartMessageUsePopup);
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeforeBeginTakeActionEvent>.ID && ID != SingletonEvent<GeneralAmnestyEvent>.ID)
		{
			return ID == PooledEvent<GetCompanionStatusEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetCompanionStatusEvent E)
	{
		if (E.Object == Object)
		{
			E.AddStatus("poisoned", 30);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		GameObject.Validate(ref Owner);
		if (Duration > 0 && !Object.CurrentCell.HasObjectWithPart("GasPoison"))
		{
			Object.TakeDamage((int)Math.Ceiling((float)DamageIncrement.RollCached() / 2f), "from %t {{g|poison}}!", "Poison Unavoidable", null, null, Owner, null, null, null, null, Accidental: false, Environmental: false, Indirect: true);
			if (Duration > 0 && Duration != 9999)
			{
				Duration--;
			}
			if (Duration <= 0)
			{
				int duration = Stat.Random((int)(35f + 0.8f * (float)Level * 4.5f), (int)(35f + 1.2f * (float)Level * 4.5f));
				Object.ApplyEffect(new Ill(duration, Level, null, StopMessageUsePopup, StopMessageUsePopup));
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
