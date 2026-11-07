using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class RadiusEventSender : IPoweredPart
{
	public string Event;

	public int Radius = 1;

	public bool RealRadius;

	[NonSerialized]
	private List<Cell> Cells;

	public RadiusEventSender()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		RadiusEventSender radiusEventSender = p as RadiusEventSender;
		if (radiusEventSender.Event != Event)
		{
			return false;
		}
		if (radiusEventSender.Radius != Radius)
		{
			return false;
		}
		if (radiusEventSender.RealRadius != RealRadius)
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
		Registrar.Register("EndTurn");
		Registrar.Register("EnteredCell");
		Registrar.Register("Equipped");
		Registrar.Register("Unequipped");
		base.Register(Object, Registrar);
	}

	public void SendEvent()
	{
		if (string.IsNullOrEmpty(Event) || !IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null || cell.ParentZone == null)
		{
			return;
		}
		if (Cells == null)
		{
			Cells = cell.ParentZone.FastFloodNeighbors(cell.X, cell.Y, Radius);
		}
		foreach (Cell cell2 in Cells)
		{
			if (!RealRadius || !(cell.RealDistanceTo(cell2) > (double)Radius))
			{
				cell2.FireEvent(Event);
			}
		}
		if (ParentObject.InInventory != null)
		{
			Cells = null;
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			SendEvent();
		}
		else if (E.ID == "EnteredCell")
		{
			Cells = null;
		}
		else if (E.ID == "Equipped")
		{
			E.GetGameObjectParameter("EquippingObject").RegisterPartEvent(this, "EnteredCell");
		}
		else if (E.ID == "Unequipped")
		{
			E.GetGameObjectParameter("UnequippingObject").UnregisterPartEvent(this, "EnteredCell");
		}
		return base.FireEvent(E);
	}
}
