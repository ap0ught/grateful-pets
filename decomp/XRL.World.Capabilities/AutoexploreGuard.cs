using System;
using Genkit;
using XRL.Rules;
using XRL.World.AI.Pathfinding;

namespace XRL.World.Capabilities;

public static class AutoexploreGuard
{
	public const int MAX_HISTORY_DEPTH = 16;

	public const int MAX_CYCLE_VISITS = 2;

	public const int MAX_STEPS_WITHOUT_PROGRESS = 160;

	public const int MAX_FAILED_MOVES_BEFORE_MARKING_CELL_BLOCKED = 2;

	public const int MAX_FAILED_MOVES_BEFORE_STOPPING_AUTOEXPLORE = 6;

	public const int COST_MARGIN_BEFORE_SWITCHING_TARGETS = 3;

	private static int CurrentSessionId;

	private static readonly int[,] DeadGoal = new int[80, 25];

	private static readonly int[,] Blocked = new int[80, 25];

	private static readonly Location2D[] History = new Location2D[16];

	private static readonly int[] HistoryProgress = new int[16];

	private static int HistoryHead;

	private static int HistoryCount;

	private static Cell LastCell;

	private static string LastStep = ".";

	private static int StepsWithoutMoving;

	public static int LastProgress;

	private static int StepsWithoutProgress;

	public static int StepsSinceLastPlan;

	public static int NoMoveStrikes => StepsWithoutMoving;

	public static int StepsWithoutProgressCount => StepsWithoutProgress;

	public static void Reset(Zone Z)
	{
		CurrentSessionId++;
		if (CurrentSessionId >= 2147483646)
		{
			CurrentSessionId = 0;
		}
		HistoryHead = (HistoryCount = 0);
		LastCell = null;
		LastStep = ".";
		StepsWithoutMoving = 0;
		LastProgress = Progress(Z);
		StepsWithoutProgress = 0;
		StepsSinceLastPlan = 0;
	}

	public static int Progress(Zone Z)
	{
		int num = 0;
		bool[] exploredMap = Z.ExploredMap;
		int i = 0;
		for (int num2 = exploredMap.Length; i < num2; i++)
		{
			if (exploredMap[i])
			{
				num++;
			}
		}
		return num;
	}

	public static bool IsDeadCell(int X, int Y)
	{
		return DeadGoal[X, Y] == CurrentSessionId;
	}

	public static bool IsBlocked(int X, int Y)
	{
		return Blocked[X, Y] == CurrentSessionId;
	}

	public static void MarkCellAsDead(Cell C)
	{
		DeadGoal[C.X, C.Y] = CurrentSessionId;
	}

	public static void MarkCellAsBlocked(Cell C)
	{
		Blocked[C.X, C.Y] = CurrentSessionId;
	}

	public static string DoSanityCheck(Cell Current, Zone zone)
	{
		if (Current == null || Current.ParentZone != zone)
		{
			return null;
		}
		int num = Progress(zone);
		if (num != LastProgress)
		{
			LastProgress = num;
			StepsWithoutProgress = 0;
		}
		else
		{
			StepsWithoutProgress++;
		}
		bool flag = LastCell != Current;
		if (!flag && LastStep != "." && AutoAct.Digging == 0)
		{
			StepsWithoutMoving++;
			if (StepsWithoutMoving >= 6)
			{
				return "you can't move";
			}
			if (StepsWithoutMoving >= 2)
			{
				Cell cellFromDirection = Current.GetCellFromDirection(LastStep);
				if (cellFromDirection != null)
				{
					MarkCellAsBlocked(cellFromDirection);
					MarkCellAsDead(cellFromDirection);
				}
			}
		}
		else
		{
			StepsWithoutMoving = 0;
		}
		if (flag)
		{
			int num2 = 0;
			bool flag2 = true;
			for (int i = 0; i < HistoryCount; i++)
			{
				int num3 = (HistoryHead - 1 - i + 16) % 16;
				if (History[num3].X == Current.X && History[num3].Y == Current.Y)
				{
					num2++;
					if (HistoryProgress[num3] != num)
					{
						flag2 = false;
					}
				}
			}
			if ((num2 >= 2) & flag2)
			{
				return "you are going in circles";
			}
		}
		if (StepsWithoutProgress > 160 && AutoAct.Digging == 0)
		{
			return "you aren't making progress exploring the area";
		}
		if (flag)
		{
			History[HistoryHead] = Current.Location;
			HistoryProgress[HistoryHead] = num;
			HistoryHead = (HistoryHead + 1) % 16;
			if (HistoryCount < 16)
			{
				HistoryCount++;
			}
		}
		return null;
	}

	public static void RecordStepHistory(Cell Current, string Step)
	{
		LastCell = Current;
		LastStep = Step ?? ".";
	}

	public static bool ShouldSwitchTarget(int NewCost, int RemainingCost)
	{
		return RemainingCost - NewCost >= 3;
	}

	public static bool IsOpposite(int From, int To)
	{
		if (From == 0 || To == 0)
		{
			return false;
		}
		if ((From & To) == 0)
		{
			return (Courses.Opposite[From] & To) != 0;
		}
		return false;
	}

	public static int CostRemaining(FindPath Path, int From, bool GoalAlwaysCostsOne = false)
	{
		int num = 0;
		int num2 = Path.Steps.Count - 1;
		for (int i = From + 1; i <= num2; i++)
		{
			num += ((GoalAlwaysCostsOne && i == num2) ? 1 : Math.Max(Path.Weights[i], 1));
		}
		return num;
	}
}
