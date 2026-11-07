using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class PlastronoidSwarm : IPart
{
	public int SpawnTurns = 20;

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!ParentObject.IsPlayer() && !ParentObject.IsNowhere() && !ParentObject.OnWorldMap())
		{
			List<Cell> list = new List<Cell>();
			ParentObject.CurrentCell.GetAdjacentCells(3, list);
			list.Add(ParentObject.CurrentCell);
			foreach (Cell item in list)
			{
				if (item.IsEmptyForPopulation() && 50.in100())
				{
					item.AddObject("Plastronoid").MakeActive();
				}
			}
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}
}
