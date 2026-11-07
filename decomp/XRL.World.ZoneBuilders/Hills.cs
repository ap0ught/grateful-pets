using System;
using System.Collections.Generic;
using Genkit;
using XRL.Core;

namespace XRL.World.ZoneBuilders;

public class Hills : ZoneBuilderSandbox
{
	public static float[,] HillsNoise;

	private const int MaxWidth = 1200;

	private const int MaxHeight = 375;

	public static void Save(SerializationWriter Writer)
	{
		Writer.Write(0);
	}

	public static void Load(SerializationReader Reader)
	{
		if (Reader.ReadInt32() == 0)
		{
			HillsNoise = null;
			return;
		}
		HillsNoise = null;
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
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		Z.ClearReachableMap();
		if (HillsNoise == null)
		{
			Random seededRand = GetSeededRand("HillsNoise");
			List<PerlinNoise2Df> list = null;
			list = new List<PerlinNoise2Df>();
			list.Add(new PerlinNoise2Df(2, 1f, seededRand));
			list.Add(new PerlinNoise2Df(4, 0.9f, seededRand));
			list.Add(new PerlinNoise2Df(8, 0.86f, seededRand));
			list.Add(new PerlinNoise2Df(32, 0.72f, seededRand));
			list.Add(new PerlinNoise2Df(64, 0.6f, seededRand));
			list.Add(new PerlinNoise2Df(128, 0.48f, seededRand));
			list.Add(new PerlinNoise2Df(300, 0.36f, seededRand));
			list.Add(new PerlinNoise2Df(600, 0.12f, seededRand));
			list.Add(new PerlinNoise2Df(1200, 0.06f, seededRand));
			HillsNoise = PerlinNoise2Df.sumNoiseFunctions(1200, 375, 0, 0, list, 0.5f);
			HillsNoise = PerlinNoise2Df.Smooth(HillsNoise, 1200, 375, 1);
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
		if (Z.X == 0 && Z.wX > 0)
		{
			string objectTypeForZone = ZoneManager.GetObjectTypeForZone(Z.wX - 1, Z.wY, Z.GetZoneWorld());
			if (!objectTypeForZone.Contains("TerrainJoppaRedrockChannel") && !objectTypeForZone.Contains("Canyon") && !objectTypeForZone.Contains("Hills") && !objectTypeForZone.Contains("Mountains") && !objectTypeForZone.Contains("Asphalt"))
			{
				for (int k = 0; k < Z.Height; k++)
				{
					for (int l = 0; l < 10; l++)
					{
						array[l, k] = -1.0 + 0.1 * (double)l;
					}
				}
			}
		}
		if (Z.X == 2 && Z.wX < 79)
		{
			string objectTypeForZone2 = ZoneManager.GetObjectTypeForZone(Z.wX + 1, Z.wY, Z.GetZoneWorld());
			if (!objectTypeForZone2.Contains("TerrainJoppaRedrockChannel") && !objectTypeForZone2.Contains("Canyon") && !objectTypeForZone2.Contains("Hills") && !objectTypeForZone2.Contains("Mountains") && !objectTypeForZone2.Contains("Asphalt"))
			{
				for (int m = 0; m < Z.Height; m++)
				{
					for (int n = Z.Width - 10; n < Z.Width; n++)
					{
						array[n, m] = -1.0 + 0.1 * (double)(Z.Width - n);
					}
				}
			}
		}
		if (Z.Y == 0 && Z.wY > 0)
		{
			string objectTypeForZone3 = ZoneManager.GetObjectTypeForZone(Z.wX, Z.wY - 1, Z.GetZoneWorld());
			if (!objectTypeForZone3.Contains("TerrainJoppaRedrockChannel") && !objectTypeForZone3.Contains("Canyon") && !objectTypeForZone3.Contains("Hills") && !objectTypeForZone3.Contains("Mountains") && !objectTypeForZone3.Contains("Asphalt"))
			{
				for (int num3 = 0; num3 < 10; num3++)
				{
					for (int num4 = 0; num4 < Z.Width; num4++)
					{
						array[num4, num3] = -1.0 + 0.1 * (double)num3;
					}
				}
			}
		}
		if (Z.Y == 2 && Z.wY < 24)
		{
			string objectTypeForZone4 = ZoneManager.GetObjectTypeForZone(Z.wX, Z.wY + 1, Z.GetZoneWorld());
			if (!objectTypeForZone4.Contains("TerrainJoppaRedrockChannel") && !objectTypeForZone4.Contains("Canyon") && !objectTypeForZone4.Contains("Hills") && !objectTypeForZone4.Contains("Mountains") && !objectTypeForZone4.Contains("Asphalt"))
			{
				for (int num5 = Z.Height - 10; num5 < Z.Height; num5++)
				{
					for (int num6 = 0; num6 < Z.Width; num6++)
					{
						array[num6, num5] = -1.0 + 0.1 * (double)(Z.Height - num5);
					}
				}
			}
		}
		string objectBlueprint = "Shale";
		string objectTypeForZone5 = ZoneManager.GetObjectTypeForZone(Z.wX, Z.wY, Z.GetZoneWorld());
		float num7 = 0.6f;
		if (objectTypeForZone5.Contains("Mountains"))
		{
			objectBlueprint = "Limestone";
			num7 = 0.5f;
		}
		if (objectTypeForZone5.Contains("Hills"))
		{
			objectBlueprint = "Marl";
			num7 = 0.55f;
		}
		int num8 = 0;
		for (int num9 = 0; num9 < Z.Height; num9++)
		{
			for (int num10 = 0; num10 < Z.Width; num10++)
			{
				if ((double)HillsNoise[num10 + num, num9 + num2] + array[num10, num9] >= (double)num7)
				{
					num8++;
					Z.GetCell(num10, num9).AddObject(GameObjectFactory.Factory.CreateObject(objectBlueprint));
				}
			}
		}
		if (objectTypeForZone5.Contains("Mountains") || objectTypeForZone5.Contains("Hills"))
		{
			for (int num11 = 4; num11 < 16; num11++)
			{
				Cell cell = Z.GetRandomCell();
				for (int num12 = 3; num12 < 16; num12++)
				{
					cell.Clear();
					cell = cell.GetRandomLocalAdjacentCell();
					if (cell == null)
					{
						break;
					}
				}
			}
			Z.BuildReachabilityFromEdges();
		}
		else
		{
			Z.ClearReachableMap();
			foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
			{
				if (!Z.IsReachable(zoneConnection.X, zoneConnection.Y))
				{
					Z.BuildReachableMap(zoneConnection.X, zoneConnection.Y, bClearFirst: false);
				}
			}
		}
		return true;
	}
}
