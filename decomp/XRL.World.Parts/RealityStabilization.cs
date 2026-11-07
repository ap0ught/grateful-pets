using System;
using UnityEngine;
using XRL.Core;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is set to
/// true, which it is not by default, and ChargeUse is over 0 and
/// Strength is not explicitly set so that ChargeStrengthFactor is in
/// play, the effective value of ChargeStrengthFactor will be increased
/// by ((power load - 100) / 30), i.e. 10 for the standard overload power
/// load of 400.
/// </remarks>
[Serializable]
public class RealityStabilization : IPoweredPart
{
	public int Visibility = 2;

	public int SelfVisibility;

	public int CellVisibilityOffset;

	public int ChargeStrengthFactor = 25;

	public int FullChargeStrength = 100;

	public bool Projective;

	public bool FromGas;

	public bool GlimmerInterference;

	public GameObject FromSource;

	public bool HitpointsAffectPerformance;

	public string _VariableStrength;

	[NonSerialized]
	private int HighestGlimmer;

	[NonSerialized]
	private Gas pGas;

	[NonSerialized]
	private DieRoll VariableStrengthRoll;

	[NonSerialized]
	private int EffectiveStrengthCache;

	[NonSerialized]
	private long EffectiveStrengthCacheTurn;

	[NonSerialized]
	private bool EffectiveStrengthCacheDisabled;

	public int _Strength;

	[NonSerialized]
	private Predicate<GameObject> ApplyDelegate;

	public string VariableStrength
	{
		get
		{
			return _VariableStrength;
		}
		set
		{
			_VariableStrength = value;
			VariableStrengthRoll = null;
		}
	}

	public int Strength
	{
		get
		{
			if (WasDisabled())
			{
				return 0;
			}
			if (VariableStrength != null)
			{
				if (VariableStrengthRoll == null)
				{
					VariableStrengthRoll = new DieRoll(VariableStrength);
				}
				return Math.Max(Stat.Roll(VariableStrengthRoll), _Strength);
			}
			if (_Strength != 0)
			{
				return _Strength;
			}
			if (FromSource != null)
			{
				if (FromSource.IsInvalid())
				{
					FromSource = null;
				}
				else
				{
					RealityStabilization part = FromSource.GetPart<RealityStabilization>();
					if (part != null)
					{
						return part.EffectiveStrength;
					}
					Debug.LogError("Had FromSource in " + ParentObject.DisplayNameOnly + " of object with no RealityStabilization part, " + FromSource.DisplayNameOnly);
				}
			}
			if (FromGas)
			{
				if (pGas == null)
				{
					pGas = ParentObject.GetPart<Gas>();
				}
				if (pGas != null)
				{
					return Math.Max(pGas.Density * (100 + pGas.Level * 20) / 100, 1);
				}
				Debug.LogError("Had RealityStabilization FromGas but no gas part on " + ParentObject.DebugName);
			}
			if (ChargeUse > 0)
			{
				int num = ChargeStrengthFactor;
				if (IsPowerLoadSensitive)
				{
					num += MyPowerLoadBonus(int.MinValue, 100, 30);
				}
				int num2 = ChargeUse * num;
				int num3 = ParentObject.QueryCharge(LiveOnly: false, 0L);
				if (num3 >= num2)
				{
					return FullChargeStrength;
				}
				return Math.Max(num3 * FullChargeStrength / num2, 0);
			}
			return 0;
		}
		set
		{
			_Strength = value;
		}
	}

	public int EffectiveStrength
	{
		get
		{
			if (EffectiveStrengthCacheTurn >= XRLCore.CurrentTurn && EffectiveStrengthCacheDisabled == WasDisabled())
			{
				return EffectiveStrengthCache;
			}
			int num = Strength;
			if (GlimmerInterference)
			{
				HighestGlimmer = 0;
				ForeachActivePartSubject(CollectGlimmer);
				if (HighestGlimmer > 0)
				{
					num -= HighestGlimmer;
					if (num < 0)
					{
						num = 0;
					}
				}
			}
			if (HitpointsAffectPerformance && num != 0)
			{
				Statistic stat = ParentObject.GetStat("Hitpoints");
				if (stat != null)
				{
					int value = stat.Value;
					int baseValue = stat.BaseValue;
					if (value < baseValue)
					{
						num = (int)Math.Round((float)(num * value) / (float)baseValue);
					}
				}
			}
			EffectiveStrengthCache = num;
			EffectiveStrengthCacheTurn = XRLCore.CurrentTurn;
			EffectiveStrengthCacheDisabled = WasDisabled();
			return num;
		}
	}

