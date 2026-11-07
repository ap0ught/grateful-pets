using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class EquippedFungusAchievement : IPart
{
	public bool Triggered;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Equipped");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Equipped" && !Triggered)
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("EquippingObject");
			if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
			{
				Body body = gameObjectParameter.Body;
				List<string> list = new List<string>();
				foreach (GameObject equippedObject in body.GetEquippedObjects())
				{
					if (equippedObject.HasPart<EquippedFungusAchievement>() && !list.Contains(equippedObject.Blueprint))
					{
						list.Add(equippedObject.Blueprint);
					}
				}
				if (list.Count >= 3)
				{
					Achievement.GET_FUNGAL_INFECTIONS.Unlock();
				}
				Triggered = true;
			}
		}
		return base.FireEvent(E);
	}
}
