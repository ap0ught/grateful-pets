using System;

namespace XRL.World.Parts;

[Serializable]
public class LeavesTrail : IPart
{
	public string TrailBlueprint = "SmallSlimePuddle";

	public int TrailChance = 100;

	public int TemporaryChance;

	public string TemporaryDuration = "2d3";

	public string TemporaryTurnInto;

	public int TemporaryTurnIntoChance = 100;

	public bool PassAttitudes;

	public bool VillageDeactivate = true;

	public bool Active = true;

	public bool OnEnter = true;

	public string Range = "0";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != EnteredCellEvent.ID || !OnEnter || !Active))
		{
			if (ID == LeftCellEvent.ID && !OnEnter)
			{
				return Active;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (OnEnter && Active && !ParentObject.OnWorldMap())
		{
			LeaveTrailIn(E.Cell);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		if (!OnEnter && Active && !ParentObject.OnWorldMap())
		{
			LeaveTrailIn(E.Cell);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("VillageInit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "VillageInit" && VillageDeactivate)
		{
			Active = false;
		}
		return base.FireEvent(E);
	}

	public bool LeaveTrailIn(Cell Cell)
	{
		if (Cell == null || Cell.OnWorldMap())
		{
			return false;
		}
		if (TrailBlueprint.IsNullOrEmpty())
		{
			return false;
		}
		if (!TrailChance.in100())
		{
			return false;
		}
		bool zeroRangeIncluded = Range.RollMinCached() <= 0;
		bool any = false;
		GameObjectFactory.ProcessSpecification(TrailBlueprint, (GameObject obj) =>
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
				if (TemporaryChance.in100())
				{
					Temporary p = ((TemporaryTurnInto.IsNullOrEmpty() || !TemporaryTurnIntoChance.in100()) ? new Temporary(TemporaryDuration.RollCached()) : new Temporary(TemporaryDuration.RollCached(), TemporaryTurnInto));
					obj.AddPart(p);
				}
				if (PassAttitudes)
				{
					obj.TakeBaseAllegiance(ParentObject);
				}
				randomLocalAdjacentCell.AddObject(obj);
				any = true;
			}
			else
			{
				obj.Obliterate();
			}
		}, null, 1, 0, 0, null, "LeavesTrail");
		return any;
	}
}
