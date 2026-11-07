using System;

namespace Genkit;

[Serializable]
public class CellNavigationValue : IComparable
{
	public PathfinderNode cCell;

	public PathfinderNode Parent;

	public string Direction;

	public int StartToNode;

	public int EstimatedNodeToGoal;

	public int EstimatedTotalCost;

	public CellNavigationValue(int g, int h, PathfinderNode _Cell, PathfinderNode pParent, string pDirection)
	{
		StartToNode = g;
		cCell = _Cell;
		EstimatedNodeToGoal = h;
		EstimatedTotalCost = g + h;
		Parent = pParent;
		Direction = pDirection;
	}

	public CellNavigationValue Set(int g, int h, PathfinderNode _Cell, PathfinderNode pParent, string pDirection)
	{
		StartToNode = g;
		cCell = _Cell;
		EstimatedNodeToGoal = h;
		EstimatedTotalCost = g + h;
		Parent = pParent;
		Direction = pDirection;
		return this;
	}

	public int CompareTo(object Nav2)
	{
		return EstimatedTotalCost.CompareTo((Nav2 as CellNavigationValue).EstimatedTotalCost);
	}
}
