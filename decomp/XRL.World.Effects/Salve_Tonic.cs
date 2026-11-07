using System;
using XRL.Core;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class Salve_Tonic : ITonicEffect, ITierInitialized
{
	public float HealAmount;

	public Salve_Tonic()
	{
	}

	public Salve_Tonic(int Duration)
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = 5;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "{{Y|salve}} tonic";
	}

	public override string GetStateDescription()
	{
		return "under the effects of {{Y|salve}} tonic";
	}

	public override string GetDetails()
	{
		if (Object.IsTrueKin())
		{
			return "Recovers 0.9 hit points per level (minimum 5) each turn.";
		}
		return "Recovers 0.6 hit points per level (minimum 3) each turn.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("You feel a soothing tingle in your chest as your wounds start to close.");
		}
		if (Object.GetLongProperty("Overdosing", 0L) == 1)
		{
			FireEvent(Event.New("Overdose"));
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("The soothing tingle fades.");
		}
		base.Remove(Object);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndAction");
		Registrar.Register("Overdose");
		base.Register(Object, Registrar);
	}

	public override void ApplyAllergy(GameObject subject)
	{
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndAction")
		{
			if (Object.HasStat("Hitpoints") && Object.HasStat("Level"))
			{
				int num = Object.BaseStat("Level");
				if (Object.IsTrueKin())
				{
					HealAmount += (float)Math.Max(5.0, Math.Ceiling(0.9 * (double)(float)num));
				}
				else
				{
					HealAmount += (float)Math.Max(3.0, Math.Ceiling(0.6 * (double)(float)num));
				}
				if (HealAmount >= 1f)
				{
					Object.Heal((int)Math.Floor(HealAmount), Message: true, FloatText: true, RandomMinimum: true);
					HealAmount -= (float)Math.Floor(HealAmount);
				}
			}
		}
		else if (E.ID == "Overdose" && Duration > 0)
		{
			if (Object.HasPart<TonicAllergy>())
			{
				TonicAllergy.SalveOverdose(Object);
			}
			else
			{
				Duration = 0;
				if (Object.IsPlayer())
				{
					if (Object.GetLongProperty("Overdosing", 0L) == 1)
					{
						Popup.Show("Your mutant physiology reacts adversely to the tonic. The soothing tingle fades.");
					}
					else
					{
						Popup.Show("The tonics you ingested react adversely to each other. The soothing tingle fades.");
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (Duration > 0 && num > 15 && num < 25)
		{
			E.Tile = null;
			E.RenderString = "+";
			E.ColorString = "&Y";
		}
		return true;
	}
}
