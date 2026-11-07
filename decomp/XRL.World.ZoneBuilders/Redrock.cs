using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.Core;
using XRL.Rules;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class Redrock : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		if (Z.Z > 10)
		{
			new StairsUp().BuildZone(Z);
		}
		if (Z.Z < 14)
		{
			new StairsDown().BuildZone(Z);
		}
		List<NoiseMapNode> extraNodes = new List<NoiseMapNode>();
		Z.GetCells().ForEach((Cell c) =>
		{
			c.AddObject("Shale");
		});
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			extraNodes.Add(new NoiseMapNode(zoneConnection.X, zoneConnection.Y));
		}
		Z.GetCells().ForEach((Cell c) =>
		{
			if (c.HasObjectWithTag("Stairs"))
			{
				extraNodes.Add(new NoiseMapNode(c.X, c.Y));
			}
		});
		string seedsPerSector = "0-7";
		if (Z.Z == 11)
		{
			seedsPerSector = "1-3";
		}
		if (Z.Z == 12)
		{
			seedsPerSector = "0-5";
		}
		if (Z.Z == 13)
		{
			seedsPerSector = "0-6";
		}
		if (Z.Z == 14)
		{
			seedsPerSector = "0-7";
		}
		int maximumSeeds = int.MaxValue;
		if (Z.Z == 11)
		{
			maximumSeeds = 3;
		}
		NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 2, seedsPerSector, 80, 240, 0, 4, -3, 1, extraNodes, -1, maximumSeeds);
		for (int num = 0; num < 80; num++)
		{
			for (int num2 = 0; num2 < 25; num2++)
			{
				if (num > 0 && num2 > 0 && num < 79 && num2 < 24 && noiseMap.Noise[num, num2] >= 1 && !Z.GetCell(num, num2).HasObject("Fulcrete"))
				{
					Z.GetCell(num, num2).ClearWalls();
				}
			}
		}
		string dice = "0";
		if (Z.Z == 11)
		{
			dice = "0";
		}
		if (Z.Z == 12)
		{
			dice = "1";
		}
		if (Z.Z == 13)
		{
			dice = "2-3";
		}
		if (Z.Z == 14)
		{
			dice = "3-4";
		}
		int num3 = Stat.Roll(dice);
		List<NoiseMapNode> list = new List<NoiseMapNode>();
		for (int num4 = 0; num4 < num3; num4++)
		{
			Location2D randomElement = noiseMap.PlacedSeeds.GetRandomElement();
			list.Add(new NoiseMapNode(randomElement.X, randomElement.Y));
		}
		NoiseMap noiseMap2 = new NoiseMap(80, 25, 10, 1, 1, "0", 80, 240, 0, 3, -3, 1, list, -1, num3 * 3);
		for (int num5 = 0; num5 < Z.Height; num5++)
		{
			for (int num6 = 0; num6 < Z.Width; num6++)
			{
				if (num6 > 0 && num5 > 0 && num6 < 79 && num5 < 24 && noiseMap2.Noise[num6, num5] >= 1)
				{
					Z.GetCell(num6, num5).AddObject("DeepBrackishPool");
				}
			}
		}
		foreach (NoiseMapNode featureLocation in list)
		{
			Cell randomElement2 = (from c in Z.GetCells()
				where c.HasWall() && c.DistanceTo(featureLocation.x, featureLocation.y) < 30
				select c).GetRandomElement();
			ZoneBuilderSandbox.TunnelTo(Z, Location2D.Get(featureLocation.x, featureLocation.y), Location2D.Get(randomElement2.X, randomElement2.Y), pathWithNoise: true, 0.2f, 200, (Cell c) =>
			{
				c.Clear();
				c.AddObject("DeepBrackishPool");
			});
		}
		InfluenceMap influenceMap = ZoneBuilderSandbox.GenerateInfluenceMap(Z, null, InfluenceMapSeedStrategy.FurthestPoint, 400);
		List<GameObject> stairs = Z.FindObjects((GameObject o) => o.HasTag("Stairs"));
		List<InfluenceMapRegion> list2 = influenceMap.Regions.Where((InfluenceMapRegion r) => !r.Cells.Any((Location2D l) => Z.GetCell(l).HasObjectWithTag("Stairs"))).ToList();
		if (stairs.Count > 0)
		{
			list2.Sort((InfluenceMapRegion a, InfluenceMapRegion b) => stairs.Sum((GameObject s) => b.Center.Distance(s.CurrentCell.Location)) - stairs.Sum((GameObject s) => a.Center.Distance(s.CurrentCell.Location)));
		}
		int num7 = 3;
		if (Z.Z == 11)
		{
			num7 = Stat.Random(0, 1);
		}
		if (Z.Z == 12)
		{
			num7 = Stat.Random(1, 2);
		}
		if (Z.Z == 13)
		{
			num7 = Stat.Random(2, 3);
		}
		if (Z.Z == 14)
		{
			num7 = Stat.Random(3, 4);
		}
		for (int num8 = 0; num8 < num7; num8++)
		{
			Location2D start = noiseMap.PlacedSeeds.GetRandomElement();
			noiseMap.PlacedSeeds.Sort((Location2D a, Location2D b) => b.Distance(start) - a.Distance(start));
			ZoneBuilderSandbox.TunnelTo(Z, start, noiseMap.PlacedSeeds.First(), pathWithNoise: true, 0.2f, 200);
		}
		ZoneBuilderSandbox.EnsureAllVoidsConnected(Z, pathWithNoise: true);
		foreach (Cell item in from c in Z.GetCells()
			where !c.HasWall()
			select c)
		{
			item.SetReachable(State: true);
		}
		if (Z.Z == 12)
		{
			int num9 = 1;
			if (75.in100())
			{
				num9++;
			}
			if (num9 == 2 && 30.in100())
			{
				num9++;
			}
			for (int num10 = 0; num10 < list2.Count && num10 < num9; num10++)
			{
				list2[num10].Cells.ForEach((Location2D c) =>
				{
					Z.GetCell(c).AddObject("InfluenceMapBlocker");
				});
				ZoneBuilderSandbox.PlacePopulationInRegion(Z, list2[num10], "RedrockLevel2Encounters");
			}
		}
		if (Z.Z == 13)
		{
			string blueprint = PopulationManager.RollOneFrom("RedrockSnapjawFortStyle").Blueprint;
			if (blueprint == "Stockade")
			{
				new RedrockStockadeMaker().BuildZone(Z, ClearCombatObjectsFirst: true, "BrinestalkStakes", "SnapjawParty1", null, "SnapjawStockadeRoom with Snapjaws", "SnapjawStockadeRoom with Bear", "SnapjawStockadeRoom Small with Snapjaws", "SnapjawStockadeRoom Small with Bear", "16-35", "12-20", SpecialRedrockBuilder: true, "SnapjawStockadeOuterArea");
			}
			if (blueprint == "City")
			{
				InfluenceMap iF = ZoneBuilderSandbox.GenerateInfluenceMap(Z, null, InfluenceMapSeedStrategy.FurthestPoint, 100);
				new CaveCity().BuildZone(Z, iF, "Brinestalk Gate", "BrinestalkStakes", 2, 9, 3, 3, "3-5", 100, "RedrockSnapjawCaveRoomDecoration", "RedrockSnapjawCaveRoomCreatures", "SnapjawBoss", Z.FindObject("StairsUp")?.CurrentCell?.Location);
			}
			if (blueprint == "Fort")
			{
				new SnapjawFortMaker().BuildZone(Z);
			}
		}
		if (Z.Z == 14)
		{
			new RiverStartMouth().BuildZone(Z);
			new RiverSouthMouth().BuildZone(Z);
			new RiverBuilder().BuildZone(Z);
		}
		ZoneBuilderSandbox.EnsureAllVoidsConnected(Z, pathWithNoise: true);
		return true;
	}
}
