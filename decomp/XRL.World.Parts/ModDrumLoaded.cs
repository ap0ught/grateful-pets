using System;

namespace XRL.World.Parts;

[Serializable]
public class ModDrumLoaded : IModification
{
	public ModDrumLoaded()
	{
	}

	public ModDrumLoaded(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart<MagazineAmmoLoader>())
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		MagazineAmmoLoader part = Object.GetPart<MagazineAmmoLoader>();
		if (part != null)
		{
			part.MaxAmmo = (int)Math.Round((float)part.MaxAmmo * 1.2f);
			if (part.MaxAmmo == 1)
			{
				part.MaxAmmo = 2;
			}
			else
			{
				MissileWeapon part2 = Object.GetPart<MissileWeapon>();
				if (part2 != null)
				{
					int num = part.MaxAmmo % part2.AmmoPerAction;
					if (num != 0)
					{
						if ((double)num < (double)part2.AmmoPerAction * 0.5)
						{
							part.MaxAmmo -= num;
						}
						else
						{
							part.MaxAmmo += part2.AmmoPerAction - num;
						}
					}
				}
			}
		}
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
			E.AddAdjective("drum-loaded");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Drum-loaded: This weapon may hold 20% additional ammo.");
		return base.HandleEvent(E);
	}
}
