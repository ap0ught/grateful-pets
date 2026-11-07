using System;

namespace XRL.World.Parts;

[Serializable]
public class Displacement : IActivePart
{
	public string Distance = "0-2";

	public int Chance = 100;

	public Displacement()
	{
		WorksOnSelf = true;
		NameForStatus = "SpatialTransposer";
	}

	public override bool SameAs(IPart p)
	{
		Displacement displacement = p as Displacement;
		if (displacement.Distance != Distance)
		{
			return false;
		}
		if (displacement.Chance != Chance)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && Chance.in100())
		{
			if (ActivePartHasMultipleSubjects())
			{
				foreach (GameObject activePartSubject in GetActivePartSubjects())
				{
					int num = Distance.RollCached();
					if (num > 0 && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
					{
						int maxDistance = num;
						bool interruptMovement = !activePartSubject.IsPlayer();
						IPart mutation = this;
						activePartSubject.RandomTeleport(IComponent<GameObject>.Visible(activePartSubject), mutation, null, null, E, 0, maxDistance, interruptMovement);
					}
				}
			}
			else
			{
				int num2 = Distance.RollCached();
				if (num2 > 0)
				{
					GameObject activePartFirstSubject = GetActivePartFirstSubject();
					if (activePartFirstSubject != null && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
					{
						int maxDistance = num2;
						bool interruptMovement = !activePartFirstSubject.IsPlayer();
						IPart mutation = this;
						activePartFirstSubject.RandomTeleport(IComponent<GameObject>.Visible(activePartFirstSubject), mutation, null, null, E, 0, maxDistance, interruptMovement);
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
