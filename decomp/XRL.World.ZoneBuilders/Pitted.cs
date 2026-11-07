using System.Collections.Generic;
using Genkit;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

/// <summary>
/// Should typically be added as a builder to zones in a single column on Z level 10 through 13 of an area that will be pitted.
/// </summary>
public class Pitted : ZoneBuilderSandbox
{
	public int MinWells = 3;

	public int MaxWells = 6;

	public int MinRadius = 3;

	public int MaxRadius = 10;

	public int XMargin = 4;

	public int YMargin = 2;

	public int PitTop = 10;

	public int PitDepth = 2;

	public bool PitDetailsRandom;

	public string Liquid = "SaltyWaterExtraDeepPool";

	public string CenterConnectionType;

	public bool Lazy;

	public bool BuildZone(Zone Z)
	{
		return BuildZone(Z, new List<Location2D>(), new List<Location2D>());
	}

	public bool BuildZone(Zone Z, List<Location2D> pitCellsOut, List<Location2D> centerCellsOut)
	{
		return BuildPits(Z, MinWells, MaxWells, MinRadius, MaxRadius, XMargin, YMargin, pitCellsOut, centerCellsOut, PitTop, PitDepth, Liquid, PitDetailsRandom, null, "Pit", null, Lazy);
	}

	public static bool BuildPits(Zone Z, int MinWells, int MaxWells, int MinRadius, int MaxRadius, int XMargin, int YMargin, List<Location2D> pitCellsOut, List<Location2D> centerCellsOut, int PitTop = 10, int PitDepth = 2, string Liquid = "SaltyWaterExtraDeepPool", bool PitDetailsRandom = false, string CenterConnectionType = null, string PitObject = "Pit", IList<Location2D> Avoid = null, bool Lazy = false)
	{
		if (Z.Z == 10)
		{
			Z.GetCell(0, 0).RequireObject("DaylightWidget");
		}
		int num = ZoneBuilderSandbox.GetOracleIntColumn(Z, "nwells", MinWells, MaxWells);
		if (PitDetailsRandom)
		{
			num = Stat.Random(MinWells, MaxWells);
		}
		List<(Location2D, int)> list = new List<(Location2D, int)>();
		for (int i = 0; i < num; i++)
		{
			int num2 = ZoneBuilderSandbox.GetOracleIntColumn(Z, "well" + i + "_r", MinRadius, MaxRadius);
			int x = ZoneBuilderSandbox.GetOracleIntColumn(Z, "well" + i + "_x", XMargin + num2 * 2, 80 - XMargin - num2 * 2);
			int y = ZoneBuilderSandbox.GetOracleIntColumn(Z, "well" + i + "_y", YMargin + num2, 24 - YMargin - num2);
			if (PitDetailsRandom)
			{
				num2 = Stat.Random(MinRadius, MaxRadius);
				x = Stat.Random(XMargin + num2 * 2, 80 - XMargin - num2 * 2);
				y = Stat.Random(YMargin + num2, 24 - YMargin - num2);
			}
			if (!Avoid.IsNullOrEmpty())
			{
				bool flag = false;
				int j = 0;
				for (int count = Avoid.Count; j < count; j++)
				{
					if (Avoid[j].Distance(x, y) <= MaxRadius)
					{
						flag = true;
						break;
					}
				}
				if (flag && num < MaxWells * 5)
				{
					num++;
					continue;
				}
			}
			Cell cell = Z.GetCell(x, y);
			centerCellsOut?.Add(cell.Location);
			list.Add((cell.Location, num2));
			if (!CenterConnectionType.IsNullOrEmpty())
			{
				Z.AddZoneConnection("d", x, y, CenterConnectionType, null);
			}
		}
		foreach (var item2 in list)
		{
			int x2 = item2.Item1.X;
			int y2 = item2.Item1.Y;
			int item = item2.Item2;
			List<Cell> list2 = new List<Cell>();
			for (int k = -(item * 2); k <= item * 2; k++)
			{
				for (int l = -item; l <= item; l++)
				{
					Cell cell2 = Z.GetCell(x2 + k, y2 + l);
					if (cell2 != null && cell2.CosmeticDistanceTo(x2, y2) <= item - (Z.Z - PitTop))
					{
						cell2.Clear();
						cell2.AddObject("FlyingWhitelistArea");
						list2.Add(cell2);
						if (Z.Z <= PitTop + PitDepth)
						{
							GameObject gameObject = GameObjectFactory.Factory.CreateObject(PitObject);
							gameObject.GetPart<XRL.World.Parts.StairsDown>().ConnectLanding = false;
							cell2.AddObject(gameObject);
							pitCellsOut.Add(cell2.Location);
						}
						else
						{
							cell2.AddObject(Liquid);
						}
					}
				}
			}
			for (int m = -MaxRadius; m <= MaxRadius; m++)
			{
				for (int n = -(MaxRadius * 2); n <= MaxRadius * 2; n++)
				{
					Cell cell3 = Z.GetCell(x2 + n, y2 + m);
					if (cell3 != null && cell3.CosmeticDistanceTo(x2, y2) <= MaxRadius)
					{
						cell3.AddObject("StairBlocker");
						cell3.AddObject("InfluenceMapBlocker");
					}
				}
			}
			if (pitCellsOut.Count == 0)
			{
				pitCellsOut.Add(item2.Item1);
			}
		}
		if (Lazy)
		{
			Zone.ObjectEnumerator enumerator2 = Z.IterateObjects().GetEnumerator();
			while (enumerator2.MoveNext())
			{
				GameObject current2 = enumerator2.Current;
				if (current2.TryGetPart<PitMaterial>(out var Part))
				{
					Part.Lazy = true;
				}
				if (current2.TryGetPart<ChasmMaterial>(out var _))
				{
					Part.Lazy = true;
				}
			}
		}
		return true;
	}
}
