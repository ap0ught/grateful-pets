using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class GiantAmoeba : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeDeathRemoval");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeathRemoval" && ParentObject.Physics.CurrentCell != null)
		{
			List<Cell> adjacentCells = ParentObject.Physics.CurrentCell.GetAdjacentCells();
			adjacentCells.Add(ParentObject.Physics.CurrentCell);
			foreach (Cell item in adjacentCells)
			{
				if (!item.IsOccluding() && Stat.Random(1, 100) <= 80)
				{
					item.AddObject(GameObjectFactory.Factory.CreateObject("SlimePuddle"));
				}
			}
		}
		return base.FireEvent(E);
	}
}
