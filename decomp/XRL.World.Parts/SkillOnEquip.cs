using System;
using XRL.World.Parts.Skill;
using XRL.World.Skills;

namespace XRL.World.Parts;

/// <summary>This class is not used in the base game.</summary>
/// <remarks>There is currently no concept of temporary skills, granted skill will function as prerequisite.</remarks>
[Serializable]
public class SkillOnEquip : IActivePart
{
	public string DisplayName;

	public string ClassName;

	public int AddedID;

	public bool Describe = true;

	public SkillOnEquip()
	{
		ChargeUse = 0;
		IsBootSensitive = false;
		IsEMPSensitive = false;
		WorksOnWearer = true;
		WorksOnEquipper = true;
	}

	public override void Initialize()
	{
		if (!ClassName.IsNullOrEmpty())
		{
			BaseSkill genericSkill = Skills.GetGenericSkill(ClassName);
			if (genericSkill != null)
			{
				if (DisplayName == null)
				{
					DisplayName = genericSkill.DisplayName;
				}
				return;
			}
		}
		if (DisplayName == null)
		{
			DisplayName = SkillFactory.GetSkillOrPowerName(ClassName);
		}
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		SkillOnEquip obj = base.DeepCopy(Parent, MapInv) as SkillOnEquip;
		obj.AddedID = 0;
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
		E.Postfix.AppendRules("Grants you " + DisplayName + ".");
		return base.HandleEvent(E);
	}

	private void ApplyBonus(GameObject Subject)
	{
		if (AddedID <= 0 || !Subject.IDMatch(AddedID))
		{
			UnapplyBonus(Subject);
			if (IsObjectActivePartSubject(Subject) && !Subject.HasPart(ClassName))
			{
				Subject.AddSkill(ClassName, ParentObject, "SkillOnEquip");
				AddedID = Subject.BaseID;
			}
		}
	}

	private void UnapplyBonus(GameObject Subject)
	{
		if (AddedID > 0)
		{
			if (GameObject.Validate(ref Subject) && Subject.IDMatch(AddedID))
			{
				Subject.RemoveSkill(ClassName);
			}
			else
			{
				GameObject.FindByID(AddedID)?.RemoveSkill(ClassName);
			}
			AddedID = 0;
		}
	}

	public void CheckApplyBonus(GameObject Subject, bool UseCharge = false)
	{
		if (AddedID > 0)
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
