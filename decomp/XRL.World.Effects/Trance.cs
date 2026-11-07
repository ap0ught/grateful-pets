using System;
using XRL.Core;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Trance : Effect
{
	public int MentalBonus;

	public Trance()
	{
		DisplayName = "trance";
	}

	public Trance(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 2;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "entranced";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect(typeof(Trance)))
		{
			Duration = 50;
			return true;
		}
		if (Object.FireEvent(Event.New("ApplyTrance", "Effect", this)))
		{
			DidX("enter", "a trance", "!", null, null, Object);
			Object.ModIntProperty("MentalMutationShift", MentalBonus, RemoveIfZero: true);
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		Object.ModIntProperty("MentalMutationShift", -MentalBonus, RemoveIfZero: true);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (Duration > 0 && Object.TryGetPart<ActivatedAbilities>(out var Part))
		{
			Part.TickCooldowns(10);
		}
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 25 && num < 35)
			{
				E.Tile = null;
				E.RenderString = "*";
				E.ColorString = "&G";
			}
		}
		return true;
	}
}
