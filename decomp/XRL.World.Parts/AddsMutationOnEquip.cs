using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
[Obsolete("Use MutationOnEquip")]
public abstract class AddsMutationOnEquip<T> : IActivePart where T : BaseMutation, new()
{
	public int moddedAmount;

	public int Level = 1;

	public string _MutationDisplayName;

	public string ClassName;

	public string TrackingProperty;

	public bool MutationWasAdded;

	public string MutationWasAddedTo;

	public bool CanLevel;

	public bool Describe = true;

	public Guid mutationTracker;

	public string MutationDisplayName
	{
		get
		{
			if (_MutationDisplayName == null)
			{
				T val = new T();
				_MutationDisplayName = val.DisplayName;
			}
			return _MutationDisplayName;
		}
		set
		{
			_MutationDisplayName = value;
		}
	}

	public AddsMutationOnEquip()
	{
		T val = new T();
		_MutationDisplayName = val.DisplayName;
		ClassName = val.Name;
		TrackingProperty = "Equipped" + ClassName;
		CanLevel = val.CanLevel();
		ChargeUse = 0;
		IsBootSensitive = false;
		IsEMPSensitive = false;
		WorksOnEquipper = true;
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		AddsMutationOnEquip<T> obj = base.DeepCopy(Parent, MapInv) as AddsMutationOnEquip<T>;
		obj.MutationWasAdded = false;
		obj.MutationWasAddedTo = null;
		return obj;
	}

	public override bool SameAs(IPart p)
	{
		if (!(p is AddsMutationOnEquip<T>))
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BootSequenceDoneEvent.ID && ID != BootSequenceInitializedEvent.ID && ID != CellChangedEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != EquippedEvent.ID && (ID != GetShortDescriptionEvent.ID || !Describe) && ID != PowerSwitchFlippedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BootSequenceDoneEvent E)
	{
		CheckApplyBonus(ParentObject.Equipped);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceInitializedEvent E)
	{
		CheckApplyBonus(ParentObject.Equipped);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		CheckApplyBonus(ParentObject.Equipped);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckApplyBonus(ParentObject.Equipped);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		CheckApplyBonus(ParentObject.Equipped);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PowerSwitchFlippedEvent E)
	{
		CheckApplyBonus(ParentObject.Equipped);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		CheckApplyBonus(ParentObject.Equipped, UseCharge: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		CheckApplyBonus(E.Actor, UseCharge: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		UnapplyBonus(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (CanLevel)
		{
			E.Postfix.AppendRules("Grants you " + MutationDisplayName + " at level " + Level + ".");
		}
		else
		{
			E.Postfix.AppendRules("Grants you " + MutationDisplayName + ".");
		}
		return base.HandleEvent(E);
	}

	private void ApplyBonus(GameObject Subject)
	{
		if (MutationWasAdded && !Subject.IDMatch(MutationWasAddedTo))
		{
			GameObject gameObject = GameObject.FindByID(MutationWasAddedTo);
			if (gameObject != null)
			{
				UnapplyBonus(gameObject);
			}
		}
		if (!MutationWasAdded && IsObjectActivePartSubject(Subject))
		{
			mutationTracker = Subject.RequirePart<Mutations>().AddMutationMod(typeof(T), null, Level, Mutations.MutationModifierTracker.SourceType.Equipment, ParentObject.DisplayName);
			MutationWasAdded = true;
			MutationWasAddedTo = Subject.ID;
		}
	}

	private void UnapplyBonus(GameObject Subject)
	{
		if (MutationWasAdded)
		{
			if (GameObject.Validate(ref Subject) && Subject.IDMatch(MutationWasAddedTo))
			{
				Subject.RequirePart<Mutations>().RemoveMutationMod(mutationTracker);
			}
			else
			{
				GameObject.FindByID(MutationWasAddedTo)?.RequirePart<Mutations>().RemoveMutationMod(mutationTracker);
			}
			MutationWasAdded = false;
			MutationWasAddedTo = null;
		}
	}

	public void CheckApplyBonus(GameObject Subject, bool UseCharge = false)
	{
		if (MutationWasAdded)
		{
			if (!IsReady(UseCharge, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) || !IsObjectActivePartSubject(Subject))
			{
				UnapplyBonus(Subject);
			}
		}
		else if (IsReady(UseCharge, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ApplyBonus(Subject);
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
