using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class ModFilters : IModification
{
	public ModFilters()
	{
	}

	public ModFilters(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnEquipper = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return !Object.HasPart<GasMask>();
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyAndComplexity(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetRespiratoryAgentPerformanceEvent>.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == ModifyDefendingSaveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddWithClause("{{Y|filters}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetRespiratoryAgentPerformanceEvent E)
	{
		if (!E.WillAllowSave && E.Object == ParentObject.Equipped)
		{
			E.LinearAdjustment -= GetFilterPerformance() * 5;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (SavingThrows.Applicable("Inhaled Gas", E.Vs))
		{
			E.Roll += GetFilterPerformance();
			E.IgnoreNatural1 = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Damage != null && E.Damage.HasAttribute("Gas") && E.Damage.HasAttribute("InhaleDanger") && E.Object == ParentObject.Equipped)
		{
			E.Damage.Amount = E.Damage.Amount * (100 - GetFilterPerformance() * 10) / 100;
		}
		return base.HandleEvent(E);
	}

	public static int GetFilterPerformance(int Tier)
	{
		return 2 + Tier;
	}

	public int GetFilterPerformance()
	{
		return GetFilterPerformance(Tier);
	}

	public static string GetDescription(int Tier)
	{
		return "Fitted with filters: This item protects against breathing in dangerous gases.";
	}
}
