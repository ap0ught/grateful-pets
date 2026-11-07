using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CursedCellSocket : IPart
{
	public string AllowedBlueprint = "*";

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeRemoveCell");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeRemoveCell")
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CellChangedEvent.ID && ID != PooledEvent<CellDepletedEvent>.ID)
		{
			return ID == CanBeSlottedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		if (E.NewCell != null)
		{
			EnergyCell part = E.NewCell.GetPart<EnergyCell>();
			if (part != null)
			{
				part.IsRechargeable = false;
			}
			LiquidVolume liquidVolume = E.NewCell.LiquidVolume;
			if (liquidVolume != null)
			{
				liquidVolume.Sealed = true;
				liquidVolume.ManualSeal = false;
				liquidVolume.Collector = false;
				liquidVolume.AutoCollectLiquidType = null;
			}
			if (E.Actor.IsPlayer())
			{
				Popup.Show(E.NewCell.Does("lock") + " firmly into the socket, preventing removal.", null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellDepletedEvent E)
	{
		if (E.Cell.SlottedIn == ParentObject)
		{
			EmitMessage(E.Object.T() + " burns into bright slag.");
			E.Object.RemoveFromContext(E);
			E.Object.Destroy();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanBeSlottedEvent E)
	{
		if (E.Cell != null && E.Actor == ParentObject && AllowedBlueprint != null && AllowedBlueprint != "*")
		{
			if (!E.Cell.HasAnyCharge())
			{
				return false;
			}
			if (!AllowedBlueprint.CachedCommaExpansion().Contains(E.Item.Blueprint))
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}
}
