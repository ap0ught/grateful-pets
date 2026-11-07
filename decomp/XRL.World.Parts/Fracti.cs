using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Fracti : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<PhysicalContactEvent>.ID && ID != ObjectCreatedEvent.ID && ID != ObjectEnteredCellEvent.ID)
		{
			return ID == GetNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (E.PhaseMatches(ParentObject) && CanAndWillDamage(E.Actor))
		{
			E.MinWeight(98);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		ParentObject.Render.Tile = "Terrain/sw_fracti" + Stat.Random(1, 8) + ".bmp";
		return base.HandleEvent(E);
	}

	public bool CanAndWillDamage(GameObject obj)
	{
		if (obj != null && obj.IsCombat && Factions.GetFeelingFactionToObject("Succulents", obj) < 50 && obj.PhaseAndFlightMatches(ParentObject))
		{
			return obj != ParentObject;
		}
		return false;
	}

	public void Damage(GameObject Object)
	{
		if (CanAndWillDamage(Object))
		{
			if (Object.TakeDamage(1, "from %t thorns.", "Thorns", null, null, null, ParentObject))
			{
				Object.Bloodsplatter();
			}
			if (Object.Energy != null)
			{
				Object.Energy.BaseValue -= 500;
			}
		}
	}

	public override bool HandleEvent(PhysicalContactEvent E)
	{
		if (E.Object == ParentObject)
		{
			Damage(E.Actor);
		}
		else if (E.Actor == ParentObject)
		{
			Damage(E.Object);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		Damage(E.Object);
		return base.HandleEvent(E);
	}

	public void Grow(string OkFloor, int Size, string Direction = null)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null || Size == 0)
		{
			return;
		}
		if (Direction == null)
		{
			Grow(OkFloor, Size, "N");
			Grow(OkFloor, Size, "S");
			Grow(OkFloor, Size, "E");
			Grow(OkFloor, Size, "W");
			return;
		}
		if (15.in100())
		{
			if (40.in100())
			{
				Cell localCellFromDirection = cell.GetLocalCellFromDirection(Direction);
				if (localCellFromDirection != null && localCellFromDirection.Objects.Count == 1 && localCellFromDirection.HasObject(OkFloor))
				{
					localCellFromDirection.AddObject("Fracti").GetPart<Fracti>().Grow(OkFloor, Size - 1, Direction);
				}
			}
			switch (Direction)
			{
			case "N":
				Direction = "W";
				break;
			case "W":
				Direction = "S";
				break;
			case "S":
				Direction = "E";
				break;
			case "E":
				Direction = "N";
				break;
			}
		}
		Cell localCellFromDirection2 = cell.GetLocalCellFromDirection(Direction);
		if (localCellFromDirection2 != null && localCellFromDirection2.Objects.Count == 1 && localCellFromDirection2.HasObject(OkFloor))
		{
			localCellFromDirection2.AddObject("Fracti").GetPart<Fracti>().Grow(OkFloor, Size - 1, Direction);
		}
	}
}
