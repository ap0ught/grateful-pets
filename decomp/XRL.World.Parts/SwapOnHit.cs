using System;
using System.Collections.Generic;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class SwapOnHit : IActivePart
{
	public int CopyDuration = 5;

	public int HostileCopyChance;

	public string HostileCopyColorString;

	public string HostileCopyPrefix;

	public string FriendlyCopyColorString;

	public string FriendlyCopyPrefix;

	[NonSerialized]
	public Dictionary<GameObject, Cell> Targets = new Dictionary<GameObject, Cell>();

	public SwapOnHit()
	{
		IsRealityDistortionBased = true;
		WorksOnEquipper = true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<ShotCompleteEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ShotCompleteEvent E)
	{
		OnShotComplete();
		Targets.Clear();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponMissleWeaponFiring");
		Registrar.Register("WeaponMissileWeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponMissileWeaponHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			Targets[gameObjectParameter] = gameObjectParameter.CurrentCell;
		}
		else if (E.ID == "WeaponMissleWeaponFiring")
		{
			Targets.Clear();
		}
		return base.FireEvent(E);
	}

	public void OnShotComplete()
	{
		if (Targets.Count <= 0 || !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		if (activePartFirstSubject == null || !activePartFirstSubject.IsValid() || activePartFirstSubject.IsNowhere())
		{
			return;
		}
		Cell objectCell = activePartFirstSubject.CurrentCell;
		GameObject randomElement = Targets.Keys.GetRandomElement();
		Event e = Event.New("InitiateRealityDistortionTransit", "Object", (object)null, "Device", (object)ParentObject, "Cell", (object)null);
		foreach (KeyValuePair<GameObject, Cell> target in Targets)
		{
			SwapPositions(activePartFirstSubject, objectCell, target.Key, target.Value, e, target.Key != randomElement);
		}
		if (!Achievement.BEAMSPLIT_SPACE_INVERTER.Achieved && activePartFirstSubject.IsPlayer() && Targets.Count > 1)
		{
			Achievement.BEAMSPLIT_SPACE_INVERTER.Unlock();
		}
	}

	public Cell GetPassableCellFor(GameObject Object, Cell Target)
	{
		if (!Target.IsPassable(Object))
		{
			Cell closestPassableCellFor = Target.getClosestPassableCellFor(Object);
			if (closestPassableCellFor != null)
			{
				return closestPassableCellFor;
			}
		}
		return Target;
	}

	public void SwapPositions(GameObject Object, Cell ObjectCell, GameObject Target, Cell TargetCell, Event E, bool Clone = false)
	{
		if (Target == null || TargetCell == null)
		{
			return;
		}
		bool flag = Target.IsValid() && !Target.IsNowhere() && Physics.IsMoveable(Target);
		E.SetParameter("Object", Target);
		E.SetParameter("Cell", ObjectCell);
		if (flag && (!Target.FireEvent(E) || !TargetCell.RemoveObject(Target)))
		{
			return;
		}
		if (Clone)
		{
			Cell passableCellFor = GetPassableCellFor(Object, TargetCell);
			TemporalFugue.CreateFugueCopyOf(Object, Object, passableCellFor, ParentObject, IsRealityDistortionBased: true, CopyDuration, HostileCopyChance, "space inverter", FriendlyCopyColorString, HostileCopyColorString, FriendlyCopyPrefix, HostileCopyPrefix);
		}
		else
		{
			E.SetParameter("Object", Object);
			E.SetParameter("Cell", TargetCell);
			if (!Object.FireEvent(E) || !ObjectCell.RemoveObject(Object))
			{
				if (flag)
				{
					TargetCell.AddObject(Target);
				}
				return;
			}
			GetPassableCellFor(Object, TargetCell).AddObject(Object);
			Object.SmallTeleportSwirl();
			if (Object.IsPlayer() || Target.IsPlayer())
			{
				IComponent<GameObject>.XDidYToZ(Object, "swap", "positions with", Target, null, "!");
			}
		}
		if (flag)
		{
			GetPassableCellFor(Target, ObjectCell).AddObject(Target);
		}
	}
}
