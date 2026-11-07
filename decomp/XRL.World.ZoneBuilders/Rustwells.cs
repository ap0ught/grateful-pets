using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.Rules;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class Rustwells : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		if (Z.Z == 10)
		{
			Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		}
		Z.ForeachObjectWithTagOrProperty("Stairs", (GameObject O) =>
		{
			MetricsManager.LogError("Pre-existing stair in " + O.CurrentCell);
		});
		Pitted pitted = new Pitted();
		pitted.MaxWells = 1;
		pitted.MinWells = 1;
		pitted.MinRadius = 8;
		pitted.MaxRadius = 10;
		pitted.XMargin = 20;
		List<Location2D> list = new List<Location2D>();
		List<Location2D> list2 = new List<Location2D>();
		pitted.BuildZone(Z, list, list2);
		Rect2D pitBounds = new Rect2D(list).ExpandBy(1, clampToScreenSize: true);
		int num = 10;
		if (25.in100())
		{
			num += Stat.Random(1, 6);
		}
		int depth = ZoneBuilderSandbox.GetOracleIntColumn(Z, 1, 4);
		float anglechunks = ZoneBuilderSandbox.GetOracleIntColumn(Z, 1, num);
		int divs = ZoneBuilderSandbox.GetOracleIntColumn(Z, 2, 4);
		Z.ForeachCell((Cell C) =>
		{
			int num6 = C.CosmeticDistanceTo(pitBounds.Center);
			int num7 = ((int)(Math.Atan2(C.Pos2D.x - pitBounds.Center.x, C.Pos2D.y - pitBounds.Center.y) / (Math.PI / (double)anglechunks)) + num6 / (divs * depth)) % divs;
			if (!pitBounds.Contains(C.Pos2D) && num6 / depth % 2 == 0 && num7 == 0)
			{
				C.AddObject("Fulcrete");
			}
		});
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("Dirty"));
		List<NoiseMapNode> list3 = new List<NoiseMapNode>();
		foreach (ZoneConnection zoneConnection in The.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			list3.Add(new NoiseMapNode(zoneConnection.X, zoneConnection.Y));
		}
		if (Z.Z > 10)
		{
			NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 3, 4, 80, 80, 6, 3, -3, 1, new List<NoiseMapNode>());
			for (int num2 = 0; num2 < 80; num2++)
			{
				for (int num3 = 0; num3 < 25; num3++)
				{
					if (num2 > 0 && num3 > 0 && num2 < 79 && num3 < 24 && noiseMap.Noise[num2, num3] > 2 && !Z.GetCell(num2, num3).HasObject("Fulcrete"))
					{
						Z.GetCell(num2, num3).ClearWalls();
					}
				}
			}
		}
		Z.GetCell(10 + (Z.Z - 1) / 3, Z.Height - 1).AddObject("Fulcrete");
		if (Z.Z > 10)
		{
			StairsUp stairsUp = new StairsUp();
			stairsUp.Reachable = false;
			stairsUp.BuildZone(Z);
		}
		if (list.Count > 9)
		{
			StairsDown stairsDown = new StairsDown();
			stairsDown.Reachable = false;
			stairsDown.EmptyOnly = false;
			stairsDown.x = "1-78";
			stairsDown.y = "1-23";
			stairsDown.BuildZone(Z);
		}
		Z.FireEvent("FirmPitEdges");
		ZoneBuilderSandbox.EnsureAllVoidsConnected(Z, pathWithNoise: true);
		if (Z.Z == 10)
		{
			int num4 = 0;
			for (int num5 = Stat.Random(0, 4); num4 < num5; num4++)
			{
				using Pathfinder pathfinder = Z.getPathfinder();
				Cell randomElement = (from c in Z.GetEmptyCells()
					where c.X == 0 || c.Y == 0 || c.X == 79 || c.Y == 24
					select c).GetRandomElement();
				if (randomElement == null || !pathfinder.FindPath(Z.GetCell(randomElement.X, randomElement.Y).Location, Z.GetCell(40, 15).Location, Display: false, CardinalDirectionsOnly: true, 24300))
				{
					continue;
				}
				foreach (PathfinderNode step in pathfinder.Steps)
				{
					Z.GetCell(step.X, step.Y).AddObject("SaltyWaterDeepPool");
				}
			}
		}
		Z.ForeachCell((Action<Cell>)SanityCheck);
		if (Z.Z == 13)
		{
			ZoneBuilderSandbox.PlacePopulationInRegion(Z, list2, "RustwellsBottomPool");
		}
		return true;
	}

	public void SanityCheck(Cell C)
	{
		if (!C.HasWall())
		{
			C.SetReachable(State: true);
		}
		else if (C.HasStairs())
		{
			MetricsManager.LogError("Stairs generated blocked by wall, clearing.");
			C.ClearWalls();
			C.SetReachable(State: true);
		}
	}
}
