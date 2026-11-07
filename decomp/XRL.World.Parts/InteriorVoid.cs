using System;

namespace XRL.World.Parts;

[Serializable]
public class InteriorVoid : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ZoneActivatedEvent.ID && ID != ObjectEnteringCellEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		return true;
	}

	public override bool HandleEvent(ObjectEnteringCellEvent E)
	{
		if (!E.Forced)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Object != ParentObject && E.Object.IsReal && ParentObject.CurrentZone is InteriorZone interiorZone)
		{
			Cell escapeCell = interiorZone.GetEscapeCell(E.Object);
			if (escapeCell != null)
			{
				E.Object.SystemMoveTo(escapeCell);
				int fallDistance = interiorZone.ParentObject.GetPart<Interior>().FallDistance;
				if (fallDistance > 1)
				{
					StairsDown.InflictFallDamage(E.Object, fallDistance);
				}
				if (escapeCell.OnWorldMap())
				{
					E.Object.PullDown();
					interiorZone.ParentObject?.PullDown();
				}
			}
		}
		return base.HandleEvent(E);
	}

	public bool IsRenderable(GameObject Object)
	{
		if (Object.Brain != null)
		{
			return false;
		}
		return true;
	}

	public bool Paint()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		Zone zone = (cell.ParentZone as InteriorZone)?.ParentObject?.CurrentZone;
		if (zone == null)
		{
			return false;
		}
		Render render = ParentObject.Render;
		render.DetailColor = "k";
		render.ColorString = (render.TileColor = "&K");
		Cell cell2 = zone.GetCell(cell.X, cell.Y);
		GameObject highestRenderLayerObject = cell2.GetHighestRenderLayerObject(IsRenderable);
		if (highestRenderLayerObject != null)
		{
			render.Tile = highestRenderLayerObject.Render.Tile;
		}
		else
		{
			render.Tile = cell2.PaintTile;
		}
		return true;
	}
}
