using System;

namespace XRL.World.Parts;

[Serializable]
public class ModScoped : IModification
{
	public ModScoped()
	{
	}

	public ModScoped(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart<MissileWeapon>())
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.GetPart<MissileWeapon>().AimVarianceBonus = 4;
		IncreaseDifficultyAndComplexity(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!ParentObject.HasPart<ModSmart>() && E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("scoped");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!ParentObject.HasPart<ModSmart>())
		{
			E.Base.Compound(GetPhysicalDescription());
			E.Postfix.AppendRules(GetDescription(Tier));
		}
		return base.HandleEvent(E);
	}

	public string GetPhysicalDescription()
	{
		return "";
	}

	public static string GetDescription(int Tier)
	{
		return "Scoped: This weapon has increased accuracy.";
	}
}
