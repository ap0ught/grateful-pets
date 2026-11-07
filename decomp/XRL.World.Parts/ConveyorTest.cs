using System;
using XRL.UI;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts;

[Serializable]
public class ConveyorTest : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			Cell cell = PickTarget.ShowPicker(PickTarget.PickStyle.EmptyCell, 1, 999, 40, 12, Locked: false, AllowVis.Any, null, null, null, null, "Conveyor Start");
			Cell cell2 = PickTarget.ShowPicker(PickTarget.PickStyle.EmptyCell, 1, 999, 40, 12, Locked: false, AllowVis.Any, null, null, null, null, "Conveyor End");
			ConveyorBelt conveyorBelt = new ConveyorBelt();
			conveyorBelt.x1 = cell.X;
			conveyorBelt.y1 = cell.Y;
			conveyorBelt.x2 = cell2.X;
			conveyorBelt.y2 = cell2.Y;
			conveyorBelt.BuildZone(ParentObject.Physics.CurrentCell.ParentZone);
		}
		return base.FireEvent(E);
	}
}
