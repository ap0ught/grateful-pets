using System;
using XRL.Rules;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, the chance of taking effect is increased by
/// the standard power load bonus, i.e. 2 for the standard overload power
/// load of 400.
/// </remarks>
[Serializable]
public class ModNanon : IModification
{
	public int Chance;

	public ModNanon()
	{
	}

	public ModNanon(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		ChargeUse = 100;
		IsPowerLoadSensitive = true;
		IsTechScannable = true;
		WorksOnSelf = true;
		NameForStatus = "Nanon";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart<MissileWeapon>())
		{
			return false;
		}
		EnergyAmmoLoader part = Object.GetPart<EnergyAmmoLoader>();
		if (part == null)
		{
			return false;
		}
		if (part.ChargeUse <= 0)
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Chance = Stat.Random(GetLowChance(Tier), GetHighChance(Tier));
		EnergyAmmoLoader part = Object.GetPart<EnergyAmmoLoader>();
		if (part != null)
		{
			ChargeUse = part.ChargeUse / 5;
		}
		IncreaseDifficultyAndComplexity(2, 2);
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
			E.AddAdjective("{{K|nanon}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription());
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("LauncherProjectileHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "LauncherProjectileHit")
		{
			int num = MyPowerLoadLevel();
			int? powerLoadLevel = num;
			if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel) && E.GetIntParameter("Penetrations") > 0)
			{
				GameObject Object = E.GetGameObjectParameter("Attacker");
				GameObject Object2 = E.GetGameObjectParameter("Defender");
				if (GameObject.Validate(ref Object2) && !Object2.IsNowhere() && GameObject.Validate(ref Object) && !Object.IsNowhere() && GetSpecialEffectChanceEvent.GetFor(Object, ParentObject, "Modification ModNanon Dismember", Subject: Object2, Projectile: E.GetGameObjectParameter("Projectile"), Chance: Chance + IComponent<GameObject>.PowerLoadBonus(num)).in100())
				{
					bool flag = 1.in1000();
					Axe_Dismember.Dismember(Object, Object2, null, null, ParentObject, null, "sfx_characterTrigger_dismember", flag, !flag, weaponActing: true);
				}
			}
		}
		return base.FireEvent(E);
	}

	public static int GetLowChance(int Tier)
	{
		return Tier;
	}

	public static int GetHighChance(int Tier)
	{
		return Tier + 1;
	}

	public static string GetDescription(int Tier)
	{
		if (Tier == 0)
		{
			return "Nanon: This weapon has a chance to dismember on penetration.";
		}
		int lowChance = GetLowChance(Tier);
		int highChance = GetHighChance(Tier);
		if (lowChance == highChance)
		{
			return "Nanon: " + highChance + "% chance to dismember on penetration";
		}
		return "Nanon: " + lowChance + "-" + highChance + "% chance to dismember on penetration";
	}

	public string GetInstanceDescription()
	{
		return "Nanon: " + GetSpecialEffectChanceEvent.GetFor(ParentObject.Equipped ?? ParentObject.Implantee, ParentObject, "Modification ModNanon Dismember", Chance + MyPowerLoadBonus()) + "% chance to dismember upon penetration.";
	}
}
