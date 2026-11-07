using System;
using Genkit;
using XRL.Collections;
using XRL.UI;
using XRL.Wish;
using XRL.World.AI.Pathfinding;

namespace XRL.World.Capabilities;

[HasWishCommand]
public static class FasterDMapAutoexplore
{
	private const int MAX_COST = 100;

	private const int MAX_RINGS = 101;

	[NonSerialized]
	public static long CurrentPass = 0L;

	[NonSerialized]
	private static readonly long[,] PassMap = new long[80, 25];

	[NonSerialized]
	private static readonly int[,] SearchCost = new int[80, 25];

	[NonSerialized]
	private static readonly int[,] NavWeightCache = new int[80, 25];

	[NonSerialized]
	private static readonly byte[,] SearchFirstStep = new byte[80, 25];

	[NonSerialized]
	private static readonly Location2D[,] SearchCellParent = new Location2D[80, 25];

	[NonSerialized]
	private static Rack<Location2D>[] SearchRing;

	private const int REPLAN_STEPS = int.MaxValue;

	public static bool GoalAlwaysCostsOne = true;

	private static readonly Rack<GameObject> AutoexploreObjects = new Rack<GameObject>();

	private static readonly Rack<Cell> AutoexploreCells = new Rack<Cell>();

	private static readonly int[,] AutoexploreObjectMarks = new int[80, 25];

	private static int AutoexploreObjectMarkerValue;

	private static Zone AutoexploreZone;

	private static bool AutoexploreLastDigging;

	private static Cell AutoexploreLastTarget;

	private static int AutoexploreLastAct;

	private static int AutoexploreLastCourse;

	private static int AutoexploreAdjacentTurns;

	private static Cell AutoexploreAdjacentTarget;

	private static FindPath AutoexplorePath = new FindPath();

	private static FindPath NewAutoexplorePath = new FindPath();

	public static bool DebugDraw => Options.DrawFloodExplore;

	private static Rack<Location2D>[] InitRings()
	{
		Rack<Location2D>[] array = new Rack<Location2D>[101];
		for (int i = 0; i < 101; i++)
		{
			array[i] = new Rack<Location2D>(64);
		}
		return array;
	}

