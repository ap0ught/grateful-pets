namespace XRL.World.ZoneBuilders;

public class Connecter
{
	public bool BuildZone(Zone Z)
	{
		foreach (ZoneConnection zoneConnection in The.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (zoneConnection.Type == "StairsUp" || zoneConnection.Type == "StairsDown")
			{
				Z.GetCell(zoneConnection.X, zoneConnection.Y).ClearWalls();
				Z.GetCell(zoneConnection.X, zoneConnection.Y).ClearImpassableObjects(The.Player);
				Z.BuildReachableMap(zoneConnection.X, zoneConnection.Y);
				break;
			}
		}
		return true;
	}
}
