using System;
using System.Collections.Generic;

namespace Genkit;

public class PathfinderNode : IDisposable
{
	public int weight;

	public Location2D pos;

	public List<PathfinderNode> adjacentNodes = new List<PathfinderNode>();

	public Dictionary<string, PathfinderNode> nodesByDirection = new Dictionary<string, PathfinderNode>();

	private static Queue<PathfinderNode> nodePool = new Queue<PathfinderNode>();

	public int X => pos.X;

	public int Y => pos.Y;

	public void clear()
	{
		weight = 0;
		pos = Location2D.Invalid;
		adjacentNodes.Clear();
		nodesByDirection.Clear();
	}

	public override int GetHashCode()
	{
		return pos.GetHashCode();
	}

	public int SquareDistance(PathfinderNode target)
	{
		return pos.SquareDistance(target.pos);
	}

	public PathfinderNode GetNodeFromDirection(string dir)
	{
		if (nodesByDirection.TryGetValue(dir, out var value))
		{
			return value;
		}
		return null;
	}

	public static PathfinderNode fromPool()
	{
		if (nodePool.Count > 0)
		{
			return nodePool.Dequeue();
		}
		return new PathfinderNode();
	}

	public void Dispose()
	{
		clear();
		nodePool.Enqueue(this);
	}
}
