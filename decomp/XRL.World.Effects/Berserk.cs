using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Berserk : Effect, ITierInitialized
{
	public Berserk()
	{
		DisplayName = "berserk";
	}

	public Berserk(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(5, 15);
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override int GetEffectType()
	{
		return 67108866;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		return "100% chance to dismember with axes.";
	}

	public override bool Apply(GameObject Object)
	{
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_enraged");
		if (Object.TryGetEffect<Berserk>(out var Effect))
		{
			if (Duration > Effect.Duration)
			{
				Effect.Duration = Duration;
			}
			return false;
		}
		return true;
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
		if (Duration > 0 && Duration != 9999 && Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage(Duration.Things("turn remains", "turns remain") + " until your berserker rage ends.");
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			E.RenderEffectIndicator("!", "Abilities/abil_berserk.bmp", "&R", "R", 45, 55);
		}
		return base.Render(E);
	}
}
