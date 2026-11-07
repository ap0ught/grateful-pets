using Genkit;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class FungalTrailKlanqHut : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		int num = Stat.Random(20, 50);
		int num2 = Stat.Random(10, 15);
		foreach (ZoneConnection zoneConnection in The.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (!(zoneConnection.Type != "FungalTrailStart"))
			{
				num = zoneConnection.X;
				num2 = zoneConnection.Y;
				break;
			}
		}
		Rect2D r = new Rect2D(num - 3, num2 - 4, num + 3, num2 + 4);
		ZoneBuilderSandbox.PlaceObjectInRect(Z, r.ReduceBy(1, 1), "PaxKlanq");
		PlaceHut(Z, r, "FungalTrailBrick", "GodshroomWall", "PaxKlanqHut", Round: true);
		return true;
	}
}
