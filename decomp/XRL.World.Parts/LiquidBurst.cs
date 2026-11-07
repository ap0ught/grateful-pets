using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class LiquidBurst : IPart
{
	public string Liquid = "SlimePuddle";

	public int EelChance;

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
					item.AddObject(GameObjectFactory.Factory.CreateObject(Liquid));
					if (Stat.Random(1, 100) <= EelChance && !item.HasObjectWithPart("EelSpawn"))
					{
						item.AddObject(GameObjectFactory.Factory.CreateObject("EelSpawn"));
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
