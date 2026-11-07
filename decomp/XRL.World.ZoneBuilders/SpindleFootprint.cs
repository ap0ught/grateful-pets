using System;
using Genkit;

namespace XRL.World.ZoneBuilders;

public class SpindleFootprint : ZoneBuilderSandbox
{
	public enum FootprintMode
	{
		Spindle,
		Foundation
	}

	public string wallMaterial = "EbonFulcrete";

	public bool BuildZone(Zone Z, FootprintMode mode = FootprintMode.Spindle)
	{
		new FastNoise();
		if (mode == FootprintMode.Spindle)
		{
			wallMaterial = "Burnished Azzurum";
		}
		if (mode == FootprintMode.Foundation)
		{
			wallMaterial = "EbonFulcrete";
		}
		Action<Cell> after = (Cell c) =>
		{
			c.AddObject("EnsureVoidBlocker");
		};
		Action<Rect2D, string> action = (Rect2D r, string d) =>
		{
			if (mode != FootprintMode.Foundation)
			{
				r.ForEachLocation((Location2D l) =>
				{
					Z.GetCell(l).ClearWalls();
				});
				switch (d)
				{
				case "N":
				case "S":
				{
					for (int num2 = r.x1; num2 <= r.x2; num2++)
					{
						Z.GetCell(num2, (d == "N") ? r.y1 : r.y2).AddObject("SpindleGlassWall");
						Z.GetCell(num2, (d == "N") ? r.y2 : r.y1).AddObject("SpindleLight");
					}
					break;
				}
				case "E":
				case "W":
				{
					for (int num = r.y1; num <= r.y2; num++)
					{
						Z.GetCell((d == "W") ? r.x1 : r.x2, num).AddObject("SpindleLight");
						Z.GetCell((d == "E") ? r.x2 : r.x1, num).AddObject("SpindleGlassWall");
					}
					break;
				}
				}
			}
		};
		if (Z.X == 0 && Z.Y == 0)
		{
			Z.ClearBoxWith(new Rect2D(76, 22, 79, 24), wallMaterial, after);
			Z.GetCell(76, 22).ClearWalls();
		}
		if (Z.X == 1 && Z.Y == 0)
		{
			if (Z.Z != 9)
			{
				Z.ClearBoxWith(new Rect2D(26, 21, 53, 21), wallMaterial, after);
			}
			Z.ClearBoxWith(new Rect2D(0, 22, 79, 24), wallMaterial, after);
			action(new Rect2D(73, 22, 74, 24), "N");
			action(new Rect2D(7, 22, 8, 24), "N");
			action(new Rect2D(17, 22, 18, 24), "N");
			if (Z.Z != 9)
			{
				action(new Rect2D(27, 21, 28, 24), "N");
			}
			if (Z.Z != 9)
			{
				action(new Rect2D(51, 21, 52, 24), "N");
			}
			action(new Rect2D(63, 22, 64, 24), "N");
		}
		if (Z.X == 2 && Z.Y == 0)
		{
			Z.ClearBoxWith(new Rect2D(0, 22, 3, 24), wallMaterial, after);
			Z.GetCell(3, 22).ClearWalls();
		}
		if (Z.X == 0 && Z.Y == 1)
		{
			Z.ClearBoxWith(new Rect2D(76, 0, 79, 24), wallMaterial, after);
			Z.ClearBoxWith(new Rect2D(75, 7, 75, 18), wallMaterial, after);
			action(new Rect2D(76, 3, 79, 4), "W");
			action(new Rect2D(75, 9, 79, 10), "W");
			action(new Rect2D(75, 15, 79, 16), "W");
			action(new Rect2D(76, 20, 79, 21), "W");
		}
		if (Z.X == 2 && Z.Y == 1)
		{
			Z.ClearBoxWith(new Rect2D(0, 0, 3, 24), wallMaterial, after);
			Z.ClearBoxWith(new Rect2D(4, 7, 4, 18), wallMaterial, after);
			action(new Rect2D(0, 3, 3, 4), "E");
			action(new Rect2D(0, 9, 4, 10), "E");
			action(new Rect2D(0, 15, 4, 16), "E");
			action(new Rect2D(0, 20, 3, 21), "E");
		}
		if (Z.X == 0 && Z.Y == 2)
		{
			Z.ClearBoxWith(new Rect2D(76, 0, 79, 2), wallMaterial, after);
			Z.GetCell(76, 2).ClearWalls();
		}
		if (Z.X == 1 && Z.Y == 2)
		{
			if (Z.Z != 9)
			{
				Z.ClearBoxWith(new Rect2D(26, 3, 53, 3), wallMaterial, after);
			}
			Z.ClearBoxWith(new Rect2D(0, 0, 79, 2), wallMaterial, after);
			action(new Rect2D(73, 0, 74, 1), "S");
			action(new Rect2D(7, 0, 8, 1), "S");
			action(new Rect2D(17, 0, 18, 3), "S");
			if (Z.Z != 9)
			{
				action(new Rect2D(27, 0, 28, 3), "S");
			}
			if (Z.Z != 9)
			{
				action(new Rect2D(51, 0, 52, 3), "S");
			}
			action(new Rect2D(63, 0, 64, 3), "S");
		}
		if (Z.X == 2 && Z.Y == 2)
		{
			Z.ClearBoxWith(new Rect2D(0, 0, 3, 2), wallMaterial, after);
			Z.GetCell(3, 2).ClearWalls();
		}
		return true;
	}
}
