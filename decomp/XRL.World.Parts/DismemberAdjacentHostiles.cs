using System;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, ChanceToActivate and ChancePerHostile will,
/// if less than 100, be increased by a relative percentage of
/// ((power load - 100) / 10), i.e. 30% for the standard overload power
/// load of 400.
/// </remarks>
[Serializable]
public class DismemberAdjacentHostiles : IPoweredPart
{
	public int ChanceToActivate = 100;

	public int ChancePerHostile = 10;

	public bool CanAlwaysDecapitate;

	public bool UsesChargeToActivate;

	public bool UsesChargePerHostile;

	public bool UsesChargePerDismemberment = true;

	public DismemberAdjacentHostiles()
	{
		ChargeUse = 1000;
		IsPowerLoadSensitive = true;
		WorksOnEquipper = true;
		NameForStatus = "AntipersonnelSystems";
	}

	public override bool SameAs(IPart p)
	{
		DismemberAdjacentHostiles dismemberAdjacentHostiles = p as DismemberAdjacentHostiles;
		if (dismemberAdjacentHostiles.ChanceToActivate != ChanceToActivate)
		{
			return false;
		}
		if (dismemberAdjacentHostiles.ChancePerHostile != ChancePerHostile)
		{
			return false;
		}
		if (dismemberAdjacentHostiles.CanAlwaysDecapitate != CanAlwaysDecapitate)
		{
			return false;
		}
		if (dismemberAdjacentHostiles.UsesChargeToActivate != UsesChargeToActivate)
		{
			return false;
		}
		if (dismemberAdjacentHostiles.UsesChargePerHostile != UsesChargePerHostile)
		{
			return false;
		}
		if (dismemberAdjacentHostiles.UsesChargePerDismemberment != UsesChargePerDismemberment)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckDismemberment();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ExamineCriticalFailureEvent.ID)
		{
			return ID == PooledEvent<GenericQueryEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GenericQueryEvent E)
	{
		if (E.Query == "PhaseHarmonicEligible" && IsTechScannable)
		{
			E.Result = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineCriticalFailureEvent E)
	{
		if (ExamineFailure(E, 25))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	private bool ExamineFailure(IExamineEvent E, int Chance)
	{
		if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && Chance.in100() && CheckDismemberment(E.Actor, E.Actor, null, RequireHostility: false, CheckChance: false, IgnoreSubject: true, UsePopups: true))
		{
			E.Identify = true;
			return true;
		}
		return false;
	}

	public void CheckDismemberment()
	{
		GameObject gameObject = ParentObject.Equipped ?? ParentObject.Implantee;
		if (gameObject == null)
		{
			return;
		}
		Cell cell = gameObject.CurrentCell;
		if (cell == null || cell.OnWorldMap())
		{
			return;
		}
		int num = MyPowerLoadLevel();
		int num2 = ChanceToActivate;
		if (num2 < 100 && IsPowerLoadSensitive)
		{
			num2 = num2 * (100 + IComponent<GameObject>.PowerLoadBonus(num, 100, 10)) / 100;
		}
		if (!num2.in100())
		{
			return;
		}
		bool usesChargeToActivate = UsesChargeToActivate;
		int? powerLoadLevel = num;
		if (!IsReady(usesChargeToActivate, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			return;
		}
		CheckDismemberment(cell, gameObject, num);
		foreach (Cell localAdjacentCell in cell.GetLocalAdjacentCells())
		{
			CheckDismemberment(localAdjacentCell, gameObject, num);
		}
	}

	public void CheckDismemberment(Cell C, GameObject user = null, int? load = null)
	{
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			GameObject gameObject = C.Objects[i];
			if (gameObject.Brain == null)
			{
				continue;
			}
			CheckDismemberment(gameObject, user, load);
			if (count != C.Objects.Count)
			{
				count = C.Objects.Count;
				if (i < count && C.Objects[i] != gameObject)
				{
					i--;
				}
			}
		}
	}

	public bool CheckDismemberment(GameObject Target, GameObject Actor = null, int? PowerLoad = null, bool RequireHostility = true, bool CheckChance = true, bool IgnoreSubject = false, bool UsePopups = false)
	{
		if (!GameObject.Validate(ref Target))
		{
			return false;
		}
		if (Actor == null)
		{
			Actor = ParentObject.Equipped ?? ParentObject.Implantee;
		}
		if (RequireHostility && !Target.IsHostileTowards(Actor))
		{
			return false;
		}
		if (Actor.IsInStasis())
		{
			return false;
		}
		if (Target.Body == null)
		{
			return false;
		}
		if (!Actor.FlightMatches(Target))
		{
			return false;
		}
		int? powerLoadLevel;
		if (UsesChargePerHostile)
		{
			powerLoadLevel = PowerLoad;
			if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
			{
				return false;
			}
		}
		if (CheckChance)
		{
			int num = ChancePerHostile;
			if (num < 100 && IsPowerLoadSensitive)
			{
				num = num * (100 + IComponent<GameObject>.PowerLoadBonus(PowerLoad.Value, 100, 10)) / 100;
			}
			if (!num.in100())
			{
				return false;
			}
		}
		int vsPhase = GetActivationPhaseEvent.GetFor(ParentObject, Actor.GetPhase());
		if (!Target.PhaseMatches(vsPhase))
		{
			return false;
		}
		if (!PowerLoad.HasValue)
		{
			PowerLoad = MyPowerLoadLevel();
		}
		bool usesChargePerDismemberment = UsesChargePerDismemberment;
		powerLoadLevel = PowerLoad;
		if (!IsReady(usesChargePerDismemberment, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			return false;
		}
		return Axe_Dismember.Dismember(Actor, Target, null, null, ParentObject, null, "sfx_characterTrigger_dismember", CanAlwaysDecapitate, suppressDecapitate: false, weaponActing: true, UsePopups);
	}
}
