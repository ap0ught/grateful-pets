using System;
using HistoryKit;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class LoveTonic : Effect, ITierInitialized
{
	public bool bOverdose;

	public LoveTonic()
	{
		DisplayName = "{{amorous|love}} tonic";
	}

	public LoveTonic(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		if (If.CoinFlip())
		{
			bOverdose = true;
		}
		Duration = Stat.Roll(500, 700);
	}

	public override string GetStateDescription()
	{
		return "under the effects of {{amorous|love}} tonic";
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool UseThawEventToUpdateDuration()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override string GetDescription()
	{
		return "{{amorous|love}} tonic";
	}

	public override string GetDetails()
	{
		return "Will fall in love with the first thing examined.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("Your heart swells with a burning sensation.");
		}
		if (Object.GetLongProperty("Overdosing", 0L) == 1 || bOverdose)
		{
			FireEvent(Event.New("Overdose"));
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("Your heart rate slows again.");
		}
		base.Remove(Object);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("LookedAt");
		Registrar.Register("Overdose");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "LookedAt")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (gameObjectParameter != null && (gameObjectParameter.IsCombat || gameObjectParameter.GetBlueprint().InheritsFrom("Furniture")))
			{
				Object.ApplyEffect(new Lovesick(Stat.Random(3000, 3600), gameObjectParameter));
				Duration = 0;
				Object.CleanEffects();
			}
		}
		else if (E.ID == "Overdose" && Duration > 0)
		{
			Duration = 0;
			if (Object.IsPlayer())
			{
				if (Object.GetLongProperty("Overdosing", 0L) == 1)
				{
					Popup.Show("Your mutant physiology reacts adversely to the tonic. You erupt into flames!");
				}
				else
				{
					Popup.Show("The tonics you ingested react adversely to each other. You erupt into flames!");
				}
			}
			Object.Physics.Temperature = Object.Physics.FlameTemperature + 200;
			Object.CleanEffects();
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (Duration > 0 && num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "\u0003";
			switch (Stat.RandomCosmetic(1, 4))
			{
			case 1:
				E.ColorString = "&r";
				break;
			case 2:
				E.ColorString = "&R";
				break;
			case 3:
				E.ColorString = "&M";
				break;
			case 4:
				E.ColorString = "&m";
				break;
			}
		}
		return true;
	}
}
