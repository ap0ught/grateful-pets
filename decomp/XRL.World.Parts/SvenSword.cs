using System;

namespace XRL.World.Parts;

[Serializable]
public class SvenSword : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Equipped");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Equipped")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("EquippingObject");
			if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
			{
				foreach (GameObject equippedObject in gameObjectParameter.Body.GetEquippedObjects((GameObject o) => o.HasPart<SvenSword>()))
				{
					if (equippedObject != ParentObject && equippedObject.Blueprint != ParentObject.Blueprint)
					{
						Achievement.WIELD_CAS_POL.Unlock();
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
