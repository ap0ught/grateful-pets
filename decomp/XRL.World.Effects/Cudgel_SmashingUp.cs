using System;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class Cudgel_SmashingUp : Effect, ITierInitialized
{
	public Cudgel_SmashingUp()
	{
		DisplayName = "demolishing";
	}

	public Cudgel_SmashingUp(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(5, 15);
	}

	public override int GetEffectType()
	{
		return 67108992;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Slam has no cooldown.\n100% chance to daze with cudgels.";
	}

	public override bool Apply(GameObject Object)
	{
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_enraged");
		return true;
	}

	public override void Remove(GameObject Object)
	{
		Cudgel_Slam part = Object.GetPart<Cudgel_Slam>();
		part?.CooldownMyActivatedAbility(part.ActivatedAbilityID, 50);
		base.Remove(Object);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 45 && num < 55)
			{
				E.Tile = null;
				E.RenderString = "!";
				E.ColorString = "&R";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (Duration > 0)
			{
				Duration--;
			}
			if (Duration > 0 && Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage(Duration.Things("turn remains", "turns remain") + " until you stop demolishing.");
			}
			return true;
		}
		return base.FireEvent(E);
	}
}
