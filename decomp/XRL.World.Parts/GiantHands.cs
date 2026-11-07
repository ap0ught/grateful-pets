using System;
using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class GiantHands : IActivePart
{
	private static readonly string[] AffectedSlotTypes = new string[3] { "Hand", "Hands", "Missile Weapon" };

	public GiantHands()
	{
		WorksOnSelf = true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetSlotsRequiredEvent>.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetSlotsRequiredEvent E)
	{
		if (Array.IndexOf(AffectedSlotTypes, E.SlotType) >= 0 && IsObjectActivePartSubject(E.Actor) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Decreases++;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		CheckAffected(E.Implantee, E.Part?.ParentBody);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		CheckAffected(E.Implantee, E.Part?.ParentBody);
		return base.HandleEvent(E);
	}

	public void CheckAffected(GameObject Actor, Body Body)
	{
		if (Actor == null || Body == null)
		{
			return;
		}
		List<GameObject> list = Event.NewGameObjectList();
		foreach (BodyPart item in Body.LoopParts())
		{
			if (Array.IndexOf(AffectedSlotTypes, item.Type) < 0)
			{
				continue;
			}
			GameObject equipped = item.Equipped;
			if (equipped != null && !list.Contains(equipped))
			{
				list.Add(equipped);
				int partCountEquippedOn = Body.GetPartCountEquippedOn(equipped);
				int slotsRequiredFor = equipped.GetSlotsRequiredFor(Actor, item.Type);
				if (partCountEquippedOn != slotsRequiredFor && item.TryUnequip(Silent: true, SemiForced: true) && partCountEquippedOn > slotsRequiredFor)
				{
					equipped.SplitFromStack();
					item.Equip(equipped, 0, Silent: true, ForDeepCopy: false, Forced: false, SemiForced: true);
				}
			}
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
