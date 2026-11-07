using System;

namespace XRL.World.Parts;

[Serializable]
public class DrinkMagnifier : IActivePart
{
	public int Chance;

	public int Percent = 200;

	public DrinkMagnifier()
	{
		WorksOnHolder = true;
		WorksOnCarrier = true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID && ID != EquippedEvent.ID && ID != UnequippedEvent.ID && ID != DroppedEvent.ID)
		{
			return ID == TakenEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Chance > 0)
		{
			E.Postfix.Compound("{{rules|", '\n').Append(Chance).Append('%')
				.Append(" chance for liquid-drinking effects to apply twice}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DroppedEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public void EvaluateAsSubject(GameObject Object)
	{
		if (IsObjectActivePartSubject(Object))
		{
			Object.RegisterPartEvent(this, "ModifyDrinkEffects");
		}
		else
		{
			Object.UnregisterPartEvent(this, "ModifyDrinkEffects");
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ModifyDrinkEffects" && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && Chance.in100())
		{
			int intParameter = E.GetIntParameter("Magnitude");
			E.SetParameter("Magnitude", intParameter * Percent / 100);
		}
		return base.FireEvent(E);
	}

	public static int Magnify(GameObject Target, int Magnitude = 100)
	{
		if (Target.HasRegisteredEvent("ModifyDrinkEffects"))
		{
			Event obj = Event.New("ModifyDrinkEffects", "Magnitude", Magnitude);
			Target.FireEvent(obj);
			Magnitude = obj.GetIntParameter("Magnitude");
		}
		return Magnitude;
	}
}
