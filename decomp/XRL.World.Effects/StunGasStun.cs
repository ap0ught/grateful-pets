using System;
using XRL.Core;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class StunGasStun : Effect
{
	public int Density = 20;

	public int SpeedPenalty;

	public StunGasStun()
	{
		Duration = 1;
		DisplayName = GetDescription();
	}

	public StunGasStun(int _Density)
		: this()
	{
		Density = _Density;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override int GetEffectType()
	{
		return 117440520;
	}

	public override string GetDescription()
	{
		return "{{C|stunned by gas}}";
	}

	public override string GetDetails()
	{
		if (Density > 60)
		{
			return "(very dense) Can't take actions.";
		}
		if (Density > 40)
		{
			return "(dense) -60% Quickness";
		}
		return "(light) -30% Quickness";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasPart<Brain>())
		{
			return false;
		}
		if (!Object.FireEvent("CanApplyStunGasStun"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "StunGasStun", this))
		{
			return false;
		}
		return Object.FireEvent("ApplyStunGasStun");
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("CommandTakeAction");
		Registrar.Register("IsMobile");
		base.Register(Object, Registrar);
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift("Speed", -SpeedPenalty);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts();
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 10 && num < 25)
			{
				if (Density > 60)
				{
					E.Tile = null;
					E.RenderString = "!";
					E.ColorString = "&C^c";
				}
				else if (SpeedPenalty > 0)
				{
					E.Tile = null;
					E.RenderString = "\u0019";
					E.ColorString = "&C^c";
				}
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandTakeAction")
		{
			if (Duration > 0)
			{
				Duration = 0;
				if (Object.FireEvent("ApplyStun"))
				{
					if (Density > 60)
					{
						DidX("are", "stunned", "!", null, null, null, Object);
						Object.ForfeitTurn();
						Object.ParticleText("*stunned*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
						return false;
					}
					if (Density > 40)
					{
						if (SpeedPenalty != 0)
						{
							UnapplyStats();
						}
						SpeedPenalty = Object.Stat("Speed") * 6 / 10;
						ApplyStats();
					}
					else
					{
						if (SpeedPenalty != 0)
						{
							UnapplyStats();
						}
						SpeedPenalty = Object.Stat("Speed") * 3 / 10;
						ApplyStats();
					}
				}
			}
		}
		else if (E.ID == "IsMobile")
		{
			if (Duration > 0 && Density > 60)
			{
				return false;
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}
}
