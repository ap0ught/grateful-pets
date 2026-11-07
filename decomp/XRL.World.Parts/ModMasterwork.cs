using System;

namespace XRL.World.Parts;

[Serializable]
public class ModMasterwork : IModification
{
	public int Bonus = 1;

	public ModMasterwork()
	{
	}

	public ModMasterwork(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart<MeleeWeapon>() && !Object.HasPart<MissileWeapon>())
		{
			return false;
		}
		if (Object.HasPart<GeomagneticDisc>())
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
	}

	public override bool SameAs(IPart Part)
	{
		if ((Part as ModMasterwork).Bonus != Bonus)
		{
			return false;
		}
		return base.SameAs(Part);
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
		if (E.Understood() && !E.Object.HasProperName && !E.Object.HasPart<ModLegendary>())
		{
			E.AddAdjective("{{Y|masterwork}}", -20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!E.Object.HasPart<ModLegendary>())
		{
			E.Postfix.Append("\n{{rules|Masterwork").Append(": This weapon scores critical hits ").Append(Bonus * 5 + 5)
				.Append("% of the time instead of 5%.}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCriticalThresholdEvent E)
	{
		if ((E.Weapon == ParentObject || E.Projectile == ParentObject) && !ParentObject.HasPart<ModLegendary>())
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
