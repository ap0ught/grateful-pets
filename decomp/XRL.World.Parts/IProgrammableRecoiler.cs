using System;

namespace XRL.World.Parts;

[Serializable]
public abstract class IProgrammableRecoiler : IPoweredPart
{
	public string Sound;

	public bool Reprogrammable;

	public int TimesProgrammed;

	public IProgrammableRecoiler()
	{
		ChargeUse = 10000;
		IsRealityDistortionBased = true;
		MustBeUnderstood = true;
		WorksOnCarrier = true;
		WorksOnHolder = true;
	}

	public override bool SameAs(IPart p)
	{
		IProgrammableRecoiler programmableRecoiler = p as IProgrammableRecoiler;
		if (programmableRecoiler.Reprogrammable != Reprogrammable)
		{
			return false;
		}
		if (programmableRecoiler.TimesProgrammed != TimesProgrammed)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public virtual void ProgrammedForLocation(Zone Z, Cell C)
	{
	}

	public static bool ProgramObjectForLocation(GameObject obj, Zone Z, Cell C = null, IProgrammableRecoiler pPR = null)
	{
		if (Z == null)
		{
			return false;
		}
		if (Z.IsWorldMap())
		{
			return false;
		}
		ITeleporter partDescendedFrom = obj.GetPartDescendedFrom<ITeleporter>();
		if (partDescendedFrom == null)
		{
			return false;
		}
		if (pPR == null)
		{
			pPR = obj.GetPartDescendedFrom<IProgrammableRecoiler>();
			if (pPR == null)
			{
				return false;
			}
		}
		pPR.TimesProgrammed++;
		partDescendedFrom.DestinationZone = Z.ZoneID;
		partDescendedFrom.DestinationX = C?.X ?? (-1);
		partDescendedFrom.DestinationY = C?.Y ?? (-1);
		pPR.PlayWorldSound(pPR.Sound);
		pPR.ProgrammedForLocation(Z, C);
		return true;
	}

	public static bool ProgramObjectForLocation(GameObject obj, Cell C)
	{
		return ProgramObjectForLocation(obj, C.ParentZone, C);
	}

	public static bool ProgramObjectForLocation(GameObject obj)
	{
		return ProgramObjectForLocation(obj, obj.CurrentCell);
	}

	public bool ProgramForLocation(Zone Z, Cell C = null)
	{
		return ProgramObjectForLocation(ParentObject, Z, C, this);
	}

	public bool ProgramForLocation(Cell C)
	{
		if (C == null)
		{
			return false;
		}
		return ProgramForLocation(C.ParentZone, C);
	}

	public bool ProgramForLocation(GameObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		return ProgramForLocation(obj.CurrentCell);
	}

	public bool ProgramForLocation()
	{
		return ProgramForLocation(ParentObject);
	}

	public bool ProgramRecoiler(GameObject Actor, IEvent FromEvent = null)
	{
		if (!IsObjectActivePartSubject(Actor))
		{
			return false;
		}
		Cell cell = Actor.CurrentCell;
		if (cell.ParentZone is InteriorZone interiorZone)
		{
			cell = interiorZone.ResolveBasisCell();
		}
		if ((!Reprogrammable && TimesProgrammed > 0) || cell == null || cell.ParentZone == null || cell.OnWorldMap())
		{
			return Actor.Fail(ParentObject.Does("click", int.MaxValue, null, null, "merely") + ".");
		}
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: true, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus != ActivePartStatus.Operational)
		{
			if (activePartStatus == ActivePartStatus.Unpowered)
			{
				Actor.Fail(ParentObject.Does("do") + " not have enough charge to be imprinted with the current location.");
			}
			else
			{
				Actor.Fail(ParentObject.Does("click", int.MaxValue, null, null, "merely") + ".");
			}
			return false;
		}
		if (IsRealityDistortionBased && !Actor.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", Actor, "Device", ParentObject), FromEvent))
		{
			return false;
		}
		SoundManager.PlayUISound("Sounds/Abilities/sfx_ability_mutation_mental_generic_activate");
		DidX("vibrate", "as the current location is imprinted in " + ParentObject.its + " geospatial core", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		ProgramForLocation(cell);
		return true;
	}
}
