using System;
using System.Collections.Generic;
using UnityEngine;
using Wintellect.PowerCollections;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class StairsDown
{
	public int Number = 1;

	public string x = "-1";

	public string y = "-1";

	public bool Reachable = true;

	public bool EmptyOnly = true;

	[NonSerialized]
	private static string[] UpStairsBlueprints = new string[6] { "StairsUp", "OpenShaft", "LazyPit", "LazyAir", "Pit", "StairBlocker" };

	public StairsDown setEmptyOnly(bool val)
	{
		EmptyOnly = val;
		return this;
	}

	public bool BuildZone(Zone Z)
	{
		for (int i = 0; i < Number; i++)
		{
			if (!AddStairDown(Z, x, y, Reachable, EmptyOnly))
			{
				return false;
			}
		}
		return true;
	}

	public static bool AddStairDown(Zone Z, string x, string y, bool Reachable, bool EmptyOnly)
	{
		bool flag = false;
		foreach (ZoneConnection item in The.ZoneManager.GetZoneConnectionsCopy(Z.ZoneID))
		{
			if (item.Type != "StairsDown")
			{
				continue;
			}
			Cell cell = Z.GetCell(item.X, item.Y);
			if (!cell.HasObjectWithTag("Stairs"))
			{
				if (cell.IsReachable())
				{
					cell.Clear();
				}
				cell.ClearWalls();
				cell.AddObject(item.Object.IsNullOrEmpty() ? "StairsDown" : item.Object);
				flag = true;
			}
		}
		if (The.ZoneManager.IsZoneBuilt(The.ZoneManager.GetZoneFromIDAndDirection(Z.ZoneID, "d")))
		{
			return true;
		}
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		int num4 = -1;
		if (x != "-1" && !string.IsNullOrEmpty(x))
		{
			try
			{
				num = Stat.RollMin(x);
				num2 = Stat.RollMax(x);
			}
			catch (Exception)
			{
				Debug.LogError("Invalid x value: " + x);
				num = -1;
				num2 = -1;
			}
		}
		if (y != "-1" && !string.IsNullOrEmpty(y))
		{
			try
			{
				num3 = Stat.RollMin(y);
				num4 = Stat.RollMax(y);
			}
			catch
			{
				Debug.LogError("Invalid y value: " + y);
				num3 = -1;
				num4 = -1;
			}
		}
		if (num == -1 || num3 == -1 || num2 - num != 0 || num4 - num3 != 0)
		{
			if (flag)
			{
				return true;
			}
			foreach (GameObject item2 in Z.GetObjectsWithTag("Stairs"))
			{
				if (item2.TryGetPart<XRL.World.Parts.StairsDown>(out var Part) && !item2.HasTagOrProperty("IgnoreForStairConnections") && Part.Connected)
				{
					return true;
				}
			}
		}
		List<Cell> list = null;
		if (EmptyOnly)
		{
			list = (Reachable ? Z.GetEmptyReachableCellsWithout(UpStairsBlueprints) : Z.GetEmptyCells());
		}
		else
		{
			list = ((!Reachable) ? Z.GetCells((Cell c) => !c.HasWall()) : Z.GetReachableCellsWithout(UpStairsBlueprints));
		}
		if (list == null || list.Count == 0)
		{
			list = Z.GetEmptyCells();
		}
		if (list == null || list.Count == 0)
		{
			list = Z.GetCells((Cell c) => !c.HasWall());
		}
		Algorithms.RandomShuffleInPlace(list, Stat.Rand);
		foreach (Cell item3 in list)
		{
			if ((num != -1 && item3.X < num) || (num2 != -1 && item3.X > num2) || (num3 != -1 && item3.Y < num3) || (num4 != -1 && item3.Y > num4))
			{
				continue;
			}
			int startX = item3.X;
			int startY = item3.Y;
			if (!item3.HasObjectWithTag("Stairs") && (!Reachable || item3.IsReachable()))
			{
				item3.Clear();
				item3.AddObject("StairsDown");
				Z.CacheZoneConnection("d", startX, startY, "StairsUp", "StairsUp");
				Z.CacheZoneConnection("-", startX, startY, "Connection", null);
				if (Reachable)
				{
					Z.BuildReachableMap(startX, startY);
				}
				return true;
			}
		}
		if (list.Count > 0)
		{
			list[0].Clear();
			list[0].AddObject("StairsDown");
			return true;
		}
		return false;
	}
}
