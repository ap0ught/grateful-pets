using System;
using System.Linq;
using XRL.Collections;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Fan : IPoweredPart
{
	public static readonly int ICON_COLOR_PRIORITY = 200;

	public string Direction = "S";

	public int BlowStrength = 500;

	public int BlowRange = 16;

	public Fan()
	{
		WorksOnSelf = true;
	}

	public override bool Render(RenderEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 0 && num < 15)
			{
				E.Tile = null;
				E.RenderString = Directions.GetArrowForDirection(Direction);
				E.ApplyColors("&C", ICON_COLOR_PRIORITY);
				return false;
			}
		}
		return true;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		Cell cellFromDirection = ParentObject.GetCurrentCell();
		if (cellFromDirection == null || !IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		using ScopeDisposedList<Cell> scopeDisposedList = ScopeDisposedList<Cell>.GetFromPool();
		for (int i = 0; i < BlowRange; i++)
		{
			cellFromDirection = cellFromDirection.GetCellFromDirection(Direction);
			if (cellFromDirection == null || cellFromDirection.IsSolid())
			{
				break;
			}
			scopeDisposedList.Add(cellFromDirection);
		}
		scopeDisposedList.Reverse();
		foreach (Cell item in scopeDisposedList)
		{
			foreach (GameObject item2 in from o in item.GetObjectsViaEventList()
				where o.IsCombatObject() || (o.Physics != null && !o.Physics.Solid && (o.Physics.Takeable || o.HasPart<Gas>()))
				where o.CanBeInvoluntarilyMoved() && o.PhaseMatches(ParentObject)
				select o)
			{
				int num = (item2.HasStat("Strength") ? (item2.GetStatValue("Strength") * 20) : (item2.Weight * 3 / 2));
				int num2 = item2.DistanceTo(ParentObject);
				int num3 = BlowStrength * Math.Max(10 - num2, 0);
				int val = Math.Min(BlowRange, (int)Math.Ceiling((float)(num3 - num) / 100f));
				val = Math.Max(val, 0);
				if (val == 0 && num3 > 100 && item2.HasStat("Strength") && item2.IsCombatObject())
				{
					IComponent<GameObject>.XDidYToZ(item2, "resist", "being blown back by", ParentObject, null, null, null, null, item2);
				}
				for (int num4 = 0; num4 < val; num4++)
				{
					Cell cell = item2.CurrentCell;
					Cell cell2 = cell?.GetCellFromDirection(Direction);
					if (cell2 != null && cell2.IsSolidFor(item2))
					{
						break;
					}
					if (num4 % 3 == 0 || !item2.HasPart<Gas>())
					{
						item2.Smoke();
					}
					item2.Move(Direction, Forced: true, System: false, IgnoreGravity: false, NoStack: false, AllowDashing: true, DoConfirmations: true, null, null, NearestAvailable: false, 0);
					if (item2.IsPlayer() && !item2.CurrentZone.IsActive())
					{
						item2.CurrentZone.SetActive();
					}
					if (item2.CurrentCell == cell)
					{
						break;
					}
					if (item2.IsVisible())
					{
						XRLCore.Core.RenderBase();
					}
				}
				if (val > 0 && item2.IsCombatObject())
				{
					if (val == 1)
					{
						IComponent<GameObject>.XDidYToZ(item2, "are", "blown back by", ParentObject, null, null, null, null, null, item2);
					}
					else if (val == 2)
					{
						IComponent<GameObject>.XDidYToZ(item2, "are", "blown forcefully back by", ParentObject, null, null, null, null, null, item2);
					}
					else if (val >= 3)
					{
						IComponent<GameObject>.XDidYToZ(item2, "are", "blown into the air by", ParentObject, null, "!", null, null, null, item2);
					}
				}
			}
		}
	}
}
