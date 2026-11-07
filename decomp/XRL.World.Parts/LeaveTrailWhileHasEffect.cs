using System;

namespace XRL.World.Parts;

[Serializable]
public class LeaveTrailWhileHasEffect : IPart
{
	public string Effect = "Burrowed";

	public string Specification = "PlantWall";

	public string Range = "0";

	public int Chance = 100;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == LeftCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		if (ParentObject.HasEffect(Effect))
		{
			LeaveTrailIn(E.Cell);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool LeaveTrailIn(Cell Cell)
	{
		if (Cell == null || Cell.OnWorldMap())
		{
			return false;
		}
		if (Specification.IsNullOrEmpty())
		{
			return false;
		}
		if (!Chance.in100())
		{
			return false;
		}
		bool zeroRangeIncluded = Range.RollMinCached() <= 0;
		bool any = false;
		GameObjectFactory.ProcessSpecification(Specification, (GameObject obj) =>
		{
			int num = 0;
			Cell randomLocalAdjacentCell;
			do
			{
				randomLocalAdjacentCell = Cell.GetRandomLocalAdjacentCell(Range.RollCached(), zeroRangeIncluded);
			}
			while ((randomLocalAdjacentCell == null || !randomLocalAdjacentCell.IsEmptyForPopulation()) && ++num < 10);
			if (randomLocalAdjacentCell != null && randomLocalAdjacentCell.IsEmptyForPopulation())
			{
				randomLocalAdjacentCell.AddObject(obj);
				any = true;
			}
			else
			{
				obj.Obliterate();
			}
		}, null, 1, 0, 0, null, "LeaveTrailWhileHasEffect");
		return any;
	}
}
