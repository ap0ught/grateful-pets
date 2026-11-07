using System;
using System.Linq;

namespace XRL.World.Parts;

[Serializable]
public class DoubleEnclosing : IPart
{
	public string Direction;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("SyncOpened");
		Registrar.Register("SyncClosed");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "SyncOpened")
		{
			GameObject gameObject = ParentObject.CurrentCell.GetCellFromDirection(Direction).GetObjectsWithPart("Enclosing").First();
			Enclosing part = gameObject.GetPart<Enclosing>();
			if (part.OpenColor != null)
			{
				gameObject.Render.ColorString = part.OpenColor;
			}
			if (part.OpenTileColor != null)
			{
				gameObject.Render.TileColor = part.OpenTileColor;
			}
			if (part.OpenRenderString != null)
			{
				gameObject.Render.RenderString = part.OpenRenderString;
			}
			if (part.OpenTile != null)
			{
				gameObject.Render.Tile = part.OpenTile;
			}
			if (part.OpenLayer != int.MinValue)
			{
				gameObject.Render.RenderLayer = part.OpenLayer;
			}
			return true;
		}
		if (E.ID == "SyncClosed")
		{
			GameObject gameObject2 = ParentObject.CurrentCell.GetCellFromDirection(Direction).GetObjectsWithPart("Enclosing").First();
			Enclosing part2 = gameObject2.GetPart<Enclosing>();
			if (part2.ClosedColor != null)
			{
				gameObject2.Render.ColorString = part2.ClosedColor;
			}
			if (part2.ClosedTile != null)
			{
				gameObject2.Render.TileColor = part2.ClosedTile;
			}
			if (part2.ClosedRenderString != null)
			{
				gameObject2.Render.RenderString = part2.ClosedRenderString;
			}
			if (part2.ClosedTile != null)
			{
				gameObject2.Render.Tile = part2.ClosedTile;
			}
			if (part2.ClosedLayer != int.MinValue)
			{
				gameObject2.Render.RenderLayer = part2.ClosedLayer;
			}
			return true;
		}
		return base.FireEvent(E);
	}
}
