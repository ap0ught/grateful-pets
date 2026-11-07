using System;
using Genkit;
using XRL.World.AI.Pathfinding;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class MoveTo : IMovementGoal
{
	public const int DEFAULT_MAX_WEIGHT = 95;

	public string dZone;

	public int dCx;

	public int dCy;

	public int MaxTurns = -1;

	public bool careful;

	public bool overridesCombat;

	public int shortBy;

	public bool wandering;

	public bool global;

	public bool juggernaut;

	public int AbortIfMoreSteps = -1;

	public int MaxWeight = 95;

	public MoveTo()
	{
	}

	public MoveTo(bool careful, bool overridesCombat, int shortBy, bool wandering, bool global, bool juggernaut, int MaxTurns, int AbortIfMoreSteps, int MaxWeight)
		: this()
	{
		this.careful = careful;
		this.overridesCombat = overridesCombat;
		this.shortBy = shortBy;
		this.wandering = wandering;
		this.global = global;
		this.juggernaut = juggernaut;
		this.MaxTurns = MaxTurns;
		this.AbortIfMoreSteps = AbortIfMoreSteps;
		this.MaxWeight = MaxWeight;
	}

	public MoveTo(Cell C, bool careful = false, bool overridesCombat = false, int shortBy = 0, bool wandering = false, bool global = false, bool juggernaut = false, int MaxTurns = -1, int AbortIfMoreSteps = -1, int MaxWeight = 95)
		: this(careful, overridesCombat, shortBy, wandering, global, juggernaut, MaxTurns, AbortIfMoreSteps, MaxWeight)
	{
		if (C != null)
		{
			Zone parentZone = C.ParentZone;
			if (parentZone != null)
			{
				dZone = parentZone.ZoneID;
			}
			dCx = C.X;
			dCy = C.Y;
		}
	}

	public MoveTo(Location2D L, bool careful = false, bool overridesCombat = false, int shortBy = 0, bool wandering = false, bool global = false, bool juggernaut = false, int MaxTurns = -1, int AbortIfMoreSteps = -1, int MaxWeight = 95)
		: this(careful, overridesCombat, shortBy, wandering, global, juggernaut, MaxTurns, AbortIfMoreSteps, MaxWeight)
	{
		if (L != null)
		{
			dCx = L.X;
			dCy = L.Y;
		}
	}

	public MoveTo(GameObject obj, bool careful = false, bool overridesCombat = false, int shortBy = 0, bool wandering = false, bool global = false, bool juggernaut = false, int MaxTurns = -1, int AbortIfMoreSteps = -1, int MaxWeight = 95)
		: this(obj.GetCurrentCell(), careful, overridesCombat, shortBy, wandering, global, juggernaut, MaxTurns, AbortIfMoreSteps, MaxWeight)
	{
	}

	public MoveTo(string ZoneID, int Cx, int Cy, bool careful = false, bool overridesCombat = false, int shortBy = 0, bool wandering = false, bool global = false, bool juggernaut = false, int MaxTurns = -1, int AbortIfMoreSteps = -1, int MaxWeight = 95)
		: this(careful, overridesCombat, shortBy, wandering, global, juggernaut, MaxTurns, AbortIfMoreSteps, MaxWeight)
	{
		dZone = ZoneID;
		dCx = Cx;
		dCy = Cy;
	}

	public MoveTo(GlobalLocation loc, bool careful = false, bool overridesCombat = false, int shortBy = 0, bool wandering = false, bool global = false, bool juggernaut = false, int MaxTurns = -1, int AbortIfMoreSteps = -1, int MaxWeight = 95)
		: this(loc.ZoneID, loc.CellX, loc.CellY, careful, overridesCombat, shortBy, wandering, global, juggernaut, MaxTurns, AbortIfMoreSteps, MaxWeight)
	{
	}

	public override bool CanFight()
	{
		return !overridesCombat;
	}

	public override bool Finished()
	{
		if (!ParentObject.IsMobile())
		{
			return true;
		}
		Cell currentCell = ParentObject.CurrentCell;
		if (currentCell != null)
		{
			if (currentCell.X == dCx && currentCell.Y == dCy)
			{
				return true;
			}
			if (shortBy > 0 && currentCell.DistanceTo(dCx, dCy) <= shortBy)
			{
				return true;
			}
		}
		return false;
	}

	public override void TakeAction()
	{
		if (!ParentObject.IsMobile())
		{
			FailToParent();
			return;
		}
		string text = ParentObject.CurrentZone?.ZoneID;
		if (text == null)
		{
			Pop();
			return;
		}
		if (dZone.IsNullOrEmpty())
		{
			dZone = text;
		}
		Cell currentCell = ParentObject.CurrentCell;
		if (text == dZone)
		{
			Cell cell = currentCell.ParentZone.GetCell(dCx, dCy);
			if (cell == null || cell == currentCell || (shortBy == 1 && cell.IsAdjacentTo(currentCell)) || (shortBy > 1 && cell.PathDistanceTo(currentCell) <= shortBy))
			{
				Pop();
				return;
			}
			if (AICommandList.HandleCommandList(AIGetMovementAbilityListEvent.GetFor(ParentObject, null, cell), ParentObject, null, cell))
			{
				return;
			}
		}
		FindPath findPath = new FindPath(text, currentCell.X, currentCell.Y, dZone, dCx, dCy, global, careful, Juggernaut: juggernaut, Looker: ParentObject, IgnoreCreatures: false, IgnoreGases: false, FlexPhase: false, MaxWeight: MaxWeight);
		ParentObject.UseEnergy(1000, "Pathfinding");
		if (findPath.Usable)
		{
			int num = findPath.Directions.Count;
			if (MaxTurns > -1)
			{
				Pop();
				if (num > MaxTurns)
				{
					num = MaxTurns;
				}
			}
			if (AbortIfMoreSteps > -1 && num > AbortIfMoreSteps)
			{
				FailToParent();
				return;
			}
			findPath.Directions.Reverse();
			for (int i = shortBy; i < num; i++)
			{
				PushGoal(new Step(findPath.Directions[i], careful, overridesCombat, wandering, juggernaut, null, global));
			}
		}
		else
		{
			FailToParent();
		}
	}

	public Cell GetDestinationCell()
	{
		Zone zone = The.ZoneManager.GetZone(dZone);
		if (zone != null)
		{
			Cell cell = zone.GetCell(dCx, dCy);
			if (cell != null)
			{
				return cell;
			}
		}
		return null;
	}
}
