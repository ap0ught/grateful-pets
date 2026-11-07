using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class OverlandWater
{
	public bool Underground;

	public string ZoneIDFromXY(string World, int xp, int yp)
	{
		int parasangX = (int)Math.Floor((float)xp / 3f);
		int parasangY = (int)Math.Floor((float)yp / 3f);
		return ZoneID.Assemble(World, parasangX, parasangY, xp % 3, yp % 3, 10);
	}

	public bool[,] GetWatermap(int Wx, int Wy)
	{
		if (Wy < 0)
		{
			bool[,] array = new bool[3, 3];
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					array[j, i] = true;
				}
			}
			return array;
		}
		bool[,] array2 = new bool[3, 3];
		Zone zone = XRLCore.Core.Game.ZoneManager.GetZone("JoppaWorld");
		for (int k = -1; k <= 1; k++)
		{
			for (int l = -1; l <= 1; l++)
			{
				if (Wy + k >= 0)
				{
					Cell cell = zone.GetCell(Wx + l, Wy + k);
					array2[l + 1, k + 1] = cell.HasObjectWithPropertyOrTagEqualToValue("Terrain", "Water");
				}
				else
				{
					array2[l + 1, k + 1] = true;
				}
			}
		}
		bool[,] array3 = new bool[3, 3];
		array3[0, 0] = array2[0, 0] && array2[1, 0] && array2[0, 1];
		array3[1, 0] = array2[1, 0];
		array3[2, 0] = array2[1, 0] && array2[2, 0] && array2[2, 1];
		array3[0, 1] = array2[0, 1];
		array3[1, 1] = true;
		array3[2, 1] = array2[2, 1];
		array3[0, 2] = array2[0, 1] && array2[0, 2] && array2[1, 2];
		array3[1, 2] = array2[1, 2];
		array3[2, 2] = array2[2, 1] && array2[2, 2] && array2[1, 2];
		return array3;
	}

	public bool BuildZone(Zone Z)
	{
		if (Z._ZoneID.Contains("JoppaWorld"))
		{
			int wX = Z.wX;
			int wY = Z.wY;
			bool[,] array = new bool[9, 9];
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					bool[,] watermap = GetWatermap(wX + i, wY + j);
					for (int k = 0; k < 3; k++)
					{
						for (int l = 0; l < 3; l++)
						{
							array[k + (i + 1) * 3, l + (j + 1) * 3] = watermap[k, l];
						}
					}
				}
			}
			bool[,] array2 = new bool[3, 3];
			for (int m = 0; m < 3; m++)
			{
				for (int n = 0; n < 3; n++)
				{
					array2[n, m] = false;
				}
			}
			wX = Z.X + 3;
			wY = Z.Y + 3;
			if (array[wX, wY])
			{
				if (array[wX - 1, wY - 1] && array[wX - 1, wY] && array[wX, wY - 1])
				{
					array2[0, 0] = true;
				}
				if (array[wX, wY - 1])
				{
					array2[1, 0] = true;
				}
				if (array[wX + 1, wY - 1] && array[wX + 1, wY] && array[wX, wY - 1])
				{
					array2[2, 0] = true;
				}
				if (array[wX - 1, wY])
				{
					array2[0, 1] = true;
				}
				array2[1, 1] = true;
				if (array[wX + 1, wY])
				{
					array2[2, 1] = true;
				}
				if (array[wX - 1, wY + 1] && array[wX - 1, wY] && array[wX, wY + 1])
				{
					array2[0, 2] = true;
				}
				if (array[wX, wY + 1])
				{
					array2[1, 2] = true;
				}
				if (array[wX + 1, wY + 1] && array[wX + 1, wY] && array[wX, wY + 1])
				{
					array2[2, 2] = true;
				}
				for (int num = 0; num < 3; num++)
				{
					for (int num2 = 0; num2 < 3; num2++)
					{
						int num3 = 0;
						int num4 = 0;
						int num5 = 0;
						int num6 = 0;
						if (num == 0)
						{
							num3 = 0;
							num4 = 26;
						}
						if (num == 1)
						{
							num3 = 27;
							num4 = 54;
						}
						if (num == 2)
						{
							num3 = 55;
							num4 = 79;
						}
						if (num2 == 0)
						{
							num5 = 0;
							num6 = 7;
						}
						if (num2 == 1)
						{
							num5 = 8;
							num6 = 17;
						}
						if (num2 == 2)
						{
							num5 = 17;
							num6 = 24;
						}
						if (!array2[num, num2] || !Z.GetCell(num, num2).IsPassable())
						{
							continue;
						}
						for (int num7 = num3; num7 <= num4; num7++)
						{
							for (int num8 = num5; num8 <= num6; num8++)
							{
								Z.GetCell(num7, num8).AddObject("SaltyWaterExtraDeepPool");
							}
						}
					}
				}
			}
		}
		List<NoiseMapNode> extraNodes = new List<NoiseMapNode>();
		NoiseMap noiseMap;
		if (!Underground)
		{
			noiseMap = new NoiseMap(Z.Width, Z.Height, 10, 1, 1, Stat.Random(16, 36), 40, 225, 0, 10, 0, 1, extraNodes, 5);
			for (int num9 = 0; num9 < Z.Height; num9++)
			{
				for (int num10 = 0; num10 < Z.Width; num10++)
				{
					if ((double)noiseMap.Noise[num10, num9] > 0.01)
					{
						Z.GetCell(num10, num9).Clear();
					}
				}
			}
		}
		noiseMap = new NoiseMap(Z.Width, Z.Height, 10, 1, 1, Stat.Random(6, 26), 40, 225, 0, 10, 0, 1, extraNodes, 5);
		for (int num11 = 0; num11 < Z.Height; num11++)
		{
			for (int num12 = 0; num12 < Z.Width; num12++)
			{
				if ((double)noiseMap.Noise[num12, num11] > 0.01)
				{
					Z.GetCell(num12, num11).AddObject("SaltyWaterDeepPool");
				}
			}
		}
		Z.GetCell(0, 0).AddObject("Grassy");
		return true;
	}
}
