namespace XRL.World.ZoneBuilders;

public class ClearWallAddObject
{
	public string x = "-1";

	public string y = "-1";

	public string obj;

	public bool BuildZone(Zone Z)
	{
		Cell cell = Z.GetCell(x.RollCached(), y.RollCached());
		cell.ClearWalls();
		cell.AddObject(obj);
		return true;
	}
}
