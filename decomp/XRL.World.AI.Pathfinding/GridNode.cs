using System;

namespace XRL.World.AI.Pathfinding;

internal class GridNode : IComparable<GridNode>, IEquatable<GridNode>
{
	public static int[][] Offsets = new int[8][]
	{
		new int[2] { 0, -1 },
		new int[2] { 1, -1 },
		new int[2] { 1, 0 },
		new int[2] { 1, 1 },
		new int[2] { 0, 1 },
		new int[2] { -1, 1 },
		new int[2] { -1, 0 },
		new int[2] { -1, -1 }
	};

	public static int[][] OffsetsCardinal = new int[4][]
	{
		new int[2] { 0, -1 },
		new int[2] { 1, 0 },
		new int[2] { 0, 1 },
		new int[2] { -1, 0 }
	};

	public GridNode Parent;

	public int X;

	public int Y;

	public int Cost;

	public int Weight;

	public int Total;

	public int Distance;

	public int Length;

	public int Index;

	public int Version;

	public bool Closed;

	public bool Diagonal
	{
		get
		{
			if (Parent == null)
			{
				return false;
			}
			return DiagonalTo(Parent);
		}
	}

	public string Direction
	{
		get
		{
			if (Parent == null)
			{
				return ".";
			}
			int x = X;
			int y = Y;
			int x2 = Parent.X;
			int y2 = Parent.Y;
			if (x == x2)
			{
				if (y == y2)
				{
					return ".";
				}
				if (y < y2)
				{
					return "N";
				}
				return "S";
			}
			if (x < x2)
			{
				if (y == y2)
				{
					return "W";
				}
				if (y < y2)
				{
					return "NW";
				}
				return "SW";
			}
			if (y == y2)
			{
				return "E";
			}
			if (y < y2)
			{
				return "NE";
			}
			return "SE";
		}
	}

	public GridNode()
	{
	}

	public GridNode(int X, int Y)
	{
		this.X = X;
		this.Y = Y;
	}

	public void Set(Cell Cell, FindPath Pathfinder, GridNode Parent = null)
	{
		Weight = Cell.NavigationWeight(Pathfinder.Looker, Pathfinder.Nav);
		Distance = Cell.ManhattanDistanceTo(Pathfinder.Destination);
		if (Parent == null)
		{
			Cost = 0;
		}
		else if (Distance == 0)
		{
			Cost = 1;
		}
		else
		{
			Cost = Weight + 1;
		}
		Closed = false;
		SetParent(Parent);
	}

	public bool DiagonalTo(GridNode Node)
	{
		if (X == Node.X)
		{
			return false;
		}
		if (Y == Node.Y)
		{
			return false;
		}
		return true;
	}

	public void SetParent(GridNode Node)
	{
		Parent = Node;
		if (Node != null)
		{
			Total = Node.Total + Cost;
			Length = Node.Length + 1;
		}
		else
		{
			Total = Cost;
			Length = 1;
		}
	}

	public int ManhattanDistanceTo(GridNode L)
	{
		return Math.Abs(L.X - X) + Math.Abs(L.Y - Y);
	}

	public int CompareTo(GridNode Other)
	{
		return Total + Distance - (Other.Total + Other.Distance);
	}

	public bool Equals(GridNode other)
	{
		if (other == null)
		{
			return false;
		}
		if (X == other.X)
		{
			return Y == other.Y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Y << 15) ^ X;
	}
}
