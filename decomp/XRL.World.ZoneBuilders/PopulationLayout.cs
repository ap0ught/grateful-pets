using System.Collections.Generic;
using Genkit;
using UnityEngine;

namespace XRL.World.ZoneBuilders;

public class PopulationLayout
{
	public Rect2D innerRect;

	public InfluenceMap map;

	public int seed;

	public List<Location2D> inside = new List<Location2D>();

	public List<Location2D> insideWall = new List<Location2D>();

	public List<Location2D> insideCorner = new List<Location2D>();

	public List<Location2D> outside = new List<Location2D>();

	public List<Location2D> outsideWall = new List<Location2D>();

	public List<Location2D> outsideCorner = new List<Location2D>();

	public bool hasStructure;

	public Zone zone;

	public InfluenceMapRegion originalRegion;

	public Location2D lastPosition;

	public InfluenceMapRegion region
	{
		get
		{
			if (map.Regions.Count <= seed)
			{
				return map.Regions[0];
			}
			return map.Regions[seed];
		}
	}

	public Location2D position
	{
		get
		{
			if (lastPosition != null)
			{
				return lastPosition;
			}
			if (map.Seeds.Count <= seed)
			{
				Debug.LogError("Couldn't get a seed:" + seed);
				return null;
			}
			return map.Seeds[seed];
		}
		set
		{
			lastPosition = value;
			map.Seeds[seed] = value;
		}
	}

	public PopulationLayout(Zone zone, InfluenceMapRegion region, Rect2D innerRect)
	{
		this.zone = zone;
		map = region.map;
		seed = region.Seed;
		this.innerRect = innerRect;
		position = this.innerRect.Center.location;
		originalRegion = region;
	}

	public PopulationLayout(Zone zone, InfluenceMapRegion region, Rect2D innerRect, Location2D position)
	{
		this.zone = zone;
		map = region.map;
		seed = region.Seed;
		this.innerRect = innerRect;
		this.position = position;
		originalRegion = region;
	}
}
