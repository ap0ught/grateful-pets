using System;
using HistoryKit;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Blaze_Tonic : ITonicEffect, ITierInitialized
{
	public int x;

	public int y;

	public string zone;

	public bool bOverdose;

	public Blaze_Tonic()
	{
	}

	public Blaze_Tonic(int Duration)
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
		Duration = Stat.Roll(41, 50);
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		if (Object.IsTrueKin())
		{
			return "+100 Heat Resistance\nCan't be frozen.\n+25% heat damage dealt.\n-50 Cold Resistance\n+20 Quickness\nTemperature can't be increased by external heat sources.\nMust move around to avoid combusting and losing the effects of this tonic.";
		}
		return "+100 Heat Resistance.\nCan't be frozen.\n+25% heat damage dealt.\n-50 Cold Resistance\n+10 Quickness\nTemperature can't be increased by external heat sources.\nMust move around to avoid combusting and losing the effects of this tonic.";
	}

	public override string GetDescription()
	{
		return "{{blaze|blaze}} tonic";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("Your stomach swells with a burning sensation.");
		}
		if (Object.Physics.Temperature <= Object.Physics.FreezeTemperature)
		{
			Object.Physics.Temperature = Object.Physics.FreezeTemperature + 1;
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
		if (Object != null && Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("The " + GetDescription() + " burns out of your system.");
		}
		UnapplyStats();
		base.Remove(Object);
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift("Speed", Object.IsTrueKin() ? 20 : 10);
		StatShifter.SetStatShift("ColdResistance", -50);
		StatShifter.SetStatShift("HeatResistance", 100);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID)
		{
			return ID == SingletonEvent<EndActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == Object && E.Damage.IsHeatDamage())
		{
			NotifyTargetImmuneEvent.Send(E.Weapon, E.Object, E.Actor, E.Damage, this);
			E.Damage.Amount = 0;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Object.Physics.Temperature <= Object.Physics.FreezeTemperature)
		{
			Object.Physics.Temperature = Object.Physics.FreezeTemperature + 1;
		}
		if (Object.Physics.Temperature >= Object.Physics.FlameTemperature)
		{
			Object.RemoveEffect(this);
		}
		else if (Duration > 0)
		{
			if (Duration != 9999)
			{
				Duration--;
			}
			if (Duration <= 0)
			{
				if (Object.IsPlayer())
				{
					Popup.Show("You start to cool off.");
				}
				Object.RemoveEffect(this);
			}
			else if (Object.CurrentCell != null)
			{
				x = Object.CurrentCell.X;
				y = Object.CurrentCell.Y;
				zone = Object.CurrentZone?.ZoneID;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndActionEvent E)
	{
		if (Duration > 0)
		{
			AfterMovement();
		}
		return base.HandleEvent(E);
	}

	public void AfterMovement()
	{
		Cell cell = Object.CurrentCell;
		if (cell != null)
		{
			if (x == cell.X && y == cell.Y && zone == cell.ParentZone.ZoneID)
			{
				Object.Physics.Temperature += 120;
			}
			else if (Object.Physics.Temperature > 45)
			{
				Object.Physics.Temperature -= 20;
			}
		}
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("AttackerDealingDamage");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("BeforeTemperatureChange");
		Registrar.Register("Overdose");
		Registrar.Register("Juked");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Juked")
		{
			AfterMovement();
		}
		else if (E.ID == "AttackerDealingDamage")
		{
			Damage damage = E.GetParameter("Damage") as Damage;
			if (damage.HasAttribute("Heat") || damage.HasAttribute("Fire"))
			{
				damage.Amount = (int)Math.Ceiling((float)damage.Amount * 1.25f);
			}
		}
		else if (E.ID == "TonicAutoApplied")
		{
			if (E.GetParameter("Damage") is Damage damage2 && damage2.IsHeatDamage())
			{
				damage2.Amount = 0;
				return false;
			}
		}
		else if (E.ID == "BeforeTemperatureChange")
		{
			if (E.GetIntParameter("Amount") > 0)
			{
				return false;
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
				Popup.Show("Your mutant physiology reacts adversely to the tonic. You erupt into flames!");
			}
			else
			{
				Popup.Show("The tonics you ingested react adversely to each other. You erupt into flames!");
			}
		}
		if (Object.Physics != null)
		{
			int num = Object.Physics.FlameTemperature + 200;
			if (Object.Physics.Temperature < num)
			{
				Object.Physics.Temperature = num;
			}
		}
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (Duration > 0 && num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "!";
			switch (Stat.RandomCosmetic(1, 3))
			{
			case 1:
				E.ColorString = "&R";
				break;
			case 2:
				E.ColorString = "&W";
				break;
			case 3:
				E.ColorString = "&r";
				break;
			}
		}
		return true;
	}
}
