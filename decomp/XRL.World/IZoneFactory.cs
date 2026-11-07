using System;

namespace XRL.World;

public abstract class IZoneFactory
{
	public WorldBlueprint Blueprint;

	public virtual void Initialize()
	{
	}

	[Obsolete("CanBuildZone(string) superseded by CanBuildZone(ZoneRequest).")]
	public virtual bool CanBuildZone(string ZoneID)
	{
		return true;
	}

	public virtual bool CanBuildZone(ZoneRequest Request)
	{
		return true;
	}

	public virtual void AddBlueprintsFor(ZoneRequest Request)
	{
	}

	public virtual Zone GenerateZone(ZoneRequest Request, int Width, int Height)
	{
		return new Zone(Width, Height);
	}

	[Obsolete("BuildZone(string) superseded by BuildZone(ZoneRequest).")]
	public virtual Zone BuildZone(string ZoneID)
	{
		return null;
	}

	public virtual Zone BuildZone(ZoneRequest Request)
	{
		return null;
	}

	public virtual void AfterBuildZone(Zone Zone, ZoneManager Manager)
	{
	}
}
