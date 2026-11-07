using System;

namespace XRL.World.Parts;

[Serializable]
public class HUD : IPoweredPart
{
	public string EventHandled = "HandleSmartData";

	public HUD()
	{
		WorksOnWearer = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as HUD).EventHandled != EventHandled)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Equipped");
		Registrar.Register("Unequipped");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			ConsumeChargeIfOperational();
		}
		else if (E.ID == EventHandled)
		{
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return false;
			}
		}
		else if (E.ID == "Equipped")
		{
			E.GetGameObjectParameter("EquippingObject").RegisterPartEvent(this, EventHandled);
		}
		else if (E.ID == "Unequipped")
		{
			E.GetGameObjectParameter("UnequippingObject").UnregisterPartEvent(this, EventHandled);
		}
		return base.FireEvent(E);
	}
}
