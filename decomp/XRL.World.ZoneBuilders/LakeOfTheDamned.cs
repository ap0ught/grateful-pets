using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.Core;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class LakeOfTheDamned : ZoneBuilderSandbox
{
	private FastNoise fastNoise = new FastNoise();

	public static List<string> lakeLiquids = new List<string> { "putrid", "blood", "wax", "ink" };

	public int getSeed(string seed)
	{
		return XRLCore.Core.Game.GetWorldSeed(seed);
	}

	public double sampleSimplexNoise(string type, int x, int y, int z, int amplitude, float frequencyMultiplier = 1f)
	{
		fastNoise.SetSeed(getSeed(type));
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(0.25f * frequencyMultiplier);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetFractalOctaves(3);
		fastNoise.SetFractalLacunarity(0.7f);
		fastNoise.SetFractalGain(1f);
		return Math.Ceiling((double)fastNoise.GetNoise(x, y, z) * (double)amplitude);
	}

	public GameObject getLiquid(int x, int y, Zone zone)
	{
		int num = zone.wX * 3 + zone.X;
		int num2 = zone.wY * 3 + zone.Y;
		int offsetX = num * zone.Width;
		int offsetY = num2 * zone.Height;
		List<Tuple<string, double>> list = lakeLiquids.Select((string liquid) => new Tuple<string, double>(liquid, sampleSimplexNoise(liquid, offsetX + x, offsetY + y, zone.Z, 10, 0.33f))).ToList();
		list.Sort((Tuple<string, double> a, Tuple<string, double> b) => b.Item2.CompareTo(a.Item2));
		GameObject gameObject = GameObject.Create("Water");
		LiquidVolume part = gameObject.GetPart<LiquidVolume>();
		if (list[0].Item2 >= 3.0 && list[1].Item2 < 3.0)
		{
			part.ComponentLiquids.Clear();
			part.SetComponent(list[0].Item1, 1000);
		}
		else
		{
			part.ComponentLiquids.Clear();
			part.SetComponent(list[0].Item1, 500);
			part.SetComponent(list[1].Item1, 500);
		}
		part.Volume = 2000;
		part.Update();
		return gameObject;
	}

	public List<string> getStairsZones()
	{
		return ZoneBuilderSandbox.GetOracleNFromList(new List<string> { "JoppaWorld.53.3.0.0.12", "JoppaWorld.53.3.1.0.12", "JoppaWorld.53.3.2.0.12", "JoppaWorld.53.3.0.1.12", "JoppaWorld.53.3.2.1.12", "JoppaWorld.53.3.0.2.12", "JoppaWorld.53.3.1.2.12", "JoppaWorld.53.3.2.2.12" }, 2, "LakeOfTheDamnedStairs");
	}

	public static Location2D getStairsLocation(string zone)
	{
		zone = zone.Substring(0, zone.LastIndexOf("."));
		Box box = new Box(39, 12, 40, 13);
		Location2D location2D = Location2D.Get(39, 12);
		while (box.contains(location2D))
		{
			location2D = Location2D.Get(ZoneBuilderSandbox.GetOracleIntFromString(zone + "x", 6, 70), ZoneBuilderSandbox.GetOracleIntFromString(zone + "y", 5, 19));
		}
		return location2D;
	}

	public static List<string> getCatacombsStairsZones()
	{
		return ZoneBuilderSandbox.GetOracleNFromList(new List<string> { "JoppaWorld.53.3.0.0.11", "JoppaWorld.53.3.1.0.11", "JoppaWorld.53.3.2.0.11", "JoppaWorld.53.3.0.1.11", "JoppaWorld.53.3.2.1.11", "JoppaWorld.53.3.0.2.11", "JoppaWorld.53.3.1.2.11", "JoppaWorld.53.3.2.2.11" }, 2, "LakeOfTheDamnedStairs");
	}

	public bool BuildZone(Zone zone)
	{
		if (zone.X == 0 && zone.Y == 0)
		{
			new MapBuilder("SultanFrameNW.rpm").BuildZone(zone);
		}
		if (zone.X == 1 && zone.Y == 0)
		{
			new MapBuilder("SultanFrameN.rpm").BuildZone(zone);
		}
		if (zone.X == 2 && zone.Y == 0)
		{
			new MapBuilder("SultanFrameNE.rpm").BuildZone(zone);
		}
		if (zone.X == 0 && zone.Y == 1)
		{
			new MapBuilder("SultanFrameW.rpm").BuildZone(zone);
		}
		if (zone.X == 2 && zone.Y == 1)
		{
			new MapBuilder("SultanFrameE.rpm").BuildZone(zone);
		}
		if (zone.X == 0 && zone.Y == 2)
		{
			new MapBuilder("SultanFrameSW.rpm").BuildZone(zone);
		}
		if (zone.X == 1 && zone.Y == 2)
		{
			new MapBuilder("SultanFrameS.rpm").BuildZone(zone);
		}
		if (zone.X == 2 && zone.Y == 2)
		{
			new MapBuilder("SultanFrameSE.rpm").BuildZone(zone);
		}
		int num = zone.wX * 3 + zone.X;
		int num2 = zone.wY * 3 + zone.Y;
		int num3 = num * zone.Width;
		int num4 = num2 * zone.Height;
		int amplitude = 5;
		for (int i = 0; i < 80; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				if (sampleSimplexNoise("islands", num3 + i, num4 + j, zone.Z, amplitude, 0.33f) <= 1.0)
				{
					Cell cell = zone.GetCell(i, j);
					if (cell.IsEmpty())
					{
						cell.AddObject(getLiquid(i, j, zone));
					}
				}
			}
		}
		InfluenceMap influenceMap = ZoneBuilderSandbox.GenerateInfluenceMap(zone, new List<Point>(), InfluenceMapSeedStrategy.LargestRegion, 50, null, bDraw: false, (Cell c) => (c.HasWall() || c.HasObjectWithPart("LiquidVolume")) ? 1 : 0);
		influenceMap.SeedAllUnseeded();
		ZoneTemplateManager.Templates["LakeOfTheDamnedIslands"].Execute(zone, influenceMap);
		ZoneTemplateManager.Templates["LakeOfTheDamnedIslandPopulation"].Execute(zone, influenceMap);
		InfluenceMap influenceMap2 = ZoneBuilderSandbox.GenerateInfluenceMap(zone, new List<Point>(), InfluenceMapSeedStrategy.LargestRegion, 50, null, bDraw: false, (Cell c) => (!c.HasObjectWithPart("LiquidVolume")) ? 1 : 0);
		influenceMap2.SeedAllUnseeded();
		ZoneTemplateManager.Templates["LakeOfTheDamnedWaterPopulation"].Execute(zone, influenceMap2);
		if (getStairsZones().Contains(zone.ZoneID))
		{
			zone.GetCell(getStairsLocation(zone.ZoneID)).AddObject("StairsUp");
		}
		foreach (Cell item in zone.GetCellsWithObject("StairsUp"))
		{
			EnsureCellReachable(zone, item);
		}
		foreach (Cell item2 in zone.GetCellsWithObject("StairsDown"))
		{
			EnsureCellReachable(zone, item2);
		}
		zone.GetCell(0, 0).AddObject("PaleDirty");
		zone.GetCell(0, 0).AddObject("Finish_TombOfTheEaters_EnterTheTombOfTheEaters");
		return true;
	}
}
