using System;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is set to
/// true, which it is not by default, the property change is increased
/// in magnitude by the standard power load bonus, i.e. 2 for the standard
/// overload power load of 400. ("Magnitude" change means a negative
/// amount will have two subtracted, not two added.)
/// </remarks>
[Serializable]
public class IntPropertyChanger : IPoweredPart
{
	public string _AffectedProperty;

	public string AfterUpdateEvent;

	public string BehaviorDescription;

	public int Amount = 1;

	public int AmountApplied = int.MinValue;

	public int PowerLoadBonusDivisor = 150;

	public bool Applied;

	public string AffectedProperty
	{
		get
		{
			return _AffectedProperty;
		}
		set
		{
			if (NameForStatus == null || NameForStatus == _AffectedProperty)
			{
				NameForStatus = value;
			}
			_AffectedProperty = value;
		}
	}

	public IntPropertyChanger()
	{
		MustBeUnderstood = true;
		WorksOnWearer = true;
	}

	public override bool SameAs(IPart p)
	{
		IntPropertyChanger intPropertyChanger = p as IntPropertyChanger;
		if (intPropertyChanger.AffectedProperty != AffectedProperty)
		{
			return false;
		}
		if (intPropertyChanger.AfterUpdateEvent != AfterUpdateEvent)
		{
			return false;
		}
		if (intPropertyChanger.Amount != Amount)
		{
			return false;
		}
		if (intPropertyChanger.BehaviorDescription != BehaviorDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeUnequippedEvent.ID && ID != BootSequenceDoneEvent.ID && ID != BootSequenceInitializedEvent.ID && ID != CellChangedEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != EquippedEvent.ID && ID != PooledEvent<ExamineSuccessEvent>.ID)
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return !BehaviorDescription.IsNullOrEmpty();
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(ExamineSuccessEvent E)
	{
		if (E.Object == ParentObject)
		{
			CheckApply();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!BehaviorDescription.IsNullOrEmpty())
		{
			E.Postfix.AppendRules(BehaviorDescription, GetEventSensitiveAddStatusSummary(E));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		CheckApply(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeUnequippedEvent E)
	{
		Unapply(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckApply();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		CheckApply();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceDoneEvent E)
	{
		CheckApply();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceInitializedEvent E)
	{
		CheckApply();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		CheckApply();
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckApply(null, !IsWorldMapActive, Amount);
	}

	private void Apply(GameObject GO)
	{
		if (Applied)
		{
			return;
		}
		if (GO != null)
		{
			int num = Amount;
			if (IsPowerLoadSensitive)
			{
				int num2 = MyPowerLoadBonus(int.MinValue, 100, PowerLoadBonusDivisor);
				if (num2 != 0)
				{
					num = ((num < 0) ? (num - num2) : (num + num2));
				}
			}
			GO.ModIntProperty(AffectedProperty, num, RemoveIfZero: true);
			AmountApplied = num;
			if (!AfterUpdateEvent.IsNullOrEmpty())
			{
				GO.FireEvent(AfterUpdateEvent);
			}
		}
		Applied = true;
	}

	private void Unapply(GameObject GO)
	{
		if (!Applied)
		{
			return;
		}
		if (GO != null)
		{
			int num = AmountApplied;
			if (num == int.MinValue)
			{
				num = Amount;
			}
			GO.ModIntProperty(AffectedProperty, -num, RemoveIfZero: true);
			if (!AfterUpdateEvent.IsNullOrEmpty())
			{
				GO.FireEvent(AfterUpdateEvent);
			}
		}
		Applied = false;
	}

	private void CheckApply(GameObject GO = null, bool ConsumeCharge = false, int Turns = 1)
	{
		if (Applied)
		{
			if (IsDisabled(ConsumeCharge, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, Turns, null, UseChargeIfUnpowered: false, 0L))
			{
				Unapply(GO ?? ParentObject.Equipped);
			}
		}
		else if (IsReady(ConsumeCharge, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, Turns, null, UseChargeIfUnpowered: false, 0L))
		{
			Apply(GO ?? ParentObject.Equipped);
		}
	}
}
