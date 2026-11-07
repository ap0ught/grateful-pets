using System;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class MaintenanceSystems : IPoweredPart
{
	public int RegenCounter;

	public int HitCounter;

	[NonSerialized]
	private static Event eRegenerating = new Event("Regenerating", "Amount", 0);

	public MaintenanceSystems()
	{
		ChargeUse = 0;
		IsRustSensitive = false;
		IsPowerSwitchSensitive = false;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != PooledEvent<HealsNaturallyEvent>.ID)
		{
			return ID == TookDamageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(HealsNaturallyEvent E)
	{
		if (E.Object == ParentObject)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		ProcessDamageControl();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (E.Object == ParentObject)
		{
			HitCounter = 10;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool ProcessDamageControl(int Turns = 1)
	{
		if (HitCounter > 0)
		{
			HitCounter -= Turns;
		}
		if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		bool flag = ParentObject.HasPart<Regeneration>();
		if (HitCounter > 0 && !flag)
		{
			return false;
		}
		int value = (20 + 2 * ParentObject.StatMod("Toughness") + 2 * ParentObject.StatMod("Willpower")) * Turns;
		eRegenerating.ID = "Regenerating";
		eRegenerating.SetParameter("Amount", value);
		ParentObject.FireEvent(eRegenerating);
		eRegenerating.ID = "Regenerating2";
		ParentObject.FireEvent(eRegenerating);
		value = eRegenerating.GetIntParameter("Amount");
		if (value < 0)
		{
			value = 0;
		}
		if (HitCounter > 0)
		{
			value /= 2;
		}
		if (ParentObject.HasEffect<Meditating>())
		{
			value *= 3;
		}
		if (ParentObject.HasPart<LuminousInfection>() && IsDay() && ParentObject.CurrentZone != null && ParentObject.CurrentZone.Z <= 10)
		{
			value = value * 85 / 100;
		}
		RegenCounter += value;
		if (RegenCounter > 100)
		{
			int num = (int)Math.Floor((double)RegenCounter / 100.0);
			RegenCounter %= 100;
			ParentObject.GetStat("Hitpoints").Penalty -= num;
			if (ParentObject.IsPlayer())
			{
				Sidebar.Update();
			}
		}
		return true;
	}
}
