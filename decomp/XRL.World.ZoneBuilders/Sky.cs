namespace XRL.World.ZoneBuilders;

public class Sky
{
	public bool BuildZone(Zone Z)
	{
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		foreach (Cell cell in Z.GetCells())
		{
			cell.AddObject("Air");
		}
		Z.ClearReachableMap();
		if (Z.BuildReachableMap(0, 0, bClearFirst: false) < 400)
		{
			return false;
		}
		return true;
	}
}
