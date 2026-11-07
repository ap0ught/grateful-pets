using System;
using System.Collections.Generic;

namespace XRL.World;

[Serializable]
public struct ZoneRequest
{
	public string ZoneID;

	public string WorldID;

	public string Schema;

	public string Instance;

	public string Seed;

	public int WorldX;

	public int WorldY;

	public int X;

	public int Y;

	public int Z;

	public bool IsWorldZone;

	public Zone Zone;

	public WorldBlueprint World;

	public List<ZoneBlueprint> Blueprints;

	public ZoneRequest(string ZoneID)
	{
		this.ZoneID = ZoneID;
		IsWorldZone = ZoneID.IndexOf('.') == -1;
		Seed = null;
		Zone = null;
		World = null;
		Blueprints = null;
		if (IsWorldZone)
		{
			WorldID = ZoneID;
			Schema = (Instance = null);
			WorldX = (WorldY = (X = (Y = -1)));
			Z = 10;
		}
		else
		{
			XRL.World.ZoneID.Parse(ZoneID, out WorldID, out Schema, out Instance, out WorldX, out WorldY, out X, out Y, out Z);
		}
	}

	public ZoneRequest(Zone Zone)
	{
		this.Zone = Zone;
		ZoneID = Zone.ZoneID;
		IsWorldZone = Zone.IsWorldMap();
		Seed = null;
		World = null;
		Blueprints = null;
		if (IsWorldZone)
		{
			WorldID = ZoneID;
			Schema = (Instance = null);
			WorldX = (WorldY = (X = (Y = -1)));
			Z = 10;
		}
		else
		{
			WorldID = Zone.ZoneWorld;
			WorldX = Zone.wX;
			WorldY = Zone.wY;
			X = Zone.X;
			Y = Zone.Y;
			Z = Zone.Z;
		}
		if (Zone is InteriorZone interiorZone)
		{
			Schema = interiorZone.Schema;
			Instance = interiorZone.Instance;
		}
		else
		{
			Schema = (Instance = null);
		}
	}

	public ZoneRequest(string WorldID, int WorldX, int WorldY, int X, int Y, int Z)
	{
		ZoneID = XRL.World.ZoneID.Assemble(WorldID, WorldX, WorldY, X, Y, Z);
		IsWorldZone = false;
		Seed = null;
		Zone = null;
		World = null;
		Schema = null;
		Instance = null;
		Blueprints = null;
		this.WorldID = WorldID;
		this.WorldX = WorldX;
		this.WorldY = WorldY;
		this.X = X;
		this.Y = Y;
		this.Z = Z;
	}

	public override string ToString()
	{
		return "[ZoneID: " + ZoneID + ", WorldID: " + WorldID + ", Schema: " + Schema + ", Instance: " + Instance + ", Seed: " + Seed + ", Position: (" + WorldX + ", " + WorldY + ", " + X + ", " + Y + ", " + Z + "), IsWorldZone: " + IsWorldZone + "]";
	}
}
