using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using UnityEngine;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class SixDayTents : ZoneBuilderSandbox
{
	private const int MaxWidth = 1200;

	private const int MaxHeight = 375;

	public bool Noise;

	[NonSerialized]
	public static Maze3D TunnelMaze;

	public bool BuildZone(Zone Z)
	{
		Z.ClearReachableMap(bValue: true);
		if (!XRLCore.Core.Game.HasIntGameState("SixDaySeed"))
		{
			XRLCore.Core.Game.SetIntGameState("SixDaySeed", Stat.Random(0, 2147483646));
		}
		List<Location2D> list = new List<Location2D>();
		if (Z.Y == 0)
		{
			if (Z.X == 0)
			{
				list.Add(Location2D.Get(38, 12));
				list.Add(Location2D.Get(79, 12));
				list.Add(Location2D.Get(38, 12));
				list.Add(Location2D.Get(38, 24));
			}
			if (Z.X == 1)
			{
				list.Add(Location2D.Get(0, 12));
				list.Add(Location2D.Get(79, 12));
			}
			if (Z.X == 2)
			{
				list.Add(Location2D.Get(0, 12));
				list.Add(Location2D.Get(38, 12));
				list.Add(Location2D.Get(38, 12));
				list.Add(Location2D.Get(38, 24));
			}
		}
		if (Z.Y == 1)
		{
			if (Z.X == 0)
			{
				list.Add(Location2D.Get(38, 0));
				list.Add(Location2D.Get(38, 24));
			}
			_ = Z.X;
			_ = 1;
			if (Z.X == 2)
			{
				list.Add(Location2D.Get(38, 0));
				list.Add(Location2D.Get(38, 24));
			}
		}
		if (Z.Y == 2)
		{
			if (Z.X == 0)
			{
				list.Add(Location2D.Get(38, 12));
				list.Add(Location2D.Get(79, 12));
				list.Add(Location2D.Get(38, 0));
				list.Add(Location2D.Get(38, 12));
			}
			if (Z.X == 1)
			{
				MapBuilder mapBuilder = new MapBuilder();
				mapBuilder.ID = "StiltSouth.rpm";
				mapBuilder.BuildZone(Z);
				list.Add(Location2D.Get(22, 0));
				list.Add(Location2D.Get(38, 24));
				list.Add(Location2D.Get(23, 0));
				list.Add(Location2D.Get(38, 24));
				list.Add(Location2D.Get(38, 0));
				list.Add(Location2D.Get(38, 24));
				list.Add(Location2D.Get(40, 0));
				list.Add(Location2D.Get(40, 24));
				list.Add(Location2D.Get(38, 12));
				list.Add(Location2D.Get(0, 12));
				list.Add(Location2D.Get(38, 12));
				list.Add(Location2D.Get(79, 12));
				ZoneManager.instance.SetZoneProperty(Z.ZoneID, "PullDownLocation", "38,20");
			}
			if (Z.X == 2)
			{
				list.Add(Location2D.Get(0, 12));
				list.Add(Location2D.Get(38, 12));
				list.Add(Location2D.Get(38, 0));
				list.Add(Location2D.Get(38, 12));
			}
		}
		List<Location2D> list2 = new List<Location2D>();
		if (list.Count > 0)
		{
			list2 = BuildSimplePathWithObject(Z, list, "SaltPath", 4, Noise: true);
		}
		list2.AddRange(from c in Z.GetCellsWithObject("NoStiltSpawn")
			select c.Location);
		int num = 300;
		InfluenceMap influenceMap;
		string text;
		int num2;
		int num3;
		int num4;
		while (true)
		{
			influenceMap = ZoneBuilderSandbox.GenerateInfluenceMap(Z, null, InfluenceMapSeedStrategy.RandomPointFurtherThan4, num, list2);
			text = ".";
			if (Z.X == 0 && Z.Y == 0)
			{
				text = "NW";
			}
			if (Z.X == 1 && Z.Y == 0)
			{
				text = "N";
			}
			if (Z.X == 2 && Z.Y == 0)
			{
				text = "NE";
			}
			if (Z.X == 0 && Z.Y == 1)
			{
				text = "W";
			}
			if (Z.X == 1 && Z.Y == 1)
			{
				text = ".";
			}
			if (Z.X == 2 && Z.Y == 1)
			{
				text = "E";
			}
			if (Z.X == 0 && Z.Y == 2)
			{
				text = "SW";
			}
			if (Z.X == 1 && Z.Y == 2)
			{
				text = "S";
			}
			if (Z.X == 2 && Z.Y == 2)
			{
				text = "SE";
			}
			num2 = "3-4".RollCached();
			num3 = 0;
			num4 = 0;
			if (text == "SE")
			{
				num4 = 4;
			}
			if (num2 < num4)
			{
				num2 = num4;
			}
			int num5 = 0;
			foreach (InfluenceMapRegion region in influenceMap.Regions)
			{
				Rect2D rect2D = GridTools.MaxRectByArea(region.GetGrid()).ReduceBy(1, 1).Translate(region.BoundingBox.UpperLeft);
				if (rect2D.Width >= 6 && rect2D.Height >= 6)
				{
					num5++;
				}
			}
			if (num5 >= num4 || num >= 2000)
			{
				break;
			}
			num += 100;
		}
		foreach (InfluenceMapRegion region2 in influenceMap.Regions)
		{
			Rect2D r = GridTools.MaxRectByArea(region2.GetGrid()).ReduceBy(1, 1).Translate(region2.BoundingBox.UpperLeft);
			if (r.Width >= 6 && r.Height >= 6 && Stat.Random(1, 100) <= 15 && num3 >= num4)
			{
				Location2D location = r.GetRandomDoorCell().location;
				ZoneBuilderSandbox.PlaceObjectOnRect(Z, "BrinestalkFence", r);
				GetCell(Z, location).Clear();
				GetCell(Z, location).AddObject("Brinestalk Gate");
				string cellSide = r.GetCellSide(location.Point);
				Rect2D r2 = r.ReduceBy(0, 0);
				int num6 = 0;
				if (cellSide == "N")
				{
					num6 = ((Stat.Random(0, 1) == 0) ? 2 : 3);
				}
				if (cellSide == "S")
				{
					num6 = ((Stat.Random(0, 1) != 0) ? 1 : 0);
				}
				if (cellSide == "E")
				{
					num6 = ((Stat.Random(0, 1) != 0) ? 3 : 0);
				}
				if (cellSide == "W")
				{
					num6 = ((Stat.Random(0, 1) == 0) ? 1 : 2);
				}
				if (num6 == 0 || num6 == 1)
				{
					r2.y2 = r2.y1 + 3;
				}
				else
				{
					r2.y1 = r2.y2 - 3;
				}
				if (num6 == 0 || num6 == 3)
				{
					r2.x2 = r2.x1 + 3;
				}
				else
				{
					r2.x1 = r2.x2 - 3;
				}
				ClearRect(Z, r2);
				ZoneBuilderSandbox.PlaceObjectOnRect(Z, "BrinestalkWall", r2);
				Location2D location2 = r2.GetRandomDoorCell(cellSide, 1).location;
				Z.GetCell(location2).Clear();
				Z.GetCell(location2).AddObject("Door");
				if (PopulationManager.HasPopulation("StiltAnimalPen"))
				{
					ZoneBuilderSandbox.PlacePopulationInRect(Z, r.ReduceBy(1, 1), "StiltAnimalPen");
				}
				if (PopulationManager.HasPopulation("StiltAnimalPenHut"))
				{
					ZoneBuilderSandbox.PlacePopulationInRect(Z, r2.ReduceBy(1, 1), "StiltAnimalPenHut");
				}
			}
			else if (r.Width >= 6 && r.Height >= 6 && num2 > 0)
			{
				Location2D location3 = r.Center.location;
				Debug.Log(location3);
				Debug.Log(location3.RegionDirection(80, 25));
				Debug.Log(Calc.GetOppositeDirection(location3.RegionDirection(80, 25)[0]));
				Location2D location4 = r.GetRandomDoorCell((location3.RegionDirection(80, 25) == ".") ? Calc.GetOppositeDirection(text[0]) : Calc.GetOppositeDirection(location3.RegionDirection(80, 25)[0])).location;
				if (r.DoorDirection == "N" || r.DoorDirection == "S")
				{
					r.Pinch = r.Height / 2;
				}
				if (r.DoorDirection == "E" || r.DoorDirection == "W")
				{
					r.Pinch = r.Width / 2;
				}
				ZoneBuilderSandbox.PlaceObjectOnRect(Z, "CanvasWall", r);
				GetCell(Z, location4).Clear();
				if (PopulationManager.HasPopulation("StiltTents_" + text + "#" + num3))
				{
					ZoneBuilderSandbox.PlacePopulationInRegionRect(Z, region2, r.ReduceBy(1, 1), r.DoorDirection, "StiltTents_" + text + "#" + num3, "Merchants");
				}
				else if (PopulationManager.HasPopulation("StiltTents_" + text))
				{
					ZoneBuilderSandbox.PlacePopulationInRegionRect(Z, region2, r.ReduceBy(1, 1), r.DoorDirection, "StiltTents_" + text, "Merchants");
				}
				else
				{
					ZoneBuilderSandbox.PlacePopulationInRegionRect(Z, region2, r.ReduceBy(1, 1), r.DoorDirection, "StiltTents", "Merchants");
				}
				num2--;
				num3++;
			}
			else if (PopulationManager.HasPopulation("StiltFields_" + text))
			{
				ZoneBuilderSandbox.PlacePopulationInRegion(Z, region2, "StiltFields_" + text);
			}
			else
			{
				ZoneBuilderSandbox.PlacePopulationInRegion(Z, region2, "StiltFields");
			}
		}
		foreach (GameObject @object in Z.GetObjects("CanvasWall"))
		{
			@object.SetIntProperty("NoClear", 1);
		}
		if (PopulationManager.HasPopulation("StiltRoads_" + text))
		{
			ZoneBuilderSandbox.PlacePopulationInCells(Z, list2, "StiltRoads_" + text);
		}
		else if (PopulationManager.HasPopulation("StiltRoads"))
		{
			ZoneBuilderSandbox.PlacePopulationInCells(Z, list2, "StiltRoads");
		}
		Z.BuildReachabilityFromEdges();
		return true;
	}
}
