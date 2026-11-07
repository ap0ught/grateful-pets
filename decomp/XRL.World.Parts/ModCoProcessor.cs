using System;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, the item's tier for purposes of calculating
/// bonuses is increased by the standard power load bonus, i.e. 2 for
/// the standard overload power load of 400.
/// </remarks>
[Serializable]
public class ModCoProcessor : IModification
{
	public const string DEFAULT_ATTRIBUTE = "Intelligence";

	public const float DEFAULT_ATTRIBUTE_BONUS_FACTOR = 0.25f;

	public const float DEFAULT_COMPUTE_POWER_FACTOR = 2.5f;

	public string Attribute = "Intelligence";

	public float AttributeBonusFactor = 0.25f;

	public int AttributeBonusApplied;

	public float ComputePowerFactor = 2.5f;

	public ModCoProcessor()
	{
	}

	public ModCoProcessor(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		ChargeUse = 1;
		WorksOnWearer = true;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		IsPowerLoadSensitive = true;
		IsTechScannable = true;
		NameForStatus = "CoProcessor";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return IModification.CheckWornSlot(Object, "Head", null, null, null, AllowGeneric: false);
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RequirePart<EnergyCellSocket>();
		int num = Math.Max((Tier >= 5) ? (10 - Tier) : (Tier + 2), 2);
		BootSequence part = Object.GetPart<BootSequence>();
		if (part == null)
		{
			part = new BootSequence();
			part.BootTime = num;
			part.ReadoutInName = true;
			part.ReadoutInDescription = true;
			Object.AddPart(part);
		}
		else if (part.BootTime < num)
		{
			part.BootTime = num;
		}
		IncreaseDifficultyAndComplexityIfComplex(1, 2);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BootSequenceDoneEvent.ID && ID != BootSequenceInitializedEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != EquippedEvent.ID && ID != SingletonEvent<GetAvailableComputePowerEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != PooledEvent<ModificationAppliedEvent>.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		CheckApplyBonus(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		UnapplyBonus(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceDoneEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceInitializedEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddWithClause("{{brainbrine|co-processor}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription(), GetEventSensitiveAddStatusSummary(E));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAvailableComputePowerEvent E)
	{
		if (WasReady())
		{
			E.Amount += GetComputePower();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("circuitry", 10);
			E.Add("scholarship", 5);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static int GetAttributeBonus(int Tier, int PowerLoad = 100, float Factor = 0.25f)
	{
		return (int)Math.Ceiling((float)(Tier + IComponent<GameObject>.PowerLoadBonus(PowerLoad)) * Factor);
	}

	public int GetAttributeBonus(int PowerLoad)
	{
		return GetAttributeBonus(Tier, PowerLoad, AttributeBonusFactor);
	}

	public int GetAttributeBonus()
	{
		return GetAttributeBonus(MyPowerLoadLevel());
	}

	public static int GetComputePower(int Tier, int PowerLoad = 100, float Factor = 2.5f)
	{
		return (int)Math.Ceiling((float)(Tier + IComponent<GameObject>.PowerLoadBonus(PowerLoad)) * Factor);
	}

	public int GetComputePower(int PowerLoad)
	{
		return GetComputePower(Tier, PowerLoad, ComputePowerFactor);
	}

	public int GetComputePower()
	{
		return GetComputePower(MyPowerLoadLevel());
	}

	public void ApplyBonus(GameObject who = null)
	{
		if (who == null)
		{
			who = ParentObject.Equipped;
			if (who == null)
			{
				return;
			}
		}
		if (AttributeBonusApplied != 0)
		{
			UnapplyBonus();
		}
		AttributeBonusApplied = GetAttributeBonus();
		StatShifter.DefaultDisplayName = "co-processor";
		StatShifter.SetStatShift(who, Attribute, AttributeBonusApplied);
	}

	public void UnapplyBonus(GameObject who = null)
	{
		if (AttributeBonusApplied == 0)
		{
			return;
		}
		if (who == null)
		{
			who = ParentObject.Equipped;
			if (who == null)
			{
				return;
			}
		}
		StatShifter.RemoveStatShifts(who);
		AttributeBonusApplied = 0;
	}

	public bool ShouldApplyBonus(GameObject who = null)
	{
		if (who == null)
		{
			who = ParentObject.Equipped;
			if (who == null)
			{
				return false;
			}
		}
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return IsObjectActivePartSubject(who);
		}
		return false;
	}

	public bool CheckApplyBonus(GameObject who = null)
	{
		if (who == null)
		{
			who = ParentObject.Equipped;
			if (who == null)
			{
				return false;
			}
		}
		if (ShouldApplyBonus())
		{
			if (AttributeBonusApplied == 0)
			{
				ApplyBonus(who);
				return true;
			}
		}
		else if (AttributeBonusApplied != 0)
		{
			UnapplyBonus(who);
			return true;
		}
		return false;
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
		CheckApplyBonus();
	}

	public static string GetDescription(int Tier)
	{
		return "Co-Processor: When powered, this item grants " + ((Tier == 0) ? "bonus" : GetAttributeBonus(Tier, 100, 0.25f).Signed()) + " Intelligence and provides " + ((Tier == 0) ? "" : (GetComputePower(Tier, 100, 2.5f) + " units of ")) + "compute power to the local lattice.";
	}

	public static string GetDescription(int Tier, GameObject obj)
	{
		int powerLoadLevel = obj.GetPowerLoadLevel();
		return "Co-Processor: When powered, this item grants " + ((Tier == 0) ? "bonus" : GetAttributeBonus(Tier, powerLoadLevel).Signed()) + " Intelligence and provides " + ((Tier == 0) ? "" : (GetComputePower(Tier, powerLoadLevel) + " units of ")) + "compute power to the local lattice.";
	}

	public string GetInstanceDescription()
	{
		int powerLoad = MyPowerLoadLevel();
		return "Co-processor: When powered, this item grants +" + GetAttributeBonus(powerLoad) + " " + Attribute + " and provides " + GetComputePower(powerLoad) + " units of compute power to the local lattice.";
	}
}
