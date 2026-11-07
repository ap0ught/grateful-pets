using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class SporeCloudPoison : Effect, ITierInitialized
{
	public int Damage = 2;

	public GameObject Owner;

	public SporeCloudPoison()
	{
		DisplayName = "{{W|covered in spores}}";
	}

	public SporeCloudPoison(int Duration, GameObject Owner)
		: this()
	{
		this.Owner = Owner;
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(2, 5);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override bool SameAs(Effect e)
	{
		SporeCloudPoison sporeCloudPoison = e as SporeCloudPoison;
		if (sporeCloudPoison.Damage != Damage)
		{
			return false;
		}
		if (sporeCloudPoison.Owner != Owner)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return Damage + " damage per turn.";
	}

	public override bool Apply(GameObject Object)
	{
		Object.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_poisoned");
		if (Object.FireEvent(Event.New("ApplySporeCloudInfection")))
		{
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == SingletonEvent<GeneralAmnestyEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		GameObject.Validate(ref Owner);
		if (Duration > 0)
		{
			Object.TakeDamage(Damage, "from %t spores!", "Poison Gas Fungal Spores Unavoidable", null, null, Owner, null, null, null, null, Accidental: false, Environmental: false, Indirect: true);
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
			DidX("shake", "the spores off", null, null, null, Object);
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			E.RenderEffectIndicator("à", "Tiles2/status_spores.bmp", "&M", "M", 45);
		}
		return true;
	}
}
