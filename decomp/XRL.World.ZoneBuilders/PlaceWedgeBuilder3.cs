using System.Collections.Generic;

namespace XRL.World.ZoneBuilders;

public class PlaceWedgeBuilder3
{
	public bool BuildZone(Zone Z)
	{
		List<Cell> emptyReachableCells = Z.GetEmptyReachableCells();
		if (emptyReachableCells.Count == 0)
		{
			return false;
		}
		emptyReachableCells.GetRandomElement().AddObject("WedgeChest3");
		return true;
	}
}
