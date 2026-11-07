using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRL.World.ZoneFactories;

public class BlueprintZoneFactory : IZoneFactory
{
	public override bool CanBuildZone(ZoneRequest Request)
	{
		if (!Request.IsWorldZone)
		{
			return Request.ZoneID.LastIndexOf("-", StringComparison.Ordinal) > Request.ZoneID.LastIndexOf(".", StringComparison.Ordinal);
		}
		return false;
	}

	public override void AddBlueprintsFor(ZoneRequest Request)
	{
		GameObject terrainObjectForZone = ZoneManager.GetTerrainObjectForZone(Request.WorldX, Request.WorldY, Request.WorldID);
		Dictionary<string, CellBlueprint> cellBlueprintsByApplication = Request.World.CellBlueprintsByApplication;
		int num = Mathf.Clamp(Request.Z, 0, 49);
		object obj;
		if (cellBlueprintsByApplication == null)
		{
			obj = null;
		}
		else
		{
			CellBlueprint value = cellBlueprintsByApplication.GetValue(terrainObjectForZone?.Blueprint);
			obj = ((value != null) ? value.LevelBlueprint[Request.X, Request.Y, num] : null);
		}
		if (obj == null)
		{
			if (cellBlueprintsByApplication == null)
			{
				obj = null;
			}
			else
			{
				CellBlueprint value2 = cellBlueprintsByApplication.GetValue("*Default");
				obj = ((value2 != null) ? value2.LevelBlueprint[Request.X, Request.Y, num] : null);
			}
		}
		ZoneBlueprint zoneBlueprint = (ZoneBlueprint)obj;
		if (zoneBlueprint != null)
		{
			Request.Blueprints.Add(zoneBlueprint);
		}
		string key = Request.WorldX + "." + Request.WorldY;
		object obj2;
		if (cellBlueprintsByApplication == null)
		{
			obj2 = null;
		}
		else
		{
			CellBlueprint value3 = cellBlueprintsByApplication.GetValue(key);
			obj2 = ((value3 != null) ? value3.LevelBlueprint[Request.X, Request.Y, num] : null);
		}
		zoneBlueprint = (ZoneBlueprint)obj2;
		if (zoneBlueprint != null)
		{
			Request.Blueprints.Add(zoneBlueprint);
		}
	}

	public override Zone BuildZone(ZoneRequest Request)
	{
		return new Zone();
	}

	public override void AfterBuildZone(Zone zone, ZoneManager zoneManager)
	{
		ZoneManager.PaintWalls(zone);
		ZoneManager.PaintWater(zone);
	}
}
