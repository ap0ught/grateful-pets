using System;
using XRL.Core;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class PoisonGasPoison : Effect, ITierInitialized
{
	public int Damage = 2;

	public GameObject Owner;

	public PoisonGasPoison()
	{
		DisplayName = "{{G|poisoned by gas}}";
	}

	public PoisonGasPoison(int Duration, GameObject Owner)
		: this()
	{
		this.Owner = Owner;
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Tier = XRL.World.Capabilities.Tier.Constrain(Stat.Random(Tier - 2, Tier + 2));
		Duration = Stat.Random(4, 10);
		Damage = Stat.Roll((int)((double)Tier * 1.5) + "d2");
	}

	public override int GetEffectType()
	{
		return 117506056;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return Damage + " damage per turn.";
	}

	public override bool Apply(GameObject Object)
	{
		Object.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_poisoned");
		if (Object.FireEvent("ApplyPoisonGasPoison"))
		{
			return ApplyEffectEvent.Check(Object, "PoisonGasPoison", this);
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeforeBeginTakeActionEvent>.ID)
		{
			return ID == SingletonEvent<GeneralAmnestyEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		GameObject.Validate(ref Owner);
		if (Duration > 0 && !Object.CurrentCell.HasObjectWithPart("GasPoison"))
		{
			Object.TakeDamage(Damage, "from %t {{g|poison}}!", "Poison Gas Unavoidable", null, null, Owner, null, null, null, null, Accidental: false, Environmental: false, Indirect: true);
			if (Duration > 0 && Duration != 9999)
			{
				Duration--;
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
		Registrar.Register("Recuperating");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Recuperating")
		{
			Duration = 0;
			DidX("are", "no longer poisoned", "!", null, null, Object);
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "!";
			E.ColorString = "&g^c";
		}
		return true;
	}
}
