using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AISeekHealingPool : AIBehaviorPart
{
	public float Trigger = 0.35f;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AITakingAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AITakingAction" && ParentObject.Statistics.ContainsKey("Hitpoints"))
		{
			Statistic statistic = ParentObject.Statistics["Hitpoints"];
			Cell cell = ParentObject.CurrentCell;
			if ((float)statistic.Penalty >= (float)statistic.BaseValue * Trigger && cell != null && !cell.HasHealingPool())
			{
				Zone parentZone = cell.ParentZone;
				if (parentZone != null)
				{
					List<Cell> cells = parentZone.GetCells((Cell C) => C.HasHealingPool());
					if (cells.Count > 0)
					{
						cells.Sort(new XRLCore.SortCellBydistanceToObject(ParentObject));
						ParentObject.Brain.Goals.Clear();
						ParentObject.Brain.PushGoal(new MoveTo(parentZone.ZoneID, cells[0].X, cells[0].Y));
						if (ParentObject.Brain.Goals.Count > 0)
						{
							ParentObject.Brain.Goals.Peek().TakeAction();
							return false;
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
