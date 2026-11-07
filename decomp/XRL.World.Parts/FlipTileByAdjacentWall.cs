using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class FlipTileByAdjacentWall : IPart
{
	public string WallCheckDirection = "W";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ZoneBuiltEvent.ID)
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public void checkTile()
	{
		if (ParentObject.Render.Tile == null)
		{
			return;
		}
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null)
		{
			return;
		}
		Cell cellFromDirection = cell.GetCellFromDirection(WallCheckDirection);
		Cell cellFromDirection2 = cell.GetCellFromDirection(Directions.GetOppositeDirection(WallCheckDirection));
		if (cellFromDirection == null || cellFromDirection2 == null)
		{
			return;
		}
		bool partParameter = ParentObject.GetBlueprint().GetPartParameter("Render", "HFlip", Default: false);
		if (cellFromDirection.HasWall())
		{
			if (!cellFromDirection2.HasWall())
			{
				ParentObject.Render.HFlip = !partParameter;
			}
		}
		else if (cellFromDirection2.HasWall())
		{
			ParentObject.Render.HFlip = partParameter;
		}
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		checkTile();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		checkTile();
		return base.HandleEvent(E);
	}
}
