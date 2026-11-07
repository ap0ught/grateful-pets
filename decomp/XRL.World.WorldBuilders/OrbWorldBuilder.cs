using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.Rules;

namespace XRL.World.WorldBuilders;

public class OrbWorldBuilder : WorldBuilder
{
	public override bool BuildWorld(string worldName)
	{
		Zone zone = ZoneManager.instance.GetZone(worldName);
		List<Tuple<Location2D, int>> list = new List<Tuple<Location2D, int>>();
		for (int i = 0; i < 5; i++)
		{
			int item = Stat.Random(4, 16);
			Location2D item2 = Location2D.Get(Stat.Random(0, zone.Width - 1), Stat.Random(0, zone.Height - 1));
			list.Add(new Tuple<Location2D, int>(item2, item));
		}
		List<Tuple<Location2D, int>> list2 = new List<Tuple<Location2D, int>>();
		for (int j = 0; j < 5; j++)
		{
			int item3 = Stat.Random(15, 40);
			Location2D item4 = Location2D.Get(Stat.Random(0, zone.Width - 1), Stat.Random(0, zone.Height - 1));
			list2.Add(new Tuple<Location2D, int>(item4, item3));
		}
		for (int x = 0; x < zone.Width; x++)
		{
			int y;
			for (y = 0; y < zone.Height; y++)
			{
				if (x == 40 && y == 15)
				{
					zone.GetCell(x, y).AddObject("TerrainOtar");
					continue;
				}
				GameObject gameObject = zone.GetCell(x, y).AddObject("TerrainCraters");
				if (list2.Any((Tuple<Location2D, int> t) => t.Item1.Distance(Location2D.Get(x, y)) == t.Item2))
				{
					gameObject.SetStringProperty("SmallCrater", "yes");
				}
				if (list.Any((Tuple<Location2D, int> t) => t.Item1.Distance(Location2D.Get(x, y)) == t.Item2))
				{
					gameObject.SetStringProperty("BigCrater", "yes");
				}
			}
		}
		zone.HandleEvent(new BeforeZoneBuiltEvent());
		zone.HandleEvent(new ZoneBuiltEvent());
		zone.HandleEvent(new AfterZoneBuiltEvent());
		return true;
	}
}
