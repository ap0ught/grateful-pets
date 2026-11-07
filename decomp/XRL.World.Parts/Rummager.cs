using System;

namespace XRL.World.Parts;

[Serializable]
public class Rummager : IPart
{
	public bool Active = true;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("VillageInit");
		Registrar.Register("BeginTakeAction");
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "VillageInit")
		{
			Active = false;
		}
		if (E.ID == "BeginTakeAction")
		{
			if (CheckPickUp())
			{
				return false;
			}
		}
		else if (E.ID == "EnteredCell")
		{
			CheckPickUp();
		}
		return base.FireEvent(E);
	}

	private bool CheckPickUp()
	{
		if (Active && !ParentObject.IsPlayerControlled() && ParentObject.Target == null)
		{
			foreach (GameObject item in ParentObject.CurrentCell.GetObjectsWithPart("Physics"))
			{
				if (item != ParentObject && !item.IsCombat && item.IsTakeable() && ParentObject.GetCarriedWeight() + item.Weight < ParentObject.GetMaxCarriedWeight() && ParentObject.TakeObject(item, NoStack: false, Silent: true, 0))
				{
					DidXToY("pick", "up", item);
					return true;
				}
			}
		}
		return false;
	}
}
