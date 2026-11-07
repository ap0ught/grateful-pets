using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class CryochamberWall : IPart
{
	public string Direction = "SE";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WalltrapTrigger");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WalltrapTrigger")
		{
			Cell cell = ParentObject.GetCurrentCell();
			if (cell != null && !IsBroken() && !IsRusted() && !IsEMPed())
			{
				List<string> adjacentDirections = Directions.GetAdjacentDirections(Direction, 1);
				Cell cellFromDirection = cell.GetCellFromDirection(adjacentDirections[0]);
				Cell cellFromDirection2 = cell.GetCellFromDirection(adjacentDirections[1]);
				if (cellFromDirection != null && cellFromDirection.IsEmpty() && 50.in100())
				{
					cell = cellFromDirection;
				}
				if (cellFromDirection2 != null && cellFromDirection2.IsEmpty() && 50.in100())
				{
					cell = cellFromDirection2;
				}
				if (cell != null && !cell.IsSolid(ForFluid: true))
				{
					if (!cell.HasObjectWithPart("Gas"))
					{
						cell.AddObject("CryoGas");
					}
					cell = cell.GetCellFromDirection(Direction);
					if (cell != null && !cell.IsSolid(ForFluid: true) && !cell.HasObjectWithPart("Gas"))
					{
						cell.AddObject("CryoGas");
					}
				}
			}
		}
		return true;
	}
}
