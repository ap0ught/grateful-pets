using System;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class Breeder : IPart
{
	public int Chance;

	public int ReductionChance;

	public string Blueprint;

	public string Range = "1";

	public bool TakeOnAttitudes = true;

	public override bool SameAs(IPart p)
	{
		Breeder breeder = p as Breeder;
		if (breeder.Chance != Chance)
		{
			return false;
		}
		if (breeder.ReductionChance != ReductionChance)
		{
			return false;
		}
		if (breeder.Blueprint != Blueprint)
		{
			return false;
		}
		if (breeder.Range != Range)
		{
			return false;
		}
		if (breeder.TakeOnAttitudes != TakeOnAttitudes)
		{
			return false;
		}
		return true;
	}

	public override bool WantTurnTick()
	{
		return Chance > 0;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckBreed(Amount);
	}

	public int CheckBreed(int Turns = 1)
	{
		int result = 0;
		if (Chance <= 0)
		{
			return result;
		}
		if (!GameObject.Validate(ParentObject))
		{
			return result;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null || cell.OnWorldMap())
		{
			return result;
		}
		string blueprint = Blueprint ?? ParentObject.Blueprint;
		for (int i = 1; i <= Turns; i++)
		{
			if (!Chance.in100())
			{
				continue;
			}
			bool any = false;
			Cell cell2;
			if (Range == "1")
			{
				cell2 = cell.GetRandomLocalAdjacentCell((Cell c) => c.IsEmptyForPopulation());
			}
			else
			{
				cell2 = cell.GetRandomLocalAdjacentCell(Range.RollCached(), (Cell c) => c.IsEmptyForPopulation());
			}
			if (cell2 != null)
			{
				GameObjectFactory.ProcessSpecification(blueprint, (GameObject obj) =>
				{
					Chance--;
					if (obj.TryGetPart<Breeder>(out var Part))
					{
						if (ReductionChance.in100())
						{
							Part.Chance = Chance - 1;
						}
						else
						{
							Part.Chance = Chance;
						}
					}
					if (TakeOnAttitudes)
					{
						obj.TakeAllegiance<AllyBirth>(ParentObject);
						obj.CopyTarget(ParentObject);
						obj.CopyLeader(ParentObject);
					}
					obj.MakeActive();
					cell2.AddObject(obj);
					int num = result;
					result = num + 1;
					any = true;
				}, null, 1, 0, 0, null, "Breeder");
				if (any)
				{
					break;
				}
			}
			if (!any || Chance <= 0)
			{
				break;
			}
		}
		return result;
	}
}
