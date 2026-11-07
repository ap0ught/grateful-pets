using System;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class ModLegendary : IModification
{
	public int Bonus = 4;

	public ModLegendary()
	{
	}

	public ModLegendary(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart<ModMasterwork>();
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ModLegendary).Bonus != Bonus)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetCriticalThresholdEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == PooledEvent<TriggersMakersMarkCreationEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(TriggersMakersMarkCreationEvent E)
	{
		if (E.ModAdded == null || E.ModAdded == this)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{Y|lege{{W|n}}dary}}", -20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.Append("\n{{rules|Legendary").Append(": This weapon is ").Append(Bonus * 5)
			.Append("% ")
			.Append((Bonus >= 0) ? "more" : "less")
			.Append(" likely to score critical hits (standard chance is 5%).")
			.Append("}}");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCriticalThresholdEvent E)
	{
		if (E.Weapon == ParentObject || E.Projectile == ParentObject)
		{
			E.Threshold -= Bonus;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("might", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
