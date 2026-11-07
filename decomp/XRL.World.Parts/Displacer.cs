using System;
using XRL.Rules;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, maximum teleport distance is increased by
/// the standard power load bonus, i.e. 2 for the standard overload power
/// load of 400.
/// </remarks>
[Serializable]
public class Displacer : IPoweredPart
{
	public int MinDistance;

	public int MaxDistance = 2;

	public int Chance = 100;

	public Displacer()
	{
		WorksOnEquipper = true;
		ChargeUse = 1;
		IsPowerLoadSensitive = true;
		NameForStatus = "SpatialTransposer";
	}

	public override bool SameAs(IPart p)
	{
		Displacer displacer = p as Displacer;
		if (displacer.MinDistance != MinDistance)
		{
			return false;
		}
		if (displacer.MaxDistance != MaxDistance)
		{
			return false;
		}
		if (displacer.Chance != Chance)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetDefensiveItemListEvent.ID && ID != ExamineCriticalFailureEvent.ID)
		{
			return ID == ExamineFailureEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetDefensiveItemListEvent E)
	{
		if (IsPowerSwitchSensitive && GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) == ActivePartStatus.SwitchedOff)
		{
			E.Add("PowerSwitchOn", 1, ParentObject, Inv: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineFailureEvent E)
	{
		if (ExamineFailure(E, 25))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineCriticalFailureEvent E)
	{
		if (ExamineFailure(E, 50))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null || cell.OnWorldMap() || !Chance.in100())
		{
			return;
		}
		int num = MyPowerLoadLevel();
		int? powerLoadLevel = num;
		if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			return;
		}
		int value = MaxDistance + IComponent<GameObject>.PowerLoadBonus(num);
		if (ActivePartHasMultipleSubjects())
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				PerformTeleport(activePartSubject, null, value, num, IgnoreSubject: false, Voluntary: true, DoAIEvaluate: true);
			}
			return;
		}
		PerformTeleport(GetActivePartFirstSubject(), null, value, num, IgnoreSubject: false, Voluntary: true, DoAIEvaluate: true);
	}

	private bool PerformTeleport(GameObject Subject, GameObject Actor = null, int? UseMaxDistance = null, int? PowerLoad = null, bool IgnoreSubject = false, bool Voluntary = true, bool DoAIEvaluate = false, bool UsePopups = false, IEvent FromEvent = null)
	{
		if (!GameObject.Validate(ref Subject))
		{
			return false;
		}
		int load = PowerLoad ?? MyPowerLoadLevel();
		int high = UseMaxDistance ?? (MaxDistance + IComponent<GameObject>.PowerLoadBonus(load));
		int num = Stat.Random(MinDistance, high);
		if (num <= 0)
		{
			return false;
		}
		int? powerLoadLevel = PowerLoad;
		if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			return false;
		}
		GameObject gameObject = Subject;
		int maxDistance = num;
		bool interruptMovement = !Subject.IsPlayer();
		GameObject parentObject = ParentObject;
		GameObject deviceOperator = Actor ?? Subject;
		bool swirl = IComponent<GameObject>.Visible(Subject);
		bool voluntary = Voluntary;
		bool usePopups = UsePopups;
		bool flag = gameObject.RandomTeleport(swirl, null, parentObject, deviceOperator, FromEvent, 0, maxDistance, interruptMovement, null, Forced: false, IgnoreCombat: true, voluntary, usePopups);
		if (flag & DoAIEvaluate)
		{
			AIEvaluate();
		}
		return flag;
	}

	public void AIEvaluate(GameObject Actor = null)
	{
		if (IsPowerSwitchSensitive)
		{
			if (Actor == null)
			{
				Actor = ParentObject.Equipped ?? ParentObject.Implantee;
			}
			if (GameObject.Validate(ref Actor) && !Actor.IsPlayer() && Actor.Target == null)
			{
				InventoryActionEvent.Check(ParentObject, Actor, ParentObject, "PowerSwitchOff");
			}
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private bool ExamineFailure(IExamineEvent E, int Chance)
	{
		if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && Chance.in100())
		{
			if (PerformTeleport(ParentObject.Holder ?? ParentObject, E.Actor, null, null, IgnoreSubject: true, Voluntary: false, DoAIEvaluate: false, UsePopups: true, E))
			{
				E.Identify = true;
				E.IdentifyIfDestroyed = true;
				return true;
			}
		}
		return false;
	}
}