	public static bool TryGetNextTarget(Zone Zone, Cell From, GameObject Looker, int Nav, int[,] AutoexploreObjectMarks, int AutoexploreObjectMarkerValue, int Course, bool GoalAlwaysCostsOne, FindPath Path, out int Cost, out bool Blackout)
	{
		Cost = int.MaxValue;
		Blackout = false;
		if (From == null || From.ParentZone != Zone)
		{
			return false;
		}
		if (SearchRing == null)
		{
			SearchRing = InitRings();
		}
		for (int i = 0; i < 101; i++)
		{
			SearchRing[i].Clear();
		}
		long num = ++CurrentPass;
		if (CurrentPass >= 9223372036854775806L)
		{
			CurrentPass = 0L;
		}
		long[,] passMap = PassMap;
		int[,] searchCost = SearchCost;
		Location2D[,] searchCellParent = SearchCellParent;
		Cell[][] map = Zone.Map;
		bool[] explored = Zone.ExploredMap;
		int width = Zone.Width;
		Location2D location = From.Location;
		passMap[location.X, location.Y] = num;
		searchCost[location.X, location.Y] = 0;
		searchCellParent[location.X, location.Y] = null;
		NavWeightCache[location.X, location.Y] = From.NavigationWeight(Looker, Nav);
		SearchRing[0].Add(location);
		Location2D location2D = null;
		int num2 = int.MaxValue;
		int num3 = 0;
		int num4 = 0;
		while (num4 < 101)
		{
			Rack<Location2D> rack = SearchRing[num3 % 101];
			if (rack.Count == 0)
			{
				num4++;
			}
			else
			{
				num4 = 0;
				Location2D[] array = rack.GetArray();
				int j = 0;
				for (int count = rack.Count; j < count; j++)
				{
					Location2D location2D2 = array[j];
					if ((object)location2D2 != location && IsGoal(location2D2.X, location2D2.Y))
					{
						int num5 = TieScore(location2D2);
						if (num5 < num2)
						{
							num2 = num5;
							location2D = location2D2;
						}
					}
				}
				if (location2D != null)
				{
					Cost = num3;
					break;
				}
				int k = 0;
				for (int count2 = rack.Count; k < count2; k++)
				{
					Location2D location2D3 = array[k];
					int l = 0;
					for (int num6 = Location2D.Directions.Length; l < num6; l++)
					{
						Location2D location2D4 = location2D3.FromDirection(Location2D.Directions[l]);
						if (location2D4 == null || location2D4.X >= Zone.Width || location2D4.Y >= Zone.Height)
						{
							continue;
						}
						int x = location2D4.X;
						int y = location2D4.Y;
						byte b = (((object)location2D3 == location) ? ((byte)l) : SearchFirstStep[location2D3.X, location2D3.Y]);
						if (passMap[x, y] == num)
						{
							int num7 = NavWeightCache[x, y];
							int num8 = ((num7 >= 95 || (GoalAlwaysCostsOne && IsGoal(x, y))) ? 1 : Math.Max(num7, 1));
							if (searchCost[x, y] != int.MaxValue && searchCost[x, y] == num3 + num8 && b < SearchFirstStep[x, y])
							{
								SearchFirstStep[x, y] = b;
								searchCellParent[x, y] = location2D3;
							}
							continue;
						}
						passMap[x, y] = num;
						SearchFirstStep[x, y] = b;
						Cell cell = map[x][y];
						int num9 = (AutoexploreGuard.IsBlocked(x, y) ? 101 : cell.NavigationWeight(Looker, Nav));
						NavWeightCache[x, y] = num9;
						if (num9 >= 95)
						{
							searchCost[x, y] = int.MaxValue;
							if (!explored[x + y * width] && cell.IsBlackedOut())
							{
								Blackout = true;
							}
							else if (IsGoal(x, y))
							{
								int num10 = (searchCost[x, y] = num3 + 1);
								searchCellParent[x, y] = location2D3;
								SearchRing[num10 % 101].Add(location2D4);
							}
						}
						else
						{
							int num11 = Math.Max(num9, 1);
							if (GoalAlwaysCostsOne && IsGoal(x, y))
							{
								num11 = 1;
							}
							int num12 = (searchCost[x, y] = num3 + num11);
							searchCellParent[x, y] = location2D3;
							SearchRing[num12 % 101].Add(location2D4);
						}
					}
				}
				rack.Clear();
			}
			num3++;
		}
		if (location2D == null)
		{
			return false;
		}
		Path.Clear();
		Location2D location2D5 = location2D;
		while (location2D5 != null)
		{
			Path.Steps.Add(map[location2D5.X][location2D5.Y]);
			location2D5 = searchCellParent[location2D5.X, location2D5.Y];
		}
		Path.Steps.Reverse();
		int m = 0;
		for (int count3 = Path.Steps.Count; m < count3; m++)
		{
			Cell cell2 = Path.Steps[m];
			Path.Weights.Add(NavWeightCache[cell2.X, cell2.Y]);
			if (m + 1 < count3)
			{
				Path.Directions.Add(cell2.GetDirectionFromCell(Path.Steps[m + 1]));
			}
		}
		Path.Found = Path.Steps.Count > 1;
		return Path.Found;
		bool IsGoal(int num13, int num14)
		{
			if (AutoexploreGuard.IsDeadCell(num13, num14))
			{
				return false;
			}
			if (!explored[num13 + num14 * width])
			{
				return true;
			}
			return AutoexploreObjectMarks[num13, num14] == AutoexploreObjectMarkerValue;
		}
		static int TieScore(Location2D goal)
		{
			return SearchFirstStep[goal.X, goal.Y];
		}
	}

	public static void FindAutoexploreStep(out string Step, out bool Blackout)
	{
		FindAutoexploreStep(Force: false, out Step, out Blackout);
	}

	private static void MarkAutoexploreObjects(Zone zone, ref bool Blackout)
	{
		AutoexploreObjectMarkerValue++;
		GameObject[] array = AutoexploreObjects.GetArray();
		for (int num = AutoexploreObjects.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = array[num];
			if (!IsAutoexploreObject(gameObject, zone))
			{
				AutoexploreObjects.RemoveAt(num);
			}
			else
			{
				Cell currentCell = gameObject.CurrentCell;
				if (currentCell.IsBlackedOut())
				{
					Blackout = true;
				}
				else if (currentCell.IsExplored() && !AutoexploreGuard.IsDeadCell(currentCell.X, currentCell.Y))
				{
					AutoexploreObjectMarks[currentCell.X, currentCell.Y] = AutoexploreObjectMarkerValue;
				}
			}
		}
	}

