using System;
using System.Linq;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class FollowersGetTeleport : IActivePart
{
	public string BehaviorDescription;

	public FollowersGetTeleport()
	{
		ChargeUse = 0;
		IsBootSensitive = true;
		IsEMPSensitive = true;
		MustBeUnderstood = true;
		WorksOnWearer = true;
	}

	public virtual void ApplyEffectTo(GameObject go)
	{
	}

	public virtual void UnapplyEffectTo(GameObject go)
	{
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID && ID != EquippedEvent.ID && (ID != GetShortDescriptionEvent.ID || string.IsNullOrEmpty(BehaviorDescription)))
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(BehaviorDescription);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			GameObject equipped = ParentObject.Equipped;
			if (equipped != null)
			{
				Cell cell = equipped.Physics.CurrentCell;
				if (cell != null)
				{
					foreach (GameObject item in from o in cell.ParentZone.GetObjectsWithPart("Combat")
						where o.Brain != null && o.Brain.PartyLeader == equipped
						select o)
					{
						if (item.TryGetEffect<IsAFollowerGettingTeleport>(out var Effect))
						{
							Effect.Duration = 3;
						}
						else
						{
							item.ApplyEffect(new IsAFollowerGettingTeleport());
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}
}
