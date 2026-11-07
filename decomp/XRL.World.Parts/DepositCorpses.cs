using System;
using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class DepositCorpses : IActivePart
{
	private const string RESERVE_PROPERTY = "DepositeCorpsesReserve";

	public string Tag = "Corpse";

	public int MaxNavigationWeight = 30;

	public bool OwnersOnlyIfOwned = true;

	public DepositCorpses()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart Part)
	{
		DepositCorpses depositCorpses = Part as DepositCorpses;
		if (depositCorpses.Tag != Tag)
		{
			return false;
		}
		if (depositCorpses.MaxNavigationWeight != MaxNavigationWeight)
		{
			return false;
		}
		if (depositCorpses.OwnersOnlyIfOwned != OwnersOnlyIfOwned)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<IdleQueryEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (TryDepositCorpses(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool TryDepositCorpses(GameObject Actor)
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone == null)
		{
			return false;
		}
		if (Actor.Brain == null)
		{
			return false;
		}
		if (Actor.HasTagOrProperty("NoHauling"))
		{
			return false;
		}
		if (OwnersOnlyIfOwned)
		{
			string owner = ParentObject.Owner;
			if (!owner.IsNullOrEmpty() && !Actor.IsMemberOfFaction(owner))
			{
				return false;
			}
		}
		List<GameObject> list = Event.NewGameObjectList();
		currentZone.FindObjectsWithTagOrProperty(list, "Corpse");
		if (list.Count <= 0)
		{
			return false;
		}
		GameObject randomElement = list.GetRandomElement();
		if (randomElement.HasIntProperty("DepositeCorpsesReserve"))
		{
			randomElement.ModIntProperty("DepositeCorpsesReserve", -1);
			if (randomElement.HasIntProperty("DepositeCorpsesReserve"))
			{
				return false;
			}
		}
		if (Actor.WouldBeOverburdened(randomElement))
		{
			return false;
		}
		Cell cell = randomElement.CurrentCell;
		if (cell == null || cell.GetNavigationWeightFor(Actor) >= MaxNavigationWeight)
		{
			return false;
		}
		if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		randomElement.SetIntProperty("DepositeCorpsesReserve", 50);
		Actor.Brain.PushGoal(new DisposeOfCorpse(randomElement, ParentObject));
		return true;
	}
}
