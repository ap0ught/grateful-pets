using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class InteriorContainer : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer() && FireEvent(Event.New("Open", "Opener", E.Actor)))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Open", "open", "Open", null, 'o');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Open")
		{
			ParentObject.FireEvent(Event.New("Open", "Opener", E.Actor), E);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Open");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Open" && E.GetGameObjectParameter("Opener") == The.Player && ParentObject.CurrentZone is InteriorZone interiorZone)
		{
			TradeUI.ShowTradeScreen(interiorZone.ParentObject, 0f);
			The.Player.UseEnergy(1000, "Companion Trade");
			E.RequestInterfaceExit();
		}
		return base.FireEvent(E);
	}
}
