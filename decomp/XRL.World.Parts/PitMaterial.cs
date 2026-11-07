using System;
using System.Linq;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class PitMaterial : IPart
{
	public bool Lazy;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ZoneBuiltEvent.ID && ID != ZoneActivatedEvent.ID)
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("PaintPitEdges");
		Registrar.Register("FirmPitEdges");
	}

	private bool isPitAdjacent(Cell ce)
	{
		if (ce != null && !ce.HasObject("Pit") && !ce.HasObject("LazyPit"))
		{
			return ce.HasObjectWithTag("Pit");
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (Lazy)
		{
			Lazy = false;
			FireEvent(Event.New("FirmPitEdges"));
			PaintPit();
		}
		return true;
	}

	private void PaintPit()
	{
		ParentObject.RemovePart<StairHighlight>();
		if (Lazy && ParentObject.CurrentZone != null && !ParentObject.CurrentZone.IsActive())
		{
			return;
		}
		if (ParentObject.Physics.CurrentCell.AllInDirections(Directions.DirectionList, 1, isPitAdjacent))
		{
			Cell cellFromDirection = ParentObject.Physics.CurrentCell.GetCellFromDirection("D", BuiltOnly: false);
			if (cellFromDirection != null)
			{
				RenderEvent renderEvent = cellFromDirection.Render();
				ParentObject.Render.RenderString = renderEvent.RenderString;
				if (cellFromDirection.HasObjectWithTag("PitDetailColor"))
				{
					ParentObject.Render.DetailColor = cellFromDirection.GetObjectsWithTag("PitDetailColor").FirstOrDefault().GetTag("PitDetailColor");
				}
				else
				{
					ParentObject.Render.DetailColor = "k";
				}
				ParentObject.Render.Tile = renderEvent.Tile;
			}
			else
			{
				ParentObject.Render.Tile = null;
				ParentObject.Render.RenderString = " ";
			}
			if (ParentObject.TryGetPart<StairsDown>(out var Part))
			{
				Part.PullDown = true;
			}
			ParentObject.Render.TileColor = "&K";
			ParentObject.Render.ColorString = "&K";
			ParentObject.Render.DetailColor = "k";
			ParentObject.RemoveStringProperty("PaintedWall");
			ParentObject.RemoveStringProperty("PaintWith");
			ParentObject.SetStringProperty("PaintWith", "PitVoid");
			ParentObject.Render.DisplayName = "open air";
			ParentObject.RemoveStringProperty("OverrideIArticle");
			ParentObject.Render.RenderLayer = 7;
		}
		else
		{
			if (ParentObject.TryGetPart<StairsDown>(out var Part2))
			{
				Part2.PullDown = false;
			}
			ParentObject.SetStringProperty("PaintedWall", "tile_pit_");
			ParentObject.SetStringProperty("PaintWith", "!PitVoid");
			ParentObject.Render.DisplayName = "craggy ledge";
			ParentObject.SetStringProperty("OverrideIArticle", "a");
			ParentObject.Render.RenderString = "ú";
			string zoneProperty = ParentObject.Physics.CurrentCell.ParentZone.GetZoneProperty("ChasmColor");
			if (zoneProperty.IsNullOrEmpty())
			{
				zoneProperty = ParentObject.Physics.CurrentCell.ParentZone.GetZoneProperty("primaryFloorColor1");
			}
			if (!zoneProperty.IsNullOrEmpty())
			{
				ParentObject.Render.ColorString = zoneProperty + "^k";
				ParentObject.Render.TileColor = zoneProperty + "^k";
			}
			ParentObject.GetPart<Description>()._Short = "Ground material splinters and opens onto a void.";
			ParentObject.Render.RenderLayer = 1;
		}
		if (ParentObject.CurrentCell.HasObjectWithTagOrProperty("SuspendedPlatform"))
		{
			ParentObject.Render.RenderLayer = 1;
		}
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		if (!Lazy)
		{
			FireEvent(Event.New("FirmPitEdges"));
			PaintPit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		PaintPit();
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "FirmPitEdges")
		{
			if (!Lazy && ParentObject.Physics.CurrentCell.AllInDirections(Directions.DirectionList, 1, isPitAdjacent))
			{
				if (ParentObject.TryGetPart<StairsDown>(out var Part))
				{
					Part.PullDown = true;
				}
				ParentObject.Render.DisplayName = "open air";
				ParentObject.RemoveStringProperty("OverrideIArticle");
				ParentObject.Render.RenderLayer = 7;
			}
			else
			{
				if (ParentObject.TryGetPart<StairsDown>(out var Part2))
				{
					Part2.PullDown = false;
				}
				ParentObject.Render.DisplayName = "craggy ledge";
				ParentObject.SetStringProperty("OverrideIArticle", "a");
				ParentObject.Render.RenderString = "ú";
				ParentObject.GetPart<Description>()._Short = "Ground material splinters and opens onto a void.";
				ParentObject.Render.RenderLayer = 0;
			}
			if (ParentObject.CurrentCell.HasObjectWithTagOrProperty("SuspendedPlatform"))
			{
				ParentObject.Render.RenderLayer = 0;
			}
		}
		else if (E.ID == "PaintPitEdges")
		{
			PaintPit();
		}
		return true;
	}
}
