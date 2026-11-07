using System.Collections.Generic;
using System.Linq;

namespace Genkit;

public class LocationList : ILocationArea
{
	private IEnumerable<Location2D> locations;

	private List<Location2D> border;

	private List<Location2D> nonborder;

	private Location2D center = Location2D.Get(0, 0);

	public LocationList(IEnumerable<Location2D> area)
	{
		locations = area;
		int num = int.MaxValue;
		int num2 = int.MinValue;
		int num3 = int.MaxValue;
		int num4 = int.MinValue;
		foreach (Location2D location in locations)
		{
			if (location.X < num)
			{
				num = location.X;
			}
			if (location.Y < num3)
			{
				num3 = location.Y;
			}
			if (location.X > num2)
			{
				num2 = location.X;
			}
			if (location.Y > num4)
			{
				num4 = location.Y;
			}
		}
		int x = (num2 + num) / 2;
		int y = (num4 + num3) / 2;
		Location2D location2D = Location2D.Get(x, y);
		if (!(location2D != null))
		{
			return;
		}
		int num5 = int.MaxValue;
		foreach (Location2D location2 in locations)
		{
			int num6 = location2.Distance(location2D);
			if (num6 < num5)
			{
				center = location2;
				num5 = num6;
			}
		}
	}

	public bool PointIn(Location2D location)
	{
		return locations.Contains(location);
	}

	public IEnumerable<Location2D> EnumerateLocations()
	{
		return locations;
	}

	public void generateBorders()
	{
		foreach (Location2D location in locations)
		{
			if (location.cardinalNeighbors.Count((Location2D n) => locations.Contains(n)) == 4)
			{
				nonborder.Add(location);
			}
			else
			{
				border.Add(location);
			}
		}
	}

	public IEnumerable<Location2D> EnumerateBorderLocations()
	{
		if (border == null)
		{
			generateBorders();
		}
		return border;
	}

	public IEnumerable<Location2D> EnumerateNonBorderLocations()
	{
		if (nonborder == null)
		{
			generateBorders();
		}
		return nonborder;
	}

	public Location2D GetCenter()
	{
		return center;
	}
}
