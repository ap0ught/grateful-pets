using System.Collections.Generic;
using Wintellect.PowerCollections;
using XRL.Rules;
using XRL.World.AI.Pathfinding;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class ConveyorBelt
{
	public int x1;

	public int y1;

	public int x2;

	public int y2;

	public string DriveDirection = "";

	public string StepDirection = "";

	public bool BuildZone(Zone Z)
	{
		FindPath findPath = new FindPath();
		if (StepDirection == "")
		{
			findPath = new FindPath(Z, x1, y1, Z, x2, y2, PathGlobal: false, PathUnlimited: true, null, AddNoise: false, CardinalOnly: true);
		}
		if (StepDirection != "")
		{
			int x = x1;
			int y = y1;
			Cell cell = Z.GetCell(x1, y1);
			while ((x != x2 || y != y2) && cell != null)
			{
				findPath.Steps.Add(cell);
				Cell cell2 = null;
				for (int i = 0; i < StepDirection.Length; i++)
				{
					cell2 = cell.GetCellFromDirection(StepDirection[i].ToString());
					if (cell2 != null && !cell2.HasWall() && !cell2.HasObjectWithPart("ConveyorBelt"))
					{
						break;
					}
					cell2 = null;
				}
				cell = cell2;
				if (cell != null)
				{
					x = cell.X;
					y = cell.Y;
				}
			}
			findPath.Found = true;
		}
		if (!findPath.Usable)
		{
			return false;
		}
		int num = 0;
		string text = "N";
		foreach (Cell step in findPath.Steps)
		{
			step.Clear();
			GameObject gameObject = GameObjectFactory.Factory.CreateObject("ConveyorPad");
			ConveyorPad part = gameObject.GetPart<ConveyorPad>();
			if (num < findPath.Steps.Count - 1)
			{
				part.Direction = step.GetDirectionFromCell(findPath.Steps[num + 1]);
				part.Connections = part.Direction;
				text = part.Direction;
			}
			else
			{
				part.Direction = text;
				part.Connections = text;
			}
			step.AddObject(gameObject);
			num++;
		}
		if (DriveDirection != "")
		{
			Z.GetCell(x1, y1).GetCellFromDirection(DriveDirection).AddObject("ConveyorDrive");
			return true;
		}
		List<Cell> cardinalAdjacentCells = Z.GetCell(x1, y1).GetCardinalAdjacentCells();
		Algorithms.RandomShuffleInPlace(cardinalAdjacentCells, Stat.Rand);
		foreach (Cell item in cardinalAdjacentCells)
		{
			if (item.HasObjectWithPart("ConveyorDriver"))
			{
				return true;
			}
		}
		foreach (Cell item2 in cardinalAdjacentCells)
		{
			if (!item2.IsEmpty() || item2.HasObjectWithPart("ConveyorPad"))
			{
				continue;
			}
			int num2 = 0;
			foreach (Cell adjacentCell in item2.GetAdjacentCells())
			{
				if (adjacentCell.HasObjectWithPart("ConveyorPad"))
				{
					num2++;
				}
			}
			if (num2 == 1)
			{
				item2.Clear();
				item2.AddObject(GameObjectFactory.Factory.CreateObject("ConveyorDrive"));
				return true;
			}
		}
		return true;
	}
}
