using System;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class ModHypervelocity : IModification
{
	public ModHypervelocity()
	{
	}

	public ModHypervelocity(int Tier)
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
		if (Object.TryGetPart<MagazineAmmoLoader>(out var Part))
		{
			if (Part.AmmoPart == "AmmoMissile" || Part.AmmoPart == "AmmoGrenade")
			{
				return false;
			}
			if (MissileWeapon.IsVorpal(Object))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override void ApplyModification(GameObject Object)
	{
		PoweredMissilePerformance poweredMissilePerformance = Object.RequirePart<PoweredMissilePerformance>();
		poweredMissilePerformance.WantAddAttribute("Vorpal");
		poweredMissilePerformance.PenetrateCreatures = true;
		if (poweredMissilePerformance.ChargeUse > 100)
		{
			poweredMissilePerformance.ChargeUse = 100;
		}
		Object.RequirePart<EnergyCellSocket>();
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
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{hypervelocity|hypervelocity}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Hypervelocity: When powered, this weapon matches its penetration to its target's armor and penetrates creatures.");
		return base.HandleEvent(E);
	}
}
