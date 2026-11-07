using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class ElevatorSwitch : IPart
{
	public int TopLevel = 10;

	public int FloorLevel = 15;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("SwitchActivated");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "SwitchActivated")
		{
			List<Cell> adjacentCells = ParentObject.CurrentCell.GetAdjacentCells();
			adjacentCells.Add(ParentObject.CurrentCell);
			bool flag = false;
			foreach (Cell item in adjacentCells)
			{
				foreach (GameObject item2 in item.GetObjectsWithIntProperty("ElevatorPlatform"))
				{
					flag = true;
					if (item2.CurrentCell == The.Player.CurrentCell)
					{
						item2.CurrentCell.RemoveObject(The.Player);
						Zone zoneAtLevel = item2.CurrentCell.ParentZone.GetZoneAtLevel(TopLevel);
						if (ParentObject.Physics.CurrentCell.ParentZone.Z == TopLevel)
						{
							zoneAtLevel = item2.Physics.CurrentCell.ParentZone.GetZoneAtLevel(FloorLevel);
						}
						int x = item2.CurrentCell.X;
						int y = item2.CurrentCell.Y;
						zoneAtLevel.GetCell(x, y).AddObject("Platform");
						zoneAtLevel.GetCell(x, y).AddObject(The.Player);
						The.ZoneManager.SetActiveZone(zoneAtLevel);
						The.ZoneManager.ProcessGoToPartyLeader();
						item2.Destroy();
						if (zoneAtLevel.Z < ParentObject.Physics.CurrentCell.ParentZone.Z)
						{
							Popup.Show("The chrome platform begins to hum as it ascends into the darkness.");
						}
						else
						{
							Popup.Show("The chrome platform begins to hum as it descends into the darkness.");
						}
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage("Nothing seems to happen when you hit the switch.");
					}
				}
			}
			if (!flag)
			{
				foreach (Cell item3 in adjacentCells)
				{
					foreach (GameObject @object in item3.GetObjects("OpenShaft"))
					{
						if (!@object.CurrentCell.HasObject("Platform"))
						{
							@object.CurrentCell.AddObject("Platform");
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
