using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class SpawnWithLiquid : IPart
{
	public bool spawned;

	public string LiquidObject = "SaltyWaterPuddle";

	public int AdjacentPoolChance = 25;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ZoneBuiltEvent.ID)
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (E.Cell.ParentZone.Built)
		{
			Spawn();
		}
		return true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		Spawn();
		return true;
	}

	public void Spawn()
	{
		if (spawned)
		{
			return;
		}
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null)
		{
			return;
		}
		spawned = true;
		if (!cell.HasObjectWithPart("LiquidVolume") || cell.GetFirstObjectWithPart("LiquidVolume").LiquidVolume.MaxVolume >= 0)
		{
			cell.AddObject(LiquidObject);
			foreach (Cell localCardinalAdjacentCell in cell.GetLocalCardinalAdjacentCells())
			{
				if (localCardinalAdjacentCell != null && Stat.Random(1, 100) <= AdjacentPoolChance)
				{
					localCardinalAdjacentCell.AddObject(LiquidObject);
				}
			}
		}
		ParentObject.RemovePart(this);
	}
}
