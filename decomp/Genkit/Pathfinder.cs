using System;
using System.Collections.Generic;
using System.Diagnostics;
using ConsoleLib.Console;
using UnityEngine;
using XRL;
using XRL.Core;
using XRL.UI;
using XRL.Wish;
using XRL.World;
using XRL.World.AI.Pathfinding;

namespace Genkit;

[HasWishCommand]
public class Pathfinder : IDisposable
{
	public List<PathfinderNode> addNodes = new List<PathfinderNode>(2000);

	public Dictionary<Location2D, PathfinderNode> nodesByPosition = new Dictionary<Location2D, PathfinderNode>(64);

	private int width;

	private int height;

	public static CellNavigationValue[] CellNavs = null;

	public static int nCellNav = -1;

	public static string[] DirectionList = new string[8] { "NW", "N", "NE", "E", "SE", "S", "SW", "W" };

	public static string[] DirectionListSorted = new string[8] { "N", "E", "S", "W", "NW", "NE", "SE", "SW" };

	public static string[] DirectionListU = new string[9] { "U", "NW", "N", "NE", "E", "SE", "S", "SW", "W" };

	public static string[] DirectionListD = new string[9] { "D", "NW", "N", "NE", "E", "SE", "S", "SW", "W" };

	public static string[] DirectionListCardinalOnly = new string[4] { "E", "S", "N", "W" };

	public int[,] CurrentNavigationMap;

	public const int MAXWEIGHT = 9999;

	public bool Found;

	public List<string> Directions = new List<string>();

	public List<PathfinderNode> Steps = new List<PathfinderNode>(2000);

	private Dictionary<PathfinderNode, CellNavigationValue> OpenList = new Dictionary<PathfinderNode, CellNavigationValue>();

	private Dictionary<PathfinderNode, CellNavigationValue> CloseList = new Dictionary<PathfinderNode, CellNavigationValue>();

	private static Queue<Pathfinder> pathfinderPool = new Queue<Pathfinder>();

	[WishCommand("pathbench", null)]
	public static void pathbench()
	{
		GameManager.Instance.uiQueue.queueTask(() =>
		{
			UnityEngine.Debug.Log("Beginning Pathfider Benchmark...");
			Stopwatch stopwatch = Stopwatch.StartNew();
			int num = 0;
			foreach (XRL.World.GameObject item in The.PlayerCell.ParentZone.GetObjectsWithPart("Combat"))
			{
				_ = item.CurrentZone;
				new FindPath().PerformPathfind(item.CurrentZone, item.CurrentCell.X, item.CurrentCell.Y, 80 - item.CurrentCell.X, 25 - item.CurrentCell.Y, 0, item, CardinalDirectionsOnly: false, 95);
				num++;
			}
			stopwatch.Stop();
			UnityEngine.Debug.Log($"Benchmark: {stopwatch.ElapsedMilliseconds}ms {stopwatch.ElapsedTicks}ticks [{num} paths]");
		});
	}

