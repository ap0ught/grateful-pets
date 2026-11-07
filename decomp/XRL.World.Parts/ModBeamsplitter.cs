using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class ModBeamsplitter : IModification
{
	public const int SPLIT_FACTOR = 3;

	public ModBeamsplitter()
	{
	}

	public ModBeamsplitter(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		IsTechScannable = true;
		WorksOnSelf = true;
		NameForStatus = "Beamsplitter";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart<MissileWeapon>();
	}

	public override void ApplyModification(GameObject Object)
	{
		MissileWeapon part = Object.GetPart<MissileWeapon>();
		if (part != null)
		{
			part.ShotsPerAction *= 3;
			part.ShotsPerAnimation *= 3;
		}
		IncreaseDifficultyAndComplexity(1, 2);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetFixedMissileSpreadEvent>.ID && ID != PooledEvent<GetMissileWeaponPerformanceEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetFixedMissileSpreadEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.Spread = 18;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMissileWeaponPerformanceEvent E)
	{
		if (E.Subject == ParentObject)
		{
			E.BasePenetration--;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddWithClause("{{R-R-r-r-g-g-G-G-B-B-b-b sequence|beamsplitter}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static string GetDescription(int Tier)
	{
		return "Fitted with beamsplitter: This weapon has " + Grammar.A(3) + "-way spread with each shot at -1 penetration roll.";
	}
}
