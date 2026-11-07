using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class TeleportOnEat : IPart
{
	public bool Controlled;

	public string Level = "5-6";

	public override bool SameAs(IPart p)
	{
		if ((p as TeleportOnEat).Controlled != Controlled)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("travel", 5);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("OnEat");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			GameObject actor = E.GetGameObjectParameter("Eater");
			if (actor.IsRealityDistortionUsable())
			{
				if (Controlled)
				{
					Teleportation.Cast(null, Subject: actor, Level: Level);
				}
				else
				{
					Cell cell = actor.CurrentCell;
					if (cell != null && !cell.ParentZone.IsWorldMap() && IComponent<GameObject>.CheckRealityDistortionUsability(actor, null, actor, ParentObject))
					{
						List<Cell> emptyCells = cell.ParentZone.GetEmptyCells((Cell c) => IComponent<GameObject>.CheckRealityDistortionAccessibility(null, c, actor, ParentObject));
						emptyCells.Remove(cell);
						Cell randomElement = emptyCells.GetRandomElement();
						if (randomElement != null)
						{
							actor.ParticleBlip("&C\u000f", 10, 0L);
							actor.TeleportTo(randomElement, 0);
							actor.TeleportSwirl(null, "&C", Voluntary: true);
							actor.ParticleBlip("&C\u000f", 10, 0L);
							if (actor.IsPlayer())
							{
								Popup.Show("You are transported!");
							}
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
