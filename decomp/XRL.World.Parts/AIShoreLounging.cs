using System;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class AIShoreLounging : AIBehaviorPart
{
	public int GoToShoreChance = 4;

	public int GoBackToPoolChance = 10;

	public int Range = 20;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<AIBoredEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell.HasAquaticSupportFor(ParentObject))
		{
			if (GoToShoreChance.in100())
			{
				cell.GetPassableConnectedAdjacentCells(Range);
				Cell firstPassableConnectedAdjacentCell = cell.GetFirstPassableConnectedAdjacentCell(Range, LocalOnly: true, IsShore, ParentObject);
				if (firstPassableConnectedAdjacentCell != null)
				{
					ParentObject.Brain.PushGoal(new MoveTo(firstPassableConnectedAdjacentCell));
					if (ParentObject.HasEffect<Submerged>())
					{
						ParentObject.Brain.PushGoal(new Command("CommandSubmerge"));
					}
				}
			}
		}
		else if (GoBackToPoolChance.in100())
		{
			Cell cell2 = null;
			if (ParentObject.Brain.StartingCell != null)
			{
				cell2 = ParentObject.Brain.StartingCell.ResolveCell();
				if (cell2 != null && !cell2.HasAquaticSupportFor(ParentObject))
				{
					cell2 = null;
				}
			}
			if (cell2 == null)
			{
				cell2 = cell.GetFirstPassableConnectedAdjacentCell(Range, LocalOnly: true, IsLurkable, ParentObject);
			}
			if (cell2 != null)
			{
				if (ParentObject.HasEffect<Submerged>())
				{
					ParentObject.Brain.PushGoal(new Command("CommandSubmerge"));
				}
				ParentObject.Brain.PushGoal(new MoveTo(cell2));
			}
		}
		return base.HandleEvent(E);
	}

	public bool IsShore(Cell C)
	{
		if (!C.HasOpenLiquidVolume())
		{
			return C.HasAdjacentAquaticSupportFor(ParentObject);
		}
		return false;
	}

	public bool IsLurkable(Cell C)
	{
		if (C.HasAquaticSupportFor(ParentObject))
		{
			return !C.HasAdjacentNonAquaticSupportFor(ParentObject);
		}
		return false;
	}
}
