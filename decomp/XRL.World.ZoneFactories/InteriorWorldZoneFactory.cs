using UnityEngine;

namespace XRL.World.ZoneFactories;

public class InteriorWorldZoneFactory : IZoneFactory
{
	public override bool CanBuildZone(ZoneRequest Request)
	{
		return Request.IsWorldZone;
	}

	public override Zone GenerateZone(ZoneRequest Request, int Width, int Height)
	{
		if (!Request.ZoneID.IsNullOrEmpty())
		{
			The.ZoneManager.SetZoneProperty(Request.ZoneID, "Inside", "1");
		}
		if (!Request.IsWorldZone)
		{
			return new InteriorZone(Width, Height);
		}
		return new Zone(Width, Height);
	}

	public override Zone BuildZone(ZoneRequest Request)
	{
		if (Request.IsWorldZone)
		{
			Zone zone = new Zone(80, 25);
			zone.ForeachCell((Cell c) =>
			{
				c.AddObject("TerrainTzimtzlum");
			});
			return zone;
		}
		return new InteriorZone(80, 25);
	}

	public override void AddBlueprintsFor(ZoneRequest Request)
	{
		if (Request.Schema.IsNullOrEmpty())
		{
			int num = Request.WorldID.IndexOf('@');
			if (num == -1)
			{
				return;
			}
			int num2 = Request.WorldID.IndexOf('@', ++num);
			if (num2 == -1)
			{
				num2 = Request.WorldID.Length - 1;
			}
			Request.Schema = Request.WorldID.Substring(num, num2 - num);
		}
		if (Blueprint.CellBlueprintsByName.TryGetValue(Request.Schema, out var value))
		{
			Request.Blueprints.Add(value.LevelBlueprint[Request.X, Request.Y, Mathf.Clamp(Request.Z, 0, 49)]);
		}
	}

	public override void AfterBuildZone(Zone Zone, ZoneManager ZoneManager)
	{
		Zone.SetInside(true);
		ZoneManager.PaintWalls(Zone);
		ZoneManager.PaintWater(Zone);
	}
}