	public void FlushEffectiveStrengthCache()
	{
		EffectiveStrengthCacheTurn = 0L;
	}

	private void CollectGlimmer(GameObject obj)
	{
		int psychicGlimmer = obj.GetPsychicGlimmer();
		if (psychicGlimmer > HighestGlimmer)
		{
			HighestGlimmer = psychicGlimmer;
		}
	}

	public RealityStabilization()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
		WorksOnCellContents = true;
	}

	public int GetVisibilityFor(GameObject GO)
	{
		int num = ((GO == ParentObject) ? SelfVisibility : Visibility);
		if (CellVisibilityOffset != 0 && ParentObject.CurrentCell != null)
		{
			num += CellVisibilityOffset;
		}
		return num;
	}

	public override bool SameAs(IPart p)
	{
		RealityStabilization realityStabilization = p as RealityStabilization;
		if (realityStabilization.Visibility != Visibility)
		{
			return false;
		}
		if (realityStabilization.SelfVisibility != SelfVisibility)
		{
			return false;
		}
		if (realityStabilization.CellVisibilityOffset != CellVisibilityOffset)
		{
			return false;
		}
		if (realityStabilization.ChargeStrengthFactor != ChargeStrengthFactor)
		{
			return false;
		}
		if (realityStabilization.FullChargeStrength != FullChargeStrength)
		{
			return false;
		}
		if (realityStabilization.Projective != Projective)
		{
			return false;
		}
		if (realityStabilization.FromGas != FromGas)
		{
			return false;
		}
		if (realityStabilization.FromSource != FromSource)
		{
			return false;
		}
		if (realityStabilization.HitpointsAffectPerformance != HitpointsAffectPerformance)
		{
			return false;
		}
		if (realityStabilization.VariableStrength != VariableStrength)
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
		if (!OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
		}
		TakeEffect();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BootSequenceInitializedEvent.ID && ID != CellChangedEvent.ID && ID != EquippedEvent.ID && ID != EnteredCellEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && (ID != PooledEvent<GlimmerChangeEvent>.ID || !GlimmerInterference) && ID != ObjectEnteredCellEvent.ID && ID != OnDestroyObjectEvent.ID && (ID != PooledEvent<StatChangeEvent>.ID || !HitpointsAffectPerformance) && ID != UnequippedEvent.ID)
		{
			return ID == UsingChargeEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(StatChangeEvent E)
	{
		if (E.Name == "Hitpoints" && HitpointsAffectPerformance)
		{
			FlushEffectiveStrengthCache();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GlimmerChangeEvent E)
	{
		if (GlimmerInterference)
		{
			FlushEffectiveStrengthCache();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		FlushEffectiveStrengthCache();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		FlushEffectiveStrengthCache();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		TakeEffect();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		TakeEffect();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		TakeEffect();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.GetEffect<RealityStabilized>()?.Maintain();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceInitializedEvent E)
	{
		PossibleEffectDisruption();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		PossibleEffectDisruption();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UsingChargeEvent E)
	{
		if (ChargeUse > 0)
		{
			FlushEffectiveStrengthCache();
		}
		return base.HandleEvent(E);
	}

	public bool ApplyRealityStabilization(GameObject GO)
	{
		if (!GameObject.Validate(ref GO))
		{
			return true;
		}
		RealityStabilized effect = GO.GetEffect<RealityStabilized>();
		if (effect == null)
		{
			effect = new RealityStabilized();
			if (!GO.ForceApplyEffect(effect))
			{
				return true;
			}
			if (WorksOnAdjacentCellContents && !effect.ScanAdjacentCells && !GO.InSameCellAs(ParentObject))
			{
				effect.ScanAdjacentCells = true;
			}
			effect.SynchronizeEffect();
		}
		return true;
	}

	public void TakeEffect()
	{
		if (EffectiveStrength > 0 && !ParentObject.IsInGraveyard())
		{
			ForeachActivePartSubjectWhile(ApplyRealityStabilization, MayMoveAddOrDestroy: true);
		}
	}

	public void PossibleEffectDisruption()
	{
		if (WorksOnAdjacentCellContents)
		{
			ParentObject.ExtendedLocalEvent("MaintainRealityStabilization");
		}
		else
		{
			ParentObject.LocalEvent("MaintainRealityStabilization");
		}
		FlushEffectiveStrengthCache();
	}
}
