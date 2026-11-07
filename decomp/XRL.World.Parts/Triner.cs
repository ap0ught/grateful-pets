using System;
using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class Triner : Twinner
{
	public GameObject Triplet;

	public int DesiredDistance = 3;

	public int RealityStabilizationPenetration = 80;

	public override IPart DeepCopy(GameObject Parent)
	{
		return new Triner
		{
			ParentObject = Parent,
			DesiredDistance = DesiredDistance,
			RealityStabilizationPenetration = RealityStabilizationPenetration
		};
	}

	public override void Act()
	{
		TrySpawn();
		StationKeeping();
	}

	public void TrySpawn()
	{
		if ((Twin?.CurrentCell != null && Triplet?.CurrentCell != null) || !CheckMyRealityDistortionAdvisability())
		{
			return;
		}
		List<Cell> spawnCells = GetSpawnCells();
		if (spawnCells.Count < 1)
		{
			return;
		}
		Event obj = Event.New("InitiateRealityDistortionTransit");
		obj.SetParameter("Object", ParentObject);
		obj.SetParameter("Mutation", this);
		obj.SetParameter("RealityStabilizationPenetration", RealityStabilizationPenetration);
		if (Twin?.CurrentCell == null)
		{
			Cell randomElement = spawnCells.GetRandomElement();
			if (randomElement != null)
			{
				obj.SetParameter("Cell", randomElement);
				if (ParentObject.FireEvent(obj) && randomElement.FireEvent(obj))
				{
					Twin = ParentObject.DeepCopy();
					if (Triplet != null)
					{
						Triplet.GetPart<Triner>().AddCopy(Twin);
					}
					Twin.RemovePart<GivesRep>();
					Triner part = Twin.GetPart<Triner>();
					part.Twin = ParentObject;
					part.Triplet = Triplet;
					spawnCells.Remove(randomElement);
					Spawn(Twin, randomElement);
				}
			}
		}
		if (Triplet?.CurrentCell != null)
		{
			return;
		}
		Cell randomElement2 = spawnCells.GetRandomElement();
		if (randomElement2 == null)
		{
			return;
		}
		obj.SetParameter("Cell", randomElement2);
		if (ParentObject.FireEvent(obj) && randomElement2.FireEvent(obj))
		{
			Triplet = ParentObject.DeepCopy();
			if (Twin != null)
			{
				Twin.GetPart<Triner>().AddCopy(Triplet);
			}
			Triplet.RemovePart<GivesRep>();
			Triner part2 = Triplet.GetPart<Triner>();
			part2.Twin = Twin;
			part2.Triplet = ParentObject;
			spawnCells.Remove(randomElement2);
			Spawn(Triplet, randomElement2);
		}
	}

	public void AddCopy(GameObject Object)
	{
		if (Twin?.CurrentCell == null)
		{
			Twin = Object;
		}
		else if (Triplet?.CurrentCell == null)
		{
			Triplet = Object;
		}
	}

	public void StationKeeping()
	{
		if (ParentObject.Brain == null)
		{
			return;
		}
		int num = ((Twin != null) ? ParentObject.DistanceTo(Twin) : DesiredDistance);
		int num2 = ((Triplet != null) ? ParentObject.DistanceTo(Triplet) : DesiredDistance);
		if (num == DesiredDistance && num2 == DesiredDistance)
		{
			return;
		}
		GameObject target = ParentObject.Target;
		if (target != null && ParentObject.InAdjacentCellTo(target))
		{
			GameObject gameObject = Twin?.Target;
			GameObject gameObject2 = Triplet?.Target;
			if (gameObject == null || gameObject2 == null || !Twin.InAdjacentCellTo(gameObject) || !Triplet.InAdjacentCellTo(gameObject2))
			{
				return;
			}
		}
		List<Cell> passableConnectedAdjacentCellsFor = ParentObject.CurrentCell.GetPassableConnectedAdjacentCellsFor(ParentObject, 1);
		if (passableConnectedAdjacentCellsFor.Count >= 1)
		{
			if (passableConnectedAdjacentCellsFor.Count > 1)
			{
				passableConnectedAdjacentCellsFor.Sort(CellSort);
			}
			if (ParentObject.Brain.Goals.Peek() is Step)
			{
				ParentObject.Brain.Goals.Pop();
			}
			ParentObject.Brain.Think("I'm going to try keeping station with my trins.");
			ParentObject.Brain.PushGoal(new Step(ParentObject.CurrentCell.GetDirectionFromCell(passableConnectedAdjacentCellsFor[0])));
		}
	}

	private int CellSort(Cell a, Cell b)
	{
		int num = CellAvoidance(a).CompareTo(CellAvoidance(b));
		if (num != 0)
		{
			return num;
		}
		GameObject target = ParentObject.Target;
		if (target != null)
		{
			return a.DistanceTo(target).CompareTo(b.DistanceTo(target));
		}
		return 0;
	}

	private int CellAvoidance(Cell cell)
	{
		int num = ((Twin != null) ? Math.Abs(DesiredDistance - cell.DistanceTo(Twin)) : 0);
		int num2 = ((Triplet != null) ? Math.Abs(DesiredDistance - cell.DistanceTo(Triplet)) : 0);
		return num + num2;
	}

	public override void EnteredCell()
	{
		if (Twin?.CurrentCell == null || ParentObject.CurrentCell.ParentZone == Twin.CurrentCell.ParentZone || Triplet?.CurrentCell == null || ParentObject.CurrentCell.ParentZone == Triplet.CurrentCell.ParentZone)
		{
			return;
		}
		List<Cell> followCells = GetFollowCells();
		Cell cell = ParentObject.CurrentCell;
		if (Twin != null)
		{
			cell = followCells.GetRandomElement();
			if (cell != null)
			{
				followCells.Remove(cell);
				Twin.Brain.Goals.Clear();
				Twin.SystemLongDistanceMoveTo(cell);
				Twin.UseEnergy(1000, "Move Join Leader");
			}
		}
		if (Triplet != null)
		{
			cell = followCells.GetRandomElement();
			if (cell != null)
			{
				followCells.Remove(cell);
				Triplet.Brain.Goals.Clear();
				Triplet.SystemLongDistanceMoveTo(cell);
				Triplet.UseEnergy(1000, "Move Join Leader");
			}
		}
	}

	public override void OnDestroyObject()
	{
		if (Twin != null && Twin.CurrentCell == null)
		{
			Triner part = Twin.GetPart<Triner>();
			if (part != null)
			{
				part.Twin = null;
				part.Triplet = null;
			}
			Twin.Obliterate();
		}
		if (Triplet != null && Triplet.CurrentCell == null)
		{
			Triner part2 = Triplet.GetPart<Triner>();
			if (part2 != null)
			{
				part2.Twin = null;
				part2.Triplet = null;
			}
			Triplet.Obliterate();
		}
	}
}
