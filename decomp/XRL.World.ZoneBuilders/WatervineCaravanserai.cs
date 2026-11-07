using System;
using System.Collections.Generic;
using Genkit;
using XRL.Core;
using XRL.Rules;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class WatervineCaravanserai : ZoneBuilderSandbox
{
	public static float[,] WatervineNoise;

	public const int MaxWidth = 1200;

	public const int MaxHeight = 375;

	public const int XBlackoutMin = 42;

	public const int XBlackoutMax = 55;

	public const int YBlackoutMin = 9;

	public const int YBlackoutMax = 16;

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

	public static bool InBlackoutArea(int x, int y)
	{
		if ((x >= 42 && x <= 55) || (y >= 9 && y <= 16))
		{
			return true;
		}
		return false;
	}

	public bool BuildZone(Zone Z)
	{
		if (!Underground)
		{
			Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
			Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("Dirty"));
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
		if (Z.X == 0 && Z.wX > 0 && !ZoneManager.GetObjectTypeForZone(Z.wX - 1, Z.wY, Z.GetZoneWorld()).Contains("TerrainSaltmarsh"))
		{
			for (int k = 0; k < Z.Height; k++)
			{
				for (int l = 0; l < 10; l++)
				{
					array[l, k] = -1.0 + 0.05 * (double)l;
				}
			}
		}
		if (Z.X == 2 && Z.wX < 79)
		{
			string objectTypeForZone = ZoneManager.GetObjectTypeForZone(Z.wX + 1, Z.wY, Z.GetZoneWorld());
			if (!objectTypeForZone.Contains("TerrainSaltmarsh") && !objectTypeForZone.Contains("TerrainWatervine"))
			{
				for (int m = 0; m < Z.Height; m++)
				{
					for (int n = Z.Width - 10; n < Z.Width; n++)
					{
						array[n, m] = -1.0 + 0.05 * (double)(Z.Width - n);
					}
				}
			}
		}
		if (Z.Y == 0 && Z.wY > 0)
		{
			string objectTypeForZone2 = ZoneManager.GetObjectTypeForZone(Z.wX, Z.wY - 1, Z.GetZoneWorld());
			if (!objectTypeForZone2.Contains("TerrainSaltmarsh") && !objectTypeForZone2.Contains("TerrainWatervine"))
			{
				for (int num3 = 0; num3 < 10; num3++)
				{
					for (int num4 = 0; num4 < Z.Width; num4++)
					{
						array[num4, num3] = -1.0 + 0.05 * (double)num3;
					}
				}
			}
		}
		if (Z.Y == 2 && Z.wY < 24)
		{
			string objectTypeForZone3 = ZoneManager.GetObjectTypeForZone(Z.wX, Z.wY + 1, Z.GetZoneWorld());
			if (!objectTypeForZone3.Contains("TerrainSaltmarsh") && !objectTypeForZone3.Contains("TerrainWatervine"))
			{
				for (int num5 = Z.Height - 10; num5 < Z.Height; num5++)
				{
					for (int num6 = 0; num6 < Z.Width; num6++)
					{
						array[num6, num5] = -1.0 + 0.05 * (double)(Z.Height - num5);
					}
				}
			}
		}
		if (Z.X == 0 && Z.wX > 0)
		{
			string objectTypeForZone4 = ZoneManager.GetObjectTypeForZone(Z.wX - 1, Z.wY, Z.GetZoneWorld());
			if (!objectTypeForZone4.Contains("TerrainSaltmarsh") && !objectTypeForZone4.Contains("TerrainWatervine"))
			{
				for (int num7 = 0; num7 < Z.Height; num7++)
				{
					for (int num8 = 0; num8 < 10; num8++)
					{
						array[num8, num7] = -1.0 + 0.1 * (double)num8;
					}
				}
			}
		}
		for (int num9 = 0; num9 < Z.Height; num9++)
		{
			for (int num10 = 0; num10 < Z.Width; num10++)
			{
				if (Underground && !Z.GetCell(num10, num9).IsPassable())
				{
					continue;
				}
				double num11 = (double)WatervineNoise[num10 + num, num9 + num2] + array[num10, num9];
				if (noiseMap.Noise[num10, num9] > 3 && !InBlackoutArea(num10, num9))
				{
					Z.GetCell(num10, num9).AddObject(GameObjectFactory.Factory.CreateObject("SaltyWaterPuddle"));
				}
				else if (num11 >= 0.7 && !InBlackoutArea(num10, num9))
				{
					Z.GetCell(num10, num9).AddObject(GameObjectFactory.Factory.CreateObject("SaltyWaterPuddle"));
				}
				else if (num11 >= 0.6)
				{
					if (Stat.Random(1, 100) <= 80 && !InBlackoutArea(num10, num9))
					{
						Z.GetCell(num10, num9).AddObject(GameObjectFactory.Factory.CreateObject("Watervine"));
					}
				}
				else
				{
					Z.GetCell(num10, num9).SetReachable(State: true);
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