	private static bool TryDigToNearestObject(Cell Current, out string Step)
	{
		Step = ".";
		Cell cell = null;
		int num = int.MaxValue;
		GameObject[] array = AutoexploreObjects.GetArray();
		int i = 0;
		for (int count = AutoexploreObjects.Count; i < count; i++)
		{
			Cell currentCell = array[i].CurrentCell;
			if (currentCell != null && AutoexploreObjectMarks[currentCell.X, currentCell.Y] == AutoexploreObjectMarkerValue)
			{
				int num2 = Current.DistanceTo(currentCell.X, currentCell.Y);
				if (num2 < num)
				{
					num = num2;
					cell = currentCell;
				}
			}
		}
		if (cell == null)
		{
			return false;
		}
		AutoexplorePath.PerformPathfind(Current.ParentZone, Current.X, Current.Y, cell.ParentZone, cell.X, cell.Y, PathGlobal: false, The.Player, Unlimited: true, AddNoise: false, CardinalDirectionsOnly: false, null, 95, ExploredOnly: false, Autoexploring: true);
		if (!AutoexplorePath.Found)
		{
			AutoexploreGuard.MarkCellAsDead(cell);
			return false;
		}
		AutoexploreLastDigging = true;
		AutoexploreLastTarget = cell;
		AutoexploreLastCourse = Current.GetGeneralCourseFrom(cell);
		Step = AutoexplorePath.Directions[0];
		return true;
	}

	private static bool TryFollowCurrentPath(Cell Current, out string StepDir, out int StepIndex)
	{
		StepDir = ".";
		StepIndex = -1;
		FindPath autoexplorePath = AutoexplorePath;
		if (!autoexplorePath.Found)
		{
			return false;
		}
		Cell autoexploreLastTarget = AutoexploreLastTarget;
		if (autoexploreLastTarget == null || !IsAutoexploreCell(autoexploreLastTarget))
		{
			return false;
		}
		int num = autoexplorePath.Steps.IndexOf(Current);
		if (num < 0 || num + 1 >= autoexplorePath.Steps.Count)
		{
			return false;
		}
		Cell cell = autoexplorePath.Steps[num + 1];
		if (AutoexploreGuard.IsBlocked(cell.X, cell.Y))
		{
			return false;
		}
		int nav = Zone.CalculateNav(The.Player, Autoexploring: true, Juggernaut: false, IgnoreCreatures: false, IgnoreGases: false, FlexPhase: false, AutoexploreLastDigging);
		if (cell.NavigationWeight(The.Player, nav) != autoexplorePath.Weights[num + 1])
		{
			return false;
		}
		StepDir = autoexplorePath.Directions[num];
		StepIndex = num;
		return true;
	}

	public static bool IsAutoexploreCell(Cell C)
	{
		if (C == null || C.ParentZone != AutoexploreZone)
		{
			return false;
		}
		if (AutoexploreGuard.IsDeadCell(C.X, C.Y))
		{
			return false;
		}
		if (C.IsExplored())
		{
			return C.HasObject((GameObject o) => IsAutoexploreObject(o, AutoexploreZone));
		}
		return true;
	}

	public static bool IsAutoexploreObject(GameObject Object, Zone Zone)
	{
		if (Zone == null || !Object.IsValid())
		{
			return false;
		}
		Cell currentCell = Object.CurrentCell;
		if (currentCell == null || currentCell.ParentZone != Zone)
		{
			return false;
		}
		if (currentCell.IsSolidFor(The.Player) && !Object.CanInteractInCellWithSolid(The.Player))
		{
			return false;
		}
		if (Options.AutoexploreChests && Object.ShouldAutoexploreAsChest())
		{
			return true;
		}
		return AutoexploreObjectEvent.Check(The.Player, Object, AutoAct.Setting, AutoAct.Action);
	}

