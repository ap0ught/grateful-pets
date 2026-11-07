using System;
using System.Collections.Generic;
using Genkit;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class ShugBurrowBuilder
{
	public string Wall = "BaseNephilimWall_Shug'rith";

	public bool Pairs;

	public bool HardClear;

	public ShugBurrowBuilder()
	{
	}

	public ShugBurrowBuilder(bool hardClear, string Wall = "BaseNephilimWall_Shug'rith")
	{
		this.Wall = Wall;
		HardClear = hardClear;
	}

	public bool BuildZone(Zone Z)
	{
		List<CachedZoneConnection> list = new List<CachedZoneConnection>();
		List<ZoneConnection> list2 = new List<ZoneConnection>();
		List<ZoneConnection> list3 = new List<ZoneConnection>();
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (item.TargetDirection == "-")
			{
				if (item.Type.Contains("Descending"))
				{
					list2.Add(item);
				}
				if (item.Type.Contains("Ascending"))
				{
					list3.Add(item);
				}
				if (item.Type.Contains("ShugBurrow"))
				{
					list.Add(item);
				}
			}
		}
		int num = 40;
		int num2 = 20;
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (!zoneConnection.Type.Contains("ShugBurrow"))
			{
				continue;
			}
			if (zoneConnection.Type.Contains("Descending") || zoneConnection.Type.Contains("Ascending"))
			{
				num = zoneConnection.X;
				num2 = zoneConnection.Y;
				if (zoneConnection.Type.Contains("Descending"))
				{
					list2.Add(zoneConnection);
				}
				if (zoneConnection.Type.Contains("Ascending"))
				{
					list3.Add(zoneConnection);
				}
			}
			list.Add(new CachedZoneConnection("-", zoneConnection.X, zoneConnection.Y, zoneConnection.Type, null));
		}
		if (list.Count <= 1)
		{
			num = Stat.Random(5, 75);
			num2 = Stat.Random(5, 20);
		}
		else
		{
			num = list[0].X;
			num2 = list[0].Y;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (num == list[i].X && num2 == list[i].Y)
			{
				continue;
			}
			FastNoise pathNoise = new FastNoise();
			pathNoise.SetSeed(Stat.Random(int.MinValue, int.MaxValue));
			pathNoise.SetNoiseType(FastNoise.NoiseType.Simplex);
			pathNoise.SetFractalOctaves(4);
			pathNoise.SetFrequency(0.1f);
			Pathfinder pathfinder = Z.getPathfinder((int x, int y, Cell c) =>
			{
				int num3 = 0;
				num3 = (int)(Math.Abs(pathNoise.GetNoise((x + Z.wX * 80) / 3, y + Z.wY * 25)) * 190f);
				return Z.GetCell(x, y).HasWall() ? (20 + num3) : num3;
			});
			if (pathfinder.FindPath(Location2D.Get(num, num2), Location2D.Get(list[i].X, list[i].Y), Display: false, CardinalDirectionsOnly: true, 24300, ShuffleDirections: true))
			{
				foreach (PathfinderNode step in pathfinder.Steps)
				{
					foreach (Cell item2 in Z.GetCell(step.X, step.Y).GetLocalAdjacentCellsCircular(2, includeSelf: true))
					{
						Z.ReachableMap[step.X, step.Y] = true;
						if (HardClear)
						{
							item2.Clear();
						}
						else
						{
							item2.ClearTerrain();
						}
					}
				}
			}
			foreach (PathfinderNode step2 in pathfinder.Steps)
			{
				foreach (Cell item3 in Z.GetCell(step2.X, step2.Y).GetLocalAdjacentCellsCircular(3, includeSelf: true))
				{
					if (item3.HasWall())
					{
						item3.ClearWalls();
						item3.AddObject(Wall);
					}
					else if (Stat.Random(1, 100) <= 1)
					{
						item3.AddObject(PopulationManager.RollOneFrom("IsolatedPocketDecoration").Blueprint);
					}
				}
			}
			pathfinder.Dispose();
		}
		foreach (ZoneConnection item4 in list3)
		{
			foreach (Cell item5 in Z.GetCell(item4.Loc2D).GetLocalAdjacentCellsCircular(3, includeSelf: true))
			{
				item5.ClearWalls();
			}
			foreach (Cell item6 in Z.GetCell(item4.Loc2D).GetLocalAdjacentCellsCircular(4, includeSelf: true))
			{
				if (item6.HasWall())
				{
					item6.ClearWalls();
					item6.AddObject("BaseNephilimWall_Shug'rith");
				}
			}
		}
		foreach (ZoneConnection item7 in list2)
		{
			foreach (Cell item8 in Z.GetCell(item7.Loc2D).GetLocalAdjacentCellsCircular(4, includeSelf: true))
			{
				if (item8.HasWall())
				{
					item8.ClearWalls();
					item8.AddObject("BaseNephilimWall_Shug'rith");
				}
			}
			foreach (Cell item9 in Z.GetCell(item7.Loc2D).GetLocalAdjacentCellsCircular(3, includeSelf: true))
			{
				item9.ClearWalls();
				GameObject gameObject = GameObjectFactory.Factory.CreateObject("LazyPit");
				if (Z.Z == 10)
				{
					GameObject gameObject2 = GameObject.Create(Z.GetDefaultWall());
					string text = "&" + gameObject2.Render.GetForegroundColor();
					gameObject.Render.ColorString = text + "^k";
					gameObject.Render.TileColor = text + "^k";
				}
				gameObject.GetPart<XRL.World.Parts.StairsDown>().ConnectLanding = false;
				item9.AddObject(gameObject);
				item9.AddObject("StairBlocker");
				item9.AddObject("InfluenceMapBlocker");
			}
		}
		ZoneTemplateManager.Templates["ShugruithTunnel"].Execute(Z);
		return true;
	}
}
