using System.Collections.Generic;
using System.Linq;
using Genkit;

namespace XRL.World;

public class MutabilityMap
{
	private int[,] mutable;

	private Dictionary<Location2D, bool> mutableLocations = new Dictionary<Location2D, bool>();

	private Dictionary<string, List<Location2D>> mutableLocationsByTerrainType = new Dictionary<string, List<Location2D>>();

	public Location2D popMutableLocationInArea(int x1, int y1, int x2, int y2)
	{
		foreach (Location2D item in mutableLocations.Keys.ToList().ShuffleInPlace())
		{
			if (item.X >= x1 && item.X <= x2 && item.Y >= y1 && item.Y <= y2)
			{
				SetMutable(item, 0);
				return item;
			}
		}
		return null;
	}

	public Location2D popMutableLocation()
	{
		Location2D randomElement = mutableLocations.Keys.GetRandomElement();
		SetMutable(randomElement, 0);
		return randomElement;
	}

	public Location2D GetMutableLocationWithTerrain(string Terrain)
	{
		if (!mutableLocationsByTerrainType.TryGetValue(Terrain, out var value))
		{
			return null;
		}
		if (value.Count == 0)
		{
			return null;
		}
		return value.GetRandomElement();
	}

	public void Init(int width, int height)
	{
		mutable = new int[width, height];
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				mutable[j, i] = 0;
			}
		}
		mutableLocations.Clear();
		mutableLocationsByTerrainType.Clear();
	}

	public void SetMutable(Location2D location, int value)
	{
		if (value == 0)
		{
			if (mutableLocations.ContainsKey(location))
			{
				mutableLocations.Remove(location);
			}
			foreach (KeyValuePair<string, List<Location2D>> item in mutableLocationsByTerrainType)
			{
				item.Value.Remove(location);
			}
		}
		else if (!mutableLocations.ContainsKey(location))
		{
			mutableLocations.Add(location, value: true);
		}
		mutable[location.X, location.Y] = value;
	}

	public bool GetWorldMutable(Location2D Location)
	{
		return GetMutable(Location.X * 3 + 1, Location.Y * 3 + 1) > 0;
	}

	public void SetWorldMutable(Location2D Location, int Mutable)
	{
		SetMutable(Location2D.Get(Location.X * 3 + 1, Location.Y * 3 + 1), Mutable);
	}

	public bool GetWorldBlockMutable(Location2D Location)
	{
		for (int i = 0; i <= 2; i++)
		{
			for (int j = 0; j <= 2; j++)
			{
				if (GetMutable(Location.X * 3 + j, Location.Y * 3 + i) <= 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void SetWorldBlockMutable(Location2D Location, int Mutable)
	{
		for (int i = 0; i <= 2; i++)
		{
			for (int j = 0; j <= 2; j++)
			{
				SetMutable(Location2D.Get(Location.X * 3 + j, Location.Y * 3 + i), Mutable);
			}
		}
	}

	public int GetMutable(Location2D L)
	{
		return GetMutable(L.X, L.Y);
	}

	public int GetMutable(int x, int y)
	{
		if (x < 0)
		{
			return 0;
		}
		if (y < 0)
		{
			return 0;
		}
		if (x >= 240)
		{
			return 0;
		}
		if (y >= 75)
		{
			return 0;
		}
		return mutable[x, y];
	}

	public void RemoveMutableLocation(string zoneID)
	{
		if (ZoneID.Parse(zoneID, out var ParasangX, out var ParasangY, out var ZoneX, out var ZoneY))
		{
			RemoveMutableLocation(Location2D.Get(ParasangX * 3 + ZoneX, ParasangY * 3 + ZoneY));
		}
	}

	public void RemoveMutableLocation(Location2D location)
	{
		SetMutable(location, 0);
	}

	public void AddMutableLocation(Location2D location, string terrainType = null, int value = 1)
	{
		mutable[location.X, location.Y] = value;
		mutableLocations.Add(location, value: true);
		if (!terrainType.IsNullOrEmpty())
		{
			if (!mutableLocationsByTerrainType.TryGetValue(terrainType, out var value2))
			{
				value2 = new List<Location2D>();
				mutableLocationsByTerrainType.Add(terrainType, value2);
			}
			value2.Add(location);
		}
	}
}
