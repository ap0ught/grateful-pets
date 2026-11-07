using System.Collections.Generic;
using XRL.Rules;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class WallOutcrop : ZoneBuilderSandbox
{
	public string Blueprint;

	public bool BuildZone(Zone Z)
	{
		Z.GetCell(0, 0).RequireObject("DaylightWidget");
		List<NoiseMapNode> list = new List<NoiseMapNode>();
		for (int i = 0; i < 20; i++)
		{
			list.Add(new NoiseMapNode(10, i, -10));
			list.Add(new NoiseMapNode(i, 10, -10));
		}
		NoiseMap noiseMap = new NoiseMap(20, 20, 10, 3, 3, 2, 80, 80, 4, 3, 0, 1, list);
		NoiseMap noiseMap2 = new NoiseMap(Z.Width, Z.Height, 10, 3, 3, 2, 20, 20, 4, 3, 0, 1, list);
		int num = Stat.Random(1, 59);
		int num2 = Stat.Random(1, 3);
		bool flag = false;
		foreach (ZoneConnection zoneConnection in The.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (zoneConnection.Type == "StairsDown")
			{
				num = zoneConnection.X;
				num2 = zoneConnection.Y;
				flag = true;
				break;
			}
		}
		for (int j = 0; j < 20; j++)
		{
			for (int k = 0; k < 20; k++)
			{
				if (noiseMap.Noise[k, j] > 1)
				{
					Z.GetCell(k + num, j + num2)?.ClearAndAddObject(Blueprint);
				}
			}
		}
		for (int l = 0; l < Z.Height; l++)
		{
			for (int m = 0; m < Z.Width; m++)
			{
				if (noiseMap2.Noise[m, l] > 1)
				{
					Z.GetCell(m, l).ClearAndAddObject(Blueprint);
				}
			}
		}
		if (flag)
		{
			Cell cell = Z.GetCell(num, num2);
			cell.Clear();
			cell.RequireObject("StairsDown");
			EnsureCellReachable(Z, cell);
			BuildReachableMap(Z, num, num2);
			return true;
		}
		BuildReachableMap(Z, num, num2);
		for (int n = 0; n < 11; n++)
		{
			for (int num3 = 10 - n; num3 <= 10 + n; num3++)
			{
				for (int num4 = 10 - n; num4 <= 10 + n; num4++)
				{
					Cell cell2 = Z.GetCell(num4 + num, num3 + num2);
					if (cell2.IsReachable() && cell2.IsEmpty())
					{
						cell2.AddObject("StairsDown");
						return true;
					}
				}
			}
		}
		MetricsManager.LogError("Failed placing stairs down, placing anywhere in zone");
		ZoneBuilderSandbox.PlaceObject("StairsDown", Z);
		return true;
	}

	public void BuildReachableMap(Zone Z, int xp, int yp)
	{
		for (int i = 0; i < 11; i++)
		{
			for (int j = 10 - i; j <= 10 + i; j++)
			{
				for (int k = 10 - i; k <= 10 + i; k++)
				{
					Cell cell = Z.GetCell(k + xp, j + yp);
					if (cell != null && cell.IsEmpty())
					{
						Z.ClearReachableMap(bValue: true);
						if (Z.BuildReachableMap(k, j) > 400)
						{
							return;
						}
					}
				}
			}
		}
		for (int l = 0; l < Z.Height; l++)
		{
			for (int m = 0; m < Z.Width; m++)
			{
				if (Z.GetCell(m, l).IsEmpty())
				{
					Z.ClearReachableMap(bValue: true);
					if (Z.BuildReachableMap(m, l) > 400)
					{
						return;
					}
				}
			}
		}
	}
}
