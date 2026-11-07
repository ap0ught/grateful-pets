using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class MultiIntPropertyChanger : IPoweredPart
{
	public string _AffectedProperties;

	public string AfterUpdateEvent;

	public string BehaviorDescription;

	public bool Applied;

	[NonSerialized]
	private Dictionary<string, int> _PropertyMap;

	public string AffectedProperties
	{
		get
		{
			return _AffectedProperties;
		}
		set
		{
			_AffectedProperties = value;
			_PropertyMap = null;
		}
	}

	public Dictionary<string, int> PropertyMap
	{
		get
		{
			if (_PropertyMap == null)
			{
				_PropertyMap = IComponent<GameObject>.MapFromString(_AffectedProperties);
			}
			return _PropertyMap;
		}
	}

	public MultiIntPropertyChanger()
	{
		MustBeUnderstood = true;
		WorksOnWearer = true;
		NameForStatus = "ActiveSystems";
	}

	public override bool SameAs(IPart p)
	{
		MultiIntPropertyChanger multiIntPropertyChanger = p as MultiIntPropertyChanger;
		if (multiIntPropertyChanger.AffectedProperties != AffectedProperties)
		{
			return false;
		}
		if (multiIntPropertyChanger.AfterUpdateEvent != AfterUpdateEvent)
		{
			return false;
		}
		if (multiIntPropertyChanger.BehaviorDescription != BehaviorDescription)
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
				return !string.IsNullOrEmpty(BehaviorDescription);
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

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(BehaviorDescription))
		{
			E.Postfix.AppendRules(BehaviorDescription, GetEventSensitiveAddStatusSummary(E));
		}
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
			foreach (string key in PropertyMap.Keys)
			{
				GO.ModIntProperty(key, PropertyMap[key], RemoveIfZero: true);
			}
			if (!string.IsNullOrEmpty(AfterUpdateEvent))
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
			foreach (string key in PropertyMap.Keys)
			{
				GO.ModIntProperty(key, -PropertyMap[key], RemoveIfZero: true);
			}
			if (!string.IsNullOrEmpty(AfterUpdateEvent))
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
