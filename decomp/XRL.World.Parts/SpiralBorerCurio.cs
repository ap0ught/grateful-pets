using System;
using Genkit;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class SpiralBorerCurio : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Activate", "activate", "ActivateSpiralBorer", null, 'a', FireOnActor: false, 10);
		return base.HandleEvent(E);
	}

	public bool PlaceStair(Cell c, string stairObject)
	{
		if (c == null)
		{
			return false;
		}
		if (c.HasObjectWithTag("Pit"))
		{
			return false;
		}
		c.ClearWalls();
		c.RequireObject(stairObject);
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateSpiralBorer")
		{
			if (E.Actor.OnWorldMap())
			{
				E.Actor.Fail("You cannot do that on the world map.");
				return false;
			}
			if (E.Actor.CurrentZone is InteriorZone)
			{
				E.Actor.Fail("You cannot do that inside.");
				return false;
			}
			if (!GenericQueryEvent.Check(ParentObject, Subject: E.Actor, Query: E.Command, Source: null, Level: 0, BaseResult: true))
			{
				return false;
			}
			if (!E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			Point2D pos2D = E.Actor.CurrentCell.Pos2D;
			Point2D p = ((pos2D.x <= 0) ? E.Actor.CurrentCell.Pos2D.FromDirection("E") : E.Actor.CurrentCell.Pos2D.FromDirection("W"));
			IComponent<GameObject>.PlayUISound("Sounds/Interact/sfx_interact_drill_deep");
			Popup.Show("The metal satchel opens, folds itself inside out, and transforms into a contraption studded with pinions and drills. It starts to burrow into the ground.");
			int i = 0;
			for (int num = 20; i <= num; i++)
			{
				string zoneIDFromDirection = E.Actor.CurrentZone.GetZoneIDFromDirection("D", i);
				if (The.ZoneManager.IsZoneBuilt(zoneIDFromDirection))
				{
					Zone zone = The.ZoneManager.GetZone(zoneIDFromDirection);
					if (i == 0)
					{
						PlaceStair(zone.GetCell(pos2D), "StairsDownUnconnected");
					}
					else if (i == num)
					{
						PlaceStair(zone.GetCell(p), "StairsUpUnconnected");
					}
					else if (i % 2 == 0)
					{
						PlaceStair(zone.GetCell(pos2D), "StairsDownUnconnected");
						PlaceStair(zone.GetCell(p), "StairsUpUnconnected");
					}
					else
					{
						PlaceStair(zone.GetCell(pos2D), "StairsUpUnconnected");
						PlaceStair(zone.GetCell(p), "StairsDownUnconnected");
					}
				}
				else
				{
					ZoneManager zoneManager = XRLCore.Core.Game.ZoneManager;
					if (i == 0)
					{
						zoneManager.AddZonePostBuilder(zoneIDFromDirection, "ClearWallAddObject", "x", pos2D.x.ToString(), "y", pos2D.y.ToString(), "obj", "StairsDownUnconnected");
					}
					else if (i == num)
					{
						zoneManager.AddZonePostBuilder(zoneIDFromDirection, "ClearWallAddObject", "x", p.x.ToString(), "y", p.y.ToString(), "obj", "StairsUpUnconnected");
					}
					else if (i % 2 == 0)
					{
						zoneManager.AddZonePostBuilder(zoneIDFromDirection, "ClearWallAddObject", "x", pos2D.x.ToString(), "y", pos2D.y.ToString(), "obj", "StairsDownUnconnected");
						zoneManager.AddZonePostBuilder(zoneIDFromDirection, "ClearWallAddObject", "x", p.x.ToString(), "y", p.y.ToString(), "obj", "StairsUpUnconnected");
					}
					else
					{
						zoneManager.AddZonePostBuilder(zoneIDFromDirection, "ClearWallAddObject", "x", pos2D.x.ToString(), "y", pos2D.y.ToString(), "obj", "StairsUpUnconnected");
						zoneManager.AddZonePostBuilder(zoneIDFromDirection, "ClearWallAddObject", "x", p.x.ToString(), "y", p.y.ToString(), "obj", "StairsDownUnconnected");
					}
				}
			}
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}
}
