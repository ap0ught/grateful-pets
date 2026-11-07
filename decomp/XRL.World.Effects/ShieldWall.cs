using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class ShieldWall : Effect, ITierInitialized
{
	[NonSerialized]
	private GameObject Shield;

	public ShieldWall()
	{
		DisplayName = "shield wall";
	}

	public ShieldWall(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public ShieldWall(int Duration, GameObject Shield)
		: this(Duration)
	{
		this.Shield = Shield;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Roll(5, 15);
	}

	public override int GetEffectType()
	{
		return 67108992;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "{{g|shield wall}}";
	}

	public override string GetDetails()
	{
		return "Blocks all incoming melee attacks.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<ShieldWall>())
		{
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyShieldWall", "Effect", this)))
		{
			return false;
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_defensiveBuff");
		if (Shield != null)
		{
			DidXToY("raise", Shield, "in wall formation", "!", null, null, Object, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, Object);
		}
		else
		{
			DidX("raise", Object.its + " shield in wall formation", "!", null, null, Object);
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 21 && num < 31)
		{
			E.Tile = null;
			E.RenderString = "\u0004";
			E.ApplyColors("&B", Effect.ICON_COLOR_PRIORITY);
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Object.GetShield() == null)
		{
			Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}
}
