using System;
using UnityEngine;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class UniqueWithAlternative : IPart
{
	public string Alternative = "Campfire";

	public override bool SameAs(IPart p)
	{
		if ((p as UniqueWithAlternative).Alternative != Alternative)
		{
			return false;
		}
		return true;
	}

	public override bool CanGenerateStacked()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		Registrar.Register("Equipped");
		Registrar.Register("AddedToInventory");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" || E.ID == "Equipped" || E.ID == "AddedToInventory")
		{
			if (XRLCore.Core.Game == null)
			{
				return true;
			}
			if (ParentObject == null)
			{
				Debug.LogError("UniqueWithAlternative part had no parent object on " + E.ID);
				return true;
			}
			if (XRLCore.Core.Game.GetIntGameState("UniqueAlternative_" + ParentObject.Blueprint) > 0)
			{
				ParentObject.ReplaceWith(GameObjectFactory.Factory.CreateObject(Alternative.Contains(",") ? Alternative.Split(',').GetRandomElement() : Alternative));
			}
			else
			{
				XRLCore.Core.Game.SetIntGameState("UniqueAlternative_" + ParentObject.Blueprint, 1);
				ParentObject.RemovePart(this);
			}
		}
		return base.FireEvent(E);
	}
}
