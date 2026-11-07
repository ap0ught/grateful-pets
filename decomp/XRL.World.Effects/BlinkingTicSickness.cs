using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class BlinkingTicSickness : Effect, ITierInitialized
{
	public BlinkingTicSickness()
	{
		DisplayName = "{{B|acute blinking tic}}";
	}

	public BlinkingTicSickness(int duration)
		: this()
	{
		Duration = duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Roll(960, 1440);
	}

	public override string GetDescription()
	{
		return "{{B|blinking}}";
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override int GetEffectType()
	{
		return 117440768;
	}

	public override string GetDetails()
	{
		return "There is a small chance each round you're in combat that you randomly teleport to a nearby location.";
	}

	public override bool Apply(GameObject Object)
	{
		BlinkingTicSickness effect = Object.GetEffect<BlinkingTicSickness>();
		if (effect != null)
		{
			effect.Duration = Math.Max(Duration, effect.Duration);
			return false;
		}
		Object.RegisterEffectEvent(this, "EndTurn");
		return true;
	}

	public override void Remove(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "EndTurn");
		base.Remove(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			Duration--;
			Cell cell = Object.CurrentCell;
			if (cell == null || cell.ParentZone.IsWorldMap())
			{
				return true;
			}
			if (Object.IsPlayer() && !Object.AreHostilesNearby())
			{
				return true;
			}
			if (!CheckMyRealityDistortionUsability())
			{
				return true;
			}
			if (2.in1000())
			{
				if (Object.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You lurch suddenly!", 'r');
				}
				Object.RandomTeleport(Swirl: true);
			}
			return true;
		}
		return base.FireEvent(E);
	}
}
