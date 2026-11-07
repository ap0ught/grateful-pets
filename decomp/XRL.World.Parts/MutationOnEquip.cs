using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class MutationOnEquip : IActivePart
{
	public int Level = 1;

	public string DisplayName;

	public string ClassName;

	public string Variant;

	public string MutationWasAddedTo;

	public bool CanLevel;

	public bool Describe = true;

	public bool MutationWasAdded;

	public Guid Tracker;

	public MutationOnEquip()
	{
		ChargeUse = 0;
		IsBootSensitive = false;
		IsEMPSensitive = false;
		WorksOnEquipper = true;
	}

	public override void Initialize()
	{
		if (!ClassName.IsNullOrEmpty())
		{
			BaseMutation genericMutation = Mutations.GetGenericMutation(ClassName, Variant);
			if (genericMutation != null)
			{
				if (DisplayName == null)
				{
					DisplayName = genericMutation.GetDisplayName();
				}
				CanLevel = genericMutation.CanLevel();
				return;
			}
		}
		if (MutationFactory.TryGetMutationEntry(DisplayName, ClassName, out var Entry))
		{
			if (DisplayName == null)
			{
				DisplayName = Entry.GetDisplayName();
			}
			if (ClassName == null)
			{
				ClassName = Entry.Class;
			}
			if (Variant == null)
			{
				Variant = Entry.Variant;
			}
			CanLevel = Entry.Mutation.CanLevel();
		}
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		MutationOnEquip obj = base.DeepCopy(Parent, MapInv) as MutationOnEquip;
		obj.MutationWasAdded = false;
		obj.MutationWasAddedTo = null;
		return obj;
	}

	public override bool SameAs(IPart Part)
	{
		return false;
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
			E.Postfix.AppendRules("Grants you " + DisplayName + " at level " + Level + ".");
		}
		else
		{
			E.Postfix.AppendRules("Grants you " + DisplayName + ".");
		}
		return base.HandleEvent(E);
	}

	private void ApplyBonus(GameObject Subject)
	{
		if (MutationWasAdded && Subject.IDMatch(MutationWasAddedTo))
		{
			GameObject gameObject = GameObject.FindByID(MutationWasAddedTo);
			if (gameObject != null)
			{
				UnapplyBonus(gameObject);
			}
		}
		if (!MutationWasAdded && IsObjectActivePartSubject(Subject))
		{
			Tracker = Subject.RequirePart<Mutations>().AddMutationMod(ClassName, Variant, Level, Mutations.MutationModifierTracker.SourceType.Equipment, ParentObject.DisplayName);
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
				Subject.RequirePart<Mutations>().RemoveMutationMod(Tracker);
			}
			else
			{
				GameObject.FindByID(MutationWasAddedTo)?.RequirePart<Mutations>().RemoveMutationMod(Tracker);
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
