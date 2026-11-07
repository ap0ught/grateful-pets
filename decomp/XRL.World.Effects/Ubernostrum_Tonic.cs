using System;
using XRL.Core;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class Ubernostrum_Tonic : ITonicEffect, ITierInitialized
{
	public float HealAmount;

	public Ubernostrum_Tonic()
	{
	}

	public void Initialize(int Tier)
	{
		Duration = 10;
	}

	public override void ApplyAllergy(GameObject subject)
	{
	}

	public Ubernostrum_Tonic(int Duration)
	{
		base.Duration = Duration;
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
		return "{{ubernostrum|ubernostrum}} tonic";
	}

	public override string GetStateDescription()
	{
		return "under the effects of {{ubernostrum|ubernostrum}} tonic";
	}

	public override string GetDetails()
	{
		if (Object.IsTrueKin())
		{
			return "Recovers 0.9 hit points per level (minimum 5) each turn.\nPurged of all short-term, biological debuffs and up to one severed appendage is regrown at the end of the tenth turn.";
		}
		return "Recovers 0.6 hit points per level (minimum 3) each turn.\nPurged of all short-term, biological debuffs and up to one severed appendage is regrown at the end of the tenth turn.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("A torrent of life rushes over you.");
		}
		SendRegeneraEvent();
		if (Object.GetLongProperty("Overdosing", 0L) == 1)
		{
			FireEvent(Event.New("Overdose"));
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		Object.FireEvent("Recuperating");
		RegenerateLimbEvent.Send(Object, null, null, Whole: true);
		SendRegeneraEvent();
		if (Object.IsPlayer())
		{
			Popup.Show("The torrent of life sweeps away.");
		}
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<EndActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndActionEvent E)
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
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Overdose");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Overdose" && Duration > 0)
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
						Popup.Show("Your mutant physiology reacts adversely to the tonic. The torrent of life sweeps away.");
					}
					else
					{
						Popup.Show("The tonics you ingested react adversely to each other. The torrent of life sweeps away.");
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (Duration > 0 && num > 20 && num < 30)
		{
			E.Tile = null;
			E.RenderString = "+";
			E.ColorString = "&G";
		}
		return true;
	}

	public void SendRegeneraEvent(GameObject Object)
	{
		Object.FireEvent(Event.New("Regenera", "SourceDescription", "The " + GetDescription() + " cures you of", "Level", 1));
	}

	public void SendRegeneraEvent()
	{
		SendRegeneraEvent(Object);
	}
}
