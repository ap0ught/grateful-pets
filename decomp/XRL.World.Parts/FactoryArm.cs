using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class FactoryArm : IPart
{
	public string Direction;

	public int Frequency;

	public int Counter;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Counter++;
		if (Counter >= Frequency)
		{
			Counter = 0;
			Cell cellFromDirection = currentCell.GetCellFromDirection(Direction);
			Cell cellFromDirection2 = currentCell.GetCellFromDirection(Directions.GetOppositeDirection(Direction));
			if (cellFromDirection.IsPassable() && cellFromDirection2.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: false) != null)
			{
				for (int num = cellFromDirection2.Objects.Count - 1; num >= 0; num--)
				{
					GameObject gameObject = cellFromDirection2.Objects[num];
					if (gameObject.Physics != null && gameObject.CanBeInvoluntarilyMoved() && gameObject.PhaseAndFlightMatches(ParentObject))
					{
						gameObject.DirectMoveTo(cellFromDirection, 0, Forced: true, IgnoreCombat: true);
						break;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Counter == Frequency - 1)
		{
			int num = XRLCore.CurrentFrame % 30;
			if (num > 0 && num < 15)
			{
				E.Tile = null;
				E.RenderString = Directions.GetArrowForDirection(Direction);
				E.ColorString = "&C";
				return false;
			}
		}
		return base.Render(E);
	}
}
