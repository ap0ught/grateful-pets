using System.Collections.Generic;

namespace XRL.World.ZoneFactories;

public class TzimtzlumWorldZoneFactory : IZoneFactory
{
	public override Zone BuildZone(ZoneRequest Request)
	{
		if (Request.IsWorldZone)
		{
			Zone zone = new Zone(80, 25);
			zone.GetCells().ForEach((Cell c) =>
			{
				c.AddObject("TerrainTzimtzlum");
			});
			return zone;
		}
		ClamSystem clamSystem = The.Game.RequireSystem(() => new ClamSystem());
		if (Request.ZoneID != clamSystem.ClamWorldId)
		{
			return clamSystem.GetClamZone();
		}
		Zone zone2 = new Zone(80, 25);
		zone2.ZoneID = clamSystem.ClamWorldId;
		zone2.loadMap("Tzimtzlum.rpm");
		zone2.DisplayName = "Tzimtzlum";
		zone2.IncludeContextInZoneDisplay = false;
		zone2.IncludeStratumInZoneDisplay = false;
		zone2.SetMusic("Music/Clam Dimension");
		zone2.Built = true;
		List<GameObject> objects = zone2.GetObjects("Giant Clam");
		if (objects.Count == 0)
		{
			MetricsManager.LogError("Tzimtzlum generated without clams");
		}
		for (int num = 0; num < objects.Count; num++)
		{
			objects[num].SetIntProperty("ClamId", num);
		}
		The.Game.ZoneManager.SetZoneProperty(Request.ZoneID, "SpecialUpMessage", "You’re in a pocket dimension with no worldmap.");
		return zone2;
	}

	public override void AfterBuildZone(Zone zone, ZoneManager zoneManager)
	{
		zone.SetInside(true);
		ZoneManager.PaintWalls(zone);
		ZoneManager.PaintWater(zone);
	}
}
