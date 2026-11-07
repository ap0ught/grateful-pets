using System;
using HistoryKit;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Hoarshroom_Tonic : ITonicEffect, ITierInitialized
{
	public int x;

	public int y;

	public string zone;

	public int HealRounds = 5;

	public float HealAmount;

	public bool bOverdose;

	public Hoarshroom_Tonic()
	{
	}

	public Hoarshroom_Tonic(int Duration)
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
		Duration = Stat.Roll(180, 220);
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
		return "{{C|illuminated}}";
	}

	public override string GetDetails()
	{
		return "Casts light in radius 2.\nFor the first 5 rounds after imbibing, gains 0.6 hit points per level (minimum 3) each turn.\n+20 Cold Resist\nCan't be poisoned.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_hoarshroomTonic");
			Popup.Show("You feel a cool swelling as your organs start to glow through your skin.");
		}
		ApplyStats();
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
			Popup.Show("The cool swelling deflates as your organs dim.");
		}
		UnapplyStats();
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift(Object, "ColdResistance", 20);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts(Object);
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
		AddLight(2);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("ApplyPoison");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("EndAction");
		Registrar.Register("Overdose");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyPoison")
		{
			return false;
		}
		if (E.ID == "EndAction")
		{
			if (Object.HasStat("Hitpoints") && Object.HasStat("Level") && HealRounds > 0)
			{
				HealAmount += (float)Math.Max(5.0, Math.Ceiling(0.6 * (double)(float)Object.BaseStat("Level")));
				if (HealAmount >= 1f)
				{
					Object.Heal((int)Math.Floor(HealAmount), Message: true, FloatText: true, RandomMinimum: true);
					HealAmount -= (float)Math.Floor(HealAmount);
				}
				HealRounds--;
			}
		}
		else if (E.ID == "Overdose")
		{
			if (Duration > 0)
			{
				Duration = 0;
				ApplyOverdose(Object);
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

	public override void ApplyAllergy(GameObject Object)
	{
		ApplyOverdose(Object);
	}

	public static void ApplyOverdose(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			if (Object.GetLongProperty("Overdosing", 0L) == 1)
			{
				Popup.Show("Your mutant physiology reacts adversely to the tonic. You feel awfully frigid.");
			}
			else
			{
				Popup.Show("The tonics you ingested react adversely to each other. You feel awfully frigid.");
			}
		}
		if (Object.Physics != null)
		{
			int num = Object.Physics.BrittleTemperature - 20;
			if (Object.Physics.Temperature > num)
			{
				Object.Physics.Temperature = num;
			}
		}
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (Duration > 0 && num > 40 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "@";
			switch (Stat.RandomCosmetic(1, 3))
			{
			case 1:
				E.ColorString = "&C";
				break;
			case 2:
				E.ColorString = "&B";
				break;
			case 3:
				E.ColorString = "&c";
				break;
			}
		}
		return true;
	}
}
