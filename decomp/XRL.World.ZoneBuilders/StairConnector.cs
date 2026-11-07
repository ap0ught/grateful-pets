using XRL.World.AI.Pathfinding;

namespace XRL.World.ZoneBuilders;

public class StairConnector
{
	public bool BuildZone(Zone Z)
	{
		GameObject gameObject = Z.FindObject("StairsUp");
		GameObject gameObject2 = Z.FindObject("StairsDown");
		if (gameObject == null || gameObject2 == null || !gameObject.InACell() || !gameObject2.InACell())
		{
			return true;
		}
		GameObject looker = GameObject.Create("Drillbot");
		FindPath findPath = new FindPath(Z, gameObject.CurrentCell.X, gameObject.CurrentCell.Y, Z, gameObject2.CurrentCell.X, gameObject2.CurrentCell.Y, PathGlobal: false, PathUnlimited: true, looker, AddNoise: true);
		if (!findPath.Usable)
		{
			return false;
		}
		foreach (Cell step in findPath.Steps)
		{
			step.ClearWalls();
			step.ClearImpassableObjects(The.Player);
		}
		return true;
	}
}
