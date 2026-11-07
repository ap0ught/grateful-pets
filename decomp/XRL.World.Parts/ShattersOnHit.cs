using System;

namespace XRL.World.Parts;

[Serializable]
public class ShattersOnHit : IPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ProjectileHit");
		Registrar.Register("WeaponAfterAttack");
		base.Register(Object, Registrar);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AfterThrownEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterThrownEvent E)
	{
		Shatter();
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponAfterAttack")
		{
			Shatter(E.GetGameObjectParameter("Defender"));
		}
		else if (E.ID == "ProjectileHit")
		{
			Shatter();
		}
		return base.FireEvent(E);
	}

	public void Shatter(GameObject Target = null)
	{
		DidX("shatter");
		if (Target != null)
		{
			Inventory inventory = Target.Inventory;
			if (inventory != null)
			{
				ParentObject.RemoveFromContext();
				inventory.AddObjectNoStack(ParentObject);
			}
			else if (Target.CurrentCell != null)
			{
				ParentObject.RemoveFromContext();
				ParentObject.Physics.CurrentCell = Target.CurrentCell;
			}
		}
		else if (ParentObject.CurrentCell != null)
		{
			GameObject firstObjectWithPart = ParentObject.CurrentCell.GetFirstObjectWithPart("Inventory", (GameObject o) => o != ParentObject);
			if (firstObjectWithPart != null)
			{
				ParentObject.RemoveFromContext();
				firstObjectWithPart.Inventory.AddObjectNoStack(ParentObject);
			}
		}
		ParentObject.Destroy();
	}
}
