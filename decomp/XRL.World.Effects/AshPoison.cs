using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class AshPoison : Effect, ITierInitialized
{
	public int Damage = 2;

	public GameObject Owner;

	public AshPoison()
	{
		DisplayName = "{{K|choking on ash}}";
	}

	public AshPoison(int Duration, GameObject Owner)
		: this()
	{
		this.Owner = Owner;
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(4, 10);
		Damage = Stat.Random(2, 4);
	}

	public override string GetStateDescription()
	{
		return "{{K|choked by ash}}";
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 117440520;
	}

	public override string GetDetails()
	{
		return Damage + " damage per turn.";
	}

	public override bool Apply(GameObject Object)
	{
		Object.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_poisoned");
		if (Object.FireEvent("ApplyAshPoison"))
		{
			return ApplyEffectEvent.Check(Object, "AshPoison", this);
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID && ID != SingletonEvent<GeneralAmnestyEvent>.ID)
		{
			return ID == PooledEvent<GetCompanionStatusEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetCompanionStatusEvent E)
	{
		if (E.Object == Object)
		{
			E.AddStatus("choking on ash", 90);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		GameObject.Validate(ref Owner);
		if (Duration > 0 && Object?.CurrentCell != null && !Object.CurrentCell.HasObjectWithPart("GasAsh"))
		{
			Object.TakeDamage(Damage, "from %t {{W|choking ash}}!", "Asphyxiation Gas Unavoidable", null, null, null, Owner, null, null, null, Accidental: false, Environmental: false, Indirect: true);
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

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "!";
			E.ColorString = "&W^r";
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Recuperating")
		{
			Duration = 0;
			DidX("are", "no longer choking", "!", null, null, Object);
		}
		return base.FireEvent(E);
	}
}
