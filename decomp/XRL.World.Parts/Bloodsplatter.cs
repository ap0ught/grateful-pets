using System;

namespace XRL.World.Parts;

[Serializable]
public class Bloodsplatter : IPart
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
			foreach (Cell localAdjacentCell in ParentObject.CurrentCell.GetLocalAdjacentCells())
			{
				foreach (GameObject item in localAdjacentCell.GetObjectsWithPart("Physics"))
				{
					if (item.HasPart<Render>())
					{
						item.FlingBlood();
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
