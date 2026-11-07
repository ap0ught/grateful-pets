using System;
using System.Collections.Generic;
using XRL.World.AI.Pathfinding;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class MoveToZone : IMovementGoal
{
	public const int DEFAULT_MAX_WEIGHT = 95;

	private GlobalLocation Target;

	private int Tries;

	public bool OverridesCombat;

	public int MaxWeight = 95;

	public MoveToZone()
	{
	}

	public MoveToZone(GlobalLocation Target, bool OverridesCombat = false, int MaxWeight = 95)
		: this()
	{
		this.Target = Target;
		this.OverridesCombat = OverridesCombat;
	}

	public MoveToZone(string ZoneID, bool OverridesCombat = false, int MaxTurns = -1, int MaxWeight = 95)
		: this(new GlobalLocation(), OverridesCombat, MaxWeight)
	{
		Target.ZoneID = ZoneID;
	}

	public override bool Finished()
	{
		return ParentObject.InZone(Target.ZoneID);
	}

	public override void TakeAction()
	{
		Tries++;
		if (!ParentObject.IsMobile())
		{
			FailToParent();
			return;
		}
		if (ParentObject.CurrentZone.ZoneID == null)
		{
			Pop();
			return;
		}
		if (Target == null)
		{
			Pop();
			return;
		}
		if (ParentObject.InZone(Target.ZoneID))
		{
			Pop();
			return;
		}
		ParentObject.UseEnergy(1000);
		int num = Target.ParasangX * 3 + Target.ZoneX;
		int num2 = Target.ParasangY * 3 + Target.ZoneY;
		int zoneZ = Target.ZoneZ;
		List<string> list = new List<string>();
		if (ParentObject.CurrentZone.wX * 3 + ParentObject.CurrentZone.X < num)
		{
			list.Add("E");
		}
		if (ParentObject.CurrentZone.wX * 3 + ParentObject.CurrentZone.X > num)
		{
			list.Add("W");
		}
		if (ParentObject.CurrentZone.wY * 3 + ParentObject.CurrentZone.Y < num2)
		{
			list.Add("S");
		}
		if (ParentObject.CurrentZone.wY * 3 + ParentObject.CurrentZone.Y > num2)
		{
			list.Add("N");
		}
		if (ParentObject.CurrentZone.Z < zoneZ)
		{
			GameObject gameObject = ParentObject.CurrentZone.FindClosestObjectWithPart(ParentObject, "StairsDown");
			if (gameObject != null)
			{
				ParentObject.Brain.PushGoal(new Step("D", careful: false, OverridesCombat, wandering: false, juggernaut: false, null, allowUnbuilt: true));
				ParentObject.Brain.PushGoal(new MoveTo(gameObject));
				return;
			}
		}
		if (ParentObject.CurrentZone.Z > zoneZ)
		{
			GameObject gameObject2 = ParentObject.CurrentZone.FindClosestObjectWithPart(ParentObject, "StairsUp");
			if (gameObject2 != null)
			{
				ParentObject.Brain.PushGoal(new Step("U", careful: false, OverridesCombat, wandering: false, juggernaut: false, null, allowUnbuilt: true));
				ParentObject.Brain.PushGoal(new MoveTo(gameObject2));
				return;
			}
		}
		List<(Cell, string)> list2 = new List<(Cell, string)>();
		FillExits(list, list2);
		if (list2.IsNullOrEmpty() && (ParentObject.CurrentZone.Z != zoneZ || list.Count == 0))
		{
			list.Add("N");
			list.Add("S");
			list.Add("E");
			list.Add("W");
			FillExits(list, list2);
		}
		list2.ShuffleInPlace();
		for (int i = 0; i < list2.Count; i++)
		{
			(Cell, string) tuple = list2[i];
			if (!tuple.Item1.IsReachable())
			{
				continue;
			}
			FindPath findPath = new FindPath(ParentObject.CurrentZone.ZoneID, ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, ParentObject.CurrentZone.ZoneID, tuple.Item1.X, tuple.Item1.Y, PathGlobal: true, PathUnlimited: false, ParentObject, Juggernaut: false, IgnoreCreatures: false, IgnoreGases: false, FlexPhase: false, MaxWeight);
			if (!findPath.Usable)
			{
				break;
			}
			findPath.Directions.Reverse();
			PushChildGoal(new Step(tuple.Item2, careful: false, overridesCombat: false, wandering: false, juggernaut: false, null, allowUnbuilt: true));
			{
				foreach (string direction in findPath.Directions)
				{
					PushChildGoal(new Step(direction, careful: false, OverridesCombat, wandering: false, juggernaut: false, null, allowUnbuilt: true));
				}
				break;
			}
		}
	}

	public void FillExits(List<string> Directions, List<(Cell Cell, string Dir)> Exits)
	{
		Exits.Clear();
		if (Directions.Contains("N"))
		{
			for (int i = 0; i < CurrentZone.Width; i++)
			{
				Cell cell = CurrentZone.GetCell(i, 0);
				if (cell.IsReachable())
				{
					Exits.Add((cell, "N"));
				}
			}
		}
		if (Directions.Contains("S"))
		{
			for (int j = 0; j < CurrentZone.Width; j++)
			{
				Cell cell2 = CurrentZone.GetCell(j, CurrentZone.Height - 1);
				if (cell2.IsReachable())
				{
					Exits.Add((cell2, "S"));
				}
			}
		}
		if (Directions.Contains("W"))
		{
			for (int k = 0; k < CurrentZone.Height; k++)
			{
				Cell cell3 = CurrentZone.GetCell(0, k);
				if (cell3.IsReachable())
				{
					Exits.Add((cell3, "W"));
				}
			}
		}
		if (!Directions.Contains("E"))
		{
			return;
		}
		for (int l = 0; l < CurrentZone.Height; l++)
		{
			Cell cell4 = CurrentZone.GetCell(CurrentZone.Width - 1, l);
			if (cell4.IsReachable())
			{
				Exits.Add((cell4, "E"));
			}
		}
	}
}
