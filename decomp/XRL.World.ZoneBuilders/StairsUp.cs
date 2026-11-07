using System;
using System.Collections.Generic;
using UnityEngine;
using Wintellect.PowerCollections;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class StairsUp
{
	public int Number = 1;

	public string x = "-1";

	public string y = "-1";

	public bool Reachable = true;

	[NonSerialized]
	private static string[] StairsDownBlueprints = new string[6] { "StairsDown", "OpenShaft", "LazyAir", "LazyPit", "Pit", "StairBlocker" };

	public bool BuildZone(Zone Z)
	{
		for (int i = 0; i < Number; i++)
		{
			if (!AddStairUp(Z, x, y, Reachable))
			{
				return false;
			}
		}
		return true;
	}

	public static bool AddStairUp(Zone Z, string x, string y, bool Reachable)
	{
		if (x == "-1" && y == "-1")
		{
			bool flag = false;
			foreach (ZoneConnection item in The.ZoneManager.GetZoneConnectionsCopy(Z.ZoneID))
			{
				if (item.Type != "StairsUp")
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
					cell.AddObject((!string.IsNullOrEmpty(item.Object)) ? item.Object : "StairsUp");
					flag = true;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		if (The.ZoneManager.IsZoneBuilt(The.ZoneManager.GetZoneFromIDAndDirection(Z.ZoneID, "u")))
		{
			return true;
		}
		List<Cell> list = Z.GetEmptyReachableCellsWithout(StairsDownBlueprints);
		if (list == null || list.Count == 0)
		{
			list = Z.GetEmptyCells((Cell C) => C.X > 5 && C.X < 75 && C.Y > 3 && C.Y < 21);
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
		foreach (Cell item2 in list)
		{
			if ((num != -1 && item2.X < num) || (num2 != -1 && item2.X > num2) || (num3 != -1 && item2.Y < num3) || (num4 != -1 && item2.Y > num4))
			{
				continue;
			}
			int startX = item2.X;
			int startY = item2.Y;
			if (!item2.HasObjectWithBlueprint("StairsDown") && !item2.HasObjectWithBlueprint("StairsUp") && !item2.HasObjectWithBlueprint("OpenShaft") && !item2.HasObjectWithBlueprint("LazyAir") && !item2.HasObjectWithBlueprint("LazyPit") && !item2.HasObjectWithBlueprint("Pit") && item2.IsReachable())
			{
				item2.Clear();
				item2.AddObject("StairsUp");
				Z.CacheZoneConnection("u", startX, startY, "StairsDown", "StairsDown");
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
			list[0].AddObject("StairsUp");
			return true;
		}
		return false;
	}
}
