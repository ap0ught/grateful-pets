using System;
using System.Collections.Generic;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class GenericPack : IPart
{
	public string Table = "Bethesda Susa Wharf";

	public string Amount = "3d6";

	public int Radius = 4;

	public bool Created;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == EnteredCellEvent.ID)
			{
				return !Created;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!Created)
		{
			Created = true;
			try
			{
				List<Cell> list = Event.NewCellList();
				foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells(Radius))
				{
					if (adjacentCell.IsEmptyOfSolid())
					{
						list.Add(adjacentCell);
					}
				}
				int num = Amount.RollCached();
				int i = 0;
				for (int num2 = num; i < num2; i++)
				{
					Cell randomElement = list.GetRandomElement();
					if (randomElement != null)
					{
						PopulationResult populationResult = PopulationManager.RollOneFrom(Table);
						int j = 0;
						for (int number = populationResult.Number; j < number; j++)
						{
							GameObject gameObject = GameObject.Create(populationResult.Blueprint);
							gameObject.SetAlliedLeader<AllyPack>(ParentObject);
							randomElement.AddObject(gameObject);
							gameObject.MakeActive();
							list.Remove(randomElement);
						}
						continue;
					}
					break;
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogError("GenericPack setup", x);
			}
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}
}
