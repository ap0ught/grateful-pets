using System;
using System.Collections.Generic;
using Genkit;
using XRL.Core;
using XRL.Rules;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class Stillvine : ZoneBuilderSandbox
{
	public static float[,] WatervineNoise;

	public const int MaxWidth = 1200;

	public const int MaxHeight = 375;

	public bool Underground;

	public static void Save(SerializationWriter Writer)
	{
		Writer.Write(0);
	}

	public static void Load(SerializationReader Reader)
	{
		if (Reader.ReadInt32() == 0)
		{
			WatervineNoise = null;
			return;
		}
		WatervineNoise = null;
		for (int i = 0; i < 375; i++)
		{
			for (int j = 0; j < 1200; j++)
			{
				Reader.ReadDouble();
			}
		}
	}

	public bool BuildZone(Zone Z)
	{
		if (!Underground)
		{
			Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
			Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("Dirty"));
		}
		else
		{
			Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("PaleDirty"));
		}
		List<NoiseMapNode> list = new List<NoiseMapNode>();
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			list.Add(new NoiseMapNode(zoneConnection.X, zoneConnection.Y));
		}
		NoiseMap noiseMap = new NoiseMap(Z.Width, Z.Height, 10, 1, 1, Stat.Random(30, 60), Stat.Random(50, 70), Stat.Random(125, 135), 0, 10, 0, 1, list, 5);
		if (WatervineNoise == null)
		{
			List<PerlinNoise2Df> list2 = null;
			Random seededRand = GetSeededRand("WatervineNoise");
			list2 = new List<PerlinNoise2Df>();
			list2.Add(new PerlinNoise2Df(8, 0.86f, seededRand));
			list2.Add(new PerlinNoise2Df(32, 0.72f, seededRand));
			list2.Add(new PerlinNoise2Df(64, 0.6f, seededRand));
			list2.Add(new PerlinNoise2Df(128, 0.48f, seededRand));
			list2.Add(new PerlinNoise2Df(300, 0.36f, seededRand));
			list2.Add(new PerlinNoise2Df(600, 0.12f, seededRand));
			list2.Add(new PerlinNoise2Df(1200, 0.06f, seededRand));
			WatervineNoise = PerlinNoise2Df.sumNoiseFunctions(1200, 375, 0, 0, list2, 0.5f);
			WatervineNoise = PerlinNoise2Df.Smooth(WatervineNoise, 1200, 375, 1);
		}
		int num = Z.wX * 240 + Z.X * 80;
		int num2 = Z.wY * 75 + Z.Y * 25;
		num %= 1200;
		num2 %= 375;
		double[,] array = new double[Z.Width, Z.Height];
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				array[j, i] = 0.0;
			}
		}
		for (int k = 0; k < Z.Height; k++)
		{
			for (int l = 0; l < Z.Width; l++)
			{
				if (Underground && !Z.GetCell(l, k).IsPassable())
				{
					continue;
				}
				double num3 = (double)WatervineNoise[l + num, k + num2] + array[l, k];
				if (noiseMap.Noise[l, k] > 3)
				{
					Z.GetCell(l, k).AddObject(GameObjectFactory.Factory.CreateObject("SaltyWaterPuddle"));
				}
				else if (num3 >= 0.7)
				{
					Z.GetCell(l, k).AddObject(GameObjectFactory.Factory.CreateObject("SaltyWaterPuddle"));
				}
				else if (num3 >= 0.6)
				{
					if (Stat.Random(1, 100) <= 80)
					{
						Z.GetCell(l, k).AddObject(GameObjectFactory.Factory.CreateObject("Stillvine"));
					}
				}
				else
				{
					Z.GetCell(l, k).SetReachable(State: true);
				}
			}
		}
		if (!Underground)
		{
			Z.ClearReachableMap();
			if (Z.BuildReachableMap(0, 0) < 400)
			{
				return false;
			}
		}
		return true;
	}
}
