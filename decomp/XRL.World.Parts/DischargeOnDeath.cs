using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class DischargeOnDeath : IPart
{
	public string Arcs = "1";

	public int Voltage = 3;

	public string DamageRange = "1d8";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (ParentObject.CurrentCell != null)
		{
			int num = Arcs.RollCached();
			if (num > 0)
			{
				List<Cell> adjacentCells = ParentObject.CurrentCell.GetAdjacentCells();
				for (int i = 0; i < num; i++)
				{
					Cell randomElement = adjacentCells.GetRandomElement();
					if (randomElement == null)
					{
						break;
					}
					ParentObject.Discharge(randomElement, Voltage, 0, DamageRange, null, ParentObject, ParentObject);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
