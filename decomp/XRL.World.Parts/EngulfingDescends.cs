using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class EngulfingDescends : IPart
{
	public int TurnsLeft = -1;

	public string Cooldown = "10";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurnEngulfing");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurnEngulfing")
		{
			if (TurnsLeft < 0)
			{
				TurnsLeft = Stat.Roll(Cooldown);
				return true;
			}
			if (TurnsLeft == 0)
			{
				Engulfing part = ParentObject.GetPart<Engulfing>();
				if (ParentObject.CurrentCell != null && part != null && part.Engulfed != null && part.Engulfed.CanBeInvoluntarilyMoved())
				{
					Cell cellFromDirection = ParentObject.CurrentCell.GetCellFromDirection("D", BuiltOnly: false);
					if (cellFromDirection != null && cellFromDirection.IsPassable(ParentObject) && cellFromDirection.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: false) == null)
					{
						if (ParentObject.IsPlayer())
						{
							Popup.Show("You melt through the floor and descend with your meal.");
						}
						else if (part.Engulfed.IsPlayer())
						{
							Popup.Show(ParentObject.The + ParentObject.ShortDisplayName + "&y engulfing you melts through the floor! You fall to the level below.");
						}
						else
						{
							DidXToY("melt", "through the floor and" + ParentObject.GetVerb("descend") + " with", part.Engulfed, null, null, null, null, null, part.Engulfed);
						}
						cellFromDirection.ClearWalls();
						Cell cell = ParentObject.CurrentCell;
						ParentObject.DirectMoveTo(cellFromDirection);
						cell.AddObject("OpenShaft");
						TurnsLeft = Stat.Roll(Cooldown);
						part = ParentObject.GetPart<Engulfing>();
						if (part != null && part.Engulfed != null && part.Engulfed.IsPlayer())
						{
							part.Engulfed.CurrentCell.ParentZone.SetActive();
						}
					}
				}
			}
			else
			{
				TurnsLeft--;
			}
		}
		return base.FireEvent(E);
	}
}
