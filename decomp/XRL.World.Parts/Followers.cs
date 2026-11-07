using System;
using System.Collections.Generic;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class Followers : IPart
{
	public string Table;

	public bool spawned;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == EnteredCellEvent.ID)
			{
				return !spawned;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!spawned)
		{
			spawned = true;
			Cell cell = ParentObject.CurrentCell;
			List<Cell> connectedSpawnLocations = cell.GetConnectedSpawnLocations(12);
			foreach (PopulationResult item in PopulationManager.Generate(Table))
			{
				for (int i = 0; i < item.Number; i++)
				{
					if (connectedSpawnLocations.Count <= 0)
					{
						Cell cell2 = cell.getClosestEmptyCell() ?? cell.getClosestPassableCell();
						if (cell2 != null)
						{
							connectedSpawnLocations.Add(cell2);
						}
					}
					if (connectedSpawnLocations.Count > 0)
					{
						GameObject gameObject = GameObject.Create(item.Blueprint);
						gameObject.SetAlliedLeader<AllyProselytize>(ParentObject);
						gameObject.MakeActive();
						connectedSpawnLocations[0].AddObject(gameObject);
						connectedSpawnLocations.RemoveAt(0);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}
}
