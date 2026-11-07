using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class ChavvahRoots : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		if (Z.Z > 50)
		{
			return ZoneTemplateManager.Templates["MoonStairCaves"].Execute(Z);
		}
		MapBuilder mapBuilder = new MapBuilder();
		mapBuilder.ClearBeforePlace = true;
		mapBuilder.ID = ((Z.Z == 50) ? "ChavvahRootBaseBottom.rpm" : "ChavvahRootBase.rpm");
		mapBuilder.BuildZone(Z);
		GameObject gameObject = Z.FindObject("Dirty");
		GameObject gameObject2 = Z.FindObject("Grassy");
		if (gameObject != null || gameObject2 != null)
		{
			gameObject?.Obliterate();
			gameObject2?.Obliterate();
			Z.GetCell(0, 0).AddObject("CrystalDirty");
		}
		Point2D point = Location2D.Get(40, 12).Point;
		List<Cell> list = new List<Cell>();
		List<Cell> list2 = new List<Cell>();
		List<Cell> list3 = new List<Cell>();
		List<Cell> list4 = new List<Cell>();
		foreach (Cell cell4 in Z.GetCells())
		{
			if (cell4.CosmeticDistanceTo(point) >= 14)
			{
				continue;
			}
			cell4.RemoveObjects((GameObject o) => o.Blueprint != "Chavvah Taproot Floor" && o.Blueprint != "TauSoft").ForEach((GameObject o) =>
			{
				o.Obliterate();
			});
			int num = cell4.GetAdjacentObjectCount("Chavvah Taproot Floor");
			if (cell4.HasObject("Chavvah Taproot Floor"))
			{
				num++;
			}
			if (num == 0)
			{
				cell4.Clear();
				if (Z.Z % 5 != 0)
				{
					cell4.AddObject("Pit");
				}
			}
			else if (num >= 9)
			{
				if (cell4.X <= 40)
				{
					list.Add(cell4);
				}
				else
				{
					list2.Add(cell4);
				}
				cell4.AddObject("Chavvah Taproot");
				list4.Add(cell4);
			}
			else
			{
				list3.Add(cell4);
			}
		}
		List<Cell> list5 = list4.Where((Cell c) => c.GetCardinalAdjacentCellsWhere((Cell cell4) => cell4.HasObject("Chavvah Taproot")).Count < 4).ToList();
		List<Point2D> listToUse = new List<Point2D>();
		Queue<Cell> mandatoryStartCells = new Queue<Cell>();
		Z.GetCellsWithObject("TauSoft").ForEach((Cell t) =>
		{
			t?.ForeachCardinalAdjacentCell((Cell a) =>
			{
				if (a != null)
				{
					mandatoryStartCells.Enqueue(a);
				}
			});
		});
		Stack<Cell> stack = new Stack<Cell>();
		int num2 = Stat.Random(12, 16) + mandatoryStartCells.Count;
		for (int num3 = 0; num3 < num2; num3++)
		{
			Cell cell = null;
			if (mandatoryStartCells.Count > 0)
			{
				cell = mandatoryStartCells.Dequeue();
			}
			else
			{
				cell = list5.GetRandomElement();
				if (stack.Count > 0)
				{
					cell = stack.Pop();
				}
			}
			if (cell == null)
			{
				continue;
			}
			Cell rootStartCell = cell;
			int num4 = Stat.Random(3, 8);
			int num5 = 5;
			while (num4 > 0)
			{
				int radius = Stat.Random(3, 8);
				int count = 16;
				IEnumerable<Point2D> enumerable = from point2D in cell.Pos2D.GetRadialPoints(radius, listToUse)
					where point2D.x >= 0 && point2D.x < 80 && point2D.y >= 0 && point2D.y < 25
					select point2D;
				object obj;
				if (enumerable == null)
				{
					obj = null;
				}
				else
				{
					List<Point2D> list6 = enumerable.ToList();
					obj = ((list6 == null) ? null : list6.ShuffleInPlace()?.Take(count)?.ToList());
				}
				List<Point2D> list7 = (List<Point2D>)obj;
				if (list7 == null)
				{
					break;
				}
				list7.Sort((Point2D a, Point2D b) => b.ManhattanDistance(rootStartCell.Pos2D) - a.ManhattanDistance(rootStartCell.Pos2D));
				Point2D p = list7.FirstOrDefault();
				ZoneBuilderSandbox.TunnelTo(Z, cell.Location, p.location, pathWithNoise: true, 0.2f, 0, (Cell c) =>
				{
					c.RemoveObjects((GameObject o) => o.Blueprint == "Pit");
					if (!c.HasObject("Crystalline Root"))
					{
						c.AddObject("Crystalline Root");
					}
				}, (int x, int y, int c) => Z.GetCell(x, y).HasObject("Chavvah Taproot") ? int.MaxValue : 0);
				cell = Z.GetCell(p);
				if (Stat.Random(1, 100) <= num5)
				{
					stack.Push(cell);
				}
				num4--;
			}
		}
		string rightStair = "Chavvah Spiral StairsUp";
		if (Z.Z % 5 == 0)
		{
			rightStair = "Chavvah Spiral StairsDown";
		}
		Cell cell2 = null;
		Cell cell3 = null;
		if (Z.Z % 5 <= 1)
		{
			ZoneConnection zoneConnection = (from con in Z.EnumerateConnections()
				where con.Object == rightStair
				select con).FirstOrDefault();
			cell2 = ((zoneConnection == null) ? list2.GetRandomElement() : Z.GetCell(zoneConnection.Loc2D));
			if (cell3 != null && cell2 != null)
			{
				ZoneBuilderSandbox.TunnelTo(Z, cell3.Location, cell2.Location, pathWithNoise: true, 0.2f, 200, null, (int x, int y, int c) => Z.GetCell(x, y).HasObject("Chavvah Taproot") ? Stat.Random(0, 50) : 200);
			}
			if (Z.Z != 50)
			{
				cell2?.AddObject(rightStair);
			}
		}
		Z.FireEvent("FirmPitEdges");
		ZoneBuilderSandbox.EnsureAllVoidsConnected(Z, pathWithNoise: true);
		return true;
	}
}
