using System;
using Genkit;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class InteriorPortal : IPart
{
	public bool Open;

	public bool OpenOnEnteredCell;

	public bool ExitOnEnteredCell;

	public string ClosedDisplay;

	public string OpenDisplay;

	public string ClosedTile;

	public string OpenTile;

	[NonSerialized]
	private int CloseTimer;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != InventoryActionEvent.ID && ID != ObjectEnteredCellEvent.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<GetTransitiveLocationEvent>.ID && ID != EnteredCellEvent.ID)
		{
			return ID == LeftCellEvent.ID;
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
		if (E.Actor.IsPlayer())
		{
			TryExit(E.Actor);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.CurrentCell != null)
		{
			E.AddAction("Exit", "exit", "ExitInterior", null, 'e', FireOnActor: false, 15);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ExitInterior")
		{
			if (TryExit(E.Actor))
			{
				E.RequestInterfaceExit();
			}
			else
			{
				Popup.Show("You can't exit here.");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTransitiveLocationEvent E)
	{
		if (E.Zone == ParentObject.CurrentZone && E.IsIngress)
		{
			Cell placementCell = GetPlacementCell(E.Actor);
			int priority = 500;
			string text = E.Source?.GetTagOrStringProperty("PortalKey");
			if (!text.IsNullOrEmpty() && text == ParentObject.GetTagOrStringProperty("PortalKey"))
			{
				priority = 1000;
			}
			E.AddLocation(placementCell, ParentObject, priority);
		}
		return base.HandleEvent(E);
	}

	public Cell GetPlacementCell(GameObject For = null)
	{
		Cell cell = ParentObject.CurrentCell;
		Cell cell2 = cell;
		if ((For != null && !cell2.IsPassable(For)) || ExitOnEnteredCell)
		{
			string generalDirectionFrom = cell2.GetGeneralDirectionFrom(Location2D.Get(40, 12));
			Cell cellFromDirection = cell2.GetCellFromDirection(generalDirectionFrom);
			cell2 = (cellFromDirection.IsPassable(For) ? cellFromDirection : cell2.getClosestPassableCellForExcept(For, Event.NewCellList(cell)));
		}
		return cell2;
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Object != ParentObject)
		{
			if (!Open && OpenOnEnteredCell)
			{
				SetOpened(Value: true);
			}
			if (ExitOnEnteredCell)
			{
				TryExit(E.Object);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		E.Cell.ParentZone.AddZoneConnection("-", E.Cell.X, E.Cell.Y, "Exit", null);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		E.Cell.ParentZone.RemoveZoneConnection("-", E.Cell.X, E.Cell.Y, "Exit", null);
		return base.HandleEvent(E);
	}

	public void SetOpened(bool Value)
	{
		if (Value == Open)
		{
			return;
		}
		Render render = ParentObject.Render;
		_ = ParentObject.CurrentZone;
		if (Open)
		{
			if (OpenDisplay.IsNullOrEmpty())
			{
				OpenDisplay = render.RenderString;
			}
			if (OpenTile.IsNullOrEmpty())
			{
				OpenTile = render.Tile;
			}
			if (!ClosedDisplay.IsNullOrEmpty())
			{
				render.RenderString = ClosedDisplay;
			}
			if (!ClosedTile.IsNullOrEmpty())
			{
				render.Tile = ClosedTile;
			}
			PlayCoalescedSound(ParentObject, "CloseSound");
			Open = false;
		}
		else
		{
			if (ClosedDisplay.IsNullOrEmpty())
			{
				ClosedDisplay = render.RenderString;
			}
			if (ClosedTile.IsNullOrEmpty())
			{
				ClosedTile = render.Tile;
			}
			if (!OpenDisplay.IsNullOrEmpty())
			{
				render.RenderString = OpenDisplay;
			}
			if (!OpenTile.IsNullOrEmpty())
			{
				render.Tile = OpenTile;
			}
			PlayCoalescedSound(ParentObject, "OpenSound");
			Open = true;
		}
	}

	public void PlayCoalescedSound(GameObject Source, string Clip)
	{
		if (GameObject.Validate(Source))
		{
			string text = ParentObject.GetTagOrStringProperty(Clip);
			if (text.IsNullOrEmpty())
			{
				text = (ParentObject.CurrentZone as InteriorZone)?.ParentObject?.GetTagOrStringProperty(Clip);
			}
			if (!text.IsNullOrEmpty())
			{
				Source.PlayWorldSound(text);
			}
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ClimbDown");
		Registrar.Register("PortalTransition");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ClimbDown" && E.GetParameter("GO") is GameObject actor)
		{
			return !TryExit(actor);
		}
		if (E.ID == "PortalTransition" && !Open)
		{
			SetOpened(Value: true);
		}
		return base.FireEvent(E);
	}

	[Obsolete("Use TryExit")]
	public bool Exit(GameObject Actor)
	{
		return TryExit(Actor);
	}

	public bool TryExit(GameObject Actor)
	{
		if (ParentObject.CurrentZone is InteriorZone interiorZone)
		{
			Zone zone = interiorZone.ResolveParentZone();
			GetTransitiveLocationEvent.GetFor(zone, "Egress,Interior", Actor, ParentObject, ParentObject.CurrentCell, out var Destination, out var Portal);
			if (Destination == null)
			{
				Destination = zone?.GetPullDownLocation(Actor) ?? interiorZone.GetEscapeCell(Actor);
			}
			if (Destination == null)
			{
				return false;
			}
			if (Destination.OnWorldMap())
			{
				return Actor.ShowFailure("You cannot do that while traveling on the world map.");
			}
			PlayCoalescedSound(Portal, "CloseSound");
			if (Actor.IsPlayer() && !Open)
			{
				PlayCoalescedSound(ParentObject, "OpenSound");
			}
			else
			{
				SetOpened(Value: true);
			}
			FlushWeightCacheEvent.Send(interiorZone.ParentObject);
			if (Actor.DirectMoveTo(Destination))
			{
				PlayCoalescedSound(Portal, "ExitSound");
				Portal?.FireEvent("PortalTransition");
				return true;
			}
		}
		return false;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (Open && CloseTimer++ >= 3)
		{
			if (!ParentObject.CurrentCell.HasCombatObject())
			{
				SetOpened(Value: false);
			}
			CloseTimer = 0;
		}
		base.TurnTick(TimeTick, Amount);
	}
}
