using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Blind : Effect, ITierInitialized
{
	public Blind()
	{
		DisplayName = "{{K|blind}}";
	}

	public Blind(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(20, 30);
	}

	public override string GetDetails()
	{
		return "Can't see.";
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 117448704;
	}

	public override bool Apply(GameObject Object)
	{
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_bewilderment");
		return Object.FireEvent(Event.New("ApplyBlind", "Duration", Duration));
	}

	public override void Remove(GameObject Object)
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (Duration > 0 && Object.IsPlayer())
		{
			Object.CurrentCell?.ParentZone.ClearLightMap();
			return false;
		}
		return base.HandleEvent(E);
	}
}
