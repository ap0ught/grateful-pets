using System;

namespace XRL.World.Parts;

[Serializable]
public class PopulationSpawner : IPart
{
	public bool DestroyAfterSpawn = true;

	public bool Spawned;

	public string Specification = "SaltyWaterPuddle";

	public string Range = "0";

	public override bool SameAs(IPart p)
	{
		PopulationSpawner populationSpawner = p as PopulationSpawner;
		if (populationSpawner.DestroyAfterSpawn != DestroyAfterSpawn)
		{
			return false;
		}
		if (populationSpawner.Spawned != Spawned)
		{
			return false;
		}
		if (populationSpawner.Specification != Specification)
		{
			return false;
		}
		if (populationSpawner.Range != Range)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!Spawned && E.Cell != null)
		{
			bool zeroRangeIncluded = Range.RollMinCached() <= 0;
			GameObjectFactory.ProcessSpecification(Specification, (GameObject obj) =>
			{
				int num = 0;
				Cell randomLocalAdjacentCell;
				do
				{
					randomLocalAdjacentCell = E.Cell.GetRandomLocalAdjacentCell(Range.RollCached(), zeroRangeIncluded);
				}
				while ((randomLocalAdjacentCell == null || !randomLocalAdjacentCell.IsEmptyForPopulation()) && ++num < 10);
				if (randomLocalAdjacentCell != null && randomLocalAdjacentCell.IsEmptyForPopulation())
				{
					randomLocalAdjacentCell.AddObject(obj);
					Spawned = true;
				}
				else
				{
					obj.Obliterate();
				}
			}, null, 1, 0, 0, null, "PopulationSpawner");
			if (DestroyAfterSpawn && Spawned)
			{
				ParentObject.Obliterate();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
