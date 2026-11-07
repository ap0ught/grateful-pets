using System;

namespace XRL.World.Parts;

[Serializable]
public class Sweeper : IPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			foreach (GameObject item in ParentObject.CurrentCell.GetObjectsWithPart("Physics"))
			{
				if (item != ParentObject && !item.IsCombat && item.IsTakeable() && item.Weight < 100)
				{
					ParentObject.FireEvent(Event.New("CommandTakeObject", "Object", item).SetSilent(Silent: true));
					DidXToY("consume", item);
				}
			}
		}
		return base.FireEvent(E);
	}
}
