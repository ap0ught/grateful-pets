using System;
using System.Text;
using HistoryKit;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class Rubbergum_Tonic : ITonicEffect, ITierInitialized
{
	public const int SAVE_BONUS = 5;

	public const string SAVE_BONUS_VS = "Restraint";

	public const int SAVE_PENALTY = -5;

	public const string SAVE_PENALTY_VS = "Bleeding";

	public bool bOverdose;

	private bool Overdosed;

	public Rubbergum_Tonic()
	{
	}

	public Rubbergum_Tonic(int Duration)
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

	public override string GetDescription()
	{
		return "{{rubbergum|rubbergum}} tonic";
	}

	public override string GetStateDescription()
	{
		return "under the effects of {{rubbergum|rubbergum}} tonic";
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Compound("+100 electric resistance", '\n');
		if (Object.IsTrueKin())
		{
			stringBuilder.Compound("+50 cold resistance", '\n');
		}
		else
		{
			stringBuilder.Compound("+25 cold resistance", '\n');
		}
		stringBuilder.Compound("-1 AV", '\n');
		SavingThrows.AppendSaveBonusDescription(stringBuilder, 5, "Restraint");
		SavingThrows.AppendSaveBonusDescription(stringBuilder, -5, "Bleeding");
		if (Object.IsTrueKin())
		{
			stringBuilder.Compound("Only suffers 25% damage from bludgeoning attacks.", '\n').Compound("Only suffers 25% damage from falling.", '\n');
		}
		else
		{
			stringBuilder.Compound("Only suffers 50% damage from bludgeoning attacks.", '\n').Compound("Only suffers 50% damage from falling.", '\n');
		}
		stringBuilder.Compound("Can't be grabbed.", '\n');
		return stringBuilder.ToString();
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool Apply(GameObject Object)
	{
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_rubbergumTonic");
		if (Object.IsPlayer())
		{
			Popup.Show("Your skin shrivels and dimples.");
		}
		ApplyChanges();
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
			Popup.Show("Your skin flattens out and stretches tautly around your body once again.");
		}
		UnapplyChanges();
		base.Remove(Object);
	}

	private void ApplyChanges()
	{
		StatShifter.SetStatShift(Object, "AV", -1);
		StatShifter.SetStatShift(Object, "ElectricResistance", 100);
		StatShifter.SetStatShift(Object, "ColdResistance", Object.IsTrueKin() ? 50 : 25);
	}

	private void UnapplyChanges()
	{
		StatShifter.RemoveStatShifts(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ModifyDefendingSaveEvent.ID)
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (SavingThrows.Applicable("Restraint", E))
		{
			E.Roll += 5;
		}
		if (SavingThrows.Applicable("Bleeding", E))
		{
			E.Roll += -5;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (Overdosed && !Object.IsFlying && 35.in100())
		{
			Object.ApplyEffect(new Prone());
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeApplyDamage");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("BeforeGrabbed");
		Registrar.Register("Overdose");
		base.Register(Object, Registrar);
	}

	public static bool AffectsDamage(Damage Dmg)
	{
		if (Dmg == null)
		{
			return false;
		}
		if (!Dmg.HasAttribute("Crushing") && !Dmg.HasAttribute("Falling") && !Dmg.HasAttribute("Cudgel"))
		{
			return Dmg.HasAttribute("Concussion");
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeGrabbed")
		{
			return false;
		}
		if (E.ID == "BeforeApplyDamage" || E.ID == "TonicAutoApplied")
		{
			Damage damage = E.GetParameter("Damage") as Damage;
			if (AffectsDamage(damage))
			{
				if (Object.IsTrueKin())
				{
					damage.Amount /= 4;
				}
				else
				{
					damage.Amount /= 2;
				}
			}
		}
		if (E.ID == "Overdose")
		{
			if (!Overdosed)
			{
				if (Object.IsPlayer())
				{
					if (Object.GetLongProperty("Overdosing", 0L) == 1)
					{
						Popup.Show("Your mutant physiology reacts adversely to the tonic. Your skin starts to knot and misshape.");
					}
					else
					{
						Popup.Show("The tonics you ingested react adversely to each other. Your skin starts to knot and misshape.");
					}
				}
				Overdosed = true;
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyChanges();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyChanges();
		}
		return base.FireEvent(E);
	}

	public override void ApplyAllergy(GameObject Object)
	{
		if (Object.GetLongProperty("Overdosing", 0L) == 1)
		{
			Popup.Show("Your mutant physiology reacts adversely to the tonic. Your skin starts to knot and misshape.");
		}
		else
		{
			Popup.Show("The tonics you ingested react adversely to each other. Your skin starts to knot and misshape.");
		}
		Object.ApplyEffect(new Rubbergum_Tonic_Allergy(Duration));
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (Duration > 0 && num > 50 && num < 60)
		{
			E.Tile = null;
			E.RenderString = "O";
			E.ColorString = "&r";
		}
		return true;
	}
}