	public Pathfinder(int width, int height)
	{
		this.width = width;
		this.height = height;
		CurrentNavigationMap = new int[width, height];
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Location2D location2D = Location2D.Get(i, j);
				PathfinderNode pathfinderNode = PathfinderNode.fromPool();
				pathfinderNode.pos = location2D;
				nodesByPosition.Add(location2D, pathfinderNode);
				addNodes.Add(pathfinderNode);
			}
		}
		for (int k = 0; k < width; k++)
		{
			for (int l = 0; l < height; l++)
			{
				if (k > 0)
				{
					nodesByPosition[Location2D.Get(k, l)].adjacentNodes.Add(nodesByPosition[Location2D.Get(k - 1, l)]);
					nodesByPosition[Location2D.Get(k, l)].nodesByDirection.Add("W", nodesByPosition[Location2D.Get(k - 1, l)]);
				}
				if (k < width - 1)
				{
					nodesByPosition[Location2D.Get(k, l)].adjacentNodes.Add(nodesByPosition[Location2D.Get(k + 1, l)]);
					nodesByPosition[Location2D.Get(k, l)].nodesByDirection.Add("E", nodesByPosition[Location2D.Get(k + 1, l)]);
				}
				if (l > 0)
				{
					nodesByPosition[Location2D.Get(k, l)].adjacentNodes.Add(nodesByPosition[Location2D.Get(k, l - 1)]);
					nodesByPosition[Location2D.Get(k, l)].nodesByDirection.Add("N", nodesByPosition[Location2D.Get(k, l - 1)]);
				}
				if (l < height - 1)
				{
					nodesByPosition[Location2D.Get(k, l)].adjacentNodes.Add(nodesByPosition[Location2D.Get(k, l + 1)]);
					nodesByPosition[Location2D.Get(k, l)].nodesByDirection.Add("S", nodesByPosition[Location2D.Get(k, l + 1)]);
				}
			}
		}
	}

	public static void Initalize()
	{
		if (CellNavs == null)
		{
			CellNavs = new CellNavigationValue[16800];
			for (int i = 0; i < 16800; i++)
			{
				CellNavs[i] = new CellNavigationValue(0, 0, null, null, null);
			}
		}
		else
		{
			for (int j = 0; j < 16800; j++)
			{
				CellNavs[j].Set(0, 0, null, null, null);
			}
		}
	}

	public CellNavigationValue NewCellNav(int g, int h, PathfinderNode cCell, PathfinderNode pParent, string pDirection)
	{
		if (nCellNav >= 16799)
		{
			return null;
		}
		nCellNav++;
		return CellNavs[nCellNav].Set(g, h, cCell, pParent, pDirection);
	}

	public void setWeightsFromGrid<T>(Grid<T> grid, Func<int, int, T, int> weightFunc)
	{
		grid.forEach((int x, int y, T c) =>
		{
			CurrentNavigationMap[x, y] = weightFunc(x, y, c);
		});
	}

	private int CellDistance(PathfinderNode c1, PathfinderNode c2)
	{
		int val = Math.Abs(c1.X - c2.X);
		int val2 = Math.Abs(c1.Y - c2.Y);
		return Math.Max(val, val2);
	}

	public bool FindPath(Location2D Start, Location2D End, bool Display = false, bool CardinalDirectionsOnly = false, int MaxDistance = 9999, bool ShuffleDirections = false)
	{
		return FindPath(nodesByPosition[Start], nodesByPosition[End], Display, CardinalDirectionsOnly, MaxDistance, ShuffleDirections);
	}

	public bool FindPath(PathfinderNode Start, PathfinderNode Finish, bool Display = true, bool CardinalDirectionsOnly = false, int MaxDistance = 9999, bool ShuffleDirections = false)
	{
		if (CellNavs == null)
		{
			Initalize();
		}
		nCellNav = 0;
		ScreenBuffer screenBuffer = Popup._ScreenBuffer;
		Steps.Clear();
		Directions.Clear();
		OpenList.Clear();
		CloseList.Clear();
		OpenList.Add(Start, new CellNavigationValue(0, 0, null, null, "."));
		List<string> list = ((!CardinalDirectionsOnly) ? new List<string>(DirectionListSorted) : new List<string>(DirectionListCardinalOnly));
		while (OpenList.Count > 0)
		{
			PathfinderNode pathfinderNode = null;
			int num = 999999999;
			foreach (PathfinderNode key in OpenList.Keys)
			{
				if (OpenList[key].EstimatedTotalCost < num)
				{
					num = OpenList[key].EstimatedTotalCost;
					pathfinderNode = key;
				}
			}
			if (Display)
			{
				screenBuffer.Goto(pathfinderNode.X, pathfinderNode.Y);
				screenBuffer.Write("&Y!");
				XRLCore._Console.DrawBuffer(screenBuffer);
			}
			CellNavigationValue cellNavigationValue = OpenList[pathfinderNode];
			if (Display)
			{
				screenBuffer.Goto(Start.X, Start.Y);
				screenBuffer.Write("&GS");
				screenBuffer.Goto(Finish.X, Finish.Y);
				screenBuffer.Write("&RE");
				XRLCore._Console.DrawBuffer(screenBuffer);
			}
			if (ShuffleDirections)
			{
				list.ShuffleInPlace();
			}
			foreach (string item in list)
			{
				PathfinderNode pathfinderNode2 = null;
				pathfinderNode2 = pathfinderNode.GetNodeFromDirection(item);
				if (pathfinderNode2 != Finish && (pathfinderNode2 == null || CloseList.ContainsKey(pathfinderNode2) || CurrentNavigationMap[pathfinderNode2.X, pathfinderNode2.Y] >= 9999))
				{
					continue;
				}
				if (Display)
				{
					screenBuffer.Goto(pathfinderNode2.X, pathfinderNode2.Y);
					screenBuffer.Write("&K?");
					XRLCore._Console.DrawBuffer(screenBuffer);
				}
				int num2 = CellDistance(Finish, pathfinderNode2);
				if (nCellNav >= 4000)
				{
					Found = false;
					for (int i = 0; i < 4000; i++)
					{
						CellNavs[i].cCell = null;
						CellNavs[i].Parent = null;
					}
					OpenList.Clear();
					CloseList.Clear();
					return Found;
				}
				if (OpenList.ContainsKey(pathfinderNode2))
				{
					int num3 = cellNavigationValue.EstimatedTotalCost + CurrentNavigationMap[pathfinderNode2.X, pathfinderNode2.Y] + num2;
					int num4 = num2;
					if (OpenList[pathfinderNode2].EstimatedTotalCost < num3)
					{
						OpenList[pathfinderNode2].Set(cellNavigationValue.EstimatedNodeToGoal + CurrentNavigationMap[pathfinderNode2.X, pathfinderNode2.Y], num4 + CurrentNavigationMap[pathfinderNode2.X, pathfinderNode2.Y], null, pathfinderNode, item);
					}
				}
				else if (num2 <= MaxDistance)
				{
					int num5 = num2;
					OpenList.Add(pathfinderNode2, NewCellNav(cellNavigationValue.EstimatedNodeToGoal + CurrentNavigationMap[pathfinderNode2.X, pathfinderNode2.Y], num5 + CurrentNavigationMap[pathfinderNode2.X, pathfinderNode2.Y], null, pathfinderNode, item));
				}
				if (pathfinderNode2 != Finish)
				{
					continue;
				}
				Found = true;
				if (Display)
				{
					screenBuffer.Goto(Start.X, Start.Y);
					screenBuffer.Write("S");
					screenBuffer.Goto(Finish.X, Finish.Y);
					screenBuffer.Write("E");
				}
				if (item != ".")
				{
					Steps.Add(pathfinderNode2);
					Directions.Add(item);
				}
				PathfinderNode pathfinderNode3 = pathfinderNode;
				while (pathfinderNode3 != null && !Steps.Contains(pathfinderNode3))
				{
					Steps.Add(pathfinderNode3);
					if (Display)
					{
						screenBuffer.Goto(pathfinderNode3.X, pathfinderNode3.Y);
						screenBuffer.Write(".");
						XRLCore._Console.DrawBuffer(screenBuffer);
					}
					if (OpenList.ContainsKey(pathfinderNode3))
					{
						if (OpenList[pathfinderNode3].Direction != ".")
						{
							Directions.Add(OpenList[pathfinderNode3].Direction);
						}
						pathfinderNode3 = OpenList[pathfinderNode3].Parent;
					}
					else
					{
						if (CloseList[pathfinderNode3].Direction != ".")
						{
							Directions.Add(CloseList[pathfinderNode3].Direction);
						}
						pathfinderNode3 = CloseList[pathfinderNode3].Parent;
					}
				}
				Directions.Reverse();
				Steps.Reverse();
				for (int j = 0; j <= nCellNav; j++)
				{
					CellNavs[j].Parent = null;
					CellNavs[j].cCell = null;
				}
				if (Display)
				{
					XRLCore._Console.DrawBuffer(screenBuffer);
					Keyboard.getch();
				}
				OpenList.Clear();
				CloseList.Clear();
				return Found;
			}
			if (!CloseList.ContainsKey(pathfinderNode))
			{
				CloseList.Add(pathfinderNode, cellNavigationValue);
				OpenList.Remove(pathfinderNode);
			}
		}
		Found = false;
		for (int k = 0; k <= nCellNav; k++)
		{
			CellNavs[k].Parent = null;
			CellNavs[k].cCell = null;
		}
		if (Display)
		{
			XRLCore._Console.DrawBuffer(screenBuffer);
			Keyboard.getch();
		}
		OpenList.Clear();
		CloseList.Clear();
		return Found;
	}

	public static Pathfinder next(int width, int height)
	{
		if (width != 80 || height != 25)
		{
			return new Pathfinder(width, height);
		}
		if (pathfinderPool.Count > 0)
		{
			return pathfinderPool.Dequeue();
		}
		return new Pathfinder(width, height);
	}

	public void Dispose()
	{
		try
		{
			if (width != 80 || height != 25)
			{
				foreach (PathfinderNode addNode in addNodes)
				{
					addNode?.Dispose();
				}
				return;
			}
			Steps.Clear();
			Directions.Clear();
			Found = false;
			pathfinderPool.Enqueue(this);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Pathfinder::Dispose", x);
		}
	}
}