	public static void FindAutoexploreStep(bool Force, out string StepDir, out bool Blackout)
	{
		Blackout = false;
		StepDir = ".";
		Cell playerCell = The.PlayerCell;
		Zone zone = playerCell?.ParentZone;
		if (zone == null)
		{
			return;
		}
		bool flag = AutoexploreZone != zone || AutoexploreLastAct != AutoAct.Count;
		if (Force | flag)
		{
			AutoexploreObjectMarkerValue++;
			AutoexploreZone = zone;
			AutoexploreLastAct = AutoAct.Count;
			AutoexploreObjects.Clear();
			AutoexploreCells.Clear();
			AutoexplorePath.Clear();
			AutoexploreLastTarget = null;
			AutoexploreLastDigging = false;
			AutoexploreAdjacentTurns = 0;
			AutoexploreAdjacentTarget = null;
			AutoexploreAdjacentTarget = null;
			zone.FloodAutoexplore(AutoexploreCells, AutoexploreObjects, AutoexploreLastCourse);
			if (flag)
			{
				AutoexploreGuard.Reset(zone);
			}
		}
		int lastProgress = AutoexploreGuard.LastProgress;
		string text = AutoexploreGuard.DoSanityCheck(playerCell, zone);
		if (text != null)
		{
			AutoAct.Interrupt(text);
			return;
		}
		if (lastProgress != AutoexploreGuard.LastProgress)
		{
			AutoexploreGuard.StepsSinceLastPlan = int.MaxValue;
		}
		bool flag2 = TryFollowCurrentPath(playerCell, out var StepDir2, out var StepIndex);
		if (flag2 && AutoexploreGuard.StepsSinceLastPlan < int.MaxValue)
		{
			AutoexploreGuard.StepsSinceLastPlan++;
			StepDir = StepDir2;
			AutoexploreGuard.RecordStepHistory(playerCell, StepDir);
			return;
		}
		Cell autoexploreLastTarget = AutoexploreLastTarget;
		if (autoexploreLastTarget != null && playerCell.DistanceTo(autoexploreLastTarget.X, autoexploreLastTarget.Y) <= 1 && IsAutoexploreCell(autoexploreLastTarget))
		{
			if (autoexploreLastTarget != AutoexploreAdjacentTarget)
			{
				AutoexploreAdjacentTarget = autoexploreLastTarget;
				AutoexploreAdjacentTurns = 0;
			}
			if (++AutoexploreAdjacentTurns >= 2)
			{
				AutoexploreGuard.MarkCellAsDead(autoexploreLastTarget);
			}
		}
		else
		{
			AutoexploreAdjacentTarget = null;
			AutoexploreAdjacentTurns = 0;
		}
		MarkAutoexploreObjects(zone, ref Blackout);
		int nav = Zone.CalculateNav(The.Player, Autoexploring: true, Juggernaut: false, IgnoreCreatures: false, IgnoreGases: false, FlexPhase: false, AllowDigging: false);
		bool flag3 = TryGetNextTarget(zone, playerCell, The.Player, nav, AutoexploreObjectMarks, AutoexploreObjectMarkerValue, AutoexploreLastCourse, GoalAlwaysCostsOne, NewAutoexplorePath, out var Cost, out var Blackout2);
		Blackout |= Blackout2;
		if (flag3)
		{
			Cell cell = NewAutoexplorePath.Steps[NewAutoexplorePath.Steps.Count - 1];
			if (flag2 && cell != AutoexploreLastTarget && !AutoexploreGuard.ShouldSwitchTarget(Cost, AutoexploreGuard.CostRemaining(AutoexplorePath, StepIndex, GoalAlwaysCostsOne)))
			{
				AutoexploreGuard.StepsSinceLastPlan = 1;
				StepDir = StepDir2;
				AutoexploreGuard.RecordStepHistory(playerCell, StepDir);
				return;
			}
			FindPath autoexplorePath = AutoexplorePath;
			AutoexplorePath = NewAutoexplorePath;
			NewAutoexplorePath = autoexplorePath;
			AutoexploreLastTarget = cell;
			AutoexploreLastCourse = playerCell.GetGeneralCourseFrom(cell);
			AutoexploreLastDigging = false;
			AutoexploreGuard.StepsSinceLastPlan = 1;
			StepDir = AutoexplorePath.Directions[0];
			AutoexploreGuard.RecordStepHistory(playerCell, StepDir);
		}
		else if (TryDigToNearestObject(playerCell, out StepDir))
		{
			AutoexploreGuard.StepsSinceLastPlan = 1;
			AutoexploreGuard.RecordStepHistory(playerCell, StepDir);
		}
		else
		{
			AutoexplorePath.Clear();
			AutoexploreLastTarget = null;
			AutoexploreLastDigging = false;
			if (!Force)
			{
				FindAutoexploreStep(Force: true, out StepDir, out Blackout);
			}
		}
	}
}
