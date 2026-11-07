using System.Collections.Generic;
using Genkit;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class Weald
{
	private static List<PerlinNoise2D> NoiseFunctions;

	private static double[,] WealdNoise;

	public bool BuildZone(Zone Z)
	{
		if (WealdNoise == null)
		{
			NoiseFunctions = new List<PerlinNoise2D>();
			NoiseFunctions.Add(new PerlinNoise2D(1, 3f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(4, 2f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(8, 1f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(16, 2f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(32, 3f, Stat.Rand));
			NoiseFunctions.Add(new PerlinNoise2D(128, 4f, Stat.Rand));
			WealdNoise = PerlinNoise2D.sumNoiseFunctions(1200, 375, 0, 0, NoiseFunctions);
		}
		int num = Z.wX * 240 + Z.X * 80;
		int num2 = Z.wY * 75 + Z.Y * 25;
		num %= 1200;
		num2 %= 375;
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				double num3 = WealdNoise[j + num, i + num2];
				int chance = 0;
				int chance2 = 0;
				int chance3 = 0;
				if (num3 >= 0.8)
				{
					chance = 20;
					chance2 = 80;
					chance3 = 5;
				}
				else if (num3 >= 0.7)
				{
					chance = 3;
					chance2 = 60;
					chance3 = 3;
				}
				else if (num3 >= 0.5)
				{
					chance = 1;
					chance2 = 1;
					chance3 = 30;
				}
				if (chance.in100())
				{
					Z.GetCell(j, i).AddObject("Starapple Tree");
				}
				else if (chance2.in100())
				{
					Z.GetCell(j, i).AddObject("Witchwood Tree");
				}
				else if (chance3.in100())
				{
					Z.GetCell(j, i).AddObject("Yuckwheat");
				}
			}
		}
		Z.GetCell(0, 0).AddObject("DaylightWidget");
		Z.GetCell(0, 0).AddObject("Grassy");
		Z.ClearReachableMap();
		if (Z.BuildReachableMap(0, 0) < 400)
		{
			return false;
		}
		return true;
	}
}
